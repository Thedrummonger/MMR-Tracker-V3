using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class LogicDictionaryData
    {
        public class LogicDictionary
        {
            public int LogicVersion { get; set; }
            public string LogicFormat { get; set; }
            public string GameCode { get; set; }
            public int DefaultWalletCapacity { get; set; } = 99;
            public List<DictionaryLocationEntries> LocationList { get; set; } = new List<DictionaryLocationEntries>();
            public List<DictionaryMacroEntries> MacroList { get; set; } = new List<DictionaryMacroEntries>();
            public List<DictionaryItemEntries> ItemList { get; set; } = new List<DictionaryItemEntries>();

        }

        public class DictionaryLocationEntries
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string[] AltNames { get; set; } = Array.Empty<string>();
            public string Area { get; set; }
            public string[] ValidItemTypes { get; set; } = Array.Empty<string>();

        }

        public class DictionaryMacroEntries
        {
            public string ID { get; set; }
            public string LogicShufflePairing { get; set; }

        }

        public class DictionaryItemEntries
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string[] AltNames { get; set; } = Array.Empty<string>();
            public int? WalletCapacity { get; set; } = null;
            public bool ValidStartingItem { get; set; }
            public KeyType KeyType { get; set; } = KeyType.None;
            public string[] ItemTypes { get; set; } = Array.Empty<string>();

            public ProgressiveItemData ProgressiveItemData = new ProgressiveItemData();

        }
    }
}
