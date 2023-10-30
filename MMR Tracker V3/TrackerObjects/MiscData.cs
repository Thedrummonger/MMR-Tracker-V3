using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

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
        public class OptionComboboxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }

            public override string ToString()
            {
                return Text;
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
            public Action tagAction { get; set; }
            public override string ToString()
            {
                return Display;
            }
        }

        public class CheckItemSetting
        {
            public CheckItemSetting(CheckItemSetting Copy)
            {
                TargetheckState = Copy.TargetheckState;
                CheckUnassignedLocations = Copy.CheckUnassignedLocations;
                CheckUnassignedEntrances = Copy.CheckUnassignedEntrances;
                CheckUnassignedHints = Copy.CheckUnassignedHints;
                CheckCoiceOptions = Copy.CheckCoiceOptions;
                CheckIntOPtions = Copy.CheckIntOPtions;
            }
            public CheckItemSetting(CheckState _TargetCheckState) 
            {
                TargetheckState = _TargetCheckState;
            }
            public CheckItemSetting Copy()
            {
                return new CheckItemSetting(this);
            }
            public CheckState TargetheckState;
            public bool EnforceMarkAction = false;
            public Func<IEnumerable<object>, InstanceContainer, List<ManualCheckObjectResult>> CheckUnassignedLocations = 
                (IEnumerable<object> O, InstanceContainer C) => { throw new NotImplementedException("CheckUnassignedLocations was not assigned"); };
            public Func<IEnumerable<object>, InstanceContainer, List<ManualCheckObjectResult>> CheckUnassignedEntrances =
                (IEnumerable<object> O, InstanceContainer C) => { throw new NotImplementedException("CheckUnassignedEntrances was not assigned"); };
            public Func<IEnumerable<object>, InstanceContainer, List<ManualCheckObjectResult>> CheckUnassignedHints =
                (IEnumerable<object> O, InstanceContainer C) => { throw new NotImplementedException("CheckUnassignedHints was not assigned"); };
            public Func<IEnumerable<object>, InstanceContainer, List<ManualCheckObjectResult>> CheckCoiceOptions =
                (IEnumerable<object> O, InstanceContainer C) => { throw new NotImplementedException("CheckCoiceOptions was not assigned"); };
            public Func<IEnumerable<object>, InstanceContainer, List<ManualCheckObjectResult>> CheckIntOPtions =
                (IEnumerable<object> O, InstanceContainer C) => { throw new NotImplementedException("CheckIntOPtions was not assigned"); };
            public CheckItemSetting SetTargetheckState(CheckState _TargetCheckState) { TargetheckState = _TargetCheckState; return this; }
            public CheckItemSetting SetEnforceMarkAction(bool _EnforceMarkAction) { EnforceMarkAction = _EnforceMarkAction; return this; }
            public CheckItemSetting SetCheckUnassignedLocations(Func<IEnumerable<object>, InstanceContainer, List<ManualCheckObjectResult>> func) { CheckUnassignedLocations = func; return this; }
            public CheckItemSetting SetCheckUnassignedEntrances(Func<IEnumerable<object>, InstanceContainer, List<ManualCheckObjectResult>> func) { CheckUnassignedEntrances = func; return this; }
            public CheckItemSetting SetCheckUnassignedHints(Func<IEnumerable<object>, InstanceContainer, List<ManualCheckObjectResult>> func) { CheckUnassignedHints = func; return this; }
            public CheckItemSetting SetCheckCoiceOptions(Func<IEnumerable<object>, InstanceContainer, List<ManualCheckObjectResult>> func) { CheckCoiceOptions = func; return this; }
            public CheckItemSetting SetCheckIntOPtions(Func<IEnumerable<object>, InstanceContainer, List<ManualCheckObjectResult>> func) { CheckIntOPtions = func; return this; }

        }
        public class ManualCheckObjectResult
        {
            public ManualCheckObjectResult(object _Check, object _Item, int _OwningPlayer = -1) { Check = _Check; Item = _Item; OwningPlayer = _OwningPlayer; }
            public object Check;
            public object Item;
            public int OwningPlayer = -1;
            public T GetCheck<T>() { return (T)Check; }
            public T GetItem<T>() { return (T)Item; }
        }

        public class NetConnection
        {
            public NetData.OnlineMode OnlineMode { get; set; } = NetData.OnlineMode.None;
            public TcpClient ServerConnection { get; set; } = null;
            public int PlayerID { get; set; } = -1;
            public void Reset()
            {
                OnlineMode = NetData.OnlineMode.None;
                ServerConnection = null;
                PlayerID = -1;
            }
        }

        public class InstanceContainer
        {
            public InstanceContainer()
            {
                logicCalculation = new LogicCalculation(this);
            }
            private InstanceData.TrackerInstance _Instance;
            public InstanceData.TrackerInstance Instance {
                get 
                {
                    _Instance?.SetParentContainer(this);
                    return _Instance;
                } 
                set
                {
                    _Instance = value;
                    _Instance?.SetParentContainer(this);
                } 
            }
            public LogicCalculation logicCalculation { get; set; }
            public NetConnection netConnection { get; set; } = new NetConnection();
            public List<string> UndoStringList { get; set; } = new List<string>();
            public List<string> RedoStringList { get; set; } = new List<string>();
            public string CurrentSavePath { get; set; } = "";
            public bool UnsavedChanges { get; set; } = false;

            public bool SaveInstance(string SavePath)
            {
                try
                {
                    if (Instance.StaticOptions.OptionFile.CompressSave)
                    {
                        var CompressedSave = new SaveCompressor.CompressedSave(Instance.ToString());
                        File.WriteAllBytes(SavePath, CompressedSave.Bytes);
                    }
                    else
                    {
                        File.WriteAllText(SavePath, Instance.ToString());
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            public bool LoadSave(string[] Save) { return LoadSave(string.Join("", Save)); }
            public bool LoadSave(string Save)
            {
                if (File.Exists(Save))
                {
                    switch (SaveCompressor.TestFileType(Save, Instance))
                    {
                        case SaveCompressor.SaveType.Standard:
                            Instance = InstanceData.TrackerInstance.FromJson(File.ReadAllText(Save));
                            break;
                        case SaveCompressor.SaveType.Compressed:
                            var Decomp = SaveCompressor.Decompress(File.ReadAllText(Save));
                            Instance = InstanceData.TrackerInstance.FromJson(Decomp);
                            break;
                        case SaveCompressor.SaveType.CompressedByte:
                            var ByteDecomp = SaveCompressor.Decompress(File.ReadAllBytes(Save));
                            Instance = InstanceData.TrackerInstance.FromJson(ByteDecomp);
                            break;
                        case SaveCompressor.SaveType.error:
                            return false;
                    }
                }
                else
                {
                    try { Instance = InstanceData.TrackerInstance.FromJson(Save); }
                    catch { return false; }
                }
                logicCalculation.CompileOptionActionEdits();
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
        public enum UnrandState
        {
            Any = 0,
            Unrand = 1,
            Manual = 2
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
            Debugging = 1,
            UserView = 2,
            Off = 3
        }
        public enum MathOP
        {
            add,
            subtract,
            multiply,
            divide,
            set
        }

        public enum JSONType
        {
            Newtonsoft,
            UTF8,
            DotNet
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
            Area,
            Exit,
            ChoiceOption,
            ToggleOption,
            IntOption,
            LogicEntryCollection,
            function,
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
