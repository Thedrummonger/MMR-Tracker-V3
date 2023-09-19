using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.OtherGames.OOTMMV2.datamodel;
using static MMR_Tracker_V3.OtherGames.OOTMMV2.OOTMMUtil;

namespace MMR_Tracker_V3.OtherGames.OOTMMV2
{
    internal class LogicFileCreation
    {
        public static LogicStringParser OOTMMLogicStringParser = new LogicStringParser();
        public static MMRData.LogicFile ReadAndParseLogicFile(OOTMMParserData OTTMMPaths)
        {
            var TEMPAreaConnections = new Dictionary<string, AreaConnections>();
            var TEMPlocationAreas = new Dictionary<string, string>();

            MMRData.LogicFile LogicFile = new MMRData.LogicFile() { GameCode = "OOTMM", Version = 2 };
            LogicFile.Logic = new List<MMRData.JsonFormatLogicItem>();

            string[] OOTWorldFiles = Directory.GetFiles(OTTMMPaths.OOTWorld);
            string[] OOTMQWorldFiles = Directory.GetFiles(OTTMMPaths.OOTMQWorld);
            string[] MMWorldFiles = Directory.GetFiles(OTTMMPaths.MMWorld);

            AddFileData(OOTWorldFiles, "OOT");
            AddFileData(OOTMQWorldFiles, "OOT", true);
            AddFileData(MMWorldFiles, "MM");

            AddMacroFile(OTTMMPaths.OOTMacroFile, "OOT");
            AddMacroFile(OTTMMPaths.MMMacroFile, "MM");
            AddMacroFile(OTTMMPaths.SHAREDMacroFile, "OOT");
            AddMacroFile(OTTMMPaths.SHAREDMacroFile, "MM");

            OTTMMPaths.AreaConnections = TEMPAreaConnections;
            OTTMMPaths.LocationAreas = TEMPlocationAreas;
            return LogicFile;

            void AddMacroFile(string MacroFile, string GameCode)
            {
                var FileOBJ = JsonConvert.DeserializeObject<Dictionary<string, string>>(Utility.ConvertYamlStringToJsonString(File.ReadAllText(MacroFile)));
                foreach (var item in FileOBJ)
                {
                    if (LogicEditing.IsLogicFunction(item.Key, out string Func, out _, new('(', ')')))
                    {
                        Debug.WriteLine($"Skipping Function Macro {item.Key}");
                        continue;
                    }
                    string ID = $"{GameCode}_{item.Key}";
                    List<List<string>> ConditionalLogic = LogicStringConverter.ConvertLogicStringToConditional(OOTMMLogicStringParser, item.Value, ID);
                    MMRData.JsonFormatLogicItem NewLogic = new() { Id = ID, ConditionalItems = ConditionalLogic };
                    LogicUtilities.RemoveRedundantConditionals(NewLogic);
                    LogicFile.Logic.Add(NewLogic);
                }
            }

            void AddFileData(string[] files, string GameCode, bool MQ = false)
            {
                foreach (var WorldFile in files)
                {
                    var FileOBJ = JsonConvert.DeserializeObject<Dictionary<string, MMROOTLogicEntry>>(Utility.ConvertYamlStringToJsonString(File.ReadAllText(WorldFile)));
                    foreach (var Area in FileOBJ)
                    {
                        foreach (var Location in Area.Value.locations)
                        {
                            string ID = $"{GameCode} {Location.Key}";
                            string LogicString = CheckForLogicOverride(Location.Value, ID);
                            List<List<string>> ConditionalLogic = LogicStringConverter.ConvertLogicStringToConditional(OOTMMLogicStringParser, LogicString, ID);
                            AddLogicEntry(LogicFile, ID, ConditionalLogic, WorldFile, Area);
                            TEMPlocationAreas[ID] = string.IsNullOrWhiteSpace(Area.Value.region) ? (string.IsNullOrWhiteSpace(Area.Value.dungeon) ? "Unknown" : $"{GameCode}_{Area.Value.dungeon}") : $"{GameCode}_{Area.Value.region}";
                            ScanForSafeMMLocations(Area.Key, GameCode, ID);
                        }
                        foreach (var Location in Area.Value.exits)
                        {
                            string ID = GetExitID(Area.Key, Location.Key, GameCode);
                            string LogicString = CheckForLogicOverride(Location.Value, ID);
                            List<List<string>> ConditionalLogic = LogicStringConverter.ConvertLogicStringToConditional(OOTMMLogicStringParser, LogicString, ID);
                            AddLogicEntry(LogicFile, ID, ConditionalLogic, WorldFile, Area);
                            var AreaConnectionData = ID.StringSplit(" => ").Select(x => x.Trim()).ToArray();
                            if (!TEMPAreaConnections.ContainsKey(ID)) { TEMPAreaConnections.Add(ID, new AreaConnections { Area = AreaConnectionData[0], Exit = AreaConnectionData[1] }); }
                            ScanForSafeMMLocations(Area.Key, GameCode, ID);
                        }
                        foreach (var Location in Area.Value.events)
                        {
                            string ID = $"{GameCode}_EVENT_{Location.Key}";
                            string LogicString = CheckForLogicOverride(Location.Value, ID);
                            List<List<string>> ConditionalLogic = LogicStringConverter.ConvertLogicStringToConditional(OOTMMLogicStringParser, LogicString, ID);
                            AddLogicEntry(LogicFile, ID, ConditionalLogic, WorldFile, Area);
                            ScanForSafeMMLocations(Area.Key, GameCode, ID);
                        }
                        foreach (var Gossip in Area.Value.gossip)
                        {
                            string ID = $"{GameCode} {Gossip.Key}";
                            string LogicString = CheckForLogicOverride(Gossip.Value, ID);
                            List<List<string>> ConditionalLogic = LogicStringConverter.ConvertLogicStringToConditional(OOTMMLogicStringParser, LogicString, ID);
                            AddLogicEntry(LogicFile, ID, ConditionalLogic, WorldFile, Area);
                            ScanForSafeMMLocations(Area.Key, GameCode, ID);
                        }
                    }
                }

                void ScanForSafeMMLocations(string Area, string GameCode, string ID)
                {
                    string[] SafeMMAreas = new string[] { "Clock Town", "Owl Clock Town", "Owl Milk Road", "Owl Swamp", "Owl Woodfall", "Owl Mountain", "Owl Snowhead", "Owl Great Bay", "Owl Zora Cape", "Owl Ikana", "Owl Stone Tower", "Ocean Spider House", "Swamp Spider House", "SOARING" };
                    if (GameCode == "MM")
                    {
                        OTTMMPaths.MMLogicEntries.Add(ID);
                        if (SafeMMAreas.Contains(Area)) { OTTMMPaths.MMSOTSafeLogicEntries.Add(ID); }
                    }
                }

                bool HasMQEquivilent(string WorldFile)
                {
                    return OOTMQWorldFiles.Any(x => Path.GetFileName(x) == Path.GetFileName(WorldFile).Replace(".yml", "_mq.yml"));
                }

                void AddLogicEntry(MMRData.LogicFile LogicFile, string ID, List<List<string>> ConditionalLogic, string WorldFile, KeyValuePair<string, MMROOTLogicEntry> Area)
                {
                    var Duplicates = LogicFile.Logic.FirstOrDefault(x => x.Id == ID);

                    foreach (var i in ConditionalLogic) { i.Add($"{GameCode} {Area.Key}"); }
                    if (!string.IsNullOrWhiteSpace(Area.Value.dungeon))
                    {
                        if (MQ) 
                        { 
                            foreach (var i in ConditionalLogic) 
                            { 
                                i.Add($"setting({Area.Value.dungeon}_MQ)"); 
                                if (!OTTMMPaths.DungeonLayouts.Contains($"{Area.Value.dungeon}_MQ")) { OTTMMPaths.DungeonLayouts.Add($"{Area.Value.dungeon}_MQ"); }
                            } 
                        }
                        if (!MQ && HasMQEquivilent(WorldFile)) 
                        { 
                            foreach (var i in ConditionalLogic) 
                            { 
                                i.Add($"setting({Area.Value.dungeon}_MQ, false)");
                                if (!OTTMMPaths.DungeonLayouts.Contains($"{Area.Value.dungeon}_MQ")) { OTTMMPaths.DungeonLayouts.Add($"{Area.Value.dungeon}_MQ"); }
                            } 
                        }
                    }

                    if (Duplicates is null)
                    {
                        MMRData.JsonFormatLogicItem NewLogic = new() { Id = ID, ConditionalItems = ConditionalLogic };
                        LogicUtilities.RemoveRedundantConditionals(NewLogic);
                        LogicFile.Logic.Add(NewLogic);
                    }
                    else
                    {
                        Duplicates.ConditionalItems.AddRange(ConditionalLogic);
                        LogicUtilities.RemoveRedundantConditionals(Duplicates);
                    }
                }
            }
        }

        private static string CheckForLogicOverride(string value, string iD)
        {
            Dictionary<string, string> Override = new Dictionary<string, string>
            {
                { "OOT_EVENT_SPIRIT_ADULT_DOOR", "can_lift_silver && (setting(agelessBoots) || setting(agelessHookshot) || setting(climbMostSurfacesOot) && small_keys_spirit(5)) || (setting(agelessBoots, false) || setting(agelessHookshot, false) || setting(climbMostSurfacesOot, false) && small_keys_spirit(3))" },
                { "OOT Forest Temple Maze => OOT Forest Temple Twisted 1 Normal", "(((is_adult || time_travel_at_will) && has(STRENGTH)) || climb_anywhere || hookshot_anywhere) && (setting(hookshotAnywhereOot) && setting(ageChange, none, false) && small_keys_forest(5)) || ((setting(hookshotAnywhereOot, false) || setting(ageChange, none)) && small_keys_forest(2))" },
                { "OOT Forest Temple Twisted 1 Normal => OOT Forest Temple Poe 1", "(setting(hookshotAnywhereOot) && setting(ageChange, none, false) && small_keys_forest(5)) || ((setting(hookshotAnywhereOot, false) || setting(ageChange, none)) && small_keys_forest(2))" },
                { "OOT Forest Temple Maze => OOT Forest Temple Twisted 1 Alt", "event(FOREST_TWISTED_HALL) && ((setting(hookshotAnywhereOot) && !setting(ageChange, none) && small_keys_forest(5)) || ((setting(hookshotAnywhereOot, false) || setting(ageChange, none)) && small_keys_forest(2)))" },
                { "MM Zora Hall Evan HP", "can_play_evan && (setting(erDungeons, none, false) && setting(bossWarpPads, Remains) && has_mask_zora) || (setting(erDungeons, none) || setting(bossWarpPads, Remains, false))" }
            };

            if (Override.ContainsKey(iD))
            {
                return Override[iD];
            }
            else { return value; }  
        }
    }
}
