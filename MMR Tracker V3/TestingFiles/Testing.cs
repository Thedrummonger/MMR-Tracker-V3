using MMR_Tracker_V3.TrackerObjectExtentions;
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

        public static void doDevCheck(MiscData.DebugMode? forceDevState = null, bool Modifier = false)
        {
            if (forceDevState is not null) 
            { 
                DebugMode = (MiscData.DebugMode)forceDevState;
                return;
            }

            bool isDevPC = File.Exists(References.Globalpaths.DevFile);
            bool IsDubugger = Debugger.IsAttached != Modifier;

            if (IsDubugger)
            {
                //Debugging: Full access to dev features
                Testing.DebugMode = MiscData.DebugMode.Debugging;
            }
            else if (isDevPC)
            {
                //Debugging: No access to dev features, but a button is available to enable dev
                Testing.DebugMode = MiscData.DebugMode.UserView;
            }
            else
            {
                //Debugging: No access to dev features
                Testing.DebugMode = MiscData.DebugMode.Off;
            }
        }

        public static bool Debugging()
        {
            return DebugMode == MiscData.DebugMode.Debugging;
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
        public static void TestLogicForInvalidItems(MiscData.InstanceContainer Container)
        {
            foreach (var i in Container.Instance.LogicFile.Logic)
            {
                var logicitems = i.ConditionalItems.SelectMany(x => x).Concat(i.RequiredItems).ToArray();
                foreach (var l in logicitems)
                {
                    Container.logicCalculation.LogicEntryAquired(l, new List<string>());
                }
            }
        }
        public static void TestLocationsForInvalidVanillaItem(MiscData.InstanceContainer Container)
        {
            foreach (var i in Container.Instance.LocationPool)
            {
                string OriginalItem = i.Value.GetDictEntry(Container.Instance).OriginalItem;
                if (Container.Instance.GetItemByID(OriginalItem) is null)
                {
                    Debug.WriteLine($"{OriginalItem} at loc {i.Key} is not a valid item");
                }
            }
        }
    }
}
