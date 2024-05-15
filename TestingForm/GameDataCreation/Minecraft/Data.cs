using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDMUtils;

namespace TestingForm.GameDataCreation.Minecraft
{
    internal class Data
    {
        public class MCPaths
        {
            public static string TestFolder = Path.Combine(TestingReferences.GetDevTestingPath(), "Archipelago");
            public static string ArchipelagoCodeFolder = Path.Combine(TestFolder, "Archipelago-main");
            public static string MinecraftDataFolder = Path.Combine(ArchipelagoCodeFolder, "worlds", "minecraft", "data");
            public static string excluded_locations_File = Path.Combine(MinecraftDataFolder, "excluded_locations.json");
            public static string items_File = Path.Combine(MinecraftDataFolder, "items.json");
            public static string locations_File = Path.Combine(MinecraftDataFolder, "locations.json");
            public static string regions_File = Path.Combine(MinecraftDataFolder, "regions.json");

            public static string logic_File = Path.Combine(TestingReferences.GetOtherGameDataPath("Minecraft"), "logic.json");
        }
        public class MC_excluded_locations
        {
            public string[] hard;
            public string[] unreasonable;
            public string[] ender_dragon;
            public string[] wither;
        }
        public class MC_items
        {
            public string[] all_items;
            public string[] progression_items;
            public string[] useful_items;
            public string[] trap_items;
            public Dictionary<string, int> required_pool;
            public Dictionary<string, int> junk_weights;
        }
        public class MC_locations
        {
            public string[] all_locations;
            public Dictionary<string, string[]> locations_by_region;
        }
        public class MC_regions
        {
            public List<object[]> regions;
            public List<string[]> mandatory_connections;
            public List<string[]> default_connections;
            public (string key, string[] values) ParseObj(object[] vals)
            {
                if (vals[1] is string s) { return ((string)vals[0], new string[] { s }); }
                return ((string)vals[0], Utility.SerializeConvert<string[]>(vals[1]));
            }
        }
        public class MC_logic
        {
            public Dictionary<string, string> entrances;
            public Dictionary<string, string> locations;
            public Dictionary<string, string> macros;
        }
    }
}
