using MMR_Tracker_V3.TrackerObjectExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using static MMR_Tracker_V3.TrackerDataHandling;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;

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
            public string DisplayOverride { get; set; } = null;
            public override string ToString()
            {
                string Display = DisplayOverride ?? Area;
                if (ForceUpper) { Display = Display.ToUpper(); }
                if (AddColon) { Display += ":"; }
                return Display;
            }
            public string GetHideID(DisplayListType InList)
            {
                return Area + ":::" + InList.ToString();
            }
            public bool IsMinimized(DisplayListType InList, Options OptionFile)
            {
                return OptionFile.MinimizedHeaders.Contains(GetHideID(InList));
            }
            public bool SetMinimized(DisplayListType InList, Options OptionFile, bool Minimize)
            {
                if (Minimize) { return OptionFile.MinimizedHeaders.Add(GetHideID(InList)); }
                return OptionFile.MinimizedHeaders.Remove(GetHideID(InList));
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
        public class Divider(string DividerString)
        {
            public string Display { get; set; } = DividerString;
            public override string ToString()
            {
                return Display;
            }
        }
        public class StandardListBoxItem
        {
            public StandardListBoxItem(string display, object tag) { Display = display; Tag = tag; }
            public StandardListBoxItem() { }
            public string Display { get; set; }
            public object Tag { get; set; }
            public Func<object, object> tagFunc { get; set; }
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
                TargetCheckState = Copy.TargetCheckState;
                EnforceMarkAction = Copy.EnforceMarkAction;
                CheckUnassignedLocations = Copy.CheckUnassignedLocations;
                CheckUnassignedEntrances = Copy.CheckUnassignedEntrances;
                CheckUnassignedHints = Copy.CheckUnassignedHints;
                CheckChoiceOptions = Copy.CheckChoiceOptions;
                CheckIntOptions = Copy.CheckIntOptions;
                CheckEntrancePairs = Copy.CheckEntrancePairs;
            }
            public CheckItemSetting(CheckState _TargetCheckState)
            {
                TargetCheckState = _TargetCheckState;
            }
            public CheckItemSetting Copy()
            {
                return new CheckItemSetting(this);
            }
            public CheckState TargetCheckState;
            public bool EnforceMarkAction = false;
            public bool CheckEntrancePairs = true;
            public Func<IEnumerable<object>, InstanceContainer, List<ManualCheckObjectResult>> CheckUnassignedLocations =
                (O, C) => { throw new NotImplementedException("CheckUnassignedLocations was not assigned"); };
            public Func<IEnumerable<object>, InstanceContainer, List<ManualCheckObjectResult>> CheckUnassignedEntrances =
                (O, C) => { throw new NotImplementedException("CheckUnassignedEntrances was not assigned"); };
            public Func<IEnumerable<object>, InstanceContainer, List<ManualCheckObjectResult>> CheckUnassignedHints =
                (O, C) => { throw new NotImplementedException("CheckUnassignedHints was not assigned"); };
            public Func<IEnumerable<object>, InstanceContainer, List<ManualCheckObjectResult>> CheckChoiceOptions =
                (O, C) => { throw new NotImplementedException("CheckChoiceOptions was not assigned"); };
            public Func<IEnumerable<object>, InstanceContainer, List<ManualCheckObjectResult>> CheckIntOptions =
                (O, C) => { throw new NotImplementedException("CheckIntOptions was not assigned"); };
            public CheckItemSetting SetTargetCheckState(CheckState _TargetCheckState) { TargetCheckState = _TargetCheckState; return this; }
            public CheckItemSetting SetEnforceMarkAction(bool _EnforceMarkAction) { EnforceMarkAction = _EnforceMarkAction; return this; }
            public CheckItemSetting SetCheckEntrancePairs(bool _CheckEntrancePairs) { CheckEntrancePairs = _CheckEntrancePairs; return this; }
            public CheckItemSetting SetCheckUnassignedLocations(Func<IEnumerable<object>, InstanceContainer, List<ManualCheckObjectResult>> func) { CheckUnassignedLocations = func; return this; }
            public CheckItemSetting SetCheckUnassignedEntrances(Func<IEnumerable<object>, InstanceContainer, List<ManualCheckObjectResult>> func) { CheckUnassignedEntrances = func; return this; }
            public CheckItemSetting SetCheckUnassignedHints(Func<IEnumerable<object>, InstanceContainer, List<ManualCheckObjectResult>> func) { CheckUnassignedHints = func; return this; }
            public CheckItemSetting SetCheckChoiceOptions(Func<IEnumerable<object>, InstanceContainer, List<ManualCheckObjectResult>> func) { CheckChoiceOptions = func; return this; }
            public CheckItemSetting SetCheckIntOptions(Func<IEnumerable<object>, InstanceContainer, List<ManualCheckObjectResult>> func) { CheckIntOptions = func; return this; }


        }
        public class ManualCheckObjectResult()
        {
            private object _Check;
            private object _Result;
            public ManualCheckObjectResult SetLocation(object location, object Result)
            {
                _Check = location;
                _Result = Result;
                return this;
            }
            public ManualCheckObjectResult SetItemLocation(LocationData.LocationObject location, string ItemID, int OwningPlayer = -1)
                => SetLocation(location, new ItemResult(ItemID, OwningPlayer));
            public ManualCheckObjectResult SetExitDestination(EntranceData.EntranceRandoExit Exit, EntranceData.EntranceRandoDestination destination)
                => SetLocation(Exit, destination);
            public ManualCheckObjectResult SetGossipHint(HintData.HintObject HintLocation, string Hint)
                => SetLocation(HintLocation, Hint);
            public ManualCheckObjectResult SetIntOption(OptionData.IntOption intOption, int value)
                => SetLocation(intOption, value);
            public ManualCheckObjectResult SetChoiceOption(OptionData.ChoiceOption ChoiceOption, string value)
                => SetLocation(ChoiceOption, value);
            public ManualCheckObjectResult SetPricedLocation(CheckableLocation Location, int Price)
                => SetLocation(Location, Price);
            public (TC, TR) GetLocation<TC,TR>()
            {
                return ((TC)_Check, (TR)_Result);
            }
            public (LocationData.LocationObject Check, ItemResult ItemData) GetItemLocation() 
                => ((LocationData.LocationObject)_Check, (ItemResult)_Result);
            public (EntranceData.EntranceRandoExit Exit, EntranceData.EntranceRandoDestination Destination) GetExitDestination() 
                => ((EntranceData.EntranceRandoExit)_Check, (EntranceData.EntranceRandoDestination)_Result);
            public (HintData.HintObject HintCheck, string HintText) GetGossipHint()
                => ((HintData.HintObject)_Check, (string)_Result);
            public (OptionData.IntOption IntOption, int Val) GetIntOption()
                => ((OptionData.IntOption)_Check, (int)_Result);
            public (OptionData.ChoiceOption ChoiceOption, string Val) GetChoiceOption()
                => ((OptionData.ChoiceOption)_Check, (string)_Result);
            public (CheckableLocation Location, int Price) GetPricedLocation()
                => ((CheckableLocation)_Check, (int)_Result);
        }

        public class ItemResult
        {
            public ItemResult(string I, int P) { ItemID = I; Player = P; }
            public ItemResult(string I) { ItemID = I; Player = -1; }
            public string ItemID;
            public int Player;
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

        public class TrackerLocationDataList
        {
            public InstanceContainer InstanceContainer;
            public TrackerInstance Instance { get { return InstanceContainer.Instance; } }
            public Dictionary<string, int> Categories;
            public Divider Divider;
            public string Filter;
            public DataSets DataSets;
            public bool Reverse = false;
            public int ItemsFound = 0;
            public int ItemsDisplayed = 0;
            public List<object> FinalData = [];
            private bool _ShowAll = false;
            public bool LocationsFiltered { get { return ItemsFound != ItemsDisplayed; } }
            public bool ShowUnavailableEntries { get { return _ShowAll || Filter.StartsWith("^") && !Filter.StartsWith("^^") || Filter.StartsWith("^^^"); } }
            public bool ShowInvalidEntries { get { return Filter.StartsWith("^^"); } }
            public TrackerLocationDataList(Divider _Divider, InstanceContainer _InstanceContainer, string _Filter, DataSets _DataSets = null, Dictionary<string, int> _Categories = null)
            {
                Divider = _Divider;
                InstanceContainer = _InstanceContainer;
                Filter = _Filter;
                DataSets = _DataSets is null ? CreateDataSets(Instance) : _DataSets;
                Categories = _Categories is null ? CategoryFileHandling.GetCategoriesFromFile(Instance) : _Categories;
            }
            public TrackerLocationDataList Reset() { FinalData = []; ItemsFound = 0; ItemsDisplayed = 0; return this; }
            public TrackerLocationDataList PrintReverse(bool reverse = true) { Reverse = reverse; return this; }
            public TrackerLocationDataList ShowUnavailable(bool showall = true) { _ShowAll = showall; return this; }
        }

        public class LogicItemData
        {
            public string RawID;
            public string CleanID;
            public object Object;
            public int Amount;
            public bool Literal;
            public bool HadItemCount;
            public LogicItemTypes Type;
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

        public enum DisplayListType
        {
            Locations,
            Checked,
            Entrances
        }

        public enum UILayout
        {
            Vertical = 0,
            Horizontal = 1,
            Compact = 2
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
        public enum CheckableLocationTypes
        {
            location,
            Exit,
            Hint,
            macro
        }
        public enum LogicItemTypes
        {
            item,
            Area,
            macro,
            LogicEntryCollection,
            Boolean,
            function,
            error
        }
        public enum OptionTypes
        {
            ChoiceOption,
            MultiSelectOption,
            ToggleOption,
            IntOption,
        }
        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));

                        if (memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() is DescriptionAttribute descriptionAttribute)
                        {
                            return descriptionAttribute.Description;
                        }
                    }
                }
            }

            return string.Empty;
        }
    }
}
