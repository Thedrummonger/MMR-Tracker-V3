using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using static MMR_Tracker_V3.TrackerDataHandeling;
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

            /// <summary>
            /// Creates a function that assignes an unchecked location an item based on a predefined map
            /// </summary>
            /// <param name="StaticLocationItemMap">A dictionary of location ids and the Item ID of the item at that location</param>
            /// <returns></returns>
            public CheckItemSetting SetCheckUnassignedLocations(Dictionary<string, string> StaticLocationItemMap)
            {
                CheckUnassignedLocations = (IEnumerable<object> O, InstanceContainer C) =>
                {
                    List<ManualCheckObjectResult> Results = [];
                    foreach (var obj in O)
                    {
                        LocationData.LocationObject location = obj as LocationData.LocationObject;
                        Results.Add(new ManualCheckObjectResult(location, StaticLocationItemMap[location.ID]));
                    }
                    return Results;
                };
                return this;
            }

        }
        public class ManualCheckObjectResult(object _Check, object _Item, int _OwningPlayer = -1)
        {
            public object Check = _Check;
            public object Item = _Item;
            public int OwningPlayer = _OwningPlayer;
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

        public class TrackerLocationDataList
        {
            public InstanceContainer InstanceContainer;
            public InstanceData.TrackerInstance Instance { get { return InstanceContainer.Instance; } }
            public Divider Divider;
            public string Filter;
            public DataSets DataSets;
            public bool Reverse = false;
            public int ItemsFound = 0;
            public int ItemsDisplayed = 0;
            public List<object> FinalData = [];
            private bool _ShowAll = false;
            public bool LocationsFiltered { get { return ItemsFound != ItemsDisplayed; } }
            public bool ShowUnavailableEntries { get { return _ShowAll || (Filter.StartsWith("^") && !Filter.StartsWith("^^")) || Filter.StartsWith("^^^"); } }
            public bool ShowInvalidEntries { get { return Filter.StartsWith("^^"); } }
            public TrackerLocationDataList(Divider _Divider, InstanceContainer _InstanceContainer, string _Filter, DataSets _DataSets = null) 
            {
                Divider = _Divider;
                InstanceContainer = _InstanceContainer;
                Filter = _Filter;
                DataSets = _DataSets is null ? PopulateDataSets(Instance) : _DataSets;
            }  
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
            public LogicEntryType Type;
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
            MultiSelectOption,
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
