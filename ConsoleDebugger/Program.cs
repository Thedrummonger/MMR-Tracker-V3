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
            "2. New",
            "3. Preset",
            "4. Load"
        };
        static List<string> UndoStringList = new();
        static List<string> RedoStringList = new();
        static LogicObjects.TrackerInstance newTrackerInstance;
        static bool UnsavedChanges = false;

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
                        LoadPreset();
                        break;
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        LoadSave();
                        break;
                }
            }
        }

        private static void LoadPreset()
        {
            LogicObjects.TrackerInstance NewTrackerInstance = new();
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
                TrackerInstanceCreation.ApplyLogicAndDict(NewTrackerInstance, PresetsDict[ind].LogicString, PresetsDict[ind].DictionaryString);
                TrackerInstanceCreation.PopulateTrackerObject(NewTrackerInstance);
                newTrackerInstance = NewTrackerInstance;
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

            newTrackerInstance = LogicObjects.TrackerInstance.FromJson(File.ReadAllText(path));
            References.CurrentSavePath = path;
            LoopLocationList();
        }

        static void TestItemOutput()
        {
            LogicObjects.TrackerInstance NewTrackerInstance = new();

        getpath:
            Console.WriteLine("Enter Logic Path");
            string path = Console.ReadLine();
            path = path.Replace("\"", "");

            if (!File.Exists(path)) { Console.WriteLine("Path Invalid!"); goto getpath; }

            string Logic = File.ReadAllText(path);

            TrackerInstanceCreation.ApplyLogicAndDict(NewTrackerInstance, Logic);
            TrackerInstanceCreation.PopulateTrackerObject(NewTrackerInstance);
            newTrackerInstance = NewTrackerInstance;
            LoopLocationList();
        }

        private static void LoopLocationList()
        {
            //0: Available Loc  1: Checked Loc 2: Available Ent
            int EntryType = 0;
            string Filter = "";
            while (true)
            {
                Console.Title = newTrackerInstance.LogicFile.GameCode + " Tracker" + (UnsavedChanges ? "*" : "");
                Console.Clear();
                LogicCalculation.CalculateLogic(newTrackerInstance);
                List<object> Entries = null;
                int x = 0;
                int y = 0;
                string CurrentType = "";
                MiscData.CheckState CheckAction = MiscData.CheckState.Checked;
                if (EntryType == 0)
                {
                    Entries = TrackerDataHandeling.PopulateAvailableLocationList(CreateDivider(Console.WindowWidth), newTrackerInstance, Filter, false, out  x, out  y, true);
                    CurrentType = "Available Locations";
                }
                else if (EntryType == 1)
                {
                    Entries = TrackerDataHandeling.PopulateCheckedLocationList(CreateDivider(Console.WindowWidth), newTrackerInstance, Filter, out  x, out  y, true);
                    CurrentType = "Checked Locations";
                    CheckAction = MiscData.CheckState.Unchecked;
                }
                else if (EntryType == 2)
                {
                    Entries = TrackerDataHandeling.PopulateAvailableEntraceList(CreateDivider(Console.WindowWidth), newTrackerInstance, Filter, false, out  x, out  y, true);
                    CurrentType = "Available Entrances";
                }
                Dictionary<int, object> reference = new Dictionary<int, object>();
                foreach (var i in Entries) 
                {
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
                Console.WriteLine($"Actions:");
                Console.WriteLine($"0-{y-1} = check corresponding location| type # before the number to mark");
                Console.WriteLine($"^ = apply spoiler log or settings file");
                Console.WriteLine($"z = undo | y = redo");
                Console.WriteLine($"s = save");
                Console.WriteLine($"l = show available location | c = show Checked location | e = show Available Entrances");
                Console.WriteLine($"\\ + search term = filter locations by search term");
                var input = Console.ReadLine();

                if (input.StartsWith("#")) 
                { 
                    CheckAction = MiscData.CheckState.Marked;
                    input = input[1..];
                }

                if (int.TryParse(input, out int index) && reference.ContainsKey(index))
                {
                    string CurrentState = Newtonsoft.Json.JsonConvert.SerializeObject(newTrackerInstance);
                    TrackerDataHandeling.CheckSelectedItems(new List<object> { reference[index] }, CheckAction, newTrackerInstance, HandleUnAssignedLocations, HandleUnAssignedVariables);
                    UnsavedChanges = true;
                    RedoStringList.Clear();
                    UndoStringList.Add(CurrentState);
                }
                else if (input.StartsWith(@"\"))
                {
                    Filter = input[1..];
                }
                else if (input == "^") 
                { 
                    LoadSettingFile();
                    RedoStringList.Clear();
                    UndoStringList.Add(Newtonsoft.Json.JsonConvert.SerializeObject(newTrackerInstance));
                    UnsavedChanges = true;
                }
                else if (input == "l") { EntryType = 0; }
                else if (input == "e") { EntryType = 2; }
                else if (input == "c") { EntryType = 1; }
                else if (input == "z") { undoredo("undo"); }
                else if (input == "y") { undoredo("redo"); }
                else if (input == "s") { SaveInstance(); }

            }
        }

        private static bool HandleUnAssignedLocations(IEnumerable<object> CheckObject, LogicObjects.TrackerInstance Instance)
        {
            foreach(var i in CheckObject)
            {
                if (i is LocationData.LocationObject)
                {
                    return LoopItemSelect(new List<object> { i }, Instance);
                }
                if (i is EntranceData.EntranceRandoExit)
                {
                    return LoopEntranceSelect(new List<object> { i }, Instance);
                }
            }
            return false;
        }

        private static bool HandleUnAssignedVariables(IEnumerable<object> CheckObject, LogicObjects.TrackerInstance Instance)
        {
            foreach (var i in CheckObject)
            {
                if (i is HintData.HintObject)
                {
                    return LoopHintSelect(new List<object> { i }, Instance);
                }
            }
            return false;
        }

        private static bool LoopItemSelect(IEnumerable<object> CheckObject, LogicObjects.TrackerInstance Instance)
        {
            string Fiter = "";
            if (CheckObject.First() is not LocationData.LocationObject Location) { return false; }
            while (true)
            {
                Console.Clear();
                var Names = new List<string>();
                var Counter = 0;
                var EnteredItems = new Dictionary<int, ItemData.ItemObject>();
                foreach (var i in newTrackerInstance.ItemPool.Values)
                {
                    if (string.IsNullOrWhiteSpace(i.GetDictEntry(newTrackerInstance).GetItemName(newTrackerInstance))) { continue; }
                    i.DisplayName = i.GetDictEntry(newTrackerInstance).GetItemName(newTrackerInstance);
                    if (!SearchStringParser.FilterSearch(newTrackerInstance, i, Fiter, i.DisplayName)) { continue; }
                    if (i.CanBePlaced(newTrackerInstance) && i.GetDictEntry(newTrackerInstance).ItemTypes.Intersect(Location.GetDictEntry(newTrackerInstance).ValidItemTypes).Any() && !EnteredItems.Values.Contains(i) && !Names.Contains(i.ToString()))
                    {
                        Names.Add(i.ToString());
                        EnteredItems.Add(Counter, i);
                        Console.WriteLine(Counter + ": " + i);
                        Counter++;
                    }
                }
                Console.WriteLine(CreateDivider(Console.WindowWidth));
                Console.WriteLine("Select Item at " + Location.GetDictEntry(newTrackerInstance).Name ?? Location.ID);
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

        private static bool LoopEntranceSelect(IEnumerable<object> CheckObject, LogicObjects.TrackerInstance Instance)
        {
            string Fiter = "";
            if (CheckObject.First() is not EntranceData.EntranceRandoExit Exit) { return false; }
            while (true)
            {
                Console.Clear();
                var Names = new List<string>();
                var Counter = 0;
                var EnteredItems = new Dictionary<int, EntranceData.EntranceRandoDestination>();
                foreach (var area in Instance.EntrancePool.AreaList.Values.Where(x => x.LoadingZoneExits.Any()).ToList().SelectMany(x => x.LoadingZoneExits).OrderBy(x => x.Value.ID))
                {
                    var Entry = new EntranceData.EntranceRandoDestination
                    {
                        region = area.Value.ID,
                        from = area.Value.ParentAreaID,
                    };
                    if (!SearchStringParser.FilterSearch(Instance, Entry, Fiter, Entry.ToString())) { continue; }
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

        private static bool LoopHintSelect(IEnumerable<object> CheckObject, LogicObjects.TrackerInstance Instance)
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
            if (action == "undo" && UndoStringList.Any())
            {
                string CurrentState = Newtonsoft.Json.JsonConvert.SerializeObject(newTrackerInstance);
                newTrackerInstance = LogicObjects.TrackerInstance.FromJson(UndoStringList[^1]);
                RedoStringList.Add(CurrentState);
                UndoStringList.RemoveAt(UndoStringList.Count - 1); 
                UnsavedChanges = true;
            }
            else if (action == "redo" && RedoStringList.Any())
            {
                string CurrentState = Newtonsoft.Json.JsonConvert.SerializeObject(newTrackerInstance);
                newTrackerInstance = LogicObjects.TrackerInstance.FromJson(RedoStringList[^1]);
                UndoStringList.Add(CurrentState);
                RedoStringList.RemoveAt(RedoStringList.Count - 1);
                UnsavedChanges = true;
            }
        }
        private static void SaveInstance()
        {
            if (!File.Exists(References.CurrentSavePath))
            {
                Console.WriteLine("Enter Save File Path");
                string path = Console.ReadLine();
                path = path.Replace("\"", "");
                References.CurrentSavePath = path;
            }
            File.WriteAllText(References.CurrentSavePath, newTrackerInstance.ToString());
            UnsavedChanges = false;
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

            if (newTrackerInstance.LogicFile.GameCode == "OOTR")
            {
                MMR_Tracker_V3.OtherGames.OOTRTools.HandleOOTRSpoilerLog(File.ReadAllText(path), newTrackerInstance);
                return;
            }
            else
            {
                MMRData.SpoilerLogData SpoilerLogData = null;
                try { SpoilerLogData = Newtonsoft.Json.JsonConvert.DeserializeObject<MMRData.SpoilerLogData>(File.ReadAllText(path)); }
                catch { }

                if (SpoilerLogData == null)
                {
                    Console.WriteLine("Importing Spoiler Log...");
                    newTrackerInstance.SpoilerLog = SpoilerLogTools.ReadSpoilerLog(File.ReadAllLines(path));
                    Console.WriteLine("Applying Settings File...");
                    SpoilerLogTools.ApplyMMRandoSettings(newTrackerInstance, newTrackerInstance.SpoilerLog);
                    Console.WriteLine("Applying Check Data...");
                    SpoilerLogTools.ApplyMMRandoSpoilerLog(newTrackerInstance, newTrackerInstance.SpoilerLog);
                }
                else
                {
                    Console.WriteLine("Applying Settings File...");
                    SpoilerLogTools.ApplyMMRandoSettings(newTrackerInstance, SpoilerLogData);
                }
                return;
            }
        }

    }
}
