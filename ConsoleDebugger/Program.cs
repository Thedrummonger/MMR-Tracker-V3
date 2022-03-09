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
            "3. Laod Save File"
        };
        static List<string> UndoStringList = new();
        static List<string> RedoStringList = new();
        static LogicObjects.TrackerInstance newTrackerInstance;

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Commands\n" + string.Join("\n", CommandList));
                var key = Console.ReadKey();
                Console.WriteLine();
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
                }
            }
        }

        private readonly static Newtonsoft.Json.JsonSerializerSettings _NewtonsoftJsonSerializerOptions = new Newtonsoft.Json.JsonSerializerSettings
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
        };

        static void ConvertOldDict()
        {

        }

        static void LoadSave()
        {
            LogicObjects.TrackerInstance NewTrackerInstance = LogicObjects.TrackerInstance.FromJson(File.ReadAllText(Testing.GetSavePath(true)));

            Console.WriteLine("NYI");
        }

        static void TestItemOutput()
        {
            LogicObjects.TrackerInstance NewTrackerInstance = new();

        getpath:
            Console.WriteLine("Enter Logic Path");
            string path = Console.ReadLine();

            if (!File.Exists(path)) { Console.WriteLine("Path Invalid!"); goto getpath; }

            string Logic = File.ReadAllText(path);

            TrackerInstanceCreation.ApplyLogicAndDict(NewTrackerInstance, Logic);
            TrackerInstanceCreation.PopulateTrackerObject(NewTrackerInstance);

            newTrackerInstance = NewTrackerInstance;

            LoopLocationList();
        }

        private static void LoopLocationList()
        {
            string Filter = "";
            while (true)
            {
                Console.Clear();
                LogicCalculation.CalculateLogic(newTrackerInstance);
                Dictionary<string, int> Groups = Utility.GetCategoriesFromFile(newTrackerInstance);
                var DataSets = TrackerDataHandeling.PopulateDataSets(newTrackerInstance);
                var Entries = TrackerDataHandeling.PrintToLocationList(Groups, DataSets, CreateDivider(Console.WindowWidth), newTrackerInstance, Filter, false, true);
                Console.WriteLine(Entries.Count);
                Dictionary<int, object> reference = new Dictionary<int, object>();
                foreach (var i in Entries) 
                {
                    string Print = i.ToString();
                    if ((i is LocationData.LocationObject || i is HintData.HintObject)) 
                    {
                        Print = $"{reference.Count}. {i}";
                        reference.Add(reference.Count, i);
                    }
                    Console.WriteLine(Print); 
                }
                Console.WriteLine(CreateDivider(Console.WindowWidth));
                var input = Console.ReadLine();

                if (int.TryParse(input, out int index) && reference.ContainsKey(index))
                {
                    Console.WriteLine($"Selected item {reference[index]}"); 
                    Console.ReadLine();
                }
                else if (input == "^") { LoadSettingFile(); }
            }
        }

        private static string CreateDivider(int Width, string key = "=")
        {
            string Divider = "";
            for(var i = 0; i < Width; i++) { Divider += key; }
            return Divider;
        }

        private static void LoadSettingFile()
        {
        getpath:
            Console.WriteLine("Enter Setting File Path");
            string path = Console.ReadLine();

            if (!File.Exists(path)) { Console.WriteLine("Path Invalid!"); goto getpath; }
            try
            {
                MMRData.SpoilerLogData configuration = Newtonsoft.Json.JsonConvert.DeserializeObject<MMRData.SpoilerLogData>(File.ReadAllText(path));
                if (configuration.GameplaySettings == null)
                {
                    Console.WriteLine("Setting File Invalid!");
                    return;
                }
                var SuccessLoc = SpoilerLogTools.ApplyLocationString(configuration.GameplaySettings.CustomItemListString, newTrackerInstance);
                var SuncessJunk = SpoilerLogTools.ApplyJunkString(configuration.GameplaySettings.CustomJunkLocationsString, newTrackerInstance);
                var SuncessStart = SpoilerLogTools.ApplyStartingItemString(configuration.GameplaySettings.CustomStartingItemListString, newTrackerInstance);
                foreach (var i in newTrackerInstance.MacroPool.Values.Where(x => x.isTrick(newTrackerInstance)))
                {
                    i.TrickEnabled = configuration.GameplaySettings.EnabledTricks.Contains(i.ID);
                }
            }
            catch { Console.WriteLine("Setting File Invalid!"); }
        }

        private void HandleItemSelect(List<object> Items, MiscData.CheckState checkState)
        {
            string CurrentState = Newtonsoft.Json.JsonConvert.SerializeObject(newTrackerInstance);

            List<LocationData.LocationObject> locationObjects = Items.Where(x => x is LocationData.LocationObject).Select(x => x as LocationData.LocationObject).ToList();
            List<HintData.HintObject> HintObjects = Items.Where(x => x is HintData.HintObject).Select(x => x as HintData.HintObject).ToList();

            //Items =====================================
            List<LocationData.LocationObject> ManualChecks = new List<LocationData.LocationObject>();
            foreach (LocationData.LocationObject LocationObject in locationObjects)
            {
                if (LocationObject.CheckState != MiscData.CheckState.Unchecked) { continue; }
                if (LocationObject.Randomizeditem.Item == null)
                {
                    LocationObject.Randomizeditem.Item = LocationObject.GetItemAtCheck(newTrackerInstance);
                    if (LocationObject.Randomizeditem.Item == null)
                    {
                        ManualChecks.Add(LocationObject);
                    }
                }
            }
            if (ManualChecks.Any())
            {

            }

            foreach (LocationData.LocationObject LocationObject in locationObjects)
            {
                var Action = (checkState == MiscData.CheckState.Marked && LocationObject.CheckState == MiscData.CheckState.Marked) ? MiscData.CheckState.Unchecked : checkState;
                LocationObject.ToggleChecked(Action, newTrackerInstance);
            }

            //Hints======================================

            foreach (HintData.HintObject hintObject in HintObjects)
            {
                if (hintObject.CheckState == MiscData.CheckState.Unchecked)
                {
                    if (hintObject.SpoilerHintText == null)
                    {
                        Console.WriteLine("Input Hint Text for " + hintObject.GetDictEntry(newTrackerInstance).Name);
                        string input = Console.ReadLine();
                        hintObject.HintText = input;
                    }
                    else
                    {
                        hintObject.HintText = hintObject.SpoilerHintText;
                    }
                }
                var CheckAction = (checkState == MiscData.CheckState.Marked && hintObject.CheckState == MiscData.CheckState.Marked) ? MiscData.CheckState.Unchecked : checkState;
                hintObject.CheckState = CheckAction;
                hintObject.HintText = CheckAction == MiscData.CheckState.Unchecked ? null : hintObject.HintText;
            }

            RedoStringList.Clear();
            UndoStringList.Add(CurrentState);
        }
    }
}
