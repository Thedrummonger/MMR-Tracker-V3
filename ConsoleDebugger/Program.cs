using MMR_Tracker_V3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;

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
                }
            }
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

        public static void LoopLocationSelet(MMR_Tracker_V3.LogicObjects.TrackerInstance NewTrackerInstance)
        {
            string Filter = "";
            while (true)
            {
                Console.Clear();
                int counter = -1;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                LogicCalculation.CalculateLogic(NewTrackerInstance);
                stopwatch.Stop();

                foreach (var i in NewTrackerInstance.LocationPool.Locations)
                {
                    counter++;
                    if (!i.TrackerData.Available) { continue; }
                    if (string.IsNullOrWhiteSpace(i.UIData.LocationName) || !i.UIData.LocationName.ToLower().Contains(Filter.ToLower())) { continue; }
                    if (i.TrackerData.CheckState == MMR_Tracker_V3.TrackerObjects.MiscData.CheckState.Checked) { continue; }

                    if (i.TrackerData.CheckState == MMR_Tracker_V3.TrackerObjects.MiscData.CheckState.Marked && i.TrackerData.RandomizedItem != null)
                    {
                        Console.WriteLine(counter + $" {i.UIData.LocationName}: {i.TrackerData.RandomizedItem}");
                    }
                    else
                    {
                        Console.WriteLine(counter + $" {i.UIData.LocationName}");
                    }

                }

            Item_Select:;

                Console.WriteLine("");
                Console.WriteLine("Type the number of the entry to edit.");
                Console.WriteLine("s = search");
                Console.WriteLine("i = Check Item Counts");
                Console.WriteLine("c = See Checked Locations");
                Console.WriteLine("t = terminate");
                Console.WriteLine($"Logic Calc took {stopwatch.ElapsedMilliseconds} Milisecconds");

                string Input = Console.ReadLine();

                if (Input == "s")
                {
                    Console.WriteLine("Enter Search String");
                    Filter = Console.ReadLine();
                    goto end_of_Loop;
                }
                if (Input.StartsWith(@"\"))
                {
                    Filter = Input.Substring(1);
                    goto end_of_Loop;
                }

                if (Input == "c")
                {
                    LoopCheckedItems(NewTrackerInstance);
                    goto end_of_Loop;
                }

                if (Input == "*")
                {
                    SaveInstance(NewTrackerInstance);
                    Console.WriteLine("Instance Saved");
                    Filter = Console.ReadLine();
                    goto end_of_Loop;
                }

                if (Input == "t")
                {
                    break;
                }

                if (Input == "r")
                {
                    goto end_of_Loop;
                }

                if (Input == "i")
                {
                    foreach (var i in NewTrackerInstance.ItemPool.CurrentPool)
                    {
                        if (i.GetAmountPlaced(NewTrackerInstance) > 0)
                        {
                            Console.WriteLine(i.ItemName + ": " + i.GetAmountPlaced(NewTrackerInstance));
                        }
                    }
                    Console.WriteLine("The Above Items have been place in the world");
                    goto Item_Select;
                }

                if (int.TryParse(Input, out int entry) || entry > counter)
                {
                    var SelectedEntry = NewTrackerInstance.LocationPool.Locations[entry];
                    LoopItemSelect(SelectedEntry, NewTrackerInstance);
                }
                else
                {
                    Console.WriteLine("Number Invalid!");
                    goto Item_Select;
                }

            end_of_Loop:;
            }
        }

        private static void SaveInstance(LogicObjects.TrackerInstance newTrackerInstance)
        {
            File.WriteAllText(Testing.GetSavePath(false), newTrackerInstance.ToString());
        }

        public static void LoopItemSelect(MMR_Tracker_V3.TrackerObjects.LocationData.LocationObject SelectedObject, MMR_Tracker_V3.LogicObjects.TrackerInstance NewTrackerInstance)
        {
            if (SelectedObject.TrackerData.CheckState == MMR_Tracker_V3.TrackerObjects.MiscData.CheckState.Marked && SelectedObject.TrackerData.RandomizedItem != null)
            {
                SelectedObject.TrackerData.ToggleChecked(MMR_Tracker_V3.TrackerObjects.MiscData.CheckState.Checked, NewTrackerInstance);
                return;
            }
            string Filter = "";

            while (true)
            {
                Console.Clear();
                int counter = -1;

                var validItems = new List<MMR_Tracker_V3.TrackerObjects.ItemData.ItemObject>();

                foreach (var i in NewTrackerInstance.ItemPool.CurrentPool)
                {
                    if (i.ItemTypes.Intersect(SelectedObject.TrackerData.ValidItemTypes).Any() 
                        && i.CanBePlaced(NewTrackerInstance) 
                        && !validItems.Any(x => x.ItemName == i.ItemName))
                    {
                        validItems.Add(i);
                    }
                }

                foreach (var i in validItems)
                {
                    counter++;
                    if (string.IsNullOrWhiteSpace(i.ItemName) || !i.ItemName.ToLower().Contains(Filter.ToLower())) { continue; }
                    Console.WriteLine(counter + $" {i.ItemName}");
                }

            Item_Select:;

                Console.WriteLine($"Select the item found at {SelectedObject.UIData.LocationName}.");
                Console.WriteLine("Type the number of the item to select it.");
                Console.WriteLine("s = search");
                Console.WriteLine("x = cancel");

                string Input = Console.ReadLine();

                if (Input == "s")
                {
                    Console.WriteLine("Enter Search String");
                    Filter = Console.ReadLine();
                    goto end_of_Loop;
                }
                if (Input.StartsWith(@"\"))
                {
                    Filter = Input.Substring(1);
                    goto end_of_Loop;
                }
                if (Input == "x")
                {
                    return;
                }

                if (int.TryParse(Input, out int entry) || entry > counter)
                {
                    var SelectedEntry = validItems[entry];
                    SelectedObject.TrackerData.RandomizedItem = SelectedEntry.ItemName;
                    SelectedObject.TrackerData.ToggleChecked(MMR_Tracker_V3.TrackerObjects.MiscData.CheckState.Checked, NewTrackerInstance);
                    return;
                }
                else
                {
                    Console.WriteLine("Number Invalid!");
                    goto Item_Select;
                }



            end_of_Loop:;
            }

        }

        public static void LoopCheckedItems(MMR_Tracker_V3.LogicObjects.TrackerInstance NewTrackerInstance)
        {
            string Filter = "";

            while (true)
            {
                Console.Clear();
                int counter = -1;

                var validItems = new List<MMR_Tracker_V3.TrackerObjects.LocationData.LocationObject>();

                foreach (var i in NewTrackerInstance.LocationPool.Locations)
                {
                    if (i.TrackerData.CheckState == MMR_Tracker_V3.TrackerObjects.MiscData.CheckState.Checked)
                    {
                        validItems.Add(i);
                    }
                }

                foreach (var i in validItems)
                {
                    counter++;
                    Console.WriteLine(counter + $" {i.UIData.LocationName}: {i.TrackerData.RandomizedItem}");
                }

            Item_Select:;

                Console.WriteLine("Type the number of the location uncheck it.");
                Console.WriteLine("s = search");
                Console.WriteLine("a = See Available locations");

                string Input = Console.ReadLine();

                if (Input == "s")
                {
                    Console.WriteLine("Enter Search String");
                    Filter = Console.ReadLine();
                    goto end_of_Loop;
                }
                if (Input == "a")
                {
                    return;
                }

                if (int.TryParse(Input, out int entry) || entry > counter)
                {
                    var SelectedEntry = validItems[entry];
                    SelectedEntry.TrackerData.RandomizedItem = null;
                    SelectedEntry.TrackerData.ToggleChecked(MMR_Tracker_V3.TrackerObjects.MiscData.CheckState.Unchecked, NewTrackerInstance);
                }
                else
                {
                    Console.WriteLine("Number Invalid!");
                    goto Item_Select;
                }



            end_of_Loop:;
            }

        }
    }
}
