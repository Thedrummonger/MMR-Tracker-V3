using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.OtherGames.OOTR
{
    public static class ReadOOTRItemsAndLocations
    {
        public static void Read()
        {
            string CodePath = References.TestingPaths.GetDevCodePath();
            string OOTRFolder = Path.Combine(CodePath, "MMR Tracker V3", "OtherGames", "OOTR");
            string ItemListFile = Path.Combine(OOTRFolder, "OOTRItemList.json");
            string LocationListFile = Path.Combine(OOTRFolder, "OOTRLocationList.Json");

            var LogicFiles = ReadAndParseOOTRLogic.ReadAndParse();

            List<Dictionary<string, string>> newLogicFileFormat = new List<Dictionary<string, string>>();

            Dictionary<string, string> Locations = new Dictionary<string, string>();
            Dictionary<string, string> Macros = new Dictionary<string, string>();
            Dictionary<string, string> Exits = new Dictionary<string, string>();

            foreach(var LogicFile in LogicFiles.SelectMany(x => x.LogicalRegions))
            {
                foreach(var i in LogicFile.locations?.Keys.ToList()?? new List<string>())
                {
                    if (Locations.ContainsKey(i)) { Locations[i] = $"{Locations[i]} + ({LogicFile.locations[i]})" ; }
                    else { Locations[i] = $"({LogicFile.locations[i]})"; }
                }
                foreach (var i in LogicFile.events?.Keys.ToList()?? new List<string>())
                {
                    if (Macros.ContainsKey(i)) { Macros[i] = $"{Macros[i]} + ({LogicFile.events[i]})"; }
                    else { Macros[i] = $"({LogicFile.events[i]})"; }
                }
                foreach (var i in LogicFile.exits?.Keys.ToList()?? new List<string>())
                {
                    string ExitName = $"{LogicFile.region_name} => {i}";
                    if (Exits.ContainsKey(ExitName)) { Exits[ExitName] = $"{Exits[ExitName]} + ({LogicFile.exits[i]})"; }
                    else { Exits[ExitName] = $"({LogicFile.exits[i]})"; }
                }
            }

            newLogicFileFormat.Add(Locations);
            newLogicFileFormat.Add(Macros);
            newLogicFileFormat.Add(Exits);
            var NewLogic = Newtonsoft.Json.JsonConvert.SerializeObject(newLogicFileFormat, Testing._NewtonsoftJsonSerializerOptions);
            File.WriteAllText(Path.Combine(References.TestingPaths.GetDevTestingPath(), "NewLogicFormatted.json"), NewLogic);

            MMR_Tracker_V3.TrackerObjects.MMRData.LogicFile JsonLogic = new TrackerObjects.MMRData.LogicFile() { GameCode = "OOTR", Version = 1, Logic = new List<TrackerObjects.MMRData.JsonFormatLogicItem>() };

            foreach (var Set in newLogicFileFormat)
            {
                foreach(var i in Set.Keys)
                {
                    Debug.WriteLine(i + " : " + Set[i]);
                    var LogicEntries = LogicStringParser.SplitLogicString(Set[i]);
                    var PrepedLogic = LogicStringParser.ReplaceEntryWithLetter(LogicEntries, out Dictionary<string, string> ReplacementDict);
                    var Conditional = LogicStringParser.ExpandLogicString(PrepedLogic);
                    var Logic = new TrackerObjects.MMRData.JsonFormatLogicItem { ConditionalItems = Conditional.Select(y => y.Select(x => ReplacementDict[x]).ToList()).ToList(), Id = i };
                    logicCleaner.RemoveRedundantConditionals(Logic);
                    logicCleaner.MakeCommonConditionalsRequirements(Logic);
                    JsonLogic.Logic.Add(Logic);
                }
            }

            var NewJsonLogic = Newtonsoft.Json.JsonConvert.SerializeObject(JsonLogic, Testing._NewtonsoftJsonSerializerOptions);
            File.WriteAllText(Path.Combine(References.TestingPaths.GetDevTestingPath(), "NewLogicFormatted.json"), NewJsonLogic);

            Dictionary<string, List<dynamic>> ItemList = JsonConvert.DeserializeObject<Dictionary<string, List<dynamic>>>(File.ReadAllText(ItemListFile));
            Dictionary<string, List<dynamic>> LocationList = JsonConvert.DeserializeObject<Dictionary<string, List<dynamic>>>(File.ReadAllText(LocationListFile));


        }
    }
}
