using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.Remoting;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MMR_Tracker_V3.OtherGames.SkywardSwordRando
{
    public class ReadAndParse
    {

        public class SSRDATA
        {
            public string ParentArea = null;
            public string entrance = null;
            public Dictionary<string, string> Locations = new Dictionary<string, string>();
            public Dictionary<string, string> Exits = new Dictionary<string, string>();
        }

        public static void ReadWebData() 
        {
            List<string> WebURLS= new List<string>();
            WebClient NewWebClient = new WebClient();
            string TEstYaml = NewWebClient.DownloadString("https://raw.githubusercontent.com/ssrando/ssrando/master/logic/requirements/Skyloft.yaml");

            var deserializer = new DeserializerBuilder().Build();
            Dictionary<object, dynamic> YAMLObj = deserializer.Deserialize<Dictionary<object, dynamic>>(TEstYaml);

            Dictionary<string, SSRDATA> MasterData = new Dictionary<string, SSRDATA>();

            ParseLocations("ROOT", YAMLObj);

            Testing.PrintObjectToConsole(MasterData);

            void ParseLocations(string CurrentArea, Dictionary<object, dynamic> RegionData, string ParentArea = "ERROR") 
            {
                SSRDATA CurrentRegion = new SSRDATA();
                foreach (var Entry in RegionData)
                {
                    string Section = Entry.Key as string;

                    if (Entry.Value is Dictionary<object, object> d)
                    {
                        if (Section == "exits")
                        {
                            if (!MasterData.ContainsKey(CurrentArea)) { MasterData[CurrentArea] = new SSRDATA(); }
                            foreach(KeyValuePair<object, dynamic> i in Entry.Value)
                            {
                                string ExitName = i.Key as string;
                                string LogicString = i.Value as string;
                                if (ExitName == "Exit") { ExitName = ParentArea; }
                                if (LogicString == "Nothing" || LogicString == "Night" || LogicString == "Day") { LogicString = "true"; }
                                if (LogicString == "Impossible") { LogicString = "false"; }
                                MasterData[CurrentArea].Exits.Add(ExitName, LogicString);
                            }
                        }
                        else if (Section == "locations")
                        {
                            if (!MasterData.ContainsKey(CurrentArea)) { MasterData[CurrentArea] = new SSRDATA(); }
                            foreach (KeyValuePair<object, dynamic> i in Entry.Value)
                            {
                                string LocationName = i.Key as string;
                                string LogicString = i.Value as string;
                                if (LogicString == "Nothing") { LogicString = "true"; }
                                if (LogicString == "Impossible") { LogicString = "false"; }
                                MasterData[CurrentArea].Locations.Add(LocationName, LogicString);
                            }
                        }
                        else
                        {
                            ParseLocations(Entry.Key as string, Entry.Value, CurrentArea);
                        }
                    }
                    else if (Entry.Value is string s)
                    {
                        if (Section == "entrance") { CurrentRegion.entrance = s; }
                    }
                }
            }

        }

    }
}
