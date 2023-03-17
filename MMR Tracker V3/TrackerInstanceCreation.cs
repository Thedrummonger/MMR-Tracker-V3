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

        public static InstanceState ApplyLogicAndDict(this InstanceContainer Container, string[] LogicData, string DictionaryData = null)
        {
            return Container.ApplyLogicAndDict(string.Join("", LogicData), DictionaryData);
        }
        public static InstanceState ApplyLogicAndDict(this InstanceContainer Container, string[] LogicData, string[] DictionaryData)
        {
            return Container.ApplyLogicAndDict(string.Join("", LogicData), string.Join("", DictionaryData));
        }
        public static InstanceState ApplyLogicAndDict(this InstanceContainer Container, string LogicData, string[] DictionaryData)
        {
            return Container.ApplyLogicAndDict(LogicData, string.Join("", DictionaryData));
        }
        public static InstanceState ApplyLogicAndDict(this InstanceContainer Container, string LogicData, string DictionaryData = null)
        {
            Container.Instance ??= new();
            try { Container.Instance.LogicFile = MMRData.LogicFile.FromJson(LogicData); }
            catch { return InstanceState.LogicFailure; }
            if (DictionaryData is null)
            {
                Container.Instance.LogicDictionary = Dictionaryhandeling.FindValidDictionary(Container.Instance.LogicFile, References.Globalpaths.BaseDictionaryPath);
                if (Container.Instance.LogicDictionary is null) { return InstanceState.DictionaryFailure; }
            }
            else
            {
                try { Container.Instance.LogicDictionary = LogicDictionaryData.LogicDictionary.FromJson(DictionaryData); }
                catch { Container.Instance.LogicDictionary = null; }
                try { Container.Instance.LogicDictionary = LogicDictionaryData.LogicDictionary.FromJson(Utility.ConvertYamlStringToJsonString(DictionaryData)); }
                catch { Container.Instance.LogicDictionary = null; }
                if (Container.Instance.LogicDictionary is null) { return InstanceState.DictionaryFailure; }
            }
            return InstanceState.Success;
        }
        public static InstanceState GenerateInstance(this InstanceContainer Container, string[] LogicFile, string[] DictionaryFile)
        {
            return Container.GenerateInstance(string.Join("", LogicFile), DictionaryFile);
        }
        public static InstanceState GenerateInstance(this InstanceContainer Container, string LogicFile, string[] DictionaryFile)
        {
            return Container.GenerateInstance(string.Join("", LogicFile), string.Join("", DictionaryFile));
        }
        public static InstanceState GenerateInstance(this InstanceContainer Container, string[] LogicFile, string DictionaryFile = null)
        {
            return Container.GenerateInstance(LogicFile, string.Join("", DictionaryFile));
        }
        public static InstanceState GenerateInstance(this InstanceContainer Container, string LogicFile, string DictionaryFile = null)
        {
            var Apply = Container.ApplyLogicAndDict(LogicFile, DictionaryFile);
            if (Apply != InstanceState.Success) { return Apply; }
            Container.GenerateInstance();
            return Apply;
        }

        public static bool GenerateInstance(this InstanceContainer Container)
        {
            Container.Instance ??= new();
            var Instance = Container.Instance;
            Instance.UserOptions = Instance.LogicDictionary.Options.ToDictionary(x => x.Key, y => y.Value);
            Instance.Variables = Instance.LogicDictionary.Variables.ToDictionary(x => x.Key, y => y.Value);

            int Index = 0;
            foreach(var i in Instance.LogicDictionary.ItemList)
            {
                Instance.ItemPool.Add(i.Key, new() { Id = i.Key });
                Index++;
            }

            foreach (var i in Instance.LogicDictionary.GetAreas())
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
                if (Instance.MacroPool.ContainsKey(i.Key))
                {
                    Instance.MacroPool[i.Key].Currency = i.Value.WalletCurrency;
                }
                Index++;
            }

            void ParseLogicItem(MMRData.JsonFormatLogicItem i, int Index, LogicFileType Source)
            {
                if (Instance.LogicDictionary.LocationList.Any(x => x.Key == i.Id))
                {
                    var DictEntry = Instance.LogicDictionary.LocationList.First(x => x.Key == i.Id);
                    var ValidItems = Instance.LogicDictionary.ItemList.Where(x => x.Value.ItemTypes.Intersect(DictEntry.Value.ValidItemTypes).Any());
                    Instance.LocationPool.Add(i.Id, new()
                    {
                        ID = i.Id,
                        Currency = DictEntry.Value.WalletCurrency,
                        SingleValidItem = ValidItems.Count() == 1 ? ValidItems.First().Key : null,
                        referenceData = new ReferenceData { LogicIndex = Index, LogicList = Source }
                    });

                    if (DictEntry.Value.LocationProxys.Any())
                    {
                        if (!Instance.LocationProxyData.LocationsWithProxys.ContainsKey(i.Id)) { Instance.LocationProxyData.LocationsWithProxys.Add(i.Id, new List<string>()); }
                        Instance.LocationProxyData.LocationsWithProxys[i.Id].AddRange(DictEntry.Value.LocationProxys.Select(x => x.Name));
                        foreach(var proxy in DictEntry.Value.LocationProxys) { Instance.LocationProxyData.LocationProxies.Add(proxy.ID, proxy.ToInstanceData(DictEntry.Value)); }
                    }
                }
                else if (Instance.LogicDictionary.HintSpots.Any(x => x.Key == i.Id))
                {
                    var DictEntry = Instance.LogicDictionary.HintSpots.First(x => x.Key == i.Id);
                    Instance.HintPool.Add(i.Id, new() 
                    { 
                        ID = i.Id,
                        referenceData = new ReferenceData { LogicIndex = Index, LogicList = Source }
                    });
                }
                else if (Instance.LogicDictionary.EntranceList.Any(x => x.Key == i.Id))
                {
                    var DictEntry = Instance.LogicDictionary.EntranceList.First(x => x.Key == i.Id);
                    Instance.InstanceReference.EntranceLogicNameToEntryData.Add(i.Id, new EntranceData.EntranceAreaPair { Area = DictEntry.Value.Area, Exit = DictEntry.Value.Exit });
                    Instance.AddLogicExitReference(new EntranceData.EntranceAreaPair { Area = DictEntry.Value.Area, Exit = DictEntry.Value.Exit }, i.Id);
                    Instance.EntrancePool.AreaList[DictEntry.Value.Area].Exits.Add(DictEntry.Value.Exit, new EntranceData.EntranceRandoExit
                    {
                        ParentAreaID = DictEntry.Value.Area,
                        ID = DictEntry.Value.Exit,
                        EntrancePair = DictEntry.Value.RandomizableEntrance ? DictEntry.Value.EntrancePairID : null,
                        IsWarp = DictEntry.Value.AlwaysAccessable,
                        referenceData = new ReferenceData { LogicIndex = Index, LogicList = Source }
                    });
                }
                else
                {
                    Instance.MacroPool.Add(i.Id, new() 
                    { 
                        ID = i.Id,
                        referenceData = new ReferenceData { LogicIndex = Index, LogicList = Source }
                    });
                }
            }

            //Wallet and Price Data

            Instance.PriceData.WalletEntries = Utility.GetAllWalletLogicEntries(Instance);
            Dictionary<string, Tuple<char, int>> ItemWallets = Instance.LogicDictionary.ItemList.Values
                .Where(x => x.WalletCapacity != null && (int)x.WalletCapacity > -1)
                .ToDictionary(x => x.ID, x => new Tuple<char, int>(x.WalletCurrency??'$', (int)x.WalletCapacity ) );
            Dictionary<string, Tuple<char, int>> MacroWallets = Instance.LogicDictionary.MacroList.Values
                .Where(x => x.WalletCapacity != null && (int)x.WalletCapacity > -1)
                .ToDictionary(x => x.ID, x => new Tuple<char, int>(x.WalletCurrency??'$', (int)x.WalletCapacity));
            Dictionary<string, Tuple<char, int>> AllWallets = ItemWallets.Concat(MacroWallets).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var i in AllWallets)
            {
                string CanAffordString = $"MMRTCanAfford{i.Value.Item1}{i.Value.Item2}";
                Debug.WriteLine($"Adding Wallet {CanAffordString}");
                Instance.MacroPool.Add(CanAffordString, new() { 
                    ID = CanAffordString,
                    referenceData = new ReferenceData { LogicIndex = Index, LogicList = LogicFileType.Runtime }
                });
                if (!Instance.PriceData.CapacityMap.ContainsKey(i.Value.Item1))
                    Instance.PriceData.CapacityMap.Add(i.Value.Item1, new Dictionary<int, string>());
                Instance.PriceData.CapacityMap[i.Value.Item1].Add(i.Value.Item2, CanAffordString);
                Instance.RuntimeLogic.Add(CanAffordString, new MMRData.JsonFormatLogicItem
                {
                    Id = CanAffordString,
                    RequiredItems = new List<string>(),
                    ConditionalItems = AllWallets.Where(x => x.Value.Item2 >= i.Value.Item2 && (x.Value.Item1 == i.Value.Item1)).Select(x => new List<string> { x.Key }).ToList()
                });
            }

            Instance.EntrancePool.IsEntranceRando = Instance.EntrancePool.CheckForRandomEntrances(Instance);

            Instance.PriceData.Initialized = true;

            Instance.EntrancePool.RootArea = Instance.LogicDictionary.RootArea??"Root";

            Instance.StaticOptions.MinimizedHeader.Add("Hidden Locations:::LBValidLocations", true);

            //If the number of randomized entrances is less than 10% of the number of randomized locations, show the entrances in the location box
            if (Instance.EntrancePool.IsEntranceRando)
            {
                double EntrancesRandomized = Instance.EntrancePool.GetAmountOfRandomizedEntrances(Instance);
                double Locationsrandomized = Instance.LocationPool.Where(x => x.Value.AppearsinListbox(Instance)).Count();
                double LocationEntranceRatio = Math.Round(EntrancesRandomized / Locationsrandomized, 2);
                Instance.StaticOptions.EntranceRandoFeatures = LocationEntranceRatio >= .1;
            }

            if (Instance.LogicFile.GameCode == "MMR")
            {
                Instance.ApplyMMRandoSettings(new MMRData.SpoilerLogData { GameplaySettings = new MMRData.GameplaySettings() });
            }

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
