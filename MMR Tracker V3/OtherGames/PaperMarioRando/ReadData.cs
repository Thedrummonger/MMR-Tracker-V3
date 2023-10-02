using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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

            Dictionary<string, string> MapNames = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(AreaName));
            Dictionary<string, string> MapMeta = JsonConvert.DeserializeObject<List<MapMeta>>(File.ReadAllText(MapMetaFile)).ToDictionary(x => x.name, x => x.verbose_name);
            Dictionary<string, string> MapNodes = JsonConvert.DeserializeObject<List<MapNode>>(File.ReadAllText(MapNodesFile)).Where(x => x.entrance_name is not null).ToDictionary(x => x.identifier, x => x.entrance_name);
            Dictionary<string, Dictionary<string, string>> Locations = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(LocationsFile));
            List<ItemFileData> Items = JsonConvert.DeserializeObject<List<ItemFileData>>(File.ReadAllText(ItemsFile)).Where(x => x.unplaceable == 0).ToList();
            Dictionary<string, string> ItemNames = JsonConvert.DeserializeObject<Dictionary<string,string>>(File.ReadAllText(ItemsNamesFile));

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

            Dictionary<string, string> PsuedoMacros = new Dictionary<string, string>();
            Dictionary<string, string> ExitLogic = new Dictionary<string, string>();
            foreach(var edge in Edges)
            {
                string FromID = $"{edge.from.GetVerboseName(MapNames, MapMeta, MapNodes)}";
                string ToID = $"{edge.to.GetVerboseName(MapNames, MapMeta, MapNodes)}";

                if (edge.pseudoitems is not null && edge.pseudoitems.Any())
                {
                    foreach(var item in edge.pseudoitems)
                    {
                        if (item.StartsWith("GF") || item.StartsWith("MF") || item.StartsWith("MB") || item.StartsWith("RF") || item.StartsWith("FAVOR"))
                        {
                            string Logic = GetEdgeLogic(edge, new List<List<object>> { new List<object> { ToID } });

                            if (!PsuedoMacros.ContainsKey(item))
                            {
                                PsuedoMacros.Add(item, Logic);
                            }
                            else
                            {
                                PsuedoMacros[item] = $"{PsuedoMacros[item]} || {Logic}";
                            }
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

            foreach(var i in PsuedoMacros)
            {
                PRMLogic.Logic.Add(new MMRData.JsonFormatLogicItem { Id = i.Key, ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(PMRParser, i.Value, i.Key) });
            }

            foreach(var i in ExitLogic)
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

            foreach(var i in ItemNames)
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

            void AddItemManual(string ID, string NiceName = null, int Count = -1)
            {
                PMRDict.ItemList.Add(ID, new LogicDictionaryData.DictionaryItemEntries { ID = ID, Name = NiceName??ID, MaxAmountInWorld = Count, ItemTypes = new string[] { "item" } });
            }

            AddItemManual("StarPiece", "Starpiece");
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

            File.WriteAllText(Path.Combine(TestingFoler, "PMR v1.json"), JsonConvert.SerializeObject(PMRDict, Testing._NewtonsoftJsonSerializerOptions));
            File.WriteAllText(Path.Combine(TestingFoler, "PMRLogic.json"), JsonConvert.SerializeObject(PRMLogic, Testing._NewtonsoftJsonSerializerOptions));

            List<Tuple<string, int>> Reqs = new List<Tuple<string, int>>();

            outDict = PMRDict;
            OutLogic = PRMLogic;

        }

    }
}
