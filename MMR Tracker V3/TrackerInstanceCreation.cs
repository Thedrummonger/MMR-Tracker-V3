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
        public static InstanceState ApplyLogicAndDict(TrackerInstance Instance, string LogicFile, string DictionaryFile = null)
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
                Instance.ItemPool.Add(i.ID, new() { Id = i.ID, referenceData = new ReferenceData { DictIndex = Index } });
                Index++;
            }

            foreach (var i in Instance.LogicDictionary.AreaList)
            {
                if (!Instance.EntrancePool.AreaList.ContainsKey(i)) { Instance.EntrancePool.AreaList.Add(i, new EntranceData.EntranceRandoArea() { ID = i }); }
            }

            Index = 0;
            foreach (var i in Instance.LogicFile.Logic)
            {
                if (Instance.LogicDictionary.AdditionalLogic.Any(x => x.Id == i.Id)) { Index++; continue; }
                ParseLogicItem(i, Index, LogicFileType.Logic);
                Index++;
            }
            Index = 0;
            foreach (var i in Instance.LogicDictionary.AdditionalLogic)
            {
                ParseLogicItem(i, Index, LogicFileType.Additional);
                Index++;
            }
            Index = 0;
            foreach (var i in Instance.LogicDictionary.MacroList)
            {
                if (Instance.MacroPool.ContainsKey(i.ID))
                {
                    Instance.MacroPool[i.ID].referenceData.DictIndex = Index;
                }
                Index++;
            }

            void ParseLogicItem(MMRData.JsonFormatLogicItem i, int Index, LogicFileType Source)
            {
                if (Instance.LogicDictionary.LocationList.Any(x => x.ID == i.Id))
                {
                    var DictEntry = Instance.LogicDictionary.LocationList.First(x => x.ID == i.Id);
                    var ValidItems = Instance.LogicDictionary.ItemList.Where(x => x.ItemTypes.Intersect(DictEntry.ValidItemTypes).Any());
                    Instance.LocationPool.Add(i.Id, new()
                    {
                        ID = i.Id,
                        SingleValidItem = ValidItems.Count() == 1 ? ValidItems.First().ID : null,
                        referenceData = new ReferenceData { LogicIndex = Index, DictIndex = Instance.LogicDictionary.LocationList.IndexOf(DictEntry), LogicList = Source }
                    });

                    if (DictEntry.LocationProxys.Any())
                    {
                        if (!Instance.LocationProxyData.LocationsWithProxys.ContainsKey(i.Id)) { Instance.LocationProxyData.LocationsWithProxys.Add(i.Id, new List<string>()); }
                        Instance.LocationProxyData.LocationsWithProxys[i.Id].AddRange(DictEntry.LocationProxys.Select(x => x.Name));
                        foreach(var proxy in DictEntry.LocationProxys) { Instance.LocationProxyData.LocationProxies.Add(proxy.ID, proxy.ToInstanceData(DictEntry)); }
                    }
                }
                else if (Instance.LogicDictionary.HintSpots.Any(x => x.ID == i.Id))
                {
                    var DictEntry = Instance.LogicDictionary.HintSpots.First(x => x.ID == i.Id);
                    Instance.HintPool.Add(i.Id, new() 
                    { 
                        ID = i.Id,
                        referenceData = new ReferenceData { LogicIndex = Index, DictIndex = Instance.LogicDictionary.HintSpots.IndexOf(DictEntry), LogicList = Source }
                    });
                }
                else if (Instance.LogicDictionary.EntranceList.Any(x => x.ID == i.Id))
                {
                    var DictEntry = Instance.LogicDictionary.EntranceList.First(x => x.ID == i.Id);
                    Instance.InstanceReference.EntranceLogicNameToEntryData.Add(i.Id, new EntranceData.EntranceAreaPair { Area = DictEntry.Area, Exit = DictEntry.Exit });
                    Instance.AddLogicExitReference(new EntranceData.EntranceAreaPair { Area = DictEntry.Area, Exit = DictEntry.Exit }, i.Id);
                    var DestinationList = DictEntry.RandomizableEntrance ? Instance.EntrancePool.AreaList[DictEntry.Area].LoadingZoneExits : Instance.EntrancePool.AreaList[DictEntry.Area].MacroExits;
                    DestinationList.Add(DictEntry.Exit, new EntranceData.EntranceRandoExit
                    {
                        ParentAreaID = DictEntry.Area,
                        ID = DictEntry.Exit,
                        EntrancePair = DictEntry.RandomizableEntrance ? DictEntry.EntrancePairID : null,
                        IsWarp = DictEntry.AlwaysAccessable,
                        referenceData = new ReferenceData { LogicIndex = Index, DictIndex = Instance.LogicDictionary.EntranceList.IndexOf(DictEntry), LogicList = Source }
                    });
                }
                else
                {
                    Instance.MacroPool.Add(i.Id, new() 
                    { 
                        ID = i.Id,
                        referenceData = new ReferenceData { LogicIndex = Index, DictIndex = -1, LogicList = Source }
                    });
                }
            }



            //Wallet and Price Data

            Instance.PriceData.WalletEntries = Utility.GetAllWalletLogicEntries(Instance);
            Dictionary<string, int> ItemWallets = Instance.LogicDictionary.ItemList
                .Where(x => x.WalletCapacity != null && (int)x.WalletCapacity > -1)
                .ToDictionary(x => x.ID, x => (int)x.WalletCapacity);
            Dictionary<string, int> MacroWallets = Instance.LogicDictionary.MacroList
                .Where(x => x.WalletCapacity != null && (int)x.WalletCapacity > -1)
                .ToDictionary(x => x.ID, x => (int)x.WalletCapacity);
            Dictionary<string, int> AllWallets = ItemWallets.Concat(MacroWallets).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var i in AllWallets)
            {
                string CanAffordString = $"MMRTCanAfford{i.Value}";
                Debug.WriteLine($"Adding Wallet {CanAffordString}");
                Instance.MacroPool.Add(CanAffordString, new() { 
                    ID = CanAffordString,
                    referenceData = new ReferenceData { LogicIndex = Index, DictIndex = -1, LogicList = LogicFileType.Runtime }
                });
                Instance.PriceData.CapacityMap.Add(i.Value, CanAffordString);
                Instance.RuntimeLogic.Add(CanAffordString, new MMRData.JsonFormatLogicItem
                {
                    Id = CanAffordString,
                    RequiredItems = new List<string>(),
                    ConditionalItems = AllWallets.Where(x => x.Value >= i.Value).Select(x => new List<string> { x.Key }).ToList()
                });
            }
            Instance.PriceData.Initialized = true;

            Instance.EntrancePool.RootArea = Instance.LogicDictionary.RootArea??"Root";

            Instance.StaticOptions.MinimizedHeader.Add("Hidden Locations:::LBValidLocations", true);

            Debug.WriteLine(JsonConvert.SerializeObject(Instance.PriceData.WalletEntries, Testing._NewtonsoftJsonSerializerOptions));
            Debug.WriteLine(JsonConvert.SerializeObject(Instance.PriceData.CapacityMap, Testing._NewtonsoftJsonSerializerOptions));

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
