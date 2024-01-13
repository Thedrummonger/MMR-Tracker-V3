using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static MMR_Tracker_V3.TrackerObjects.HintData;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.LocationData;
using static MMR_Tracker_V3.TrackerObjects.LogicDictionaryData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.TrackerObjects.MMRData;
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
            public EntranceData.EntrancePool EntrancePool { get; set; } = new EntranceData.EntrancePool(null);
            public LogicDictionary LogicDictionary { get; set; } = new LogicDictionary();
            public LogicFile LogicFile { get; set; } = new LogicFile();
            public SpoilerLogFileData SpoilerLog { get; set; } = null;
            public Dictionary<string, JsonFormatLogicItem> RuntimeLogic { get; set; } = [];
            public LocationProxyData LocationProxyData { get; set; } = new LocationProxyData();
            public Options StaticOptions { get; set; } = new Options();
            public PriceData PriceData { get; set; } = new PriceData();
            public InstanceReference InstanceReference { get; set; } = new InstanceReference();
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
            //A table mapping Logic names to an entrance area pair
            public Dictionary<string, EntranceData.EntranceAreaPair> EntranceLogicNameToEntryData { get; set; } = [];
            //A table Mapping an Exit to its logic name
            public Dictionary<string, string> ExitLogicMap { get; set; } = [];
            //A dictionary to keep track of an Items values after they have been edited by option data
            public Dictionary<string, ActionItemEdit> OptionActionItemEdits { get; set; } = [];
            public Dictionary<string, List<string>> OptionActionCollectionEdits { get; set; } = [];
        }

        [Serializable]
        public class Options
        {
            public string ShowOptionsInListBox { get; set; } = OptionData.DisplayListBoxes[0];

            public Dictionary<string, bool> MinimizedHeader { get; set; } = [];
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
                    var wincon = instance.LogicDictionary.WinCondition;
                    bool Literal = wincon.IsLiteralID(out string ParsedWinCon);
                    instance.GetItemEntryType(ParsedWinCon, Literal, out object ItemOut);
                    instance.GetItemEntryType(ParsedWinCon, Literal, out object LocationOut);
                    var outitem = ItemOut??LocationOut??null;
                    if (outitem is not null) { generator.FilterImportantPlaythrough(outitem); }
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
                set
                {
                    ApplyInstance(value);
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
            public bool LoadSave(string[] Save) { return LoadSave(string.Join("", Save)); }
            public bool LoadSave(string Save)
            {
                if (File.Exists(Save))
                {
                    switch (SaveCompressor.TestSaveFileType(Save, Instance))
                    {
                        case SaveCompressor.SaveType.Standard:
                            ApplyInstance(File.ReadAllText(Save));
                            break;
                        case SaveCompressor.SaveType.Compressed:
                            var Decomp = SaveCompressor.Decompress(File.ReadAllText(Save));
                            ApplyInstance(Decomp);
                            break;
                        case SaveCompressor.SaveType.CompressedByte:
                            var ByteDecomp = SaveCompressor.Decompress(File.ReadAllBytes(Save));
                            ApplyInstance(ByteDecomp);
                            break;
                        case SaveCompressor.SaveType.error:
                            return false;
                    }
                }
                else
                {
                    try { ApplyInstance(Save); }
                    catch { return false; }
                }
                logicCalculation.CompileOptionActionEdits();
                return true;
            }

            public void ApplyInstance(string JsonString)
            {
                ApplyInstance(JsonConvert.DeserializeObject<InstanceData.TrackerInstance>(JsonString));
            }
            public void ApplyInstance(InstanceData.TrackerInstance Instance)
            {
                _Instance = Instance;
                _Instance.SetParentContainer(this);
                foreach (var i in _Instance.LocationPool.Values) { i.SetParent(_Instance); }
                foreach (var i in _Instance.LocationProxyData.LocationProxies.Values) { i.SetParent(_Instance); }
                foreach (var i in _Instance.MacroPool.Values) { i.SetParent(_Instance); }
                foreach (var i in _Instance.HintPool.Values) { i.SetParent(_Instance); }
                foreach (var i in _Instance.ItemPool.Values) { i.SetParent(_Instance); }
                _Instance.EntrancePool.SetParent(_Instance);
                foreach (var i in _Instance.EntrancePool.AreaList.Values) 
                { 
                    i.SetParent(_Instance); 
                    foreach(var j in i.Exits.Values) { j.SetParent(_Instance); }
                }
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
