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
    public static class Testing
    {
        public static MiscData.DebugMode DebugMode = MiscData.DebugMode.Off;

        public static void doDevCheck(bool? forceDevState = null, bool Modifier = false)
        {
            bool ForceDev = forceDevState is not null && (bool)forceDevState;
            bool ForceUser = forceDevState is not null && !(bool)forceDevState;
            bool isDevPC = File.Exists(References.Globalpaths.DevFile) || ForceDev;
            bool IsDubugger = Debugger.IsAttached || ForceDev;

            if (IsDubugger && !Modifier && !ForceUser)
            {
                Testing.DebugMode = MiscData.DebugMode.Debugging;
            }
            else if (isDevPC && Modifier && !ForceUser)
            {
                Testing.DebugMode = MiscData.DebugMode.UserView;
            }
            else
            {
                Testing.DebugMode = MiscData.DebugMode.Off;
            }
        }

        public static bool Debugging()
        {
            return DebugMode == MiscData.DebugMode.Debugging;
        }
        public static bool UserView()
        {
            return DebugMode == MiscData.DebugMode.UserView;
        }
        public static bool IsDevUser()
        {
            return Debugging() || UserView();
        }
        public static bool StandardUser()
        {
            return DebugMode == MiscData.DebugMode.Off;
        }

        public static string CretaeTestingFile(string Name, string Extention = "txt")
        {
            return Path.Combine(References.TestingPaths.GetDevTestingPath(), $"{Name}.{Extention}");
        }

        public static void PrintObjectToConsole(object o)
        {
            string JsonString = JsonConvert.SerializeObject(o, _NewtonsoftJsonSerializerOptions);
            Debug.WriteLine(JsonString);
        }

        public readonly static Newtonsoft.Json.JsonSerializerSettings _NewtonsoftJsonSerializerOptions = new Newtonsoft.Json.JsonSerializerSettings
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
        };
    }
}
