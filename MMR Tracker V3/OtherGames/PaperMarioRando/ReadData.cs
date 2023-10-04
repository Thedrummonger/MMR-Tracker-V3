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

        public string GetFullEdgeID(Dictionary<string, string> MapNames, Dictionary<string, string> MapMeta, Dictionary<string, string> Nodes)
        {
            return $"{from.GetVerboseName(MapNames, MapMeta, Nodes)} => {to.GetVerboseName(MapNames, MapMeta, Nodes)}";
        }

        public override bool Equals(object obj)
        {
            if (obj is not PMREdge dest) { return false; }
            return this.to.id.ToString() == dest.to.id.ToString() && this.to.map == dest.to.map && this.from.id.ToString() == dest.from.id.ToString() && this.from.map == dest.from.map;
        }
    }

    public class ItemReplacement
    {
        public string id;
        public string name;
        public int maxamount;
        public List<string> Types = new List<string> { "Item" };
    }

    public class EdgeAlteration
    {
        public List<PMREdge> additions;
        public List<PMREdge> removals;
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
        public int? vanilla_item_id;
    }
    public class ItemFileData
    {
        public int ID;
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
        public static LogicStringParser PMRParser = new LogicStringParser(containerType: LogicStringParser.ContainerType.bracket);
        private static string GetEdgeLogic(PMREdge edge, List<List<object>> AdditionalReq = null)
        {
            List<List<object>> Req = new List<List<object>>();
            Req.AddRange(edge.reqs.Select(x => x.ToList()).ToList());
            if (AdditionalReq is not null && AdditionalReq.Any()) { Req.AddRange(AdditionalReq); }
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
            string EdgeAlterationsFile = Path.Combine(PMRFiles, "Edges_alterations.json");
            string LocationTagsFile = Path.Combine(PMRFiles, "SettingLocationRestrictions.json");

            Dictionary<string, string> MapNames = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(AreaName));
            Dictionary<string, string> MapMeta = JsonConvert.DeserializeObject<List<MapMeta>>(File.ReadAllText(MapMetaFile)).ToDictionary(x => x.name, x => x.verbose_name);
            Dictionary<string, string> MapNodes = JsonConvert.DeserializeObject<List<MapNode>>(File.ReadAllText(MapNodesFile)).Where(x => x.entrance_name is not null).ToDictionary(x => x.identifier, x => x.entrance_name);
            Dictionary<string, Dictionary<string, string>> Locations = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(LocationsFile));
            List<ItemFileData> Items = JsonConvert.DeserializeObject<List<ItemFileData>>(File.ReadAllText(ItemsFile)).Where(x => x.unplaceable == 0).ToList();
            Dictionary<string, string> ItemNames = JsonConvert.DeserializeObject<Dictionary<string,string>>(File.ReadAllText(ItemsNamesFile));
            Dictionary<string, string> Macros = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(MacroFile));
            List<MapNode> ItemLocationNodes = JsonConvert.DeserializeObject<List<MapNode>>(File.ReadAllText(MapNodesFile)).Where(x => x.vanilla_item_id is not null).ToList();
            Dictionary<string, List<string>> LocationTags = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText(LocationTagsFile));

            string TestingFoler = Path.Combine(References.TestingPaths.GetDevTestingPath(), "PMRTesting");

            var Edges = JsonConvert.DeserializeObject<List<PMREdge>>(File.ReadAllText(EdgesFile));
            var EdgeAlterations = JsonConvert.DeserializeObject<Dictionary<string, EdgeAlteration>>(File.ReadAllText(EdgeAlterationsFile));

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
            Dictionary<string, Tuple<string, string>> EntrancesToAdd = new Dictionary<string, Tuple<string, string>>();
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
                List<List<object>> AdditionalRequirements = new List<List<object>>();
                if (EdgeAlterations["bc_shorten"].removals.Any(x => x.Equals(edge))) { AdditionalRequirements.Add(new List<object> { "setting{BowsersCastleMode, 1, false}" }); }
                if (EdgeAlterations["bc_bossrush"].removals.Any(x => x.Equals(edge))) { AdditionalRequirements.Add(new List<object> { "setting{BowsersCastleMode, 2, false}" }); }
                if (EdgeAlterations["gear_location_shuffle"].removals.Any(x => x.Equals(edge))) { AdditionalRequirements.Add(new List<object> { "setting{BowsersCastleMode, 0}" }); }
                if (EdgeAlterations["star_hunt"].removals.Any(x => x.Equals(edge))) { AdditionalRequirements.Add(new List<object> { "setting{StarHunt, false}" }); }
                ScanEdge(edge, AdditionalRequirements);
            }
            foreach (var edge in EdgeAlterations["bc_shorten"].additions)
            {
                ScanEdge(edge, new List<List<object>> { new List<object> { "setting{BowsersCastleMode, 1}" } });
            }
            foreach (var edge in EdgeAlterations["bc_bossrush"].additions)
            {
                ScanEdge(edge, new List<List<object>> { new List<object> { "setting{BowsersCastleMode, 2}" } });
            }
            foreach (var edge in EdgeAlterations["gear_location_shuffle"].additions)
            {
                ScanEdge(edge, new List<List<object>> { new List<object> { "setting{GearShuffleMode, 0, false}" } });
            }
            foreach (var edge in EdgeAlterations["star_hunt"].additions)
            {
                ScanEdge(edge, new List<List<object>> { new List<object> { "setting{StarHunt}" } });
            }

            void ScanEdge(PMREdge edge, List<List<object>> AdditionalReq = null)
            {

                string FromID = $"{edge.from.GetVerboseName(MapNames, MapMeta, MapNodes, out string _, out string MapName, out string CurrentAreaName, out _)}";
                string ToID = $"{edge.to.GetVerboseName(MapNames, MapMeta, MapNodes)}";

                List<List<object>> extraReqs = new List<List<object>> { new List<object> { FromID } };
                if (AdditionalReq is not null && AdditionalReq.Any()) { extraReqs = extraReqs.Concat(AdditionalReq).ToList(); }

                string EdgeLogic = GetEdgeLogic(edge, extraReqs);

                if (edge.pseudoitems is not null && edge.pseudoitems.Any())
                {
                    foreach (var item in edge.pseudoitems)
                    {
                        if (item.StartsWith("GF") || item.StartsWith("MF") || item.StartsWith("MB") || item.StartsWith("RF") || item.StartsWith("FAVOR") || item.StartsWith("YOUWIN"))
                        {
                            if (!PsuedoMacros.ContainsKey(item)) { PsuedoMacros.Add(item, EdgeLogic); }
                            else { PsuedoMacros[item] = $"{PsuedoMacros[item]} || {EdgeLogic}"; }
                        }
                        else if (item.StartsWith("StarPiece"))
                        {
                            if (!QuizmoSPtracking.ContainsKey(MapName)) { QuizmoSPtracking[MapName] = 1; }
                            string CheckName = $"Quizmo - Starpiece {QuizmoSPtracking[MapName]}";
                            if (!PsuedoStarpieceLocations.ContainsKey(item)) { PsuedoStarpieceLocations.Add(item, new(CheckName, MapName, EdgeLogic)); QuizmoSPtracking[MapName]++; }
                            else { PsuedoStarpieceLocations[item] = new(PsuedoStarpieceLocations[item].Item1, MapName, $"{PsuedoStarpieceLocations[item].Item3} || {EdgeLogic}"); }
                        }
                        else if (item.StartsWith("STARSPIRIT"))
                        {
                            string CheckID = $"{CurrentAreaName} - Star Spirit";
                            PsuedoStarSpiritLocations.Add(item, new(CheckID, MapName, EdgeLogic));
                        }
                        else if (item == "MysticalKey")
                        {
                            string CheckID = $"{CurrentAreaName} - Mystical Key Chest";
                            MysticalKeyData = new(CheckID, MapName, EdgeLogic);
                        }
                        else if (FromID == "Toad Town - Southern District - Exit West" && FromID == ToID)
                        {
                            string RecipeResult = item;
                            List<string> Ingredients = new List<string>();
                            foreach (var i in edge.reqs.SelectMany(x => x)) { if (i.ToString() != "RF_CanCook" && i.ToString() != "Cookbook") { Ingredients.Add(i.ToString()); } }
                            string ID = $"Tayce T Cook {RecipeResult} ({string.Join(", ", Ingredients)})";
                            PsuedoSTayceTLocations.Add(ID, new(ID, MapName, EdgeLogic, RecipeResult));
                        }
                        else if (item == "DriedPasta" && ToID == "Dry Dry Outpost - Outpost 1 - ShopItemA")
                        {
                            DryOutpostDriedpastaData = new("Outpost 1 - Shop Item 4", MapName, EdgeLogic);
                        }
                        else
                        {
                            Debug.WriteLine($"Handle Psuedoitem {item}\n{ToID}");
                        }
                    }
                }

                if (FromID == ToID) { return; }
                string EntranceID = $"{FromID} => {ToID}";

                if (!ExitLogic.ContainsKey(EntranceID))
                {
                    EntrancesToAdd.Add(EntranceID, new(FromID, ToID));
                    ExitLogic.Add(EntranceID, EdgeLogic);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(ExitLogic[EntranceID])) { ExitLogic[EntranceID] = EdgeLogic; }
                    else if (!string.IsNullOrWhiteSpace(EdgeLogic)) { ExitLogic[EntranceID] = $"{ExitLogic[EntranceID]} || {EdgeLogic}"; }
                }
            }

            foreach(var ent in EntrancesToAdd)
            {
                PMRDict.EntranceList.Add(ent.Key, new LogicDictionaryData.DictionaryEntranceEntries { ID = ent.Key, Area = ent.Value.Item1, Exit = ent.Value.Item2 });
            }


            PMRDict.LocationList.Add(MysticalKeyData.Item1, new LogicDictionaryData.DictionaryLocationEntries { ID = MysticalKeyData.Item1, Name = MysticalKeyData.Item1, Area = MysticalKeyData.Item2, ValidItemTypes = new string[] { "MysticalKey" }, OriginalItem = "MysticalKey" });
            PRMLogic.Logic.Add(new MMRData.JsonFormatLogicItem { Id = MysticalKeyData.Item1, ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(PMRParser, MysticalKeyData.Item3, MysticalKeyData.Item1) });
            PMRDict.ItemList.Add("MysticalKey", new LogicDictionaryData.DictionaryItemEntries { ID = "MysticalKey", Name = "MysticalKey", ItemTypes = new string[] { "MysticalKey" } });

            PMRDict.LocationList.Add(DryOutpostDriedpastaData.Item1, new LogicDictionaryData.DictionaryLocationEntries { ID = DryOutpostDriedpastaData.Item1, Name = DryOutpostDriedpastaData.Item1, Area = DryOutpostDriedpastaData.Item2, ValidItemTypes = new string[] { "Outpost DriedPasta" }, OriginalItem = "DriedPasta" });
            PRMLogic.Logic.Add(new MMRData.JsonFormatLogicItem { Id = DryOutpostDriedpastaData.Item1, ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(PMRParser, DryOutpostDriedpastaData.Item3, DryOutpostDriedpastaData.Item1) });

            foreach (var i in PsuedoStarpieceLocations)
            {
                PMRDict.LocationList.Add(i.Key, new LogicDictionaryData.DictionaryLocationEntries { ID = i.Key, Name = i.Value.Item1, Area = i.Value.Item2, ValidItemTypes = new string[] { "StarPiece" }, OriginalItem = "StarPiece" });
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
                PMRDict.LocationList.Add(i.Value.Item1, new LogicDictionaryData.DictionaryLocationEntries { ID = i.Value.Item1, Name = i.Value.Item1, Area = i.Value.Item2, ValidItemTypes = new string[] { i.Key }, OriginalItem = i.Key });
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

            Dictionary<string, ItemReplacement> ItemOverrides = new Dictionary<string, ItemReplacement>()
            {
                { "StarPiece", new ItemReplacement{ id = "StarPiece", name = "Starpiece", maxamount = -1, Types = new List<string>{ "item", "StarPiece" }  } },
                { "PowerStar", new ItemReplacement{ id = "PowerStar", name = "PowerStar", maxamount = -1, Types = new List<string>{ "item" }  } },
                { "ThreeStarPieces", new ItemReplacement{ id = "ThreeStarPieces", name = "ThreeStarPieces", maxamount = -1, Types = new List<string>{ "item" }  } },
                { "BlueBerryProxy", new ItemReplacement{ id = "BlueBerry", name = "BlueBerry", maxamount = -1, Types = new List<string>{ "item" }  } },
                { "RedBerryProxy", new ItemReplacement{ id = "RedBerry", name = "RedBerry", maxamount = -1, Types = new List<string>{ "item" }  } },
                { "YellowBerryProxy", new ItemReplacement{ id = "YellowBerry", name = "YellowBerry", maxamount = -1, Types = new List<string>{ "item" }  } },
                { "BubbleBerryProxy", new ItemReplacement{ id = "BubbleBerry", name = "BubbleBerry", maxamount = -1, Types = new List<string>{ "item" }  } },
                { "BowserCastleKey", new ItemReplacement{ id = "BowserCastleKey", name = "BowserCastleKey", maxamount = 5, Types = new List<string>{ "item" }  } },
                { "KoopaFortressKey", new ItemReplacement{ id = "KoopaFortressKey", name = "KoopaFortressKey", maxamount = 5, Types = new List<string>{ "item" }  } },
                { "RuinsKey", new ItemReplacement{ id = "RuinsKey", name = "RuinsKey", maxamount = 4, Types = new List<string>{ "item" }  } },
                { "TubbaCastleKey", new ItemReplacement{ id = "TubbaCastleKey", name = "TubbaCastleKey", maxamount = 3, Types = new List<string>{ "item" }  } }
            };

            foreach (var i in ItemNames)
            {
                if (ItemOverrides.Keys.Any(x => i.Key.Contains(x))) { continue; }
                var ItemData = Items.FirstOrDefault(x => x.item_name == i.Key);
                int MaxCount = (ItemData is not null && ItemData.item_type == "ITEM") || i.Key == "Coin" ? -1 : 1;
                string NiceName = string.IsNullOrWhiteSpace(i.Value) ? i.Key : i.Value;
                PMRDict.ItemList.Add(i.Key, new LogicDictionaryData.DictionaryItemEntries { ID = i.Key, Name = NiceName, MaxAmountInWorld = MaxCount, ItemTypes = new string[] { "item" } });
            }

            void AddItemManual(string ID, string NiceName = null, int Count = -1, string[] ItemSubTypes = null)
            {
                var ItemEntry = new LogicDictionaryData.DictionaryItemEntries { ID = ID, Name = NiceName??ID, MaxAmountInWorld = Count, ItemTypes = new string[] { "item" } };
                if (ItemSubTypes is not null) { ItemEntry.ItemTypes = ItemEntry.ItemTypes.Concat(ItemSubTypes).ToArray(); }
                PMRDict.ItemList.Add(ID, ItemEntry);
            }
            foreach(var Override in ItemOverrides.Values) { AddItemManual(Override.id, Override.name, Override.maxamount, Override.Types.ToArray()); }

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
            PMRDict.Variables.Add("StarHuntRequired", new OptionData.TrackerVar { ID = "StarHuntRequired", Static = false, Name = "Required Power Stars", Value = 0 });

            AddToggleoption("BlueHouseOpen");
            AddToggleoption("Ch7BridgeVisible");
            AddToggleoption("CookWithoutFryingPan");
            AddToggleoption("ForeverForestOpen");
            AddToggleoption("MtRuggedOpen");
            AddToggleoption("PartnersAlwaysUsable");
            AddToggleoption("PrologueOpen");
            AddToggleoption("ToyboxOpen");
            AddToggleoption("WhaleOpen");
            AddToggleoption("StarHunt");
            AddToggleoption("StarHuntEndsGame");

            AddOption("HiddenBlockMode", "1", new List<Tuple<string, string>> { new("0", "vanilla"), new("1", "Watt out"), new("2", "Watt acquired"), new("3", "always visible") });
            AddOption("BowsersCastleMode", "0", new List<Tuple<string, string>> { new("0", "vanilla"), new("1", "Shorten"), new("2", "BossRush")});
            AddOption("GearShuffleMode", "0", new List<Tuple<string, string>> { new("0", "vanilla"), new("1", "location shuffle"), new("2", "full shuffle") });
            AddOption("MerlowRewardPricing", "1", new List<Tuple<string, string>> { new("0", "Cheap"), new("1", "Normal") });
            
            void AddOption(string ID, string Default, List<Tuple<string, string>> Options, string Display = null)
            {
                var option = new OptionData.TrackerOption { ID = ID, DisplayName = Display??ID, CurrentValue = Default };
                foreach(var item in Options) { option.Values.Add(item.Item1, new OptionData.actions { Name = item.Item2 }); }
                PMRDict.Options.Add(ID, option);
            }

            void AddToggleoption(string ID, string defval = "false", string Display = null)
            {
                var option = new OptionData.TrackerOption { ID = ID, DisplayName = Display??ID, CurrentValue = defval };
                option.CreateSimpleValues(new string[] {"true", "false"});
                PMRDict.Options.Add(ID, option);
            }

            Dictionary<string, string> RandomizableEntrances = new Dictionary<string, string>()
            {
                { "Koopa Region - Path to Fortress 2 - Exit East Lower", "Koopa Bros Fortress - Fortress Exterior - Exit West" },
                { "Dry Dry Desert - N3W1 Ruins Entrance - Temple Entrance", "Dry Dry Ruins - Entrance - Exit Left" },
                { "Gusty Gulch - Wasteland Ascent 2 - Exit Right", "Tubba's Castle - Outside Tubbas Castle - Exit Left" },
                { "Toad Town - Residental District - Spring to Shy Guy's Toybox", "Shy Guy's Toybox - BLU Station - Spring to Toad Town" },
                { "Shiver Region - Shiver Mountain Peaks - Exit East", "Crystal Palace - Entrance - Exit West" },
                { "Jade Jungle - Path to the Volcano - Exit Right", "Mt Lavalava - Volcano Entrance - Exit West (Volcano Entrance)" },
                { "Toad Town - Plaza District - Flower Fields Door", "Flower Fields - Center - Tree Door" }
            };

            //Exit from tubbas castle to the gultch doesn't exist but needs to for entrance rando consitancy. Shouldn't break anything?
            PRMLogic.Logic.Add(new MMRData.JsonFormatLogicItem { Id = "Tubba's Castle - Outside Tubbas Castle - Exit Left => Gusty Gulch - Wasteland Ascent 2 - Exit Right" });
            PMRDict.EntranceList.Add("Tubba's Castle - Outside Tubbas Castle - Exit Left => Gusty Gulch - Wasteland Ascent 2 - Exit Right", new LogicDictionaryData.DictionaryEntranceEntries
            {
                ID = "Tubba's Castle - Outside Tubbas Castle - Exit Left => Gusty Gulch - Wasteland Ascent 2 - Exit Right",
                Area = "Tubba's Castle - Outside Tubbas Castle - Exit Left",
                Exit = "Gusty Gulch - Wasteland Ascent 2 - Exit Right"
            });

            foreach (var entrance in RandomizableEntrances)
            {
                string EntranceID = $"{entrance.Key} => {entrance.Value}";
                string PairID = $"{entrance.Value} => {entrance.Key}";
                PMRDict.EntranceList[EntranceID].RandomizableEntrance = true;
                PMRDict.EntranceList[EntranceID].EntrancePairID = new EntranceData.EntranceAreaPair { Area = entrance.Value, Exit = entrance.Key };
                PMRDict.EntranceList[PairID].RandomizableEntrance = true;
                PMRDict.EntranceList[PairID].EntrancePairID = new EntranceData.EntranceAreaPair { Area = entrance.Key, Exit = entrance.Value };
            }

            foreach (var i in ItemLocationNodes)
            {
                var Data = i.identifier.Split("/");
                MapPoint ItemLocation = new MapPoint { map = Data[0], id = Data[1] };
                string AreaRequirement = ItemLocation.GetVerboseName(MapNames, MapMeta, MapNodes, out _, out string Currentmap, out string CurrentArea, out _);
                string CheckID = $"{CurrentArea} - {Locations[ItemLocation.map][ItemLocation.StringID()]}";
                var DictEntry = PMRDict.LocationList[CheckID.Replace("'", "")];
                var Item = Items.Find(x => x.ID == i.vanilla_item_id);

                string ItemID = Item.item_name;

                foreach (var Replacement in ItemOverrides)
                {
                    if (ItemID.Contains(Replacement.Key)) { ItemID = Replacement.Value.id; break; }
                }

                var ItemObject = PMRDict.ItemList.Values.FirstOrDefault(x => x.ID == ItemID || x.Name == ItemID);
                if (ItemObject is null) { throw new Exception($"{Item.item_name} is not valid"); }
                DictEntry.OriginalItem = ItemObject.ID;
            }
            foreach (var i in PMRDict.LocationList)
            {
                if (string.IsNullOrWhiteSpace(i.Value.OriginalItem)) { Debug.WriteLine($"{i.Key} Had not vanilla item"); }
            }

            foreach (var i in PRMLogic.Logic)
            {
                LogicUtilities.RemoveRedundantConditionals(i);
                LogicUtilities.MakeCommonConditionalsRequirements(i);
            }

            FixMerlowPrices("Shooting Star Summit - Merluvlee's House - Exit Left => Shooting Star Summit - Merluvlee's House - ShopRewardA", 1);
            FixMerlowPrices("Shooting Star Summit - Merluvlee's House - ShopRewardA => Shooting Star Summit - Merluvlee's House - ShopRewardB", 2);
            FixMerlowPrices("Shooting Star Summit - Merluvlee's House - ShopRewardB => Shooting Star Summit - Merluvlee's House - ShopRewardC", 3);
            FixMerlowPrices("Shooting Star Summit - Merluvlee's House - ShopRewardC => Shooting Star Summit - Merluvlee's House - ShopRewardD", 4);
            FixMerlowPrices("Shooting Star Summit - Merluvlee's House - ShopRewardD => Shooting Star Summit - Merluvlee's House - ShopRewardE", 5);
            FixMerlowPrices("Shooting Star Summit - Merluvlee's House - ShopRewardE => Shooting Star Summit - Merluvlee's House - ShopRewardF", 6);

            void FixMerlowPrices(string ID, int ind)
            {
                var Entry = PRMLogic.Logic.Find(x => x.Id == ID);
                Entry.RequiredItems = Entry.RequiredItems.Where(x => !x.StartsWith("starpieces")).ToList();
                if (Entry.ConditionalItems.Any()) { throw new Exception($"{ID} Already had conditionals, can't edit Merlow Reqs"); }
                Entry.ConditionalItems.Add(new List<string> { $"starpieces, {ind*5}", "setting{MerlowRewardPricing, 0}" });
                Entry.ConditionalItems.Add(new List<string> { $"starpieces, {ind*10}", "setting{MerlowRewardPricing, 1}" });
            }

            foreach(var tag in LocationTags)
            {
                foreach(var i in tag.Value)
                {
                    var Data = i.Split("/");
                    MapPoint ItemLocation = new MapPoint { map = Data[0], id = Data[1] };
                    ItemLocation.GetVerboseName(MapNames, MapMeta, MapNodes, out _, out _, out string CurrentArea, out _);
                    string CheckID = $"{CurrentArea} - {Locations[ItemLocation.map][ItemLocation.StringID()]}".Replace("'","");
                    PMRDict.LocationList[CheckID].SpoilerData.SpoilerLogNames = PMRDict.LocationList[CheckID].SpoilerData.SpoilerLogNames.Concat(new string[] { tag.Key }).ToArray();
                }
            }


            File.WriteAllText(Path.Combine(TestingFoler, "PMR v1.json"), JsonConvert.SerializeObject(PMRDict, Testing._NewtonsoftJsonSerializerOptions));
            File.WriteAllText(Path.Combine(TestingFoler, "PMRLogic.json"), JsonConvert.SerializeObject(PRMLogic, Testing._NewtonsoftJsonSerializerOptions));

            List<Tuple<string, int>> Reqs = new List<Tuple<string, int>>();

            outDict = PMRDict;
            OutLogic = PRMLogic;

        }

    }
}
