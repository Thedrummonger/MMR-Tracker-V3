using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjectExtensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.TrackerObjects.MMRData;
using static MMR_Tracker_V3.TrackerObjects.HintData;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static MMR_Tracker_V3.TrackerObjects.LogicDictionaryData;
using static MMR_Tracker_V3.TrackerObjects.OptionData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class InstanceData
    {
        [Serializable]
        public class TrackerInstance(InstanceData.InstanceContainer instanceContainer)
        {
            private InstanceContainer Parent = instanceContainer;
            public InstanceContainer GetParentContainer() { return Parent; }
            public void SetParentContainer(InstanceContainer P) { Parent = P; }
            public Dictionary<string, LocationObject> LocationPool { get; set; } = [];
            public Dictionary<string, HintObject> HintPool { get; set; } = [];
            public Dictionary<string, MacroObject> MacroPool { get; set; } = [];
            public Dictionary<string, ItemObject> ItemPool { get; set; } = [];
            public Dictionary<string, ChoiceOption> ChoiceOptions { get; set; } = [];
            public Dictionary<string, MultiSelectOption> MultiSelectOptions { get; set; } = [];
            public Dictionary<string, ToggleOption> ToggleOptions { get; set; } = [];
            public Dictionary<string, IntOption> IntOptions { get; set; } = [];
            public Dictionary<string, LogicEntryCollection> LogicEntryCollections { get; set; } = [];
            public Dictionary<string, EntranceData.EntranceRandoExit> ExitPool { get; set; } = [];
            public Dictionary<string, EntranceData.EntranceRandoArea> AreaPool { get; set; } = [];
            //public EntranceData.EntranceDepricated EntrancePool { get; set; } = new EntranceData.EntranceDepricated(null);
            public LogicDictionary LogicDictionary { get; set; } = new LogicDictionary();
            public LogicFile LogicFile { get; set; } = new LogicFile();
            public SpoilerLogFileData SpoilerLog { get; set; } = null;
            public Dictionary<string, JsonFormatLogicItem> RuntimeLogic { get; set; } = [];
            public LocationProxyData LocationProxyData { get; set; } = new LocationProxyData();
            public Options StaticOptions { get; set; } = new Options();
            public PriceData PriceData { get; set; } = new PriceData();
            public InstanceReference InstanceReference { get; set; } = new InstanceReference();
            public Dictionary<string, Dictionary<string, LogicItemData>> UnlockData { get; set; } = [];
            public override string ToString()
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this, _NewtonsoftJsonSerializerOptions);
            }
            public string ToJson(JSONType type)
            {
                return type switch
                {
                    JSONType.Newtonsoft => JsonConvert.SerializeObject(this, _NewtonsoftJsonSerializerOptions),
                    JSONType.UTF8 => Utf8Json.JsonSerializer.ToJsonString(this),
                    JSONType.DotNet => System.Text.Json.JsonSerializer.Serialize(this),
                    _ => throw new NotImplementedException(),
                };
            }
            private readonly static JsonSerializerSettings _NewtonsoftJsonSerializerOptions = new()
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
            };
        }

        [Serializable]
        public class InstanceReference
        {
            public Dictionary<string, ActionItemEdit> OptionActionItemEdits { get; set; } = [];
            public Dictionary<string, List<string>> OptionActionCollectionEdits { get; set; } = [];
        }

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
        }

        [Serializable]
        public class WinformData
        {
            public string FormFont { get; set; } = string.Empty;
            public UILayout UILayout { get; set; } = UILayout.Vertical;
            public bool ShowEntryNameTooltip { get; set; } = true;
        }

        [Serializable]
        public class PriceData
        {
            public bool Initialized { get; set; } = false;
            public List<string> WalletEntries { get; set; } = [];
            public Dictionary<char, Dictionary<int, string>> CapacityMap { get; set; } = [];
            public Dictionary<int, string> GetCapacityMap(char Currency)
            {
                if (CapacityMap.TryGetValue(Currency, out Dictionary<int, string> value)) { return value; }
                return [];
            }
        }

        public class ReferenceData
        {
            public LogicFileType LogicList { get; set; }
            public int LogicIndex { get; set; }
        }
        [Serializable]
        public class SpoilerLogFileData
        {
            public string FileName { get; set; }
            public string[] Log { get; set; }
            public Dictionary<string, PlaythroughGenerator.PlaythroughObject> Playthrough { get; set; }

            public void GetStaticPlaythrough(TrackerInstance instance)
            {
                PlaythroughGenerator generator = new(instance);
                generator.GeneratePlaythrough();
                if (instance.LogicDictionary.WinCondition != null)
                {
                    var wincon = PlaythroughTools.GetDefaultWincon(instance);
                    if (wincon is not null) { generator.FilterImportantPlaythrough(wincon); }
                    Debug.WriteLine($"Seed Beatable: {generator.Playthrough.ContainsKey(instance.LogicDictionary.WinCondition)}");
                }
                Playthrough = generator.Playthrough;
            }
        }

        public class InstanceContainer
        {
            public InstanceContainer()
            {
                logicCalculation = new LogicCalculation(this);
            }
            private InstanceData.TrackerInstance _Instance;
            public InstanceData.TrackerInstance Instance
            {
                get
                {
                    _Instance?.SetParentContainer(this);
                    return _Instance;
                }
            }
            public LogicCalculation logicCalculation { get; set; }
            public NetConnection netConnection { get; set; } = new NetConnection();
            public List<string> UndoStringList { get; set; } = [];
            public List<string> RedoStringList { get; set; } = [];
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
            public bool LoadSerializedInstance(string Json)
            {
                return LoadSave(Json);
            }
            public bool LoadInsanceFromFile(string Path)
            {
                if (!File.Exists(Path)) { return false; }
                string SerializedSave = SaveCompressor.GetSaveStringFromFile(Path, this._Instance);
                if (SerializedSave == string.Empty) { return false; }
                return LoadSave(SerializedSave);
            }
            public bool CopyAndLoadInstance(TrackerInstance ToCopy)
            {
                return LoadSerializedInstance(ToCopy.ToString());
            }
            private bool LoadSave(string Save)
            {
                try { ApplyInstance(Save); }
                catch { return false; }

                logicCalculation.CompileOptionActionEdits();
                TrackerInstanceCreation.TriggerInstanceCreatedEvent(this);
                return true;
            }

            public void CreateEmptyInstance()
            {
                ApplyInstance(new TrackerInstance(this));
            }

            private void ApplyInstance(string JsonString)
            {
                ApplyInstance(JsonConvert.DeserializeObject<InstanceData.TrackerInstance>(JsonString));
            }
            private void ApplyInstance(InstanceData.TrackerInstance Instance)
            {
                _Instance = Instance;
                _Instance.SetParentContainer(this);
                foreach (var i in _Instance.LocationPool.Values) { i.SetParent(_Instance); }
                foreach (var i in _Instance.LocationProxyData.LocationProxies.Values) { i.SetParent(_Instance); }
                foreach (var i in _Instance.MacroPool.Values) { i.SetParent(_Instance); }
                foreach (var i in _Instance.HintPool.Values) { i.SetParent(_Instance); }
                foreach (var i in _Instance.ItemPool.Values) { i.SetParent(_Instance); }
                foreach (var i in _Instance.ChoiceOptions.Values) { i.SetParent(_Instance); }
                foreach (var i in _Instance.MultiSelectOptions.Values) { i.SetParent(_Instance); }
                foreach (var i in _Instance.ToggleOptions.Values) { i.SetParent(_Instance); }
                foreach (var i in _Instance.IntOptions.Values) { i.SetParent(_Instance); }
                foreach (var i in _Instance.AreaPool.Values) { i.SetParent(_Instance); }
                foreach (var i in _Instance.ExitPool.Values) { i.SetParent(_Instance); }
                Instance.LogicDictionary.SetParentContainer(Instance);
                foreach (var i in Instance.LogicDictionary.LocationList) { i.Value.ID = i.Key; i.Value.SetParent(Instance.LogicDictionary); }
                foreach (var i in Instance.LogicDictionary.ItemList) { i.Value.ID = i.Key; i.Value.SetParent(Instance.LogicDictionary); }
                foreach (var i in Instance.LogicDictionary.EntranceList) { i.Value.ID = i.Key; i.Value.SetParent(Instance.LogicDictionary); }
                foreach (var i in Instance.LogicDictionary.HintSpots) { i.Value.ID = i.Key; i.Value.SetParent(Instance.LogicDictionary); }
                foreach (var i in Instance.LogicDictionary.MacroList) { i.Value.ID = i.Key; i.Value.SetParent(Instance.LogicDictionary); }
            }
        }

    }
}
