using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static MMR_Tracker_V3.LogicObjects;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public static class TrackerInstanceCreation
    {
        public static InstanceState ApplyLogicAndDict(LogicObjects.TrackerInstance Instance, string LogicFile, string DictionaryFile = null)
        {
            try
            {
                Instance.LogicFile = MMRData.LogicFile.FromJson(LogicFile);
            }
            catch { return InstanceState.LogicFailure; }
            try
            {
                if (DictionaryFile == null)
                {
                    DictionaryFile = File.ReadAllText(Dictionaryhandeling.GetJSONDictionaryPathForLogicFile(Instance.LogicFile));
                }
                Instance.LogicDictionary = LogicDictionaryData.LogicDictionary.FromJson(DictionaryFile);
            }
            catch { return InstanceState.DictionaryFailure; }
            return InstanceState.Success;
        }

        public static bool PopulateTrackerObject(TrackerInstance Instance)
        {
            Instance.UserOptions = Instance.LogicDictionary.Options.ToDictionary(x => x.ID, y => y);
            Instance.Variables = Instance.LogicDictionary.Variables.ToDictionary(x => x.ID, y => y);

            Instance.EntrancePool.IsEntranceRando = Instance.EntrancePool.CheckForRandomEntrances();

            int Index = 0;
            foreach(var i in Instance.LogicDictionary.ItemList)
            {
                Instance.ItemPool.Add(i.ID, new() { Id = i.ID });
                Instance.InstanceReference.ItemDictionaryMapping.Add(i.ID, Instance.ItemPool.Count - 1);
            }

            foreach (var i in Instance.LogicDictionary.AreaList)
            {
                if (!Instance.EntrancePool.AreaList.ContainsKey(i)) { Instance.EntrancePool.AreaList.Add(i, new EntranceData.EntranceRandoArea() { ID = i }); }
            }

            Index = 0;
            foreach (var i in Instance.LogicFile.Logic)
            {
                Instance.InstanceReference.LogicFileMapping.Add(i.Id, Index);
                ParseLogicItem(i);
                Index++;
            }
            Index = 0;
            foreach (var i in Instance.LogicDictionary.AdditionalLogic)
            {
                if (Instance.LogicFile.Logic.Any(x => x.Id == i.Id)) { continue; }
                Instance.InstanceReference.AdditionalLogicFileMapping.Add(i.Id, Index);
                ParseLogicItem(i);
                Index++;
            }

            void ParseLogicItem(MMRData.JsonFormatLogicItem i)
            {
                if (Instance.LogicDictionary.LocationList.Any(x => x.ID == i.Id))
                {
                    var DictEntry = Instance.LogicDictionary.LocationList.First(x => x.ID == i.Id);
                    var ValidItems = Instance.LogicDictionary.ItemList.Where(x => x.ItemTypes.Intersect(DictEntry.ValidItemTypes).Any());
                    Instance.LocationPool.Add(i.Id, new()
                    {
                        ID = i.Id,
                        SingleValidItem = ValidItems.Count() == 1 ? ValidItems.First().ID : null
                    });
                    Instance.InstanceReference.LocationDictionaryMapping.Add(i.Id, Instance.LogicDictionary.LocationList.IndexOf(DictEntry));
                }
                else if (Instance.LogicDictionary.HintSpots.Any(x => x.ID == i.Id))
                {
                    var DictEntry = Instance.LogicDictionary.HintSpots.First(x => x.ID == i.Id);
                    Instance.HintPool.Add(i.Id, new() { ID = i.Id });
                    Instance.InstanceReference.HintDictionaryMapping.Add(i.Id, Instance.LogicDictionary.HintSpots.IndexOf(DictEntry));
                }
                else if (Instance.LogicDictionary.EntranceList.Any(x => x.ID == i.Id))
                {
                    var DictEntry = Instance.LogicDictionary.EntranceList.First(x => x.ID == i.Id);
                    Instance.InstanceReference.EntranceLogicNameToEntryData.Add(i.Id, new EntranceData.EntranceAreaPair { Area = DictEntry.Area, Exit = DictEntry.Exit });
                    Instance.EntrancePool.AddLogicExitReference(new EntranceData.EntranceAreaPair { Area = DictEntry.Area, Exit = DictEntry.Exit }, i.Id);
                    var DestinationList = DictEntry.RandomizableEntrance ? Instance.EntrancePool.AreaList[DictEntry.Area].LoadingZoneExits : Instance.EntrancePool.AreaList[DictEntry.Area].MacroExits;
                    DestinationList.Add(DictEntry.Exit, new EntranceData.EntranceRandoExit
                    {
                        ParentAreaID = DictEntry.Area,
                        ID = DictEntry.Exit,
                        EntrancePair = DictEntry.RandomizableEntrance ? DictEntry.EntrancePairID : null,
                        IsWarp = DictEntry.AlwaysAccessable
                    });
                }
                else
                {
                    Instance.MacroPool.Add(i.Id, new() { ID = i.Id });
                }
            }

            Index = 0;
            foreach (var i in Instance.LogicDictionary.MacroList)
            {
                Instance.InstanceReference.MacroDictionaryMapping.Add(i.ID, Index);
                Index++;
                if (i.Static && !Instance.MacroPool.ContainsKey(i.ID))
                {
                    Instance.MacroPool.Add(i.ID, new() { ID = i.ID });
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


            //Wallet and Price Data

            Instance.PriceData.WalletEntries = Utility.GetAllWalletLogicEntries(Instance);
            var ItemWallets = Instance.LogicDictionary.ItemList
                .Where(x => x.WalletCapacity != null && (int)x.WalletCapacity > -1)
                .ToDictionary(x => x.ID, x => (int)x.WalletCapacity);
            var MacroWallets = Instance.LogicDictionary.MacroList
                .Where(x => x.WalletCapacity != null && (int)x.WalletCapacity > -1)
                .ToDictionary(x => x.ID, x => (int)x.WalletCapacity);
            Instance.PriceData.Wallets = ItemWallets.Concat(MacroWallets).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var i in Instance.PriceData.Wallets)
            {
                string CanAffordString = $"MMRTCanAfford{i.Value}";
                Debug.WriteLine($"Adding Wallet {CanAffordString}");
                Instance.MacroPool.Add(CanAffordString, new() { ID = CanAffordString });
                Instance.PriceData.CapacityMap.Add(i.Value, CanAffordString);
                Instance.LogicOverride.Add(CanAffordString, new MMRData.JsonFormatLogicItem
                {
                    Id = CanAffordString,
                    RequiredItems = new List<string>(),
                    ConditionalItems = Instance.PriceData.Wallets.Where(x => x.Value >= i.Value).Select(x => new List<string> { x.Key }).ToList()
                });
            }
            Instance.PriceData.Initialized = true;

            Debug.WriteLine(JsonConvert.SerializeObject(Instance.PriceData.WalletEntries, Testing._NewtonsoftJsonSerializerOptions));
            Debug.WriteLine(JsonConvert.SerializeObject(Instance.PriceData.CapacityMap, Testing._NewtonsoftJsonSerializerOptions));
            Debug.WriteLine(JsonConvert.SerializeObject(Instance.PriceData.Wallets, Testing._NewtonsoftJsonSerializerOptions));

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
