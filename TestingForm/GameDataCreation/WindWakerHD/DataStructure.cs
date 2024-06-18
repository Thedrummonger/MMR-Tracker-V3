using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDMUtils;

namespace TestingForm.GameDataCreation.WindWakerHD
{
    internal class DataStructure
    {
        public enum EntranceType
        {
            DUNGEON,
            BOSS,
            MINIBOSS,
            CAVE,
            FAIRY,
            DOOR,
            MISC_RESTRICTIVE,
            MISC
        }
        public class EntranceShuffleTableEntry
        {
            public EntranceType Type;
            public string Forward;
            public string Return;
            public string[] GetExitData() { return Forward.TrimSplit(","); }
            public string[] GetCoupleData() { return Forward.TrimSplit(","); }
            public string GetID() { return $"{GetExitData()[0]} => {GetExitData()[1]}"; }
            public string GetCoupledID() { return $"{GetCoupleData()[0]} => {GetCoupleData()[1]}"; }
        }
        public class LanguageData
        {
            public string English;
            public string Spanish;
            public string French;
        }
        public class ItemData
        {
            public LanguageData Names;
            [JsonProperty("Pretty Names")]
            public LanguageData Pretty_Names;
            public string GetItemID() => Names.English.Replace(" ", "_").Replace("'", "");
            public string GetItemName() => Pretty_Names.English.Replace("|","");
        }
        public class LocationData
        {
            public LanguageData Names;
            [JsonProperty("Original Item")]
            public string Original_Item;
            public string[] Category;
            public string GetLocationID() => Names.English;
        }
        public class WorldLocation
        {
            public string Name;
            public Dictionary<string, string> Locations = [];
            public Dictionary<string, string> Exits = [];
            public Dictionary<string, string> Events = [];
            public string Island;
            public string Dungeon;
            public string GetArea() => Dungeon ?? Island ?? Name;
        }
    }
}
