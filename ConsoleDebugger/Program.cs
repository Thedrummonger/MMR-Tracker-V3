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

            string Logic = File.ReadAllText(Testing.GetLogicPath());

            var Result = TrackerInstanceCreation.ApplyLogicAndDict(NewTrackerInstance, Logic);

            TrackerInstanceCreation.PopulateTrackerObject(NewTrackerInstance);

            Console.WriteLine("NYI");

        }
    }
}
