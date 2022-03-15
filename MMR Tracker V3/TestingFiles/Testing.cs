using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static MMR_Tracker_V3.TrackerObjects.OptionData;

namespace MMR_Tracker_V3
{
    public class Testing
    {
        public static bool ViewAsUserMode = false;
        public static bool ISDebugging = false;

        public static string GetLogicPath()
        {
            var testFolder = @"D:\Testing";
            var LogicFile = Path.Combine(testFolder, "Logic.txt");
            if (File.Exists(LogicFile))
            {
                return LogicFile;
            }
            Console.WriteLine("Enter Logic Path");
            return Console.ReadLine();
        }

        public static string GetDictPath()
        {
            var testFolder = @"D:\Testing";
            var DictionaryFile = Path.Combine(testFolder, "NewDict.json");
            if (File.Exists(DictionaryFile))
            {
                return DictionaryFile;
            }
            Console.WriteLine("Enter Dictionary Path");
            return Console.ReadLine();
        }

        public static string GetSavePath(bool Loading)
        {
            var testFolder = @"D:\Testing";
            var SaveFile = @"D:\Testing\Save.json";
            if (Loading)
            {
                if (File.Exists(SaveFile)) { return SaveFile; }
                else
                {
                    Console.WriteLine("Enter Save File Path");
                    return Console.ReadLine();
                }
            }
            else
            {
                if (Directory.Exists(testFolder)) { return SaveFile; }
                else
                {
                    Console.WriteLine("Enter Save File Path");
                    return Console.ReadLine();
                }
            }
        }


        public static void CodeTesting(LogicObjects.TrackerInstance instance)
        {

        }

        public readonly static Newtonsoft.Json.JsonSerializerSettings _NewtonsoftJsonSerializerOptions = new Newtonsoft.Json.JsonSerializerSettings
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
        };
    }
}
