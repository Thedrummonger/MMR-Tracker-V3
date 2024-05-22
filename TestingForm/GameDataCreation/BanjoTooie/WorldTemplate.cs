using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDMUtils;

namespace TestingForm.GameDataCreation.BanjoTooie
{
    public class WorldTemplate
    {
        public Dictionary<string, Dictionary<string, WorldArea>> WorldFiles = [];
        public class WorldArea
        {
            public Dictionary<string, WorldLocationLogic> Locations = [];
            public Dictionary<string, WorldLocationLogic> Exits = [];
            public Dictionary<string, WorldLocationLogic> Macros = [];
        }
        public class WorldLocationLogic
        {
            public string BegginerLogic = "";
            public string NormalLogic = "";
            public string AdvancedLogic = "";
            public string GlitchedLogic = "";
        }

        public void WriteWorldFiles()
        {
            Populate();
            foreach(var file in WorldFiles)
            {
                File.WriteAllText(Path.Combine(TestingReferences.GetOtherGameDataPath("BanjoTooie"), "World", $"{file.Key}.yaml"), file.ToYamlString());
            }
        }

        public Dictionary<string, Dictionary<string, WorldArea>> Populate()
        {
            //Spiral Mountain
            WorldFiles["Spiral Mountain"] = [];
            WorldFiles["Spiral Mountain"]["Menu"] = new(); //Menu
            WorldFiles["Spiral Mountain"]["SM"] = new(); //Spiral Mountain

            //Isle o hags
            WorldFiles["Isle O Hags"] = [];
            WorldFiles["Isle O Hags"]["IOHJV"] = new(); //Isle O Hags - Jinjo Village
            WorldFiles["Isle O Hags"]["IOHWH"] = new(); //Isle O Hags - Wooded Hollow
            WorldFiles["Isle O Hags"]["IOHPL"] = new(); //Isle O Hags - Plateau
            WorldFiles["Isle O Hags"]["IOHPG"] = new(); //Isle O Hags - Pine Grove
            WorldFiles["Isle O Hags"]["IOHCT"] = new(); //Isle O Hags - Cliff Top
            WorldFiles["Isle O Hags"]["IOHCT_HFP_ENTRANCE"] = new(); //Isle O Hags - Cliff Top (Across Bridge)
            WorldFiles["Isle O Hags"]["IOHWL"] = new(); //Isle O Hags - Wasteland
            WorldFiles["Isle O Hags"]["IOHQM"] = new(); //Isle O Hags -Quagmire

            //Mayahem Temple
            WorldFiles["Mayahem Temple"] = [];
            WorldFiles["Mayahem Temple"]["MT"] = new(); //Mayahem Temple

            //Glitter Gulch Mine
            WorldFiles["Glitter Gulch Mine"] = [];
            WorldFiles["Glitter Gulch Mine"]["GM"] = new(); //Glitter Gulch Mine
            WorldFiles["Glitter Gulch Mine"]["GMWSJT"] = new(); //Glitter Gulch Mine - Water Storage Jinjo Tank
            WorldFiles["Glitter Gulch Mine"]["CHUFFY"] = new(); //Chuffy's Cabin

            //Witchyworld
            WorldFiles["Witchyworld"] = [];
            WorldFiles["Witchyworld"]["WW"] = new(); //Witchyworld

            //Jolly Roger's Lagoon
            WorldFiles["Jolly Rogers Lagoon"] = [];
            WorldFiles["Jolly Rogers Lagoon"]["JR"] = new(); //Jolly Roger's Lagoon

            //Terrydactyland
            WorldFiles["Terrydactyland"] = [];
            WorldFiles["Terrydactyland"]["TL"] = new(); //Terrydactyland
            WorldFiles["Terrydactyland"]["TL_HATCH"] = new(); //Terrydactyland - Hatch Cave

            //Grunty's Industries
            WorldFiles["Gruntys Industries"] = [];
            WorldFiles["Gruntys Industries"]["GIO"] = new(); //Outside Grunty's Industries
            WorldFiles["Gruntys Industries"]["GI1"] = new(); //Jolly Roger's Lagoon
            WorldFiles["Gruntys Industries"]["GI2"] = new(); //Jolly Roger's Lagoon
            WorldFiles["Gruntys Industries"]["GI3ALL"] = new(); //Jolly Roger's Lagoon

            //Hailfire Peaks
            WorldFiles["Hailfire Peaks"] = [];
            WorldFiles["Hailfire Peaks"]["HP"] = new(); //Hailfire Peaks

            //Cloud Cuckooland
            WorldFiles["Cloud Cuckooland"] = [];
            WorldFiles["Cloud Cuckooland"]["CC"] = new(); //Cloud Cuckooland

            //Cauldron Keep
            WorldFiles["Cauldron Keep"] = [];
            WorldFiles["Cauldron Keep"]["CK"] = new(); //Cauldron Keep
            WorldFiles["Cauldron Keep"]["H1"] = new(); //HAG 1


            //EXITS

            WorldFiles["Spiral Mountain"]["Menu"].Exits.Add("SM", new());

            WorldFiles["Spiral Mountain"]["SM"].Exits.Add("IOHJV", new());

            WorldFiles["Isle O Hags"]["IOHJV"].Exits.Add("IOHWH", new());

            WorldFiles["Isle O Hags"]["IOHWH"].Exits.Add("MT", new());
            WorldFiles["Isle O Hags"]["IOHWH"].Exits.Add("IOHPL", new());

            WorldFiles["Mayahem Temple"]["MT"].Exits.Add("TL_HATCH", new());
            WorldFiles["Mayahem Temple"]["MT"].Exits.Add("GM", new());

            WorldFiles["Terrydactyland"]["TL_HATCH"].Exits.Add("TL", new());

            WorldFiles["Isle O Hags"]["IOHPL"].Exits.Add("GM", new());
            WorldFiles["Isle O Hags"]["IOHPL"].Exits.Add("IOHPG", new());
            WorldFiles["Isle O Hags"]["IOHPL"].Exits.Add("IOHCT", new());

            WorldFiles["Glitter Gulch Mine"]["GM"].Exits.Add("GMWSJT", new());
            WorldFiles["Glitter Gulch Mine"]["GM"].Exits.Add("IOHPL", new());
            WorldFiles["Glitter Gulch Mine"]["GM"].Exits.Add("CHUFFY", new());
            WorldFiles["Glitter Gulch Mine"]["GM"].Exits.Add("WW", new());

            WorldFiles["Isle O Hags"]["IOHPG"].Exits.Add("WW", new());
            WorldFiles["Isle O Hags"]["IOHPG"].Exits.Add("IOHWL", new());

            WorldFiles["Witchyworld"]["WW"].Exits.Add("IOHPG", new());
            WorldFiles["Witchyworld"]["WW"].Exits.Add("CHUFFY", new());

            WorldFiles["Isle O Hags"]["IOHCT"].Exits.Add("IOHCT_HFP_ENTRANCE", new());
            WorldFiles["Isle O Hags"]["IOHCT"].Exits.Add("HP", new());
            WorldFiles["Isle O Hags"]["IOHCT"].Exits.Add("JR", new());
            WorldFiles["Isle O Hags"]["IOHCT"].Exits.Add("CHUFFY", new());

            WorldFiles["Jolly Rogers Lagoon"]["JR"].Exits.Add("GMWSJT", new());
            WorldFiles["Jolly Rogers Lagoon"]["JR"].Exits.Add("IOHCT", new());

            WorldFiles["Hailfire Peaks"]["HP"].Exits.Add("IOHCT_HFP_ENTRANCE", new());
            WorldFiles["Hailfire Peaks"]["HP"].Exits.Add("MT", new());
            WorldFiles["Hailfire Peaks"]["HP"].Exits.Add("JR", new());
            WorldFiles["Hailfire Peaks"]["HP"].Exits.Add("CHUFFY", new());

            WorldFiles["Isle O Hags"]["IOHWL"].Exits.Add("IOHPG", new());
            WorldFiles["Isle O Hags"]["IOHWL"].Exits.Add("IOHQM", new());
            WorldFiles["Isle O Hags"]["IOHWL"].Exits.Add("TL", new());
            WorldFiles["Isle O Hags"]["IOHWL"].Exits.Add("CC", new());

            WorldFiles["Terrydactyland"]["TL"].Exits.Add("TL_HATCH", new());
            WorldFiles["Terrydactyland"]["TL"].Exits.Add("WW", new());
            WorldFiles["Terrydactyland"]["TL"].Exits.Add("CHUFFY", new());
            WorldFiles["Terrydactyland"]["TL"].Exits.Add("IOHWL", new());

            WorldFiles["Isle O Hags"]["IOHQM"].Exits.Add("GIO", new());
            WorldFiles["Isle O Hags"]["IOHQM"].Exits.Add("IOHWL", new());
            WorldFiles["Isle O Hags"]["IOHQM"].Exits.Add("CK", new());

            WorldFiles["Gruntys Industries"]["GIO"].Exits.Add("GI1", new());
            WorldFiles["Gruntys Industries"]["GIO"].Exits.Add("GI2", new());
            WorldFiles["Gruntys Industries"]["GIO"].Exits.Add("GI3ALL", new());
            WorldFiles["Gruntys Industries"]["GIO"].Exits.Add("IOHQM", new());

            WorldFiles["Gruntys Industries"]["GI1"].Exits.Add("GIO", new());
            WorldFiles["Gruntys Industries"]["GI1"].Exits.Add("GI2", new());
            WorldFiles["Gruntys Industries"]["GI1"].Exits.Add("GI3ALL", new());
            WorldFiles["Gruntys Industries"]["GI1"].Exits.Add("CHUFFY", new());

            WorldFiles["Gruntys Industries"]["GI2"].Exits.Add("GIO", new());
            WorldFiles["Gruntys Industries"]["GI2"].Exits.Add("GI1", new());
            WorldFiles["Gruntys Industries"]["GI2"].Exits.Add("GI3ALL", new());

            WorldFiles["Gruntys Industries"]["GI3ALL"].Exits.Add("GIO", new());
            WorldFiles["Gruntys Industries"]["GI3ALL"].Exits.Add("GI2", new());

            WorldFiles["Cauldron Keep"]["CK"].Exits.Add("H1", new());

            WorldFiles["Glitter Gulch Mine"]["CHUFFY"].Exits.Add("GM", new());
            WorldFiles["Glitter Gulch Mine"]["CHUFFY"].Exits.Add("WW", new());
            WorldFiles["Glitter Gulch Mine"]["CHUFFY"].Exits.Add("IOHCT", new());
            WorldFiles["Glitter Gulch Mine"]["CHUFFY"].Exits.Add("TL", new());
            WorldFiles["Glitter Gulch Mine"]["CHUFFY"].Exits.Add("GI1", new());
            WorldFiles["Glitter Gulch Mine"]["CHUFFY"].Exits.Add("HP", new());

            //LOCATIONS
            var AreaMap = Utility.DeserializeJsonFile<Dictionary<string, string[]>>(
                Path.Join(TestingReferences.GetOtherGameDataPath("BanjoTooie"), "LocationAreaMap.json"));

            foreach(var area in AreaMap)
            {
                string File = WorldFiles.First(x => x.Value.ContainsKey(area.Key)).Key;
                foreach(var i in area.Value)
                {
                    WorldFiles[File][area.Key].Locations.Add(i, new WorldLocationLogic());
                }
            }

            return WorldFiles;
        }
    }
}
