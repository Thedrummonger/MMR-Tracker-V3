using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.LogicObjects;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public static class TrackerInstanceCreation
    {
        public static InstanceState ApplyLogicAndDict(LogicObjects.TrackerInstance Instance, string LogicFile)
        {
            try
            {
                Instance.LogicFile = MMRData.LogicFile.FromJson(LogicFile);
            }
            catch { return InstanceState.LogicFailure; }
            try
            {
                string DictionaryFile = File.ReadAllText(Dictionaryhandeling.GetJSONDictionaryPathForLogicFile(Instance.LogicFile));
                Instance.LogicDictionary = LogicDictionaryData.LogicDictionary.FromJson(DictionaryFile);
            }
            catch { return InstanceState.DictionaryFailure; }
            return InstanceState.Success;
        }

        public static bool PopulateTrackerObject(LogicObjects.TrackerInstance Instance)
        {
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

            foreach (var i in Instance.LogicFile.Logic)
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
                    var NewMacro = new MacroObject { LogicData = i };
                    Instance.Macros.MacroList.Add(NewMacro);
                }

            }

            foreach(var i in Instance.LogicDictionary.MacroList)
            {
                if (!Instance.Macros.MacroList.Any(x => x.LogicData.Id == i.ID))
                {
                    if (!i.Static) { continue; }
                    var NewMacro = new MacroObject();
                    NewMacro.LogicData = new MMRData.JsonFormatLogicItem
                    {
                        Id = i.ID,
                        ConditionalItems = new List<List<string>>(),
                        RequiredItems = new List<string>(),
                        IsTrick = false,
                        TimeAvailable = TimeOfDay.None,
                        TimeNeeded = TimeOfDay.None,
                        TimeSetup = TimeOfDay.None
                    };
                    Instance.Macros.MacroList.Add(NewMacro);
                }
                var MacroObject = Instance.Macros.MacroList.Find(x => x.LogicData.Id == i.ID);
                if (i.DynamicLogicData != null) { MacroObject.DynamicLogic = i.DynamicLogicData; }
                if (i.RequiredItemsOverride != null) { MacroObject.LogicData.RequiredItems = i.RequiredItemsOverride; }
                if (i.ConditionalItemsOverride != null) { MacroObject.LogicData.ConditionalItems = i.ConditionalItemsOverride; }
            }

            Instance.TrackerOptions.Options = Instance.LogicDictionary.Options;

            CreateLogicItemMapping(Instance);
            CreateLogicLocationMapping(Instance);

            return true;
        }

        public static void CreateLogicItemMapping(TrackerInstance instance)
        {
            Dictionary<string, LogicMapping> mappingDict = new Dictionary<string, LogicMapping>();
            List<string> MacroNames = new List<string>();

            int Index = 0;
            foreach(var i in instance.Macros.MacroList)
            {
                LogicMapping MacroMap = new LogicMapping();
                MacroMap.IndexInList = Index;
                MacroMap.logicEntryType = LogicEntryType.macro;
                mappingDict.Add(i.LogicData.Id, MacroMap);
                MacroNames.Add(i.LogicData.Id);
                Index++;
            }
            Index = 0;
            foreach (var i in instance.ItemPool.CurrentPool)
            {
                LogicMapping ItemMap = new LogicMapping();
                ItemMap.IndexInList = Index;
                ItemMap.logicEntryType = LogicEntryType.item;
                if (MacroNames.Contains(i.Id)) { mappingDict.Add($"'{i.Id}'", ItemMap); }
                else { mappingDict.Add(i.Id, ItemMap); }
                Index++;
            }

            instance.InstanceReference.LogicItemMappings = mappingDict;
        }
        public static void CreateLogicLocationMapping(TrackerInstance instance)
        {
            Dictionary<string, LogicMapping> mappingDict = new Dictionary<string, LogicMapping>();
            List<string> MacroNames = new List<string>();

            int Index = 0;
            foreach (var i in instance.Macros.MacroList)
            {
                LogicMapping MacroMap = new LogicMapping();
                MacroMap.IndexInList = Index;
                MacroMap.logicEntryType = LogicEntryType.macro;
                mappingDict.Add(i.LogicData.Id, MacroMap);
                MacroNames.Add(i.LogicData.Id);
                Index++;
            }
            Index = 0;
            foreach (var i in instance.LocationPool.Locations)
            {
                LogicMapping ItemMap = new LogicMapping();
                ItemMap.IndexInList = Index;
                ItemMap.logicEntryType = LogicEntryType.location;
                mappingDict.Add($"'{i.LogicData.Id}'", ItemMap);
                if (MacroNames.Contains(i.LogicData.Id)) { mappingDict.Add($"'{i.LogicData.Id}'", ItemMap); }
                else { mappingDict.Add(i.LogicData.Id, ItemMap); }
                Index++;
            }

            instance.InstanceReference.LogicLocationMappings = mappingDict;
        }

        public static List<string> GetAllItemsUsedInLogic(MMRData.LogicFile logicFile)
        {
            List<string> FlattenedList = new List<string>();
            foreach (var item in logicFile.Logic)
            {
                foreach (var i in item.RequiredItems)
                {
                    FlattenedList.Add(i);
                }
                foreach (var con in item.ConditionalItems)
                {
                    foreach (var i in con)
                    {
                        FlattenedList.Add(i);
                    }
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
            else if (instance.ItemPool.SearchPoolForMatchingItem(Entry) != null)
            {
                var itemObject = instance.ItemPool.SearchPoolForMatchingItem(Entry);
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
