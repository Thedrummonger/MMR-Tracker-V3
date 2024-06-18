using Microsoft.VisualBasic.Logging;
using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDMUtils;
using YamlDotNet.Core.Tokens;
using static TestingForm.GameDataCreation.WindWakerHD.DataStructure;

namespace TestingForm.GameDataCreation.WindWakerHD
{
    internal class Generate
    {
        public static void GenerateData(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary)
        {
            Logic = new MMRData.LogicFile()
            {
                GameCode = "WWDHR",
                Logic = [],
                Version = 1
            };
            dictionary = new LogicDictionaryData.LogicDictionary()
            {
                GameCode = Logic.GameCode,
                LogicVersion = Logic.Version,
                RootAreas = ["Root"],
                WinCondition = "Game_Beatable"
            };

            LogicStringParser logicStringParser = new LogicStringParser(operatorType: LogicStringParser.OperatorType.PyStyle, AllowSpaces: true, quotes: '*');

            var WWHDRTestingPath = Path.Combine(TestingReferences.GetDevTestingPath(), "WWHD");
            var DataPath = Path.Combine(WWHDRTestingPath, "TWWHD-Randomizer-main", "logic", "data");

            var EntranceShuffleTable = Utility.DeserializeYAMLFile<List<EntranceShuffleTableEntry>>(Path.Combine(DataPath, "entrance_shuffle_table.yaml"));
            var ItemData = Utility.DeserializeYAMLFile<List<DataStructure.ItemData>>(Path.Combine(DataPath, "item_data.yaml"));
            var LocationData = Utility.DeserializeYAMLFile<List<DataStructure.LocationData>>(Path.Combine(DataPath, "location_data.yaml"));
            var Macros = Utility.DeserializeYAMLFile<Dictionary<string, string>>(Path.Combine(DataPath, "macros.yaml"));
            var World = Utility.DeserializeYAMLFile<List<WorldLocation>>(Path.Combine(DataPath, "world.yaml"));

            Dictionary<string, string> LogicCache = [];
            HashSet<(string, string)> Exits = [];
            Dictionary<string, string> LocationAreaMap = [];
            foreach (var entry in World)
            {
                foreach(var location in entry.Locations)
                {
                    if (LocationAreaMap.ContainsKey(location.Key)) { LocationAreaMap[location.Key] = "Multiregion"; }
                    else { LocationAreaMap.Add(location.Key, entry.GetArea()); }
                    
                    string ExistingLogic = LogicCache.TryGetValue(location.Key, out string O) ? O : null;
                    string LogicString = $"(*{entry.Name}* and ({location.Value}))";
                    if (ExistingLogic is not null) { LogicString = $"(({LogicString}) or ({ExistingLogic}))"; }
                    LogicCache[location.Key] = LogicString;
                }
                foreach (var Event in entry.Events)
                {
                    string ID = Event.Key.Replace(" ", "_");
                    string ExistingLogic = LogicCache.TryGetValue(ID, out string O) ? O : null;
                    string LogicString = $"(*{entry.Name}* and ({Event.Value}))";
                    if (ExistingLogic is not null) { LogicString = $"(({LogicString}) or ({ExistingLogic}))"; }
                    LogicCache[ID] = LogicString;
                }
                foreach (var exit in entry.Exits)
                {
                    string ID = $"{entry.Name} => {exit.Key}";
                    LogicCache.Add(ID, exit.Value);
                    Exits.Add((entry.Name, exit.Key));
                    Exits.Add((exit.Key, entry.Name));
                }
            }
            foreach(var macro in Macros)
            {
                string ID = macro.Key.Replace(" ", "_");
                string ExistingLogic = LogicCache.TryGetValue(ID, out string O) ? O : null;
                string LogicString = $"({macro.Value})";
                if (ExistingLogic is not null) { LogicString = $"(({LogicString}) or ({ExistingLogic}))"; }
                LogicCache[ID] = LogicString;
            }

            foreach(var i in LogicCache)
            {
                Logic.Logic.Add(new MMRData.JsonFormatLogicItem
                {
                    Id = i.Key,
                    ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(logicStringParser, i.Value, i.Key)
                });
            }

            dictionary.LogicEntryCollections.Add("health", new OptionData.LogicEntryCollection
            {
                ID = "health",
                Entries = ["Piece_of_Heart_2", "Piece_of_Heart", "Heart_Container", "Heart_Container", "Heart_Container", "Heart_Container"]
            });

            dictionary.ToggleOptions.Add("Skip_Refights", new OptionData.ToggleOption(null)
            {
                ID = "Skip_Refights",
                Value = true,
                Name = "Skip Boss Rematches"
            }.CreateSimpleValues());

            dictionary.ToggleOptions.Add("Remove_Swords", new OptionData.ToggleOption(null)
            {
                ID = "Remove_Swords",
                Value = false,
                Name = "Remove Swords"
            }.CreateSimpleValues());

            foreach (var entry in Logic.Logic)
            {
                LogicUtilities.TransformLogicItems(entry, x => x.Replace("*", ""));
                LogicUtilities.TransformLogicItems(entry, x => x.StartsWith("'") && x.EndsWith("'") ? x.Replace(" ", "_") : x);
                LogicUtilities.TransformLogicItems(entry, x => {
                    if (x.StartsWith("can_access("))
                    {
                        return x["can_access(".Length..][..^1];
                    }
                    else if (x.StartsWith("count("))
                    {
                        string Statement = x["count(".Length..][..^1];
                        var Segements = Statement.TrimSplit(",");
                        return $"{Segements[1]}, {Segements[0]}";
                    }
                    else if (x.StartsWith("health("))
                    {
                        string Statement = x["health(".Length..][..^1];
                        int Value = int.Parse(Statement) * 4;
                        return $"health, {Value}";
                    }
                    else if (x == "Nothing") { return true.ToString(); }
                    return x;
                });
                var DictRef = dictionary;
                LogicUtilities.TransformLogicItems(entry, x =>
                {
                    bool IsInverted = false;
                    string Value = x;
                    if (Value.StartsWith("not "))
                    {
                        IsInverted = true;
                        Value = Value["not ".Length..];
                    }
                    if (DictRef.ToggleOptions.ContainsKey(Value))
                    {
                        string Modifier = IsInverted ? ", false" : "";
                        return $"setting{{{Value}{Modifier}}}";
                    }
                    return x;
                });
            }

            foreach(var exit in Exits)
            {
                string ID = $"{exit.Item1} => {exit.Item2}";
                var RandomizationData = EntranceShuffleTable.FirstOrDefault(x => x.GetID() == ID);
                bool IsRandomizable = RandomizationData is not null;
                dictionary.EntranceList.Add(ID, new LogicDictionaryData.DictionaryEntranceEntries
                {
                    ID = ID,
                    Area = exit.Item1,
                    Exit = exit.Item2,
                    RandomizableEntrance = IsRandomizable
                });
                if (IsRandomizable)
                {
                    var CoupledEntry = RandomizationData.GetCoupleData();
                    dictionary.EntranceList[ID].EntrancePairID = new EntranceData.EntranceAreaPair { Area = CoupledEntry[0], Exit = CoupledEntry[1] };
                }
            }

            dictionary.EntranceList["Link's Spawn => Outset Island"].RandomizableEntrance = true;
            dictionary.EntranceList["Link's Spawn => Outset Island"].DisplayExit = "Starting Island";

            foreach (var loc in LocationData)
            {
                if (loc.Category.Contains("Ho Ho Hint")) { AddHint(loc, dictionary); }
                else { AddLocation(loc, dictionary); }
            }
            void AddLocation(DataStructure.LocationData loc, LogicDictionaryData.LogicDictionary dictionary)
            {
                var Location = new LogicDictionaryData.DictionaryLocationEntries()
                {
                    ID = loc.GetLocationID(),
                    Area = LocationAreaMap[loc.GetLocationID()],
                    Name = loc.Names.English,
                    OriginalItem = loc.Original_Item.Replace(" ", "_").Replace("'", ""),
                    ValidItemTypes = ["item"],
                    SpoilerData = new MMRData.SpoilerlogReference
                    {
                        Tags = loc.Category,
                        SpoilerLogNames = [loc.Names.English, loc.Names.Spanish, loc.Names.French]
                    }
                };
                dictionary.LocationList.Add(Location.ID, Location);
            }
            void AddHint(DataStructure.LocationData loc, LogicDictionaryData.LogicDictionary dictionary)
            {
                var Location = new LogicDictionaryData.DictionaryHintEntries()
                {
                    ID = loc.GetLocationID(),
                    Area = LocationAreaMap[loc.GetLocationID()],
                    Name = loc.Names.English,
                    SpoilerData = new MMRData.SpoilerlogReference
                    {
                        Tags = loc.Category,
                        SpoilerLogNames = [loc.Names.English, loc.Names.Spanish, loc.Names.French]
                    }
                };
                dictionary.HintSpots.Add(Location.ID, Location);
            }
            foreach (var item in ItemData)
            {
                if (item.GetItemID().In("Swift_Sail", "Big_Key", "Small_Key")) { continue; } //Unused Items?
                if (item.GetItemID().EndsWith("Deciphered")) { continue; } //Unused Items?
                var Item = new LogicDictionaryData.DictionaryItemEntries()
                {
                    ID = item.GetItemID(),
                    Name = item.Names.English,
                    ItemTypes = ["item"],
                    ValidStartingItem = true,
                    SpoilerData = new MMRData.SpoilerlogReference
                    {
                        SpoilerLogNames = [item.Names.English, item.Names.Spanish, item.Names.French]
                    }
                };
                dictionary.ItemList.Add(Item.ID, Item);
            }


            File.WriteAllText(Path.Combine(WWHDRTestingPath, "Logic.json"), Logic.ToFormattedJson());
            File.WriteAllText(Path.Combine(WWHDRTestingPath, "Dictionary.json"), dictionary.ToFormattedJson());

        }
    }
}
