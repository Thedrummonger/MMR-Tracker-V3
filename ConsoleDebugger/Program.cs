using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using static MMR_Tracker_V3.TrackerObjects.LogicDictionaryData;

namespace ConsoleDebugger
{
    class Program
    {
        static string[] CommandList = new string[]
        {
            "1. Convert Old Dict",
            "2. Test Item output",
            "3. Laod Save File",
            "4. Print Mapping Dict",
            "5. CreateExampleLogicVariableData",
        };

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Commands\n" + string.Join("\n", CommandList));
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        ConvertOldDict();
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        TestItemOutput();
                        break;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        LoadSave();
                        break;
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        PrintMappingDict();
                        break;
                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        CreateExampleLogicVariableData();
                        break;
                }
            }
        }

        static void CreateExampleLogicVariableData()
        {
            DictionaryMacroEntry test = new DictionaryMacroEntry();
            test.DynamicLogicData = new dynamicLogicData();
            test.ID = "AreaWoodFallTempleClear";
            test.DynamicLogicData.LocationToCompare = "AreaWoodFallTempleAccess";
            test.DynamicLogicData.Arguments.Add(new DynamicLogicArguments
            {
                ItemAtLocation = "AreaWoodFallTempleAccess",
                LogicToUse = "AreaWoodFallTempleClear"
            });
            test.DynamicLogicData.Arguments.Add(new DynamicLogicArguments
            {
                ItemAtLocation = "AreaSnowheadTempleAccess",
                LogicToUse = "AreaSnowheadTempleClear"
            });
            test.DynamicLogicData.Arguments.Add(new DynamicLogicArguments
            {
                ItemAtLocation = "AreaGreatBayTempleAccess",
                LogicToUse = "AreaGreatBayTempleClear"
            });
            test.DynamicLogicData.Arguments.Add(new DynamicLogicArguments
            {
                ItemAtLocation = "AreaInvertedStoneTowerTempleAccess",
                LogicToUse = "AreaStoneTowerClear"
            });

            System.Diagnostics.Debug.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(test));
        }

        static void PrintMappingDict()
        {

            MMR_Tracker_V3.LogicObjects.TrackerInstance NewTrackerInstance = new();

            string Logic = File.ReadAllText(Testing.GetLogicPath());

            var Result = TrackerInstanceCreation.ApplyLogicAndDict(NewTrackerInstance, Logic);

            TrackerInstanceCreation.PopulateTrackerObject(NewTrackerInstance);

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(NewTrackerInstance.InstanceReference.LogicDataMappings, _NewtonsoftJsonSerializerOptions));
        }

        private readonly static Newtonsoft.Json.JsonSerializerSettings _NewtonsoftJsonSerializerOptions = new Newtonsoft.Json.JsonSerializerSettings
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
        };

        static void ConvertOldDict()
        {
            MMR_Tracker_V3.V2Porting.LegacyFunctions.LogicDictionary OldDict = MMR_Tracker_V3.V2Porting.LegacyFunctions.LogicDictionary.FromJson(File.ReadAllText(@"D:\Visual Studio Code Stuff\MMR TRACKER V2\MMR Tracker V2\Recources\Dictionaries\MMR V11 json Logic Dictionary.json"));

            MMR_Tracker_V3.TrackerObjects.LogicDictionaryData.LogicDictionary NewDict = new();

            NewDict.DefaultWalletCapacity = OldDict.DefaultWalletCapacity;
            NewDict.GameCode = OldDict.GameCode;
            NewDict.LogicFormat = OldDict.LogicFormat;
            NewDict.LogicVersion = OldDict.LogicVersion;

            foreach (var i in OldDict.LogicDictionaryList)
            {
                if (i.FakeItem)
                {
                    continue;
                }
                else
                {
                    var LocationEntry = new MMR_Tracker_V3.TrackerObjects.LogicDictionaryData.DictionaryLocationEntries();
                    LocationEntry.ID = i.DictionaryName;
                    LocationEntry.AltNames = i.SpoilerLocation?.Where(x => x != i.LocationName && x != i.DictionaryName && !string.IsNullOrWhiteSpace(x)).ToArray();
                    LocationEntry.AltNames = LocationEntry.AltNames != null && LocationEntry.AltNames.Any() ? LocationEntry.AltNames : null;
                    LocationEntry.Area = i.LocationArea;
                    LocationEntry.Name = i.LocationName;
                    LocationEntry.ValidItemTypes = new string[] { i.ItemSubType };
                    LocationEntry.OriginalItem = i.DictionaryName;

                    NewDict.LocationList.Add(LocationEntry);

                    var ItemEntry = new MMR_Tracker_V3.TrackerObjects.LogicDictionaryData.DictionaryItemEntries();
                    ItemEntry.ID = i.DictionaryName;
                    ItemEntry.ItemTypes = new string[] { i.ItemSubType };
                    ItemEntry.AltNames = i.SpoilerItem?.Where(x => x != i.ItemName && x != i.DictionaryName && !string.IsNullOrWhiteSpace(x)).ToArray();
                    ItemEntry.AltNames = ItemEntry.AltNames != null && ItemEntry.AltNames.Any() ? ItemEntry.AltNames : null;
                    ItemEntry.KeyType = i.KeyType == "boss" ? MMR_Tracker_V3.TrackerObjects.MiscData.KeyType.Boss : i.KeyType == "small" ? MMR_Tracker_V3.TrackerObjects.MiscData.KeyType.Small : MMR_Tracker_V3.TrackerObjects.MiscData.KeyType.None;
                    ItemEntry.ValidStartingItem = i.ValidRandomizerStartingItem;
                    ItemEntry.Name = i.ItemName;
                    ItemEntry.WalletCapacity = i.WalletCapacity;
                    ItemEntry.MaxAmountInWorld = 1;

                    NewDict.ItemList.Add(ItemEntry);
                }
            }

            NewDict.ItemList.Add(new MMR_Tracker_V3.TrackerObjects.LogicDictionaryData.DictionaryItemEntries
            {
                ID = "IceTrap",
                ItemTypes = new string[] { "Item" },
                KeyType = MMR_Tracker_V3.TrackerObjects.MiscData.KeyType.None,
                ValidStartingItem = false,
                Name = "Ice Trap"
            });
            NewDict.ItemList.Add(new MMR_Tracker_V3.TrackerObjects.LogicDictionaryData.DictionaryItemEntries
            {
                ID = "RecoveryHeart",
                ItemTypes = new string[] { "Item" },
                KeyType = MMR_Tracker_V3.TrackerObjects.MiscData.KeyType.None,
                ValidStartingItem = false,
                Name = "Recovery heart"
            });

            File.WriteAllText(@"D:\Testing\NewDict.json", NewDict.ToString());

        }

        static void LoadSave()
        {
            LogicObjects.TrackerInstance NewTrackerInstance = LogicObjects.TrackerInstance.FromJson(File.ReadAllText(Testing.GetSavePath(true)));

            LoopLocationSelet(NewTrackerInstance);
        }

        static void TestItemOutput()
        {

            MMR_Tracker_V3.LogicObjects.TrackerInstance NewTrackerInstance = new();

            string Logic = File.ReadAllText(Testing.GetLogicPath());

            var Result = TrackerInstanceCreation.ApplyLogicAndDict(NewTrackerInstance, Logic);

            TrackerInstanceCreation.PopulateTrackerObject(NewTrackerInstance);

            LoopLocationSelet(NewTrackerInstance);

        }

        private static void SaveInstance(LogicObjects.TrackerInstance newTrackerInstance)
        {
            File.WriteAllText(Testing.GetSavePath(false), newTrackerInstance.ToString());
        }

        public static void LoopLocationSelet(MMR_Tracker_V3.LogicObjects.TrackerInstance NewTrackerInstance)
        {
            string Filter = "";
            while (true)
            {
            Start:
                Dictionary<string, int> Groups = new Dictionary<string, int>();
                if (File.Exists(References.Globalpaths.CategoryTextFile))
                {
                    bool AtGame = true;
                    foreach (var i in File.ReadAllLines(References.Globalpaths.CategoryTextFile))
                    {
                        var x = i.ToLower().Trim();
                        if (string.IsNullOrWhiteSpace(x) || x.StartsWith("//")) { continue; }
                        if (x.StartsWith("#gamecodestart:"))
                        {
                            AtGame = x.Replace("#gamecodestart:", "").Trim().Split(',')
                                .Select(y => y.Trim()).Contains(NewTrackerInstance.LogicFile.GameCode.ToLower());
                            continue;
                        }
                        if (x.StartsWith("#gamecodeend:")) { AtGame = true; continue; }

                        if (!Groups.ContainsKey(x) && AtGame)
                        {
                            Groups.Add(x, Groups.Count());
                        }
                    }

                }

                LogicCalculation.CalculateLogic(NewTrackerInstance);
                var DataSets = TrackerDataHandeling.PopulateDataSets(NewTrackerInstance);

                var AvailableLocations = DataSets.AvailableLocations
                .OrderBy(x => (Groups.ContainsKey(x.UIData.LocationArea.ToLower().Trim()) ? Groups[x.UIData.LocationArea.ToLower().Trim()] : DataSets.AvailableLocations.Count() + 1))
                .ThenBy(x => x.UIData.LocationArea)
                .ThenBy(x => x.UIData.DisplayName).ToList();

                string CurrentLocation = "";
                int ItemsPrinted  = 0;
                Dictionary<int, LocationData.LocationObject> IntRef = new Dictionary<int, LocationData.LocationObject>();
                Console.Clear();
                Console.WriteLine($"{AvailableLocations.Count} locations found");
                foreach (var i in AvailableLocations)
                {
                    i.UIData.DisplayName = i.UIData.LocationName ?? i.LogicData.Id;
                    if (i.TrackerData.CheckState == MiscData.CheckState.Marked)
                    {
                        var RandomizedItem = NewTrackerInstance.ItemPool.GetItemByString(i.TrackerData.RandomizedItem);
                        i.UIData.DisplayName += $": {RandomizedItem.ItemName ?? RandomizedItem.Id}";
                    }
                    if (!Utility.FilterSearch(i, Filter, i.UIData.DisplayName)) { continue; }

                    if (CurrentLocation != i.UIData.LocationArea)
                    {
                        if (ItemsPrinted > 0) { Console.WriteLine("========================================"); }
                        Console.WriteLine(i.UIData.LocationArea.ToUpper() + ":");
                        CurrentLocation = i.UIData.LocationArea;
                    }
                    Console.WriteLine($"{ItemsPrinted}. {i}");
                    IntRef.Add(ItemsPrinted, i);
                    ItemsPrinted++;
                }
                Console.WriteLine($"");
                Console.WriteLine($"Type an index to check the location");
                Console.WriteLine($"Type \"\\\" followed by a search term to filter items");
                Console.WriteLine($"Type \"#\" followed by an index to mark/unmark the item at a location");
                Console.WriteLine($"Type \"c\" to return to see checked locations");
                Console.WriteLine($"Type \"s\" to save");

                string action = Console.ReadLine();

                if (int.TryParse(action, out int selectedIndex) && IntRef.ContainsKey(selectedIndex))
                {
                    HandleItemSelect(new List<object> { IntRef[selectedIndex] }, MiscData.CheckState.Checked, NewTrackerInstance);
                }
                else if (action.StartsWith(@"\"))
                {
                    Filter = action.Substring(1);
                    goto Start;
                }
                else if (action.StartsWith(@"#"))
                {
                    if (int.TryParse(action.Substring(1), out int selectedCheckIndex) && IntRef.ContainsKey(selectedCheckIndex))
                    {
                        HandleItemSelect(new List<object> { IntRef[selectedCheckIndex] }, MiscData.CheckState.Marked, NewTrackerInstance);
                    }
                }
                else if (action.ToLower() == "c")
                {
                    LoopCheckedItems(NewTrackerInstance);
                }
                else if (action.ToLower() == "s")
                {
                    SaveInstance(NewTrackerInstance);
                }
                else
                {
                    Console.WriteLine("Action Invalid!");
                    Console.ReadLine();
                    goto Start;
                }

            }
        }

        private static void HandleItemSelect(List<object> Items, MiscData.CheckState checkState, MMR_Tracker_V3.LogicObjects.TrackerInstance NewTrackerInstance)
        {
            List<LocationData.LocationObject> ManualChecks = new List<LocationData.LocationObject>();
            foreach (LocationData.LocationObject LocationObject in Items.Where(x => x is LocationData.LocationObject))
            {
                if (LocationObject.TrackerData.CheckState != MiscData.CheckState.Unchecked) { continue; }
                if (LocationObject.TrackerData.RandomizedItem == null)
                {
                    LocationObject.TrackerData.RandomizedItem = LocationObject.TrackerData.GetItemAtCheck();
                    if (LocationObject.TrackerData.RandomizedItem == null)
                    {
                        ManualChecks.Add(LocationObject);
                    }
                }
            }

            if (ManualChecks.Any())
            {
                LoopItemSelect(ManualChecks[0], NewTrackerInstance);
            }

            foreach (LocationData.LocationObject LocationObject in Items.Where(x => x is LocationData.LocationObject))
            {
                var Action = (checkState == MiscData.CheckState.Marked && LocationObject.TrackerData.CheckState == MiscData.CheckState.Marked) ? MiscData.CheckState.Unchecked : checkState;
                LocationObject.TrackerData.ToggleChecked(Action, NewTrackerInstance);
            }

        }

        public static void LoopItemSelect(MMR_Tracker_V3.TrackerObjects.LocationData.LocationObject SelectedObject, MMR_Tracker_V3.LogicObjects.TrackerInstance NewTrackerInstance)
        {
            string Filter = "";
        Start:
            var Names = new List<string>();
            var EnteredItems = new List<MMR_Tracker_V3.TrackerObjects.ItemData.ItemObject>();
            foreach (var i in NewTrackerInstance.ItemPool.CurrentPool)
            {
                if (string.IsNullOrWhiteSpace(i.ItemName) || !i.ItemName.ToLower().Contains(Filter.ToLower())) { continue; }
                if (i.CanBePlaced(NewTrackerInstance) && i.ItemTypes.Intersect(SelectedObject.TrackerData.ValidItemTypes).Any() && !EnteredItems.Contains(i) && !Names.Contains(i.ToString()))
                {
                    Names.Add(i.ToString());
                    EnteredItems.Add(i);
                }
            }
            int ItemsPrinted = 0;
            Dictionary<int, ItemData.ItemObject> IntRef = new Dictionary<int, ItemData.ItemObject>();
            Console.Clear();
            foreach (var i in EnteredItems)
            {
                IntRef.Add(ItemsPrinted, i);
                Console.WriteLine($"{ItemsPrinted}. {i.ItemName}");
                ItemsPrinted++;
            }
            Console.WriteLine($"");
            Console.WriteLine($"type an index Select Item at {SelectedObject.UIData.DisplayName}");
            Console.WriteLine($"Type \"\\\" followed by a search term to filter items");
            Console.WriteLine($"Type \"x\" to cancel");

            string action = Console.ReadLine();

            if (int.TryParse(action, out int selectedIndex) && IntRef.ContainsKey(selectedIndex))
            {
                SelectedObject.TrackerData.RandomizedItem = IntRef[selectedIndex].Id;
            }
            else if (action.StartsWith(@"\"))
            {
                Filter = action.Substring(1);
                goto Start;
            }
            else if (action.ToLower() == "x")
            {
                return;
            }
            else
            {
                Console.WriteLine("Action Invalid!");
                Console.ReadLine();
                goto Start;
            }
        }

        public static void LoopCheckedItems(MMR_Tracker_V3.LogicObjects.TrackerInstance NewTrackerInstance)
        {
            string Filter = "";
        Start:
            while (true)
            {
                Dictionary<string, int> Groups = new Dictionary<string, int>();
                if (File.Exists(References.Globalpaths.CategoryTextFile))
                {
                    bool AtGame = true;
                    foreach (var i in File.ReadAllLines(References.Globalpaths.CategoryTextFile))
                    {
                        var x = i.ToLower().Trim();
                        if (string.IsNullOrWhiteSpace(x) || x.StartsWith("//")) { continue; }
                        if (x.StartsWith("#gamecodestart:"))
                        {
                            AtGame = x.Replace("#gamecodestart:", "").Trim().Split(',')
                                .Select(y => y.Trim()).Contains(NewTrackerInstance.LogicFile.GameCode.ToLower());
                            continue;
                        }
                        if (x.StartsWith("#gamecodeend:")) { AtGame = true; continue; }

                        if (!Groups.ContainsKey(x) && AtGame)
                        {
                            Groups.Add(x, Groups.Count());
                        }
                    }

                }


                var DataSets = TrackerDataHandeling.PopulateDataSets(NewTrackerInstance);

                var CheckedLocations = DataSets.CheckedLocations
                    .OrderBy(x => (Groups.ContainsKey(x.UIData.LocationArea.ToLower().Trim()) ? Groups[x.UIData.LocationArea.ToLower().Trim()] : DataSets.CheckedLocations.Count() + 1))
                    .ThenBy(x => x.UIData.LocationArea)
                    .ThenBy(x => x.UIData.DisplayName).ToList();

                if (!CheckedLocations.Any()) { return; }

                string CurrentLocation = "";
                int ItemsPrinted = 0;
                Dictionary<int, LocationData.LocationObject> IntRef = new Dictionary<int, LocationData.LocationObject>();
                Console.Clear();
                foreach (var i in CheckedLocations)
                {
                    var RandomizedItem = NewTrackerInstance.ItemPool.GetItemByString(i.TrackerData.RandomizedItem);
                    i.UIData.DisplayName = $"{RandomizedItem.ItemName ?? RandomizedItem.Id}: {i.UIData.LocationName ?? i.LogicData.Id}";

                    if (!Utility.FilterSearch(i, Filter, i.UIData.DisplayName)) { continue; }

                    if (CurrentLocation != i.UIData.LocationArea)
                    {
                        if (ItemsPrinted > 0) { Console.WriteLine("============================"); }
                        Console.WriteLine(i.UIData.LocationArea.ToUpper() + ":");
                        CurrentLocation = i.UIData.LocationArea;
                    }
                    Console.WriteLine($"{ItemsPrinted}. {i}");
                    IntRef.Add(ItemsPrinted, i);
                    ItemsPrinted++;
                }
                Console.WriteLine($"");
                Console.WriteLine($"Type an index to uncheck the location");
                Console.WriteLine($"Type \"\\\" followed by a search term to filter Locations");
                Console.WriteLine($"Type \"#\" followed by an index to uncheck the location but keep the item marked");
                Console.WriteLine($"Type \"x\" to return to available items");

                string action = Console.ReadLine();

                if (int.TryParse(action, out int selectedIndex) && IntRef.ContainsKey(selectedIndex))
                {
                    HandleItemSelect(new List<object> { IntRef[selectedIndex] }, MiscData.CheckState.Unchecked, NewTrackerInstance);
                }
                else if (action.StartsWith(@"\"))
                {
                    Filter = action.Substring(1);
                    goto Start;
                }
                else if (action.StartsWith(@"#"))
                {
                    if (int.TryParse(action.Substring(1), out int selectedCheckIndex) && IntRef.ContainsKey(selectedCheckIndex))
                    {
                        HandleItemSelect(new List<object> { IntRef[selectedCheckIndex] }, MiscData.CheckState.Marked, NewTrackerInstance);
                    }
                }
                else if (action.ToLower() == "x")
                {
                    return;
                }
                else
                {
                    Console.WriteLine("Action Invalid!");
                    Console.ReadLine();
                    goto Start;
                }
            }
        }
    }
}
