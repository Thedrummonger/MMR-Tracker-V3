using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.OtherGames.OOTR
{
    public static class ReadOOTRItemsAndLocations
    {
        public static string CodePath = References.TestingPaths.GetDevCodePath();
        public static string OOTRFolder = Path.Combine(CodePath, "MMR Tracker V3", "OtherGames", "OOTR");
        public static string ItemListFile = Path.Combine(OOTRFolder, "OOTRItemList.json");
        public static string LocationListFile = Path.Combine(OOTRFolder, "OOTRLocationList.Json");
        public static string OOTRTricksFile = Path.Combine(OOTRFolder, @"LogicTricks.json");
        public static string OOTRVariableFile = Path.Combine(OOTRFolder, @"Variables.json");
        public static void Read()
        {

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

            Dictionary<string, OOTRObjects.trickdata> tricks = JsonConvert.DeserializeObject<Dictionary<string, OOTRObjects.trickdata>>(File.ReadAllText(OOTRTricksFile));
            foreach(var i in tricks.Keys)
            {
                string ToolTip = Regex.Replace(tricks[i].tooltip, @"\r\n?|\n", " ");
                ToolTip = Regex.Replace(ToolTip, " {2,}", " ").Trim();
                var Logic = new TrackerObjects.MMRData.JsonFormatLogicItem { Id = tricks[i].name, IsTrick = true, TrickTooltip = ToolTip, TrickCategory = tricks[i]?.tags?[0] };
                JsonLogic.Logic.Add(Logic);
            }

            CheckForLitteralItemEntries(JsonLogic);

            CheckForUndefinedLogicEntries(JsonLogic);

            var NewJsonLogic = Newtonsoft.Json.JsonConvert.SerializeObject(JsonLogic, Testing._NewtonsoftJsonSerializerOptions);
            File.WriteAllText(Path.Combine(References.TestingPaths.GetDevTestingPath(), "NewLogicFormatted.json"), NewJsonLogic);

            Dictionary<string, List<dynamic>> ItemList = JsonConvert.DeserializeObject<Dictionary<string, List<dynamic>>>(File.ReadAllText(ItemListFile));
            Dictionary<string, List<dynamic>> LocationList = JsonConvert.DeserializeObject<Dictionary<string, List<dynamic>>>(File.ReadAllText(LocationListFile));


        }

        public static Dictionary<string, string> GetBasicItemList()
        {
            Dictionary<string, dynamic> RawItemList = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(File.ReadAllText(ItemListFile));
            Dictionary<string, string> ItemList = new Dictionary<string, string>();
            foreach (var item in RawItemList)
            {
                ItemList.Add(CleanItemName(item.Key), item.Key);
            }
            return ItemList;
            string CleanItemName(string itemName)
            {
                return itemName.Replace(" ", "_").Replace("(", "").Replace(")", "");
            }
        }

        public static Dictionary<string, string> GetAreasFromLogic(TrackerObjects.MMRData.LogicFile JsonLogic)
        {
            Dictionary<string, string> Areas = new Dictionary<string, string>();
            foreach(var i in JsonLogic.Logic)
            {
                if (i.Id.Contains(" => "))
                {
                    var data = i.Id.Split(new string[] { " => " }, StringSplitOptions.None);
                    Areas[data[0]] = "Entry";
                    Areas[data[1]] = "Exit";
                }
            }
            return Areas;
        }

        public static void CheckForLitteralItemEntries(TrackerObjects.MMRData.LogicFile JsonLogic)
        {
            //OOTR has a few macros that hsare the same name as macros. The tracker can handle this, but these items ned to be enclosed in parethases to Signal
            //to the tracker to check for the item varient first. Luckily the only tim this occurs in OOTR, the item is called in the macro of the same name
            foreach (var i in JsonLogic.Logic)
            {
                if (i.RequiredItems is not null)
                {
                    for (var j = 0; j < i.RequiredItems.Count; j++)
                    {
                        if (i.Id == i.RequiredItems[j])
                        {
                            Debug.WriteLine($"{i.Id} Required it's self making {i.RequiredItems[j]} Litteral");
                            i.RequiredItems[j] = $"'{i.Id}'";
                        }
                    }
                }
                if (i.ConditionalItems is not null)
                {
                    foreach (var k in i.ConditionalItems)
                    {
                        for (var j = 0; j < k.Count; j++)
                        {
                            if (i.Id == k[j])
                            {
                                Debug.WriteLine($"{i.Id} Required it's self making {k[j]} Litteral");
                                k[j] = $"'{i.Id}'";
                            }
                        }
                    }
                }
            }
        }

        public static void CheckForUndefinedLogicEntries(TrackerObjects.MMRData.LogicFile JsonLogic)
        {
            Dictionary<string, string> ItemList = GetBasicItemList();
            Dictionary<string, string> Areas = GetAreasFromLogic(JsonLogic);
            //OOTR has a few macros that hsare the same name as macros. The tracker can handle this, but these items ned to be enclosed in parethases to Signal
            //to the tracker to check for the item varient first. Luckily the only tim this occurs in OOTR, the item is called in the macro of the same name
            foreach (var i in JsonLogic.Logic)
            {
                foreach(var req in i.RequiredItems??new List<string>())
                {
                    CheckItem(req);
                }
                foreach(var Cond in i.ConditionalItems??new List<List<string>>())
                {
                    foreach (var req in Cond)
                    {
                        CheckItem(req);
                    }
                }
            }

            void CheckItem(string i)
            {
                string req = i;
                bool Found = false;

                if (req.StartsWith("'") && req.EndsWith("'")) { req = req.Replace("'", ""); }

                if (JsonLogic.Logic.Any(x => x.Id == req)) { Found = true; }
                else if (ItemList.ContainsKey(req)) { Found = true; }
                else if (bool.TryParse(req, out _)) { Found = true; }
                else if (Areas.ContainsKey(req)) { Found = true; }
                else
                {
                    if (req.Contains(','))
                    {
                        var data = req.Split(',').Select(x => x.Trim()).ToArray();
                        string Item = data[0];
                        if (Item.StartsWith("'") && Item.EndsWith("'")) { Item = Item.Replace("'", ""); }
                        string Other = data[1];
                        if (JsonLogic.Logic.Any(x => x.Id == Item)) { Found = true; }
                        else if (ItemList.ContainsKey(Item)) { Found = true; }
                    }
                }

                if (!Found)
                {
                    Debug.WriteLine($"The Logic Entry {req} Was not found in items or macros");
                }
            }
        }
    }
}
