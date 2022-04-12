using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.OtherGames
{
    public static class PMRTools
    {
        public class PaperMarioMasterData
        {
            public List<PaperMarioLogicJSON> Logic;
            public Dictionary<string, string> verbose_area_names;
            public Dictionary<string, string> verbose_sub_area_names;
            public Dictionary<string, Dictionary<string, string>> verbose_item_locations;
            public Dictionary<string, string> items;
            public Dictionary<string, string> verbose_item_names;
        }

        public class PaperMarioLogicJSON
        {
            public PMRLogicArea from;
            public PMRLogicArea to;
            public List<dynamic> reqs;
            public List<string> pseudoitems;
        }
        public class PMRLogicArea
        {
            public string map;
            public dynamic id;
            public String GetArea(PaperMarioMasterData MasterData, bool ID = true)
            {
                var dat = map.Split("_");
                string area = MasterData.verbose_area_names[dat[0]];
                string subArea = MasterData.verbose_sub_area_names[map];
                area = Regex.Replace(area, "[^a-zA-Z0-9 _.]+", "", RegexOptions.Compiled);
                subArea = Regex.Replace(subArea, "[^a-zA-Z0-9 _.]+", "", RegexOptions.Compiled);
                string FinalArea = $"{area} - {subArea}";
                if (ID) { FinalArea += $" - {id}"; }
                return FinalArea;
            }
        }

        public class PMRData
        {
            public List<PMRItemLocation> itemLocations = new List<PMRItemLocation>();
            public List<PMRExit> RealExits = new List<PMRExit>();
            public List<PMRExit> MacroExits = new List<PMRExit>();
            public List<PMRArea> Areas = new List<PMRArea>();
            public List<PMRMacro> Macros = new List<PMRMacro>();
            public List<PMRItemData> Items = new List<PMRItemData>();
        }
        public class PMRItemData
        {
            public string ID;
            public string Name;
            public List<string> Types = new List<string> { "item" };
            public List<string> SpoilerNames = new List<string>();
        }
        public class PMRItemLocation
        {
            public string ID;
            public string Area;
            public string Logic;
            public string Name;
            public List<string> SpoilerNames = new List<string>();
        }
        public class PMRExit
        {
            public string ParentAreaID;
            public string ID;
            public string Logic;
        }
        public class PMRMacro
        {
            public string ID;
            public string Logic;
        }
        public class PMRArea
        {
            public string ID;
        }

        public static string CreateIDName(string Text)
        {
            return Regex.Replace(Text, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }

        public static LogicObjects.TrackerInstance ReadLogicJson()
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            PaperMarioMasterData MasterReference = JsonConvert.DeserializeObject<PaperMarioMasterData>(wc.DownloadString(@"https://raw.githubusercontent.com/Thedrummonger/MMR-Tracker-V3/master/MMR%20Tracker%20V3/OtherGames/PaperMarioRandoLogic.json"));

            PMRData MasterData = new PMRData();

            foreach(var i in MasterReference.items)
            {
                PMRItemData NewItem = new PMRItemData();
                NewItem.ID = i.Key;
                NewItem.Types.Add(i.Value);
                NewItem.Name = i.Key;
                NewItem.SpoilerNames.Add(i.Key);
                if (MasterReference.verbose_item_names.ContainsKey(i.Key))
                {
                    NewItem.Name = MasterReference.verbose_item_names[i.Key];
                    NewItem.SpoilerNames.Add(MasterReference.verbose_item_names[i.Key]);
                }
                MasterData.Items.Add(NewItem);
            }

            foreach (var i in MasterReference.Logic)
            {
                //Debug.WriteLine($"{i.from.map}_{i.from.id}:{i.to.map}_{i.to.id}");
                if (i.from.id is string || i.to.id is null || i.to.map is null) { continue; }

                string LogicLine = GetLogicDetails(i, MasterReference);

                if (i.pseudoitems is not null && i.pseudoitems.Any())
                {
                    foreach(var Macro in i.pseudoitems)
                    {
                        PMRMacro NewMacro = new PMRMacro();
                        NewMacro.ID = Macro;
                        NewMacro.Logic = LogicLine;
                        MasterData.Macros.Add(NewMacro);
                    }
                }

                if (i.to.id is string) //Item Location
                {
                    PMRItemLocation NewItemLocation = new PMRItemLocation();
                    NewItemLocation.ID = CreateIDName($"{i.from.GetArea(MasterReference)}{MasterReference.verbose_item_locations[i.from.map][i.to.id]}");
                    NewItemLocation.Logic = LogicLine;
                    NewItemLocation.Area = $"{i.from.GetArea(MasterReference, false)}";
                    NewItemLocation.Name = MasterReference.verbose_item_locations[i.from.map][i.to.id];
                    NewItemLocation.SpoilerNames.Add($"{MasterReference.verbose_sub_area_names[i.from.map]} - {MasterReference.verbose_item_locations[i.from.map][i.to.id]}");
                    MasterData.itemLocations.Add(NewItemLocation);
                }
                else if (i.to.id != i.from.id || i.to.map != i.from.map) //Exit
                {
                    PMRExit NewExit = new PMRExit();
                    NewExit.ParentAreaID = i.from.GetArea(MasterReference);
                    NewExit.ID = i.to.GetArea(MasterReference);
                    NewExit.Logic = LogicLine;
                    MasterData.MacroExits.Add(NewExit);
                }
            }

            List<string> Areas = new List<string>();
            foreach(var i in MasterData.MacroExits)
            {
                if (!Areas.Contains(i.ParentAreaID)) { Areas.Add(i.ParentAreaID); }
                if (!Areas.Contains(i.ID)) { Areas.Add(i.ID); }
            }
            foreach(var i in Areas.OrderBy(x => x)) { MasterData.Areas.Add(new PMRArea { ID = i }); }

            TrackerObjects.LogicDictionaryData.LogicDictionary logicDictionary = new TrackerObjects.LogicDictionaryData.LogicDictionary
            {
                GameCode = "PMR",
                LogicVersion = 1,
                LogicFormat = "JSON",
                LocationList = MasterData.itemLocations.Select(x => new TrackerObjects.LogicDictionaryData.DictionaryLocationEntries {
                    ID = x.ID,
                    Area = x.Area,
                    Name = x.Name,
                    OriginalItem = "Coin",
                    SpoilerData = new TrackerObjects.MMRData.SpoilerlogReference { SpoilerLogNames = x.SpoilerNames.ToArray() },
                    ValidItemTypes = new string[] { "item" }
                }).ToList(),
                AreaList = MasterData.Areas.Select(x => x.ID).ToList(),
                EntranceList = MasterData.MacroExits.Select(x => new TrackerObjects.LogicDictionaryData.DictionaryEntranceEntries
                {
                    Area = x.ParentAreaID,
                    Exit = x.ID,
                    ID = $"{x.ParentAreaID} X {x.ID}",
                    RandomizableEntrance = false
                }).ToList(),
                ItemList = MasterData.Items.Select(x => new TrackerObjects.LogicDictionaryData.DictionaryItemEntries()
                {
                    ID = x.ID,
                    MaxAmountInWorld = x.ID == "Coin" ? -1 : 1,
                    Name = x.Name,
                    ItemTypes = x.Types.ToArray(),
                    SpoilerData = new TrackerObjects.MMRData.SpoilerlogReference { SpoilerLogNames = x.SpoilerNames.ToArray() },
                    ValidStartingItem = true
                }).ToList()
            };

            TrackerObjects.MMRData.LogicFile logicFile = new TrackerObjects.MMRData.LogicFile
            {
                GameCode = "PMR",
                Version = 1,
                Logic = new List<TrackerObjects.MMRData.JsonFormatLogicItem>()
            };

            Dictionary<string, string> CondensedLogic = new Dictionary<string, string>();

            foreach (var i in MasterData.itemLocations)
            {
                if (CondensedLogic.ContainsKey(i.ID)) { CondensedLogic[i.ID] += $" | ({i.Logic})"; }
                else { CondensedLogic[i.ID] = $"({i.Logic})"; }
            }
            foreach (var i in MasterData.MacroExits)
            {
                var ID = $"{i.ParentAreaID} X {i.ID}";
                if (CondensedLogic.ContainsKey(ID)) { CondensedLogic[ID] += $" | ({i.Logic})"; }
                else { CondensedLogic[i.ID] = $"({i.Logic})"; }
            }
            foreach (var i in MasterData.Macros)
            {
                if (CondensedLogic.ContainsKey(i.ID)) { CondensedLogic[i.ID] += $" | ({i.Logic})"; }
                else { CondensedLogic[i.ID] = $"({i.Logic})"; }
            }

            foreach(var i in CondensedLogic)
            {
                TrackerObjects.MMRData.JsonFormatLogicItem logicItem = new TrackerObjects.MMRData.JsonFormatLogicItem();
                logicItem.ConditionalItems = LogicStringParser.ConvertLogicStringToConditional(i.Value);
                logicItem.Id = i.Key;
                logicFile.Logic.Add(logicItem);
            }

            bool error = false;
            foreach(var i in logicFile.Logic)
            {
                if (logicFile.Logic.Where(x => x.Id == i.Id).Count() > 1) { Debug.WriteLine(i.Id + " Exists in logic multiple times"); error = true; }
            }
            if (error) { throw new Exception(); }

            LogicObjects.TrackerInstance TestInstance = new LogicObjects.TrackerInstance();
            TestInstance.LogicFile = logicFile;
            TestInstance.LogicDictionary = logicDictionary;
            File.WriteAllText(@"C:\Testing\Logic.json", logicFile.ToString());
            File.WriteAllText(@"C:\Testing\Dict.json", logicDictionary.ToString());
            return TestInstance;
        }

        public static string GetLogicDetails(PaperMarioLogicJSON i, PaperMarioMasterData masterData)
        {
            List<List<string>> NewLogicSets = new List<List<string>>() { new List<string> { i.from.GetArea(masterData) } };
            foreach (var req in i.reqs)
            {
                List<string> set = new List<string>();
                if (req is string StringReq) { set.Add(StringReq); } //Flag
                else
                {
                    foreach (KeyValuePair<string, object> d in req.ToObject<Dictionary<string, object>>())
                    {
                        if (d.Value is string) { set.Add($"{d.Value}"); } //Standard Requirement
                        else if (d.Value is Int64) { set.Add($"{d.Key}_{d.Value}"); } //Requirement with count
                        else
                        {
                            foreach (string st in d.Value as Newtonsoft.Json.Linq.JArray) { set.Add($"{st}"); } //Multiple Requirements of the same type
                        }
                    }
                }
                NewLogicSets.Add(set);
            }
            return string.Join(" & ", NewLogicSets.Where(x => x.Any()).Select(x => "(" + string.Join(" | ", x) + ")"));
        }
    }
}
