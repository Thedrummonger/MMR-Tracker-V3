using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TDMUtils;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class TrackerSettings
    {

        [Serializable]
        public class Options
        {
            public DisplayListType? ShowOptionsInListBox { get; set; } = null;

            public HashSet<string> MinimizedHeaders { get; set; } = [];
            public OptionFile OptionFile { get; set; } = new OptionFile();
        }

        [Serializable]
        public class OptionFile
        {
            public bool CheckForUpdate { get; set; } = true;
            public bool CheckHintMarkItem { get; set; } = true;
            public bool CompressSave { get; set; } = true;
            public bool ShowUnavailableMarkedLocations { get; set; } = true;
            public bool SeperateUnavailableMarkedLocations { get; set; } = true;
            public bool ShowMacroExitsPathfinder { get; set; } = false;
            public bool ShowRedundantPathfinder { get; set; } = false;
            public bool AutoCheckCoupleEntrances { get { return _AutoCheckCoupleEntrances; } set { _AutoCheckCoupleEntrances = value; } }
            private bool _AutoCheckCoupleEntrances = true;
            public bool EntranceRandoFeatures { get; set; } = true;
            public int MaxUndo { get; set; } = 10;
            public WinformData WinformData { get; set; } = new WinformData();
            public NetConfig NetConfig { get; set; } = new NetConfig();

            public OptionFile Copy()
            {
                return MiscUtilities.SerializeConvert<OptionFile>(this);
            }
            public OptionFile ToggleUpdateCheck(bool? Value = null) { CheckForUpdate = Value is null ? !CheckForUpdate : (bool)Value; return this; }
            public OptionFile ToggleHintMarkItem(bool? Value = null) { CheckHintMarkItem = Value is null ? !CheckHintMarkItem : (bool)Value; return this; }
            public OptionFile ToggleCompressSave(bool? Value = null) { CompressSave = Value is null ? !CompressSave : (bool)Value; return this; }
            public OptionFile ToggleShowUnavailableMarked(bool? Value = null) { ShowUnavailableMarkedLocations = Value is null ? !ShowUnavailableMarkedLocations : (bool)Value; return this; }
            public OptionFile ToggleSeperateUnavailableMarked(bool? Value = null) { SeperateUnavailableMarkedLocations = Value is null ? !SeperateUnavailableMarkedLocations : (bool)Value; return this; }
            public OptionFile TogglePathfinderMacros(bool? Value = null) { ShowMacroExitsPathfinder = Value is null ? !ShowMacroExitsPathfinder : (bool)Value; return this; }
            public OptionFile ToggleRedundantPaths(bool? Value = null) { ShowRedundantPathfinder = Value is null ? !ShowRedundantPathfinder : (bool)Value; return this; }
            public OptionFile ToggleCheckCoupled(bool? Value = null) { AutoCheckCoupleEntrances = Value is null ? !AutoCheckCoupleEntrances : (bool)Value; return this; }
            public OptionFile ToggleEntranceFeatures(bool? Value = null) { EntranceRandoFeatures = Value is null ? !EntranceRandoFeatures : (bool)Value; return this; }
            public OptionFile SetUILayout(UILayout Value) { this.WinformData.UILayout = Value; return this; }
            public OptionFile SetEntryTooltip(bool? Value = null) { this.WinformData.ShowEntryNameTooltip = Value is null ? !this.WinformData.ShowEntryNameTooltip : (bool)Value; return this; }
            public OptionFile SetServerIP(string Value) { this.NetConfig.ServerIP = Value; return this; }
            public OptionFile SetServerPort(int Value) { this.NetConfig.ServerPort = Value; return this; }
            public OptionFile SetAPServerIP(string Value) { this.NetConfig.APServerIP = Value; return this; }
            public OptionFile SetAPServerPort(int Value) { this.NetConfig.APServerPort = Value; return this; }

            public OptionFile SetColumnSize(float val)
            {
                float newVal = val;
                if (val > 80) { newVal = 80; }
                if (val < 20) { newVal = 20; }
                this.WinformData.ColumnSize = newVal;
                return this;
            }

            public OptionFile SetRowSize(float val)
            {
                float newVal = val;
                if (val > 80) { newVal = 80; }
                if (val < 20) { newVal = 20; }
                this.WinformData.RowSize = newVal;
                return this;
            }
            public OptionFile SetMaxUndos(int Value) 
            { 
                MaxUndo = Value < 0 ? 0 : Value; 
                return this;
            }
            public OptionFile SetFont(string Value) { this.WinformData.FormFont = Value; return this; }
        }

        public class NetConfig
        {
            public string ServerIP = "127.0.0.1";
            public int ServerPort = 25570;
            public string APServerIP = "archipelago.gg";
            public int APServerPort = 38281;
        }

        [Serializable]
        public class WinformData
        {
            public string FormFont { get; set; } = string.Empty;
            public UILayout UILayout { get; set; } = UILayout.Vertical;
            public float RowSize { get; set; } = 50F;
            public float ColumnSize { get; set; } = 50F;
            public bool ShowEntryNameTooltip { get; set; } = true;
        }

        public static OptionFile ReadDefaultOptionsFile(bool WriteFileIfUnreadable = true)
        {
            OptionFile Settings = new OptionFile();
            bool OptionFileValid = false;
            if (File.Exists(References.Globalpaths.OptionFile))
            {
                OptionFileValid = true;
                try { Settings = JsonConvert.DeserializeObject<OptionFile>(File.ReadAllText(References.Globalpaths.OptionFile)); }
                catch { Debug.WriteLine("could not parse options.txt"); OptionFileValid = false; }
            }
            if (!OptionFileValid && WriteFileIfUnreadable)
            {
                File.WriteAllText(References.Globalpaths.OptionFile, Settings.ToFormattedJson());
            }
            return Settings;
        }

        public static OptionFile UpdateDefaultOptionFile(Action<OptionFile> Transformer)
        {
            var Default = ReadDefaultOptionsFile(false);
            Transformer(Default); 
            WriteOptionFile(Default);
            return Default;
        }

        public static OptionFile WriteOptionFile(OptionFile Template)
        {
            File.WriteAllText(References.Globalpaths.OptionFile, Template.ToFormattedJson());
            return Template;
        }
    }
}
