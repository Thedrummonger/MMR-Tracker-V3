using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.SpoilerLogImporter;
using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public static class TrackerInstanceCreation
    {
        public static event Action<MMR_Tracker_V3.TrackerObjects.InstanceData.InstanceContainer> InstanceCreated;
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
            if (Container.Instance is null) { Container.CreateEmptyInstance(); }
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
            var Instance = Container.Instance;
            Instance.ChoiceOptions = Instance.LogicDictionary.ChoiceOptions.ToDictionary(x => x.Key, y => y.Value);
            Instance.MultiSelectOptions = Instance.LogicDictionary.MultiSelectOptions.ToDictionary(x => x.Key, y => y.Value);
            Instance.ToggleOptions = Instance.LogicDictionary.ToggleOptions.ToDictionary(x => x.Key, y => y.Value);
            Instance.IntOptions = Instance.LogicDictionary.IntOptions.ToDictionary(x => x.Key, y => y.Value);
            Instance.LogicEntryCollections = Instance.LogicDictionary.LogicEntryCollections.ToDictionary(x => x.Key, y => y.Value);
            foreach (var i in Instance.ChoiceOptions.Values) { i.SetParent(Instance); }
            foreach (var i in Instance.MultiSelectOptions.Values) { i.SetParent(Instance); }
            foreach (var i in Instance.ToggleOptions.Values) { i.SetParent(Instance); }
            foreach (var i in Instance.IntOptions.Values) { i.SetParent(Instance); }

            Instance.LogicDictionary.SetParentContainer(Instance);
            foreach (var i in Instance.LogicDictionary.LocationList) { i.Value.ID = i.Key; i.Value.SetParent(Instance.LogicDictionary); }
            foreach (var i in Instance.LogicDictionary.ItemList) { i.Value.ID = i.Key; i.Value.SetParent(Instance.LogicDictionary); }
            foreach (var i in Instance.LogicDictionary.EntranceList) { i.Value.ID = i.Key; i.Value.SetParent(Instance.LogicDictionary); }
            foreach (var i in Instance.LogicDictionary.HintSpots) { i.Value.ID = i.Key; i.Value.SetParent(Instance.LogicDictionary); }
            foreach (var i in Instance.LogicDictionary.MacroList) { i.Value.ID = i.Key; i.Value.SetParent(Instance.LogicDictionary); }
            foreach (var i in Instance.LogicDictionary.ChoiceOptions)
            {
                i.Value.ID = i.Key;
                foreach (var j in i.Value.ValueList) { j.Value.ID = j.Key; }
            }
            foreach (var i in Instance.LogicDictionary.ToggleOptions) { i.Value.ID = i.Key; }
            foreach (var i in Instance.LogicDictionary.IntOptions) { i.Value.ID = i.Key; }
            foreach (var i in Instance.LogicDictionary.LogicEntryCollections) { i.Value.ID = i.Key; }

            int Index = 0;
            foreach (var i in Instance.LogicDictionary.ItemList)
            {
                Instance.ItemPool.Add(i.Key, new(Instance) { ID = i.Key });
                Index++;
            }

            foreach (var i in Instance.LogicDictionary.GetAreas())
            {
                Instance.AreaPool.Add(i, new EntranceData.EntranceRandoArea(Instance) { ID = i });
            }
            foreach (var i in Instance.LogicDictionary.RootAreas ?? [])
            {
                Instance.AreaPool[i].IsRoot = true;
            }

            Index = 0;
            foreach (var i in Instance.LogicFile.Logic)
            {
                if (Instance.LogicDictionary.AdditionalLogic.Any(x => x.Id == i.Id)) { Index++; continue; }
                DoLogicCreationEdits(i, Instance);
                ParseLogicItem(i, Index, LogicFileType.Logic);
                Index++;
            }
            Index = 0;
            foreach (var i in Instance.LogicDictionary.AdditionalLogic)
            {
                DoLogicCreationEdits(i, Instance);
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
                    Instance.LocationPool.Add(i.Id, new(Instance)
                    {
                        ID = i.Id,
                        Currency = DictEntry.Value.WalletCurrency,
                        SingleValidItem = ValidItems.Count() == 1 ? ValidItems.First().Key : null,
                        referenceData = new ReferenceData { LogicIndex = Index, LogicList = Source }
                    });

                    if (DictEntry.Value.LocationProxys.Any())
                    {
                        if (!Instance.LocationProxyData.LocationsWithProxys.ContainsKey(i.Id)) { Instance.LocationProxyData.LocationsWithProxys.Add(i.Id, new List<string>()); }
                        Instance.LocationProxyData.LocationsWithProxys[i.Id].AddRange(DictEntry.Value.LocationProxys.Select(x => x.ID));
                        foreach (var proxy in DictEntry.Value.LocationProxys)
                        {
                            Instance.LocationProxyData.LocationProxies.Add(proxy.ID, proxy.ToInstanceData(DictEntry.Value, Instance));
                        }
                    }
                }
                else if (Instance.LogicDictionary.HintSpots.Any(x => x.Key == i.Id))
                {
                    var DictEntry = Instance.LogicDictionary.HintSpots.First(x => x.Key == i.Id);
                    Instance.HintPool.Add(i.Id, new(Instance)
                    {
                        ID = i.Id,
                        referenceData = new ReferenceData { LogicIndex = Index, LogicList = Source }
                    });
                }
                else if (Instance.LogicDictionary.EntranceList.Any(x => x.Key == i.Id))
                {
                    var DictEntry = Instance.LogicDictionary.EntranceList.First(x => x.Key == i.Id);
                    var ParentArea = Instance.AreaPool[DictEntry.Value.Area];
                    var ExitObject = new EntranceData.EntranceRandoExit(Instance)
                    {
                        ID = i.Id,
                        ExitID = DictEntry.Value.Exit,
                        ParentAreaID = DictEntry.Value.Area,
                        EntrancePair = DictEntry.Value.RandomizableEntrance ? DictEntry.Value.EntrancePairID : null,
                        IsWarp = DictEntry.Value.AlwaysAccessable,
                        referenceData = new ReferenceData { LogicIndex = Index, LogicList = Source }
                    };
                    Instance.ExitPool.Add(i.Id, ExitObject);
                    ParentArea.Exits.Add(ExitObject.ExitID, ExitObject.ID);
                }
                else
                {
                    Instance.MacroPool.Add(i.Id, new(Instance)
                    {
                        ID = i.Id,
                        referenceData = new ReferenceData { LogicIndex = Index, LogicList = Source }
                    });
                }
            }

            //Wallet and Price Data

            Instance.PriceData.WalletEntries = PriceRando.GetAllWalletLogicEntries(Instance);
            Dictionary<string, Tuple<char, int>> ItemWallets = Instance.LogicDictionary.ItemList.Values
                .Where(x => x.WalletCapacity != null && (int)x.WalletCapacity > -1)
                .ToDictionary(x => x.ID, x => new Tuple<char, int>(x.WalletCurrency ?? '$', (int)x.WalletCapacity));
            Dictionary<string, Tuple<char, int>> MacroWallets = Instance.LogicDictionary.MacroList.Values
                .Where(x => x.WalletCapacity != null && (int)x.WalletCapacity > -1)
                .ToDictionary(x => x.ID, x => new Tuple<char, int>(x.WalletCurrency ?? '$', (int)x.WalletCapacity));
            Dictionary<string, Tuple<char, int>> AllWallets = ItemWallets.Concat(MacroWallets).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var i in AllWallets)
            {
                string CanAffordString = $"MMRTCanAfford{i.Value.Item1}{i.Value.Item2}";
                Debug.WriteLine($"Adding Wallet {CanAffordString}");
                Instance.MacroPool.Add(CanAffordString, new(Instance)
                {
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

            Instance.PriceData.Initialized = true;

            new Areaheader { Area = "Hidden Locations" }.SetMinimized(DisplayListType.Locations, Instance.StaticOptions, true);

            if (Instance.LogicDictionary.DefaultSettings is not null)
            {
                LoadDefaultSetting(Instance);
            }

            Container.logicCalculation = new LogicCalculation(Container);

            Container.logicCalculation.CompileOptionActionEdits();

            InstanceCreated?.Invoke(Container);

            //Debug.WriteLine(JsonConvert.SerializeObject(Instance.PriceData.WalletEntries, Utility._NewtonsoftJsonSerializerOptions));
            //Debug.WriteLine(JsonConvert.SerializeObject(Instance.PriceData.CapacityMap, Utility._NewtonsoftJsonSerializerOptions));

            return true;
        }

        public static void LoadDefaultSetting(TrackerInstance instance)
        {
            var DefSet = instance.LogicDictionary.DefaultSettings;
            if (!string.IsNullOrWhiteSpace(DefSet.CustomItemListString))
            {
                SettingStringHandler.ApplyLocationString(DefSet.CustomItemListString, instance);
            }
            if (!string.IsNullOrWhiteSpace(DefSet.CustomJunkLocationsString))
            {
                SettingStringHandler.ApplyJunkString(DefSet.CustomJunkLocationsString, instance);
            }
            if (!string.IsNullOrWhiteSpace(DefSet.CustomStartingItemListString))
            {
                SettingStringHandler.ApplyStartingItemString(DefSet.CustomStartingItemListString, instance);
            }
            if (DefSet.ManualRandomizationState is not null)
            {
                foreach (var Manual in DefSet.ManualRandomizationState)
                {
                    bool Litteral = Manual.Key.IsLiteralID(out string LocationObjectID);
                    var Location = instance.GetCheckableLocationByID(LocationObjectID, Litteral);
                    if (Location is LocationData.LocationObject LO) { LO.SetRandomizedState(Manual.Value); }
                    if (Location is HintData.HintObject HO) { HO.RandomizedState = Manual.Value; }
                    if (Location is EntranceData.EntranceRandoExit EO) { EO.RandomizedState = Manual.Value; }
                }
            }
            if (DefSet.EnabledTricks is not null)
            {
                instance.ToggleAllTricks(false);
                foreach (var trick in DefSet.EnabledTricks)
                {
                    MacroObject Trick = instance.GetMacroByID(trick);
                    if (Trick is null || !Trick.isTrick()) { continue; }
                    Trick.TrickEnabled = true;
                }
            }
        }

        public static void DoLogicCreationEdits(MMRData.JsonFormatLogicItem LogicItem, TrackerInstance Instance)
        {
            if (!string.IsNullOrWhiteSpace(LogicItem.SettingExpression)) { MMRSettingExpressionParser.ConvertSettingExpressionToLogic(LogicItem, Instance.LogicDictionary); }
            if (!string.IsNullOrWhiteSpace(LogicItem.LogicInheritance)) { AddLogicInheritance(LogicItem, Instance); }
        }

        public static void AddLogicInheritance(MMRData.JsonFormatLogicItem LogicItem, TrackerInstance Instance)
        {
            LogicStringParser logicStringParser = new LogicStringParser();

            string Operator = logicStringParser._ANDOP;
            string InheritID = LogicItem.LogicInheritance;
            if (InheritID.StartsWith('&'))
            {
                InheritID = InheritID[1..];
                Operator = logicStringParser._ANDOP;
            }
            else if (InheritID.StartsWith('|'))
            {
                InheritID = InheritID[1..];
                Operator = logicStringParser._OROP;
            }
            if (Instance.LogicFile.Logic.Any(x => x.Id == InheritID)) { return; }
            var InheritedLogic = Instance.LogicFile.Logic.First(x => x.Id == InheritID).SerializeConvert<MMRData.JsonFormatLogicItem>();
            DoLogicCreationEdits(InheritedLogic, Instance);

            if (!LogicItem.RequiredItems.Any() && !LogicItem.ConditionalItems.Any())
            {
                LogicItem.RequiredItems = InheritedLogic.RequiredItems;
                LogicItem.ConditionalItems = InheritedLogic.ConditionalItems;
            }
            else
            {
                LogicUtilities.MoveRequirementsToConditionals(LogicItem);
                LogicUtilities.MoveRequirementsToConditionals(InheritedLogic);
                string CurrentLogicstring = $"({LogicStringConverter.ConvertConditionalToLogicString(logicStringParser, LogicItem.ConditionalItems)})";
                string InheritedLogicstring = $"({LogicStringConverter.ConvertConditionalToLogicString(logicStringParser, InheritedLogic.ConditionalItems)})";
                string CombinedLogicString = $"{CurrentLogicstring} {Operator} {InheritedLogicstring}";
                LogicItem.ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(logicStringParser, CombinedLogicString, LogicItem.Id);
                LogicUtilities.RemoveRedundantConditionals(LogicItem);
                LogicUtilities.MakeCommonConditionalsRequirements(LogicItem);
            }
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

        public static void TriggerInstanceCreatedEvent(InstanceContainer Container)
        {
            InstanceCreated?.Invoke(Container);
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
