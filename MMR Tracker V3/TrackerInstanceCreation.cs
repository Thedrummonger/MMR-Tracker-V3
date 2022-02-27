using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.LogicObjects;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public static class TrackerInstanceCreation
    {
        public static InstanceState PopulateTrackerObject(string LogicFile, string DictionaryFile, LogicObjects.TrackerInstance Instance)
        {
            MMRData.LogicFile MMRLogicFile;
            try
            {
                MMRLogicFile = MMRData.LogicFile.FromJson(LogicFile);
            }
            catch { return InstanceState.LogicFailure; }
            try
            {
                Instance.LogicDictionary = LogicDictionaryData.LogicDictionary.FromJson(DictionaryFile);
            }
            catch { return InstanceState.DictionaryFailure; }

            List<ItemData.ItemObject> ItemPool = new List<ItemData.ItemObject>();
            foreach(var i in Instance.LogicDictionary.ItemList)
            {
                ItemData.ItemObject NewEntry = new();
                NewEntry.Id = i.ID;
                NewEntry.ItemName = i.Name;
                NewEntry.AltItemNames = i.AltNames;
                NewEntry.ItemTypes = i.ItemTypes;
                NewEntry.MaxAmountInPool = i.MaxAmountInWorld;
                ItemPool.Add(NewEntry);
            }
            Instance.ItemPool.CurrentPool = ItemPool.ToArray();

            foreach (var i in MMRLogicFile.Logic)
            {
                if (Instance.LogicDictionary.LocationList.Any(x => x.ID == i.Id))
                {
                    var DictReference = Instance.LogicDictionary.LocationList.Find(x => x.ID == i.Id);
                    LocationData.LocationObject NewEntry = new();
                    NewEntry.LogicData = i;
                    NewEntry.UIData.LocationArea = DictReference.Area;
                    NewEntry.UIData.LocationName = DictReference.Name;
                    NewEntry.TrackerData.ValidItemTypes = DictReference.ValidItemTypes;

                    if (Instance.ItemPool.CurrentPool.Any(x => x.Id == DictReference.OriginalItem))
                    {
                        NewEntry.TrackerData.VanillaItem = Instance.ItemPool.CurrentPool.First(x => x.Id == DictReference.OriginalItem).Id;
                    }

                    Instance.LocationPool.Locations.Add(NewEntry);
                }
                else
                {
                    Instance.Macros.MacroList.Add(new MacroObject { LogicData = i });
                }

            }

            CreateLogicMapping(MMRLogicFile, Instance);

            return InstanceState.Success;
        }

        public static void CreateLogicMapping(MMRData.LogicFile logicFile, TrackerInstance instance)
        {
            Dictionary<string, LogicMapping> mappingDict = new Dictionary<string, LogicMapping>();

            foreach(var i in logicFile.Logic)
            {
                foreach(var j in FlattenLogic(i))
                {
                    if (!mappingDict.ContainsKey(j))
                    {
                        LogicMapping mapping = new LogicMapping();
                        mapping.logicEntryType = logicEntryType(instance, j, out object FoundObject);

                        if (mapping.logicEntryType == LogicEntryType.item)
                        {
                            mapping.IndexInList = Array.IndexOf(instance.ItemPool.CurrentPool, (ItemData.ItemObject)FoundObject);
                        }
                        else if (mapping.logicEntryType == LogicEntryType.macro)
                        {
                            mapping.IndexInList = instance.Macros.MacroList.IndexOf((MacroObject)FoundObject);
                        }
                        else
                        {
                            mapping.IndexInList = -1;
                        }

                        mappingDict.Add(j, mapping);
                    }
                }
            }
            instance.InstanceReference.LogicDataMappings = mappingDict;
        }

        public static List<string> FlattenLogic(MMRData.JsonFormatLogicItem item)
        {
            List<string> FlattenedList = new List<string>();
            foreach (var i in item.RequiredItems)
            {
                FlattenedList.Add(i);
            }
            foreach(var con in item.ConditionalItems)
            {
                foreach (var i in con)
                {
                    FlattenedList.Add(i);
                }
            }
            return FlattenedList.Distinct().ToList();
        }

        public static LogicEntryType logicEntryType(this LogicObjects.TrackerInstance instance, string Entry, out object Object)
        {
            bool ForceItem = false;
            if (Entry.StartsWith("'") && Entry.EndsWith("'"))
            {
                Entry = Entry.Replace("'", "");
                ForceItem = true;
            }

            if (instance.Macros.MacroList.Any(x => x.LogicData.Id == Entry) && !ForceItem)
            {
                var MacroObject = instance.Macros.MacroList.Find(x => x.LogicData.Id == Entry);
                Object = MacroObject;
                return LogicEntryType.macro;
            }
            else if (instance.ItemPool.GetItemByString(Entry) != null)
            {
                var itemObject = instance.ItemPool.GetItemByString(Entry);
                Object = itemObject;
                return LogicEntryType.item;
            }
            else
            {
                Object = null;
                return LogicEntryType.error;
            }
        }

        public enum InstanceState
        {
            Success,
            LogicFailure,
            DictionaryFailure,
            Formattingfailure
        }
    }
}
