using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static MathNet.Symbolics.Linq;

namespace MMR_Tracker_V3.OtherGames.PaperMarioRando
{
    public class PMREdge
    {
        public MapPoint from;
        public MapPoint to;
        public object[][] reqs;
        public string[] pseudoitems;
        public string ParsedReq(object j)
        {
            string Item;
            int Count;
            if (j is System.String) { return (string)j; }
            else
            {
                var json = JsonConvert.SerializeObject(j);
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
                Item = dictionary.First().Key;
                Count = dictionary.First().Value;
            }
            return $"{Item}, {Count}";
        }
    }

    public class MapMeta
    {
        public string name;
        public string verbose_name;
    }
    public class MapNode
    {
        public string identifier;
        public string entrance_name;
    }
    public class ItemFileData
    {
        public string item_name;
        public string item_type;
        public int unplaceable;
    }

    public class MapPoint
    {
        public string map;
        public object id;
        public int IntID()
        {
            if (!int.TryParse(StringID(), out int val)) { return -1; }
            return val;
        }
        public string StringID()
        {
            return id.ToString();
        }

        public string GetVerboseName(Dictionary<string, string> MapNames, Dictionary<string, string> MapMeta, Dictionary<string, string> Nodes)
        {
            return GetVerboseName(MapNames, MapMeta, Nodes, out _, out _, out _, out _);
        }
        public string GetVerboseName(Dictionary<string, string> MapNames, Dictionary<string, string> MapMeta, Dictionary<string, string> Nodes, out string MapCode, out string MapName, out string Areaname, out string NodeName)
        {
            MapCode = map.Split('_')[0];
            MapName = MapNames.ContainsKey(MapCode) ? MapNames[MapCode] : MapCode;
            Areaname = MapMeta.ContainsKey(map) ? MapMeta[map] : map;
            string NodeKey = $"{map}/{StringID()}";
            NodeName = Nodes.ContainsKey(NodeKey) ? Nodes[NodeKey] : StringID();
            return $"{MapName} - {Areaname} - {NodeName}";
        }
    }


    public class ReadData
    {
        public static string SettingWebPage = "https://paper-mario-randomizer-server.ue.r.appspot.com/randomizer_settings/4147161913";
        public static LogicStringParser PMRParser = new LogicStringParser(containerType: LogicStringParser.ContainerType.bracket);
        private static string GetEdgeLogic(PMREdge edge, List<List<object>> AdditionalReq = null)
        {
            List<List<object>> Req = new List<List<object>>();
            Req.AddRange(edge.reqs.Select(x => x.ToList()).ToList());
            if (AdditionalReq is not null) { Req.AddRange(AdditionalReq); }
            if (!Req.Any()) { return ""; }
            return $"[[{string.Join("] && [", Req.Select(x => string.Join(" || ", x.Select(y => edge.ParsedReq(y)))))}]]";
        }

        public static void ReadEadges(out MMRData.LogicFile OutLogic, out LogicDictionaryData.LogicDictionary outDict)
        {
            string CodePath = Path.Combine(References.TestingPaths.GetDevCodePath(), "MMR Tracker V3");
            string PMRFiles = Path.Combine(CodePath, "OtherGames", "PaperMarioRando");
            string EdgesFile = Path.Combine(PMRFiles, "Edges.json");
            string AreaName = Path.Combine(PMRFiles, "AreaNames.json");
            string MapMetaFile = Path.Combine(PMRFiles, "mapmeta.json");
            string LocationsFile = Path.Combine(PMRFiles, "Locations.json");
            string ItemsFile = Path.Combine(PMRFiles, "item.json");
            string ItemsNamesFile = Path.Combine(PMRFiles, "ItemNames.json");
            string MapNodesFile = Path.Combine(PMRFiles, "node.json");
            string MacroFile = Path.Combine(PMRFiles, "Macros.json");

            Dictionary<string, string> MapNames = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(AreaName));
            Dictionary<string, string> MapMeta = JsonConvert.DeserializeObject<List<MapMeta>>(File.ReadAllText(MapMetaFile)).ToDictionary(x => x.name, x => x.verbose_name);
            Dictionary<string, string> MapNodes = JsonConvert.DeserializeObject<List<MapNode>>(File.ReadAllText(MapNodesFile)).Where(x => x.entrance_name is not null).ToDictionary(x => x.identifier, x => x.entrance_name);
            Dictionary<string, Dictionary<string, string>> Locations = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(LocationsFile));
            List<ItemFileData> Items = JsonConvert.DeserializeObject<List<ItemFileData>>(File.ReadAllText(ItemsFile)).Where(x => x.unplaceable == 0).ToList();
            Dictionary<string, string> ItemNames = JsonConvert.DeserializeObject<Dictionary<string,string>>(File.ReadAllText(ItemsNamesFile));
            Dictionary<string, string> Macros = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(MacroFile));

            string TestingFoler = Path.Combine(References.TestingPaths.GetDevTestingPath(), "PMRTesting");

            var Edges = JsonConvert.DeserializeObject<List<PMREdge>>(File.ReadAllText(EdgesFile));

            LogicDictionaryData.LogicDictionary PMRDict = new LogicDictionaryData.LogicDictionary()
            {
                RootArea = "Spawn",
                GameCode = "PMR",
                WinCondition = "YOUWIN",
                LogicVersion = 1
            };
            MMRData.LogicFile PRMLogic = new MMRData.LogicFile
            {
                GameCode = PMRDict.GameCode,
                Logic = new List<MMRData.JsonFormatLogicItem>(),
                Version = PMRDict.LogicVersion
            };

            Dictionary<string, int> QuizmoSPtracking = new Dictionary<string, int>();

            Dictionary<string, string> PsuedoMacros = new Dictionary<string, string>();
            Dictionary<string, string> ExitLogic = new Dictionary<string, string>();
            //Tuple: Location Name, Location Area, Logic string
            Dictionary<string, Tuple<string, string, string>> PsuedoStarpieceLocations = new Dictionary<string, Tuple<string, string, string>>();
            //Tuple: Location Name, Location Area, Logic string
            Dictionary<string, Tuple<string, string, string>> PsuedoStarSpiritLocations = new Dictionary<string, Tuple<string, string, string>>();
            //Tuple: Location Name, Location Area, Logic string, Cooked Item
            Dictionary<string, Tuple<string, string, string, string>> PsuedoSTayceTLocations = new Dictionary<string, Tuple<string, string, string, string>>();
            //ID, Area, Logic
            Tuple<string, string, string> MysticalKeyData = null;
            //ID, Area, Logic
            Tuple<string, string, string> DryOutpostDriedpastaData = null;
            foreach (var edge in Edges)
            {
                string FromID = $"{edge.from.GetVerboseName(MapNames, MapMeta, MapNodes, out string _, out string MapName, out string CurrentAreaName, out _)}";
                string ToID = $"{edge.to.GetVerboseName(MapNames, MapMeta, MapNodes)}";

                if (edge.pseudoitems is not null && edge.pseudoitems.Any())
                {
                    foreach(var item in edge.pseudoitems)
                    {
                        string Logic = GetEdgeLogic(edge, new List<List<object>> { new List<object> { FromID } });
                        if (item.StartsWith("GF") || item.StartsWith("MF") || item.StartsWith("MB") || item.StartsWith("RF") || item.StartsWith("FAVOR") || item.StartsWith("YOUWIN"))
                        {
                            if (!PsuedoMacros.ContainsKey(item)) { PsuedoMacros.Add(item, Logic); }
                            else { PsuedoMacros[item] = $"{PsuedoMacros[item]} || {Logic}"; }
                        }
                        else if (item.StartsWith("StarPiece"))
                        {
                            if (!QuizmoSPtracking.ContainsKey(MapName)) { QuizmoSPtracking[MapName] = 1; }
                            string CheckName = $"Quizmo - Starpiece {QuizmoSPtracking[MapName]}";
                            if (!PsuedoStarpieceLocations.ContainsKey(item)) { PsuedoStarpieceLocations.Add(item, new(CheckName, MapName, Logic)); QuizmoSPtracking[MapName]++; }
                            else { PsuedoStarpieceLocations[item] = new(PsuedoStarpieceLocations[item].Item1, MapName, $"{PsuedoStarpieceLocations[item].Item3} || {Logic}"); }
                        }
                        else if (item.StartsWith("STARSPIRIT"))
                        {
                            string CheckID = $"{CurrentAreaName} - Star Spirit";
                            PsuedoStarSpiritLocations.Add(item, new(CheckID, MapName, Logic));
                        }
                        else if (item == "MysticalKey")
                        {
                            string CheckID = $"{CurrentAreaName} - Mystical Key Chest";
                            MysticalKeyData = new(CheckID, MapName, Logic);
                        }
                        else if (FromID == "Toad Town - Southern District - Exit West" && FromID == ToID)
                        {
                            string RecipeResult = item;
                            List<string> Ingredients = new List<string>();
                            foreach(var i in edge.reqs.SelectMany(x => x)) { if (i.ToString() != "RF_CanCook" && i.ToString() != "Cookbook") { Ingredients.Add(i.ToString()); } }
                            string ID = $"Tayce T Cook {RecipeResult} ({string.Join(", ", Ingredients)})";
                            PsuedoSTayceTLocations.Add(ID, new(ID, MapName, Logic, RecipeResult));
                        }
                        else if (item == "DriedPasta" && ToID == "Dry Dry Outpost - Outpost 1 - ShopItemA")
                        {
                            DryOutpostDriedpastaData = new("Outpost 1 - Shop Item 4", MapName, Logic);
                        }
                        else
                        {
                            Debug.WriteLine($"Handle Psuedoitem {item}\n{ToID}");
                        }
                    }
                }

                if (FromID == ToID) { continue; }
                string EntranceID = $"{FromID} => {ToID}";

                if (!ExitLogic.ContainsKey(EntranceID))
                {
                    PMRDict.EntranceList.Add(EntranceID, new LogicDictionaryData.DictionaryEntranceEntries { ID = EntranceID, Area = FromID, Exit = ToID });
                    ExitLogic.Add(EntranceID, GetEdgeLogic(edge));
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(ExitLogic[EntranceID])) { ExitLogic[EntranceID] = GetEdgeLogic(edge); }
                    else if (!string.IsNullOrWhiteSpace(GetEdgeLogic(edge))) { ExitLogic[EntranceID] = $"{ExitLogic[EntranceID]} || {GetEdgeLogic(edge)}"; }
                }

            }


            PMRDict.LocationList.Add(MysticalKeyData.Item1, new LogicDictionaryData.DictionaryLocationEntries { ID = MysticalKeyData.Item1, Name = MysticalKeyData.Item1, Area = MysticalKeyData.Item2, ValidItemTypes = new string[] { "MysticalKey" } });
            PRMLogic.Logic.Add(new MMRData.JsonFormatLogicItem { Id = MysticalKeyData.Item1, ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(PMRParser, MysticalKeyData.Item3, MysticalKeyData.Item1) });
            PMRDict.ItemList.Add("MysticalKey", new LogicDictionaryData.DictionaryItemEntries { ID = "MysticalKey", Name = "MysticalKey", ItemTypes = new string[] { "MysticalKey" } });

            PMRDict.LocationList.Add(DryOutpostDriedpastaData.Item1, new LogicDictionaryData.DictionaryLocationEntries { ID = DryOutpostDriedpastaData.Item1, Name = DryOutpostDriedpastaData.Item1, Area = DryOutpostDriedpastaData.Item2, ValidItemTypes = new string[] { "Outpost DriedPasta" } });
            PRMLogic.Logic.Add(new MMRData.JsonFormatLogicItem { Id = DryOutpostDriedpastaData.Item1, ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(PMRParser, DryOutpostDriedpastaData.Item3, DryOutpostDriedpastaData.Item1) });

            foreach (var i in PsuedoStarpieceLocations)
            {
                PMRDict.LocationList.Add(i.Key, new LogicDictionaryData.DictionaryLocationEntries { ID = i.Key, Name = i.Value.Item1, Area = i.Value.Item2, ValidItemTypes = new string[] { "StarPiece" } });
                PRMLogic.Logic.Add(new MMRData.JsonFormatLogicItem { Id = i.Key, ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(PMRParser, i.Value.Item3, i.Key) });
            }

            foreach (var i in PsuedoSTayceTLocations)
            {
                PMRDict.LocationList.Add(i.Key, new LogicDictionaryData.DictionaryLocationEntries { ID = i.Key, Name = i.Value.Item1, Area = i.Value.Item2, OriginalItem = i.Value.Item4, ValidItemTypes = new string[] { $"Cook {i.Value.Item4}" } });
                PRMLogic.Logic.Add(new MMRData.JsonFormatLogicItem { Id = i.Key, ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(PMRParser, i.Value.Item3, i.Key) });
            }

            foreach (var i in PsuedoStarSpiritLocations)
            {
                string[] StarSpiritName = new[] { "Eldstar", "Mamar", "Skolar", "Muskular", "Misstar", "Klevar", "Kalmar" };
                PMRDict.LocationList.Add(i.Value.Item1, new LogicDictionaryData.DictionaryLocationEntries { ID = i.Value.Item1, Name = i.Value.Item1, Area = i.Value.Item2, ValidItemTypes = new string[] { i.Key } });
                PRMLogic.Logic.Add(new MMRData.JsonFormatLogicItem { Id = i.Value.Item1, ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(PMRParser, i.Value.Item3, i.Value.Item1) });
                string SpiritName = StarSpiritName[int.Parse(i.Key.Split('_')[1])-1];
                PMRDict.ItemList.Add(i.Key, new LogicDictionaryData.DictionaryItemEntries { ID = i.Key, Name = SpiritName, ItemTypes = new string[] { i.Key } });
            }

            foreach (var i in PsuedoMacros)
            {
                PRMLogic.Logic.Add(new MMRData.JsonFormatLogicItem { Id = i.Key, ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(PMRParser, i.Value, i.Key) });
            }

            foreach (var i in ExitLogic)
            {
                PRMLogic.Logic.Add(new MMRData.JsonFormatLogicItem { Id = i.Key, ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(PMRParser, i.Value, i.Key) });
                LogicUtilities.RemoveRedundantConditionals(PRMLogic.Logic.Last());
            }

            foreach(var Area in Locations)
            {
                foreach(var check in Area.Value)
                {
                    MapPoint CheckLocation = new MapPoint { map = Area.Key, id = check.Key };
                    string AreaRequirement = CheckLocation.GetVerboseName(MapNames, MapMeta, MapNodes, out _, out string Currentmap, out string CurrentArea, out _);
                    string CheckID = $"{CurrentArea} - {check.Value}";
                    CheckID = CheckID.Replace("'", "");
                    PRMLogic.Logic.Add(new MMRData.JsonFormatLogicItem { Id = CheckID, RequiredItems = new List<string> { AreaRequirement } });
                    PMRDict.LocationList.Add(CheckID, new LogicDictionaryData.DictionaryLocationEntries { ID = CheckID, Area = $"{Currentmap}", ValidItemTypes = new string[] { "item" } });
                }
            }

            foreach (var i in Macros)
            {
                PRMLogic.Logic.Add(new MMRData.JsonFormatLogicItem { Id = i.Key, ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(PMRParser, i.Value, i.Key) });
            }

            foreach (var i in ItemNames)
            {
                if (i.Key.StartsWith("StarPiece") ||
                    i.Key.StartsWith("PowerStar") ||
                    i.Key.StartsWith("ThreeStarPieces") ||
                    i.Key.Contains("BerryProxy") ||
                    i.Key.Contains("BowserCastleKey") ||
                    i.Key.Contains("KoopaFortressKey") ||
                    i.Key.Contains("RuinsKey") ||
                    i.Key.Contains("TubbaCastleKey")
                ) { continue; }
                var ItemData = Items.FirstOrDefault(x => x.item_name == i.Key);
                int MaxCount = ItemData is not null && ItemData.item_type == "ITEM" ? -1 : 1;
                string NiceName = string.IsNullOrWhiteSpace(i.Value) ? i.Key : i.Value;
                PMRDict.ItemList.Add(i.Key, new LogicDictionaryData.DictionaryItemEntries { ID = i.Key, Name = NiceName, MaxAmountInWorld = MaxCount, ItemTypes = new string[] { "item" } });
            }

            void AddItemManual(string ID, string NiceName = null, int Count = -1, string[] ItemSubTypes = null)
            {
                var ItemEntry = new LogicDictionaryData.DictionaryItemEntries { ID = ID, Name = NiceName??ID, MaxAmountInWorld = Count, ItemTypes = new string[] { "item" } };
                if (ItemSubTypes is not null) { ItemEntry.ItemTypes = ItemEntry.ItemTypes.Concat(ItemSubTypes).ToArray(); }
                PMRDict.ItemList.Add(ID, ItemEntry);
            }

            AddItemManual("StarPiece", "Starpiece", ItemSubTypes: new string[] { "StarPiece" });
            AddItemManual("PowerStar");
            AddItemManual("ThreeStarPieces");
            AddItemManual("BlueBerry");
            AddItemManual("RedBerry");
            AddItemManual("YellowBerry");
            AddItemManual("BubbleBerry");
            AddItemManual("BowserCastleKey", Count: 5);
            AddItemManual("KoopaFortressKey", Count: 4);
            AddItemManual("RuinsKey", Count: 4);
            AddItemManual("TubbaCastleKey", Count: 3);

            foreach(var Item in PsuedoSTayceTLocations.Values.Select(x => x.Item4).Distinct())
            {
                PMRDict.ItemList[Item].ItemTypes = PMRDict.ItemList[Item].ItemTypes.Concat(new string[] { $"Cook {Item}" }).ToArray();
            }
            PMRDict.ItemList["DriedPasta"].ItemTypes = PMRDict.ItemList["DriedPasta"].ItemTypes.Concat(new string[] { "Outpost DriedPasta" }).ToArray();

            AddVariableList("Letters", PMRDict.ItemList.Where(x => x.Key.StartsWith("Letter")).Select(x => x.Key).ToList());
            AddVariableList("Partners", new List<string>() { "Goombario", "Kooper", "Bombette", "Parakarry", "Bow", "Watt", "Sushie", "Lakilester" });
            AddVariableList("starpieces", new List<string>() { "StarPiece", "ThreeStarPieces", "ThreeStarPieces", "ThreeStarPieces" });
            AddVariableList("starspirits", PMRDict.ItemList.Where(x => x.Key.StartsWith("STARSPIRIT")).Select(x => x.Key).ToList());
            AddVariableList("MagicalSeeds", PMRDict.ItemList.Where(x => x.Key.StartsWith("MagicalSeed")).Select(x => x.Key).ToList());
            void AddVariableList(string ID, List<string> Variables)
            {
                PMRDict.Variables.Add(ID, new OptionData.TrackerVar { ID = ID, Static = true, Value = Variables });
            }

            var BrokenStarbornValleyPanel = PRMLogic.Logic.First(x => x.Id =="Path to Starborn Valley - Hidden Panel");
            PRMLogic.Logic.Remove(BrokenStarbornValleyPanel);

            PRMLogic.Logic.First(x => x.Id =="RF_BeatGoombaKing").ConditionalItems.Add(new List<string> { "setting{PrologueOpen}" });
            PRMLogic.Logic.First(x => x.Id =="GF_MAC02_UnlockedHouse").ConditionalItems.Add(new List<string> { "setting{BlueHouseOpen}" });
            PRMLogic.Logic.First(x => x.Id =="GF_MAC03_BombedRock").ConditionalItems.Add(new List<string> { "setting{MtRuggedOpen}" });
            PRMLogic.Logic.First(x => x.Id =="RF_ForestPass").ConditionalItems.Add(new List<string> { "setting{ForeverForestOpen}" });
            PRMLogic.Logic.First(x => x.Id =="RF_CanRideWhale").ConditionalItems.Add(new List<string> { "setting{WhaleOpen}" });
            PRMLogic.Logic.First(x => x.Id =="RF_BuiltCh7Bridge").ConditionalItems.Add(new List<string> { "setting{Ch7BridgeVisible}" });
            PRMLogic.Logic.First(x => x.Id =="RF_CanCook").ConditionalItems.Add(new List<string> { "setting{CookWithoutFryingPan}" });

            MapPoint[] StartingMaps = new MapPoint[]
            {
                new MapPoint{ id = 4, map = "MAC_00" },
                new MapPoint{ id = 1, map = "KMR_02" },
                new MapPoint{ id = 0, map = "DRO_02" },
                new MapPoint{ id = 2, map = "JAN_03" },
            };
            Dictionary<string, string> StaringMapSetting = new Dictionary<string, string>
            {
                { "65796", "Toad Town" },
                { "257", "Goomba Village" },
                { "590080", "Dry Dry Outpost" },
                { "1114882", "Yoshi Village" }
            };
            foreach(var map in StartingMaps)
            {
                string Destination = map.GetVerboseName(MapNames, MapMeta, MapNodes);
                string ID = $"Spawn => {Destination}";
                PMRDict.EntranceList.Add(ID, new LogicDictionaryData.DictionaryEntranceEntries
                {
                    AlwaysAccessable = true,
                    ID = ID,
                    Area = "Spawn",
                    Exit = Destination
                });
                PRMLogic.Logic.Add(new MMRData.JsonFormatLogicItem { Id = ID, RequiredItems = new List<string> { $"setting{{StartingMap, {StaringMapSetting.Keys.ToArray()[Array.IndexOf(StartingMaps, map)]}}}" } });
            };


            var StartingLocationSetting = new OptionData.TrackerOption { ID = "StartingMap", DisplayName = "Starting Map", CurrentValue = "65796" };
            foreach(var map in StaringMapSetting) { StartingLocationSetting.Values.Add(map.Key, new OptionData.actions { Name = map.Value }); }
            PMRDict.Options.Add("StartingMap", StartingLocationSetting);

            var FlowerFieldsDoorLogic = PRMLogic.Logic.First(x => x.Id == "Toad Town - Plaza District - Exit West => Toad Town - Plaza District - Flower Fields Door");
            FlowerFieldsDoorLogic.ConditionalItems.Clear();
            FlowerFieldsDoorLogic.RequiredItems = new List<string> { "MagicalSeeds, MagicalSeedsRequired" };

            PRMLogic.Logic.Add(new MMRData.JsonFormatLogicItem { Id = "RF_Missable" });
            PRMLogic.Logic.Add(new MMRData.JsonFormatLogicItem { Id = "RF_OutOfLogic" });

            PMRDict.Variables.Add("MagicalSeedsRequired", new OptionData.TrackerVar { ID = "MagicalSeedsRequired", Static = false, Name = "Magical Seeds Required", Value = 4 });

            AddToggleoption("BlueHouseOpen");
            AddToggleoption("Ch7BridgeVisible");
            AddToggleoption("CookWithoutFryingPan");
            AddToggleoption("ForeverForestOpen");
            AddToggleoption("HiddenBlockMode");
            AddToggleoption("MtRuggedOpen");
            AddToggleoption("PartnersAlwaysUsable");
            AddToggleoption("PrologueOpen");
            AddToggleoption("ToyboxOpen");
            AddToggleoption("WhaleOpen");

            void AddToggleoption(string ID, string defval = "false", string Display = null)
            {
                var option = new OptionData.TrackerOption { ID = ID, DisplayName = Display??ID, CurrentValue = defval };
                option.CreateSimpleValues(new string[] {"true", "false"});
                PMRDict.Options.Add(ID, option);
            }

            File.WriteAllText(Path.Combine(TestingFoler, "PMR v1.json"), JsonConvert.SerializeObject(PMRDict, Testing._NewtonsoftJsonSerializerOptions));
            File.WriteAllText(Path.Combine(TestingFoler, "PMRLogic.json"), JsonConvert.SerializeObject(PRMLogic, Testing._NewtonsoftJsonSerializerOptions));

            List<Tuple<string, int>> Reqs = new List<Tuple<string, int>>();

            outDict = PMRDict;
            OutLogic = PRMLogic;

        }

    }
}
