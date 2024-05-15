using MMR_Tracker_V3;
using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TDMUtils;

namespace TestingForm.GameDataCreation.WindWakerRando
{
    internal class Generator
    {
        public class WWRLocation
        {
            public string Need;
            [JsonProperty("Original item")]
            public string OriginalItem;
            public string Types;
        }
        public static void GenData(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary)
        {
            LogicStringParser WWRParser = new(operatorType: LogicStringParser.OperatorType.CStlyeSingle);

            var WWRCodeFolder = Path.Combine(TestingReferences.GetOtherGameDataPath("WindWakerRando"));
            var EntranceListFile = Path.Combine(WWRCodeFolder, "EntranceList.json");
            var ItemListFile = Path.Combine(WWRCodeFolder, "ItemList.json");
            var Entrances = TestingUtility.DeserializeJsonFile<Dictionary<string, string[]>>(EntranceListFile);
            var Items = TestingUtility.DeserializeJsonFile<string[]>(ItemListFile);

            var WWRtestingFolder = Path.Combine(TestingReferences.GetDevTestingPath(), "WW");
            var LogicFolderPath = Path.Combine(WWRtestingFolder, "wwrando-master", "logic");
            var ItemLocationsFile = Path.Combine(LogicFolderPath, "item_locations.txt");
            var MacroFile = Path.Combine(LogicFolderPath, "macros.txt");
            var Locations = TestingUtility.DeserializeYAMLFile<Dictionary<string, WWRLocation>>(ItemLocationsFile);
            var Macros = TestingUtility.DeserializeYAMLFile<Dictionary<string, string>>(MacroFile);

            MMRData.LogicFile logicFile = new() { Logic = [], GameCode = "WWR", Version = 2 };
            LogicDictionaryData.LogicDictionary dictFile = new() { 
                GameCode = "WWR",
                GameName = "The Wind Waker",
                LogicVersion = 2,
                NetPlaySupported = true,
                WinCondition = "Can Reach and Defeat Ganondorf",
                SpoilerLogInstructions = new()
                {
                    ArchipelagoParserPath = "WWR"
                }
            };
            foreach (var Location in Locations) { AddLogicEntry(Location.Key, Location.Value.Need); }
            foreach (var Location in Macros) { AddLogicEntry(Location.Key, Location.Value); }
            void AddLogicEntry(string ID, string Logic)
            {
                MMRData.JsonFormatLogicItem LogicItem = new()
                {
                    Id = ID,
                    ConditionalItems = LogicStringConverter.ConvertLogicStringToConditional(WWRParser, Logic, ID)
                };
                logicFile.Logic.Add(LogicItem);
            }
            CleanLogic(logicFile);
            TransformCounts(logicFile);

            foreach (var location in Locations) 
            {
                var Entry = new LogicDictionaryData.DictionaryLocationEntries()
                {
                    ID = location.Key,
                    Name = location.Key,
                    Area = location.Key.TrimSplit(" - ")[0],
                    OriginalItem = location.Value.OriginalItem,
                    ValidItemTypes = ["item"],
                    SpoilerData = new MMRData.SpoilerlogReference { NetIDs = [location.Key], Tags = location.Value.Types.TrimSplit(",") },
                };
                dictFile.LocationList.Add(location.Key, Entry);
            }

            foreach(var set in Entrances)
            {
                foreach(var entrance in set.Value)
                {
                    var EntranceID = logicFile.Logic.First(x => x.Id == $"Can Access {entrance}");
                    var LocationName = EntranceID.ConditionalItems[0][0]["Can Access ".Length..];
                    var LocationEntry = new LogicDictionaryData.DictionaryLocationEntries()
                    {
                        ID = EntranceID.Id,
                        Name = LocationName,
                        Area = set.Key,
                        OriginalItem = EntranceID.Id,
                        ValidItemTypes = ["Entrances"],
                        SpoilerData = new() { Tags = [set.Key] }
                    };
                    var ItemEntry = new LogicDictionaryData.DictionaryItemEntries()
                    {
                        ID = EntranceID.Id,
                        Name = entrance,
                        ItemTypes = ["Entrances"],
                        MaxAmountInWorld = 1,
                        ValidStartingItem = false,
                    };
                    dictFile.LocationList.Add(EntranceID.Id, LocationEntry);
                    dictFile.ItemList.Add(EntranceID.Id, ItemEntry);
                }
            }
            foreach (var item in Items.Distinct())
            {
                var ItemEntry = new LogicDictionaryData.DictionaryItemEntries()
                {
                    ID = item,
                    Name = item,
                    ItemTypes = ["item"],
                    ValidStartingItem = true,
                    SpoilerData = new MMRData.SpoilerlogReference() { NetIDs = [item] }
                };
                dictFile.ItemList.Add(item, ItemEntry);
            }

            var TrickDifficulty = new string[] { "None", "Normal", "Hard", "Very Hard" };

            dictFile.ChoiceOptions.Add("logic_obscurity", new OptionData.ChoiceOption(null)
            {
                ID = "logic_obscurity",
                Name = "Logic Obscurity",
                Value = "None",
                Description = "Obscure tricks are ways of obtaining items that are not obvious and may involve thinking outside the box.\n" +
                "This option controls the maximum difficulty of obscure tricks the randomizer will require you to do to beat the game."
            }.CreateSimpleValues(TrickDifficulty)); ;
            dictFile.ChoiceOptions.Add("logic_precision", new OptionData.ChoiceOption(null)
            {
                ID = "logic_precision",
                Name = "Logic Precision",
                Value = "None",
                Description = "Precise tricks are ways of obtaining items that involve difficult inputs such as accurate aiming or perfect timing.\n" +
                "This option controls the maximum difficulty of precise tricks the randomizer will require you to do to beat the game."
            }.CreateSimpleValues(TrickDifficulty));
            dictFile.ChoiceOptions.Add("sword_mode", new OptionData.ChoiceOption(null)
            {
                ID = "sword_mode",
                Name = "Sword Mode",
                Value = "Start with Hero's Sword",
                Description = "Controls whether you start with the Hero's Sword, the Hero's Sword is randomized, or if there are no swords in the entire game.\n" +
                "Swordless and No Starting Sword are challenge modes. (For Swordless, Phantom Ganon at FF is vulnerable to Skull Hammer.)\n" +
                "Start with Hero's Sword: You will start the game with the basic Hero's Sword already in your inventory (the default).\n" +
                "No Starting Sword: You will start the game with no sword, and have to find it somewhere in the world like other randomized items.\n" +
                "Swordless: You will start the game with no sword, and won't be able to find it anywhere. You have to beat the entire game using other items as weapons instead of the sword.\n" +
                "(Note that Phantom Ganon in FF becomes vulnerable to Skull Hammer in this mode.)"
            }.CreateSimpleValues("Start with Hero's Sword", "No Starting Sword", "Swordless"));
            dictFile.ToggleOptions.Add("required_bosses", new OptionData.ToggleOption(null)
            {
                ID = "required_bosses",
                Name = "Required Bosses",
                Value = false,
                Description = "In this mode, you will not be allowed to beat the game until certain randomly-chosen bosses are defeated. Nothing in dungeons for other bosses will ever be required.\n" +
                "You can see which islands have the required bosses on them by opening the sea chart and checking which islands have blue quest markers."
            }.CreateSimpleValues());
            dictFile.ToggleOptions.Add("skip_rematch_bosses", new OptionData.ToggleOption(null)
            {
                ID = "skip_rematch_bosses",
                Name = "Skip Boss Rematch",
                Value = false,
                Description = "Removes the door in Ganon's Tower that only unlocks when you defeat the rematch versions of Gohma, Kalle Demos, Jalhalla, and Molgera."
            }.CreateSimpleValues());

            Logic = logicFile;
            dictionary = dictFile;

            File.WriteAllText(Path.Combine(TestingReferences.GetTestingLogicPresetsPath(), "Wind Waker Logic.json"), logicFile.ToFormattedJson());
            File.WriteAllText(Path.Combine(TestingReferences.GetTestingDictionaryPath(), "WWR V1.json"), dictFile.ToFormattedJson());

            Debug.WriteLine(dictFile.LocationList.Select(x => x.Value.Area).Distinct().ToFormattedJson());
        }

        public static void CleanLogic(MMRData.LogicFile logicFile)
        {

            foreach (var Entry in logicFile.Logic)
            {
                LogicUtilities.TransformLogicItems(Entry, x =>
                {
                    if (x.StartsWith("Option \""))
                    {
                        var Data = x.TrimSplit("\"");
                        if (Data.Length == 3)
                        {
                            bool Enabled = Data[2] == "Enabled";
                            return $"setting{{{Data[1]}, {Enabled}}}";
                        }
                        else if (Data.Length > 3)
                        {
                            bool Enabled = Data[2] == "Is";
                            return $"setting{{{Data[1]}, {Data[3]}, {Enabled}}}";
                        }
                    }
                    else if (x.StartsWith("Can Access Item Location \""))
                    {
                        var Data = x.TrimSplit("\"");
                        return $"available{{{Data[1]}}}";
                    }
                    else if (x == "Impossible") { return false.ToString(); }
                    else if (x == "Nothing") { return true.ToString(); }
                    return x;
                });
                LogicUtilities.RemoveRedundantConditionals(Entry);
                LogicUtilities.SortConditionals(Entry);
            }
        }

        public static void TransformCounts(MMRData.LogicFile logicFile)
        {
            foreach (var Entry in logicFile.Logic)
            {
                LogicUtilities.TransformLogicItems(Entry, input =>
                {
                    string pattern = @" (x)(\d+)";
                    // Check if the pattern matches in the input string
                    if (Regex.IsMatch(input, pattern))
                    {
                        // Replace occurrences of the pattern with ",$2", where $2 is the matched number
                        string result = Regex.Replace(input, pattern, " ,$2");
                        return result;
                    }
                    return input;
                });
                LogicUtilities.RemoveRedundantConditionals(Entry);
                LogicUtilities.SortConditionals(Entry);
            }
        }
    }
}
