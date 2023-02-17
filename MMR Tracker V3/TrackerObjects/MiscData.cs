using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.ItemData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public static class MiscData
    {

        [Serializable]
        public class Areaheader
        {
            public string Area { get; set; }
            public bool ForceUpper { get; set; } = true;
            public bool AddColon { get; set; } = true;
            public override string ToString()
            {
                string Display = Area;
                if (ForceUpper) { Display = Display.ToUpper(); }
                if (AddColon) { Display += ":"; }
                return Display;
            }
        }
        public class Divider
        {
            public string Display { get; set; }
            public override string ToString()
            {
                return Display;
            }
        }
        public class StandardListBoxItem
        {
            public string Display { get; set; }
            public object tag { get; set; }
            public Func<dynamic, dynamic> tagFunc { get; set; }
            public override string ToString()
            {
                return Display;
            }
        }
        public class userData
        {
            public string PastebinAPIKey { get; set; }
            public string PastebinUsername { get; set; }
            public string PastebinPassword { get; set; }
        }
        public class InstanceContainer
        {
            public InstanceContainer()
            {
                logicCalculation = new LogicCalculation(this);
            }
            public LogicObjects.TrackerInstance Instance { get; set; }
            public LogicCalculation logicCalculation { get; set; }
            public LogicRecreation LogicRecreation { get; set; } = new LogicRecreation();
            public List<string> UndoStringList { get; set; } = new List<string>();
            public List<string> RedoStringList { get; set; } = new List<string>();
            public string CurrentSavePath { get; set; } = "";
            public bool UnsavedChanges { get; set; } = false;

            public bool LoadSave(string[] Save) { return LoadSave(string.Join("", Save)); }
            public bool LoadSave(string Save)
            {
                if (File.Exists(Save))
                {
                    try { Instance = LogicObjects.TrackerInstance.FromJson(File.ReadAllText(Save)); }
                    catch { return false; }
                }
                else
                {
                    try { Instance = LogicObjects.TrackerInstance.FromJson(Save); }
                    catch { return false; }
                }
                return true;
            }
        }
        [Serializable]
        public enum CheckState
        {
            Checked = 0,
            Marked = 1,
            Unchecked = 2
        }
        [Serializable]
        public enum LogicFileType
        {
            Logic = 0,
            Additional = 1,
            Runtime = 2
        }
        public enum DebugMode
        {
            Verbose = 0,
            Debugging = 1,
            UserView = 2,
            Off = 3
        }

        [Serializable]
        public enum RandomizedState
        {
            [Description("Rand")]
            Randomized = 0,
            [Description("UnRand")]
            Unrandomized = 1,
            [Description("Manual")]
            UnrandomizedManual = 2,
            [Description("Junk")]
            ForcedJunk = 3
        }
        [Serializable]
        public enum TimeOfDay
        {
            None,
            Day1 = 1,
            Night1 = 2,
            Day2 = 4,
            Night2 = 8,
            Day3 = 16,
            Night3 = 32,
        }
        [Serializable]
        public enum LogicEntryType
        {
            item,
            location,
            macro,
            Hint,
            Bool,
            Option,
            Area,
            Exit,
            variableString,
            variableInt,
            variableBool,
            variableList,
            error
        }
        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = System.Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var descriptionAttribute = memInfo[0]
                            .GetCustomAttributes(typeof(DescriptionAttribute), false)
                            .FirstOrDefault() as DescriptionAttribute;

                        if (descriptionAttribute != null)
                        {
                            return descriptionAttribute.Description;
                        }
                    }
                }
            }

            return null; // could also return string.Empty
        }
    }
}
