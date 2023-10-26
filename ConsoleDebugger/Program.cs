using MMR_Tracker_V3;
using MMR_Tracker_V3.SpoilerLogImporter;
using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace CLIFrontEnd
{
    public class CLITracker
    {
        public static Action HideConsole;

        static string[] CommandList = new string[]
        {
            "1. Convert Old Dict",
            "2. New",
            "3. Preset",
            "4. Load",
            "5. Exit"
        };
        static MiscData.InstanceContainer newTrackerInstance = new MiscData.InstanceContainer() { Instance = new InstanceData.TrackerInstance() };

        public static void Main(string[] args)
        {
            while (true)
            {
                bool Exit = false;
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
                        CreateNewInstance();
                        break;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        LoadPreset();
                        break;
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        LoadSave();
                        break;
                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        Exit = true;
                        break;
                }
                if (Exit) 
                { 
                    break; 
                }
            }
            if (HideConsole is not null) { HideConsole(); }
        }

        private static void LoadPreset()
        {
        SelectPreset:
            var Presets = LogicPresetHandeling.GetLogicPresets();
            var PresetsDict = Presets.ToDictionary(x => Presets.IndexOf(x), x => x);
            foreach (var i in PresetsDict)
            {
                Console.WriteLine($"{i.Key}. {i.Value.Name}");
            }
            Console.WriteLine($"Enter index of Preset to Load");
            var selection = Console.ReadLine();
            if (int.TryParse(selection, out int ind) && PresetsDict.ContainsKey(ind))
            {
                newTrackerInstance.GenerateInstance(PresetsDict[ind].LogicString, PresetsDict[ind].DictionaryString);
                LoopLocationList();
            }
            else
            {
                Console.WriteLine($"Preset Invalid");
                goto SelectPreset;
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
        getpath:
            Console.WriteLine("Enter Save File Path");
            string path = Console.ReadLine();
            path = path.Replace("\"", "");

            if (!File.Exists(path)) { Console.WriteLine("Path Invalid!"); goto getpath; }
            if (!newTrackerInstance.LoadSave(path)) { Console.WriteLine("Save Invalid!"); goto getpath; }

            newTrackerInstance.CurrentSavePath = path;
            LoopLocationList();
        }

        static void CreateNewInstance()
        {
        getpath:
            Console.WriteLine("Enter Logic Path");
            string path = Console.ReadLine();
            path = path.Replace("\"", "");

            if (!File.Exists(path)) { Console.WriteLine("Path Invalid!"); goto getpath; }

            string[] LogicData = LogicFileParser.GetLogicData(path, out bool WasSpoilerLog);

            newTrackerInstance.GenerateInstance(LogicData);

            if (newTrackerInstance.Instance.LogicFile.GameCode == "MMR" && WasSpoilerLog)
            {
                SpoilerLogTools.ImportSpoilerLog(path, newTrackerInstance);
            }
            LoopLocationList();
        }

        private static void LoopLocationList()
        {
            //0: Available Loc  1: Checked Loc 2: Available Ent
            bool ShowHelp = true;
            int EntryType = 0;
            string Filter = "";
            while (true)
            {
                newTrackerInstance.Instance.EntrancePool.IsEntranceRando = newTrackerInstance.Instance.EntrancePool.CheckForRandomEntrances(newTrackerInstance.Instance);
                bool er = newTrackerInstance.Instance.EntrancePool.IsEntranceRando && newTrackerInstance.Instance.StaticOptions.OptionFile.EntranceRandoFeatures;
                Console.Title = newTrackerInstance.Instance.LogicFile.GameCode + " Tracker" + (newTrackerInstance.UnsavedChanges ? "*" : "");
                Console.Clear();
                newTrackerInstance.logicCalculation.CalculateLogic();
                List<object> Entries = null;
                int x = 0;
                int y = 0;
                string CurrentType = "";
                MiscData.CheckState CheckAction = MiscData.CheckState.Checked;
                string Action = null;
                var dataset = TrackerDataHandeling.PopulateDataSets(newTrackerInstance.Instance);
                switch (EntryType)
                {
                    case 0:
                        Entries = TrackerDataHandeling.PopulateAvailableLocationList(dataset, CreateDivider(Console.WindowWidth), newTrackerInstance, Filter, false, out x, out y, true);
                        CurrentType = "Available Locations";
                        break;
                    case 1:
                        Entries = TrackerDataHandeling.PopulateCheckedLocationList(dataset, CreateDivider(Console.WindowWidth), newTrackerInstance, Filter, out x, out y, true);
                        CurrentType = "Checked Locations";
                        CheckAction = MiscData.CheckState.Unchecked;
                        break;
                    case 2:
                        Entries = TrackerDataHandeling.PopulateAvailableEntraceList(dataset, CreateDivider(Console.WindowWidth), newTrackerInstance, Filter, false, out x, out y, true);
                        CurrentType = "Available Entrances";
                        break;
                }
                Dictionary<int, object> reference = new Dictionary<int, object>();
                foreach (var i in Entries) 
                {
                    dynamic DynamicEntry = i;
                    string Print = i.ToString();
                    if ((i is LocationData.LocationObject || i is HintData.HintObject || i is EntranceData.EntranceRandoExit || i is LocationData.LocationProxy)) 
                    {
                        Print = $"{reference.Count}. {i}";
                        reference.Add(reference.Count, i);
                    }
                    Console.WriteLine(Print); 
                }
                Console.WriteLine(CreateDivider(Console.WindowWidth));
                Console.WriteLine($"{CurrentType}: {y}" + (x != y ? $"/{x}" : ""));
                if (ShowHelp)
                {
                    Console.WriteLine(CreateDivider(Console.WindowWidth));
                    Console.WriteLine($"Check Actions:");
                    Console.WriteLine($"Type and index (0-{y - 1}) to {(EntryType == 1? "un" : "")}check the corresponding entry");
                    Console.WriteLine($"type # before an index to mark the entry");
                    Console.WriteLine($"type * before an index to star the entry");
                    Console.WriteLine($"type ! before an index to hide the entry");
                    Console.WriteLine($"type $ before an index to edit the checks Price");
                    Console.WriteLine(CreateDivider(Console.WindowWidth));
                    Console.WriteLine($"Other Actions:");
                    Console.WriteLine($"f = apply spoiler log or settings file");
                    Console.WriteLine($"z = undo | y = redo");
                    Console.WriteLine($"s = save");
                    Console.WriteLine($"l = show available location | c = show Checked location{(er ? " | e = show Available Entrances" : "" )}");
                    Console.WriteLine($"h = Hide command list");
                    Console.WriteLine(@"\ + search term = filter locations by search term");
                }
                else
                {
                    Console.WriteLine($"enter h to show commands");
                }
                var input = Console.ReadLine();

                if (input.StartsWith("#"))
                {
                    Action = "check";
                    CheckAction = MiscData.CheckState.Marked;
                    input = input[1..];
                }
                else if (input.StartsWith("!"))
                {
                    Action = "hide";
                    input = input[1..];
                }
                else if (input.StartsWith("*"))
                {
                    Action = "star";
                    input = input[1..];
                }
                else if (input.StartsWith("$"))
                {
                    Action = "price";
                    input = input[1..];
                }
                else if (input.StartsWith(@"\"))
                {
                    Filter = input[1..];
                }
                else if (input == "f")
                {
                    string CurrentState1 = Newtonsoft.Json.JsonConvert.SerializeObject(newTrackerInstance.Instance);
                    LoadSettingFile();
                    HandleChanges(CurrentState1);
                }
                else if (input == "l") { EntryType = 0;  }
                else if (input == "e" && er) { EntryType = 2;  }
                else if (input == "c") { EntryType = 1;  }
                else if (input == "z") { undoredo("undo");  }
                else if (input == "y") { undoredo("redo");  }
                else if (input == "s") { SaveInstance();  }
                else if (input == "h") { ShowHelp = !ShowHelp;  }
                else { Action = "check"; }

                if (Action is null) { continue; }
                List<int> Indexes =  new List<int>();
                var sections = input.Split(',').Select(x => x.Trim());
                foreach (var i in sections)
                {
                    if (i.Contains('-') && i.Split('-').Length == 2 && int.TryParse(i.Split('-')[0], out int I1) && int.TryParse(i.Split('-')[1], out int I2))
                    {
                        int min = Math.Min(I1, I2);
                        int max = Math.Max(I1, I2);
                        for (var q = min; q <= max; q++) { Indexes.Add(q); }
                    }
                    else if (int.TryParse(i, out int I3))
                    {
                        Indexes.Add(I3);
                    }
                }
                List<object> CheckObjects = Indexes.Where(x => reference.ContainsKey(x)).Select(x => reference[x]).ToList();
                if (!CheckObjects.Any()) { continue; }
                string CurrentState = JsonConvert.SerializeObject(newTrackerInstance.Instance);

                switch (Action)
                {
                    case "star":
                        foreach(var co in CheckObjects)
                        {
                            dynamic obj = co;
                            if (Utility.DynamicPropertyExist(obj, "Starred")) { obj.Starred = !obj.Starred; }
                        }
                        break;
                    case "price":
                        CheckObjects.ForEach(x => SetPrice(x));
                        break;
                    case "check":
                        TrackerDataHandeling.CheckSelectedItems(CheckObjects, CheckAction, newTrackerInstance, HandleUnAssignedLocations, HandleUnAssignedVariables);
                        break;
                    case "hide":
                        foreach (var co in CheckObjects)
                        {
                            dynamic obj = co;
                            if (Utility.DynamicPropertyExist(obj, "Hidden")) { obj.Hidden = !obj.Hidden; }
                        }
                        break;
                }
                HandleChanges(CurrentState);

            }
        }

        private static void HandleChanges(string CurrentState)
        {
            newTrackerInstance.RedoStringList.Clear();
            newTrackerInstance.UndoStringList.Add(CurrentState);
            newTrackerInstance.UnsavedChanges = true;
        }

        private static void SetPrice(dynamic entry)
        {
            if (!Utility.DynamicPropertyExist(entry, "Price") && !MMR_Tracker_V3.PriceRando.TestForPriceData(entry)) { return; }
            entry.GetPrice(out int p, out char c);
            if (p > -1) { entry.SetPrice(-1); return; }
            var DictEntry = entry.GetDictEntry(newTrackerInstance.Instance);
            Console.Clear();
            while (p == -1)
            {
                Console.WriteLine($"Enter Price for {DictEntry.Name ?? DictEntry.ID}");
                var input = Console.ReadLine();
                if (int.TryParse(input, out int newPrice) && newPrice > -1) { entry.SetPrice(newPrice); }
                else { Console.WriteLine($"{input} is not a valid price. Price must be a positive number"); }
                entry.GetPrice(out p, out c);
            }
        }

        private static bool HandleUnAssignedLocations(IEnumerable<object> CheckObject, MiscData.InstanceContainer Instance)
        {
            foreach(var i in CheckObject)
            {
                if (i is LocationData.LocationObject)
                {
                    LoopItemSelect(new List<object> { i }, Instance.Instance);
                }
                if (i is EntranceData.EntranceRandoExit)
                {
                    LoopEntranceSelect(new List<object> { i }, Instance.Instance);
                }
            }
            return false;
        }

        private static bool HandleUnAssignedVariables(IEnumerable<object> CheckObject, MiscData.InstanceContainer Instance)
        {
            foreach (var i in CheckObject)
            {
                if (i is HintData.HintObject)
                {
                    LoopHintSelect(new List<object> { i }, Instance.Instance);
                }
            }
            return false;
        }

        private static bool LoopItemSelect(IEnumerable<object> CheckObject, InstanceData.TrackerInstance Instance)
        {
            string Fiter = "";
            if (CheckObject.First() is not LocationData.LocationObject Location) { return false; }
            while (true)
            {
                Console.Clear();
                var Counter = 0;
                var EnteredItems = new Dictionary<int, ItemData.ItemObject>();
                foreach (var i in Instance.GetValidItemsForLocation(Location, Fiter))
                {
                    EnteredItems.Add(Counter, i);
                    Console.WriteLine(Counter + ": " + i);
                    Counter++;
                }
                Console.WriteLine(CreateDivider(Console.WindowWidth));
                Console.WriteLine("Select Item at " + Location.GetDictEntry(newTrackerInstance.Instance).GetName(Instance));
                var input = Console.ReadLine();
                if (int.TryParse(input, out int index) && EnteredItems.ContainsKey(index))
                {
                    Location.Randomizeditem.Item = EnteredItems[index].Id;
                    break;
                }
                else if (input.StartsWith(@"\"))
                {
                    Fiter = input[1..];
                }
            }
            return true;
        }

        private static bool LoopEntranceSelect(IEnumerable<object> CheckObject, InstanceData.TrackerInstance Instance)
        {
            string Fiter = "";
            if (CheckObject.First() is not EntranceData.EntranceRandoExit Exit) { return false; }
            while (true)
            {
                Console.Clear();
                var Counter = 0;
                var EnteredItems = new Dictionary<int, EntranceData.EntranceRandoDestination>();
                foreach (var Entry in Instance.GetAllLoadingZoneDestinations(Fiter))
                {
                    EnteredItems.Add(Counter, Entry);
                    Console.WriteLine(Counter + ": " + Entry);
                    Counter++;
                }
                Console.WriteLine(CreateDivider(Console.WindowWidth));
                Console.WriteLine("Select Destination at Exit " + Exit.ParentAreaID + " -> " + Exit.ID);
                var input = Console.ReadLine();
                if (int.TryParse(input, out int index) && EnteredItems.ContainsKey(index))
                {
                    Exit.DestinationExit = EnteredItems[index];
                    break;
                }
                else if (input.StartsWith(@"\"))
                {
                    Fiter = input[1..];
                }
            }
            return true;
        }

        private static bool LoopHintSelect(IEnumerable<object> CheckObject, InstanceData.TrackerInstance Instance)
        {
            Console.Clear();
            if (CheckObject.First() is not HintData.HintObject HintSpot) { return false; }
            Console.WriteLine($"Enter Hint at {HintSpot.ID}");
            string hint = Console.ReadLine();
            HintSpot.HintText = hint;
            return true;
        }

        private static void undoredo(string action)
        {
            if (action == "undo" && newTrackerInstance.UndoStringList.Any())
            {
                string CurrentState = Newtonsoft.Json.JsonConvert.SerializeObject(newTrackerInstance.Instance);
                newTrackerInstance.Instance = InstanceData.TrackerInstance.FromJson(newTrackerInstance.UndoStringList[^1]);
                newTrackerInstance.RedoStringList.Add(CurrentState);
                newTrackerInstance.UndoStringList.RemoveAt(newTrackerInstance.UndoStringList.Count - 1);
                newTrackerInstance.UnsavedChanges = true;
            }
            else if (action == "redo" && newTrackerInstance.RedoStringList.Any())
            {
                string CurrentState = Newtonsoft.Json.JsonConvert.SerializeObject(newTrackerInstance.Instance);
                newTrackerInstance.Instance = InstanceData.TrackerInstance.FromJson(newTrackerInstance.RedoStringList[^1]);
                newTrackerInstance.UndoStringList.Add(CurrentState);
                newTrackerInstance.RedoStringList.RemoveAt(newTrackerInstance.RedoStringList.Count - 1);
                newTrackerInstance.UnsavedChanges = true;
            }
        }
        private static void SaveInstance()
        {
            if (!File.Exists(newTrackerInstance.CurrentSavePath))
            {
                promptForSave:
                Console.WriteLine("Enter Save File Path");
                string path = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(path)) 
                {
                    path = AppDomain.CurrentDomain.BaseDirectory;
                }

                string Drive = Path.GetPathRoot(path);
                string Folder = Path.GetDirectoryName(path);
                string fileName = Path.GetFileNameWithoutExtension(path);
                string Extention = Path.GetExtension(path);

                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = "Save";
                }
                if (string.IsNullOrEmpty(Drive))
                {
                    Folder = string.IsNullOrEmpty(Folder) ? AppDomain.CurrentDomain.BaseDirectory : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Folder);
                }
                else if (!string.IsNullOrEmpty(Drive) && string.IsNullOrEmpty(Folder))
                {
                    Folder = Drive;
                }
                if (Extention != ".MMRTSAV")
                {
                    if (Extention.ToUpper() != ".MMRTSAV")
                        fileName += Extention;
                    Extention = ".MMRTSAV";
                }

                try
                {
                    var FinalPath = Path.Combine(Folder, fileName + Extention);
                    Console.WriteLine($"Save to following path? (y/n) \n{FinalPath}");
                    readConfirmKey:
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.N) { Console.WriteLine($""); goto promptForSave; }
                    else if (key.Key != ConsoleKey.Y) { goto readConfirmKey; }
                    if (!Directory.Exists(Folder)) { Directory.CreateDirectory(Folder); }
                    newTrackerInstance.SaveInstance(FinalPath);
                    newTrackerInstance.CurrentSavePath = FinalPath;
                    Debug.WriteLine($"{FinalPath}");
                }
                catch
                {
                    Console.WriteLine("\nSave Path Invalid!");
                    Console.ReadKey();
                    goto promptForSave;
                }  
            }
            else
            {
                newTrackerInstance.SaveInstance(newTrackerInstance.CurrentSavePath);
            }

            newTrackerInstance.UnsavedChanges = false;
        }

        private static MiscData.Divider CreateDivider(int Width, string key = "=")
        {
            string Divider = "";
            for(var i = 0; i < Width; i++) { Divider += key; }
            return new MiscData.Divider() { Display = Divider };
        }

        private static void LoadSettingFile()
        {
        getpath:
            Console.WriteLine("Enter Setting/Spoiler Log File Path");
            string path = Console.ReadLine();
            path = path.Replace("\"", "");

            if (!File.Exists(path)) { Console.WriteLine("Path Invalid!"); goto getpath; }

            newTrackerInstance.logicCalculation.ResetAutoObtainedItems();

            MMRData.SpoilerLogData SpoilerLogData = null;
            try { SpoilerLogData = Newtonsoft.Json.JsonConvert.DeserializeObject<MMRData.SpoilerLogData>(File.ReadAllText(path)); }
            catch { }

            if (SpoilerLogData != null && newTrackerInstance.Instance.LogicFile.GameCode == "MMR")
            {
                Console.WriteLine("Applying Settings File...");
                MMRSpoilerLogTools.ApplyMMRandoSettings(newTrackerInstance.Instance, SpoilerLogData);
            }
            else
            {
                Console.WriteLine("Importing Spoiler Log...");
                SpoilerLogTools.ImportSpoilerLog(File.ReadAllLines(path), path, newTrackerInstance);
            }
            newTrackerInstance.logicCalculation.CalculateLogic();
        }

    }
}
