using MMR_Tracker_V3.DataStructure;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.GameDataCreation.OOTMMV2.OOTMMUtil;
using static TestingForm.GameDataCreation.OOTMMV2.datamodel;

namespace MMR_Tracker_V3.GameDataCreation.OOTMMV2
{
    internal class LogicDictionaryCreation
    {
        public static LogicDictionaryData.LogicDictionary CreateDictionary(OOTMMParserData OTTMMPaths)
        {

            LogicDictionaryData.LogicDictionary logicDictionaryData = new LogicDictionaryData.LogicDictionary() { GameCode = "OOTMM", RootArea = "OOT SPAWN", LogicVersion = 2 };

            List<MMROOTLocation> OOTPool = JsonConvert.DeserializeObject<List<MMROOTLocation>>(Utility.ConvertCsvFileToJsonObject(File.ReadAllLines(OTTMMPaths.OOTPoolFile)));
            List<MMROOTLocation> MMPool = JsonConvert.DeserializeObject<List<MMROOTLocation>>(Utility.ConvertCsvFileToJsonObject(File.ReadAllLines(OTTMMPaths.MMPoolFile)));

            List<MMROOTHintData> OOTGossips = JsonConvert.DeserializeObject<List<MMROOTHintData>>(Utility.ConvertCsvFileToJsonObject(File.ReadAllLines(OTTMMPaths.OOTHintFile)));
            List<MMROOTHintData> MMGossips = JsonConvert.DeserializeObject<List<MMROOTHintData>>(Utility.ConvertCsvFileToJsonObject(File.ReadAllLines(OTTMMPaths.MMHintFile)));

            Dictionary<string, string> TrickList = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(OTTMMPaths.TricksFile));
            List<string> ItemList = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(OTTMMPaths.ItemsFile));
            Dictionary<string, string> ItemNames = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(OTTMMPaths.ItemNamesFile));

            Createlocations(logicDictionaryData, OOTPool, "OOT", OTTMMPaths);
            Createlocations(logicDictionaryData, MMPool, "MM", OTTMMPaths);
            AddTricks(logicDictionaryData, TrickList);
            AddItems(logicDictionaryData, ItemList, ItemNames);
            CreateEntranceConnections(OTTMMPaths, logicDictionaryData);

            AddGossips(logicDictionaryData, "OOT", OOTGossips);
            AddGossips(logicDictionaryData, "MM", MMGossips);

            return logicDictionaryData;

            static void Createlocations(LogicDictionaryData.LogicDictionary logicDictionaryData, List<MMROOTLocation> Pool, string GameCode, OOTMMParserData OTTMMPaths)
            {
                foreach (var i in Pool)
                {
                    string LocationID = $"{GameCode} {i.location}";

                    string OriginalItem = $"{GameCode}_{i.item}"; 
                    if (new string[] { 
                        "OOT_NOTHING", 
                        "OOT_FLEXIBLE", 
                        "OOT_MAGIC_JAR_SMALL",
                        "OOT_MAGIC_JAR_LARGE",
                        "OOT_RANDOM",
                        "MM_RANDOM", 
                        "MM_MAGIC_JAR_SMALL", 
                        "MM_MAGIC_JAR_LARGE",
                        "MM_ARROWS_20",
                        "MM_NOTHING"
                    }.Contains(OriginalItem)) { OriginalItem = "SHARED_RECOVERY_HEART"; }

                    if (!OTTMMPaths.LocationAreas.ContainsKey(LocationID)) { Debug.WriteLine($"Location {LocationID} Was not found in logic file. Skipping."); continue; }

                    LogicDictionaryData.DictionaryLocationEntries LocationEntry = new LogicDictionaryData.DictionaryLocationEntries()
                    {
                        Area = "UNKNOWN",
                        ID = LocationID,
                        Name = LocationID,
                        OriginalItem = OriginalItem,
                        Repeatable = IsLocationRenewable($"{GameCode} {i.location}", i.type),
                        ValidItemTypes = new string[] { "item", i.type },
                        WalletCurrency = GameCode == "MM" ? 'M' : 'O',
                        SpoilerData = new MMRData.SpoilerlogReference()
                        {
                            SpoilerLogNames= new string[] { $"{GameCode} {i.location}" }
                        }
                    };
                    logicDictionaryData.LocationList.Add($"{GameCode} {i.location}", LocationEntry);
                }
            }

            static void AddTricks(LogicDictionaryData.LogicDictionary logicDictionaryData, Dictionary<string, string> TrickList)
            {
                foreach (var trick in TrickList)
                {
                    string TrickID = $"TRICK_{trick.Key}";
                    logicDictionaryData.AdditionalLogic.Add(new MMRData.JsonFormatLogicItem { Id = TrickID, IsTrick = true, TrickCategory = GetGamecodeFromID(trick.Key) });
                    logicDictionaryData.MacroList.Add(TrickID, new LogicDictionaryData.DictionaryMacroEntry() { ID = TrickID, Name = trick.Value });
                }
            }

            static void AddGossips(LogicDictionaryData.LogicDictionary logicDictionaryData, string GameCode, List<MMROOTHintData> GossipList)
            {
                foreach (var Gossip in GossipList)
                {
                    string GossipID = $"{GameCode} {Gossip.location}";
                    logicDictionaryData.HintSpots.Add(GossipID, new LogicDictionaryData.DictionaryHintEntries() { ID = GossipID, Name = GossipID });
                }
            }

            static void AddItems(LogicDictionaryData.LogicDictionary logicDictionaryData, List<string> ItemList, Dictionary<string, string> ItemNames)
            {
                foreach (var i in ItemList)
                {
                    if (logicDictionaryData.ItemList.ContainsKey(i)) { continue; }
                    LogicDictionaryData.DictionaryItemEntries ItemEntry = new LogicDictionaryData.DictionaryItemEntries()
                    {
                        ID = i,
                        Name = GetItemNiceName(i, ItemNames),
                        MaxAmountInWorld = -1,
                        ItemTypes = new string[] { "item" },
                        ValidStartingItem = true,
                        SpoilerData = new MMRData.SpoilerlogReference()
                        {
                            SpoilerLogNames = new string[] { i, GetItemNiceName(i, ItemNames) }
                        }
                    };
                    logicDictionaryData.ItemList.Add(i, ItemEntry);
                }
            }

            static void CreateEntranceConnections(OOTMMParserData OTTMMPaths, LogicDictionaryData.LogicDictionary logicDictionaryData)
            {
                var OOTEntrances = JsonConvert.DeserializeObject<List<MMROOTEntranceData>>(Utility.ConvertCsvFileToJsonObject(File.ReadAllLines(OTTMMPaths.OOTEntrancesFile)));
                var MMEntrances = JsonConvert.DeserializeObject<List<MMROOTEntranceData>>(Utility.ConvertCsvFileToJsonObject(File.ReadAllLines(OTTMMPaths.MMEntrancesFile)));

                foreach (var Connection in OTTMMPaths.AreaConnections)
                {
                    LogicDictionaryData.DictionaryEntranceEntries entranceEntries = new LogicDictionaryData.DictionaryEntranceEntries
                    {
                        Area = Connection.Value.Area,
                        Exit = Connection.Value.Exit,
                        ID = Connection.Key,
                    };

                    var OOTEntrancePoolEntry = OOTEntrances.FirstOrDefault(x => $"OOT {x.from}" == entranceEntries.Area && $"OOT {x.to}" == entranceEntries.Exit);
                    var MMEntrancePoolEntry = MMEntrances.FirstOrDefault(x => $"MM {x.from}" == entranceEntries.Area && $"MM {x.to}" == entranceEntries.Exit);
                    if (OOTEntrancePoolEntry is not null)
                    {
                        entranceEntries.SpoilerData = new MMRData.SpoilerlogReference { SpoilerLogNames = new string[] { "OOT_" + OOTEntrancePoolEntry.id } };
                        entranceEntries.RandomizableEntrance = true;
                        if (!string.IsNullOrWhiteSpace(OOTEntrancePoolEntry.reverse) && OOTEntrancePoolEntry.reverse != "NONE")
                        {
                            var ReverseEntrance = OOTEntrances.First(x => x.id == OOTEntrancePoolEntry.reverse);
                            entranceEntries.EntrancePairID = new EntranceData.EntranceAreaPair { Area = $"OOT {ReverseEntrance.from}", Exit = $"OOT {ReverseEntrance.to}" };
                        }
                    }
                    else if (MMEntrancePoolEntry is not null)
                    {
                        entranceEntries.SpoilerData = new MMRData.SpoilerlogReference { SpoilerLogNames = new string[] { "MM_" + MMEntrancePoolEntry.id } };
                        entranceEntries.RandomizableEntrance = true;
                        if (!string.IsNullOrWhiteSpace(MMEntrancePoolEntry.reverse) && MMEntrancePoolEntry.reverse != "NONE")
                        {
                            var ReverseEntrance = MMEntrances.First(x => x.id == MMEntrancePoolEntry.reverse);
                            entranceEntries.EntrancePairID = new EntranceData.EntranceAreaPair { Area = $"MM {ReverseEntrance.from}", Exit = $"MM {ReverseEntrance.to}" };
                        }
                    }

                    logicDictionaryData.EntranceList.Add(Connection.Key, entranceEntries);
                }
            }
        }
    }
}
