using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.V2Porting
{
    public class LegacyFunctions
    {
        public class LogicDictionary
        {
            public int LogicVersion { get; set; }
            public string LogicFormat { get; set; }
            public string GameCode { get; set; }
            public int DefaultWalletCapacity { get; set; } = 99;
            public List<LogicDictionaryEntry> LogicDictionaryList { get; set; } = new List<LogicDictionaryEntry>();

            public static LogicDictionary FromJson(string json)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<LogicDictionary>(json, _NewtonsoftJsonSerializerOptions);
                //return JsonSerializer.Deserialize<LogicFile>(json, _jsonSerializerOptions);
            }
            private readonly static Newtonsoft.Json.JsonSerializerSettings _NewtonsoftJsonSerializerOptions = new Newtonsoft.Json.JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
            };

        }

        public class LogicDictionaryEntry
        {
            public string DictionaryName { get; set; } //The name the logic file uses for the item
            public string LocationName { get; set; } //The name that will be displayed as the location you check
            public string ItemName { get; set; } //The name that will be displayed as the item you recieve
            public string LocationArea { get; set; } //The General Area the location is in
            public string ItemSubType { get; set; } //The type of item it is
            public bool FakeItem { get; set; } = false; //Is the item fake.
            public string[] SpoilerLocation { get; set; } //The name of this location in the spoiler Log
            public string[] SpoilerItem { get; set; } //The name of this item in the spoiler log
            public string[] GossipLocation { get; set; } //The name Gossip stone refer to this location as
            public string[] GossipItem { get; set; } //The name Gossip stone refer to this item as
            public string KeyType { get; set; } //If this is a key, what type is it
            public int? WalletCapacity { get; set; } //If this Object is a wallet, how much can it hold
            public string[] SpoilerPriceLocations { get; set; } //The names of the entry that details the price of this check in the spoiler log
            public string[] LocationCategory { get; set; } = null; //The category this location falls under, usefull for randomizers that group location by type
            public string GameClearDungeonEntrance { get; set; } //If this Object is a dungeonclear entry, this is it's dungeon entrance
            public bool ValidRandomizerStartingItem { get; set; } = false; //Can the entry be a strartingitemin the randomizer
            public ProgressiveItemData ProgressiveItemData { get; set; } = null; //Progressive Item Data
            public string EntrancePair { get; set; } //The Paired entrance for this entry
            public bool IsWarpSong { get; set; } = false;
            public string[] RandoOnlyRequiredLogic { get; set; } //The Paired entrance for this entry
        }
        public class ProgressiveItemData
        {
            public bool IsProgressiveItem { get; set; } = true;
            public string[] ProgressiveItemSet { get; set; } = null;
            public int CountNeededForItem { get; set; } = 0;
            public string ProgressiveItemName { get; set; } = null;
        }
    }
}
