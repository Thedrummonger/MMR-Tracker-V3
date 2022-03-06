using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            int Index = 0;
            foreach(var i in Instance.LogicDictionary.ItemList)
            {
                Instance.ItemPool.Add(new() { Id = i.ID });
                Instance.InstanceReference.ItemDictionaryMapping.Add(i.ID, Instance.ItemPool.Count - 1);
            }

            Index = 0;
            foreach (var i in Instance.LogicFile.Logic)
            {
                Instance.InstanceReference.LogicFileMapping.Add(i.Id, Index);
                if (Instance.LogicDictionary.LocationList.Any(x => x.ID == i.Id))
                {
                    Instance.LocationPool.Add(new() { ID = i.Id });
                    Instance.InstanceReference.LocationDictionaryMapping.Add(i.Id, Instance.LocationPool.Count - 1);
                }
                else if (Instance.LogicDictionary.HintSpots.Any(x => x.ID == i.Id))
                {
                    Instance.HintPool.Add(new() { ID = i.Id });
                    Instance.InstanceReference.HintDictionaryMapping.Add(i.Id, Instance.HintPool.Count - 1);
                }
                else
                {
                    Instance.MacroPool.Add(new() { ID = i.Id });
                }
                Index++;
            }

            Index = 0;
            foreach (var i in Instance.LogicDictionary.MacroList)
            {
                Instance.InstanceReference.MacroDictionaryMapping.Add(i.ID, Index);
                Index++;
                if (i.Static && !Instance.MacroPool.Any(x => x.ID == i.ID))
                {
                    Instance.MacroPool.Add(new() { ID = i.ID });
                }
                if (i.RequiredItemsOverride != null || i.ConditionalItemsOverride != null)
                {
                    Instance.LogicOverride.Add(i.ID, new MMRData.JsonFormatLogicItem
                    {
                        Id = i.ID,
                        RequiredItems = i.RequiredItemsOverride,
                        ConditionalItems = i.ConditionalItemsOverride
                    });
                }
            }

            Instance.TrackerOptions = Instance.LogicDictionary.Options;

            return true;
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

        public enum InstanceState
        {
            Success,
            LogicFailure,
            DictionaryFailure,
            Formattingfailure
        }
    }
}
