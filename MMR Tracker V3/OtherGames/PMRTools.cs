using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.OtherGames
{
    public static class PMRTools
    {
        public class PaperMarioMasterData
        {
            public List<PaperMarioLogicJSON> Logic;
            public List<Dictionary<string, string>> verbose_area_names;
            public List<Dictionary<string, Dictionary<string, string>>> verbose_item_locations;
        }

        public class PaperMarioLogicJSON
        {
            public PMRLogicArea from;
            public PMRLogicArea to;
            public List<dynamic> reqs;
            public List<string> pseudoitems;
            public Dictionary<string, string> verbose_area_names;
        }
        public class PMRLogicArea
        {
            public string map;
            public dynamic id;
            public String GetArea()
            {
                return $"{map}_{id}";
            }
        }

        public class PMRData
        {
            public List<PMRItemLocation> itemLocations = new List<PMRItemLocation>();
            public List<PMRExit> RealExits = new List<PMRExit>();
            public List<PMRExit> MacroExits = new List<PMRExit>();
            public List<PMRArea> Areas = new List<PMRArea>();
            public List<PMRMacro> Macros = new List<PMRMacro>();
        }
        public class PMRItemLocation
        {
            public string ID;
            public string Area;
            public string Logic;
            public string Name;
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

        public static void ReadLogicJson()
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            PaperMarioMasterData MasterReference = JsonConvert.DeserializeObject<PaperMarioMasterData>(wc.DownloadString(@"https://raw.githubusercontent.com/Thedrummonger/MMR-Tracker-V3/master/MMR%20Tracker%20V3/OtherGames/PaperMarioRandoLogic.json"));

            PMRData MasterData = new PMRData();

            foreach (var i in MasterReference.Logic)
            {
                if (i.from.id is string) { continue; }

                string LogicLine = GetLogicDetails(i);

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
                    NewItemLocation.ID = $"{i.from.map}_{i.to.id}";
                    NewItemLocation.Logic = LogicLine;
                    NewItemLocation.Area = i.from.GetArea();
                    NewItemLocation.Name = i.to.id;
                    MasterData.itemLocations.Add(NewItemLocation);
                }
                else if (i.to.id != i.from.id || i.to.map != i.from.map) //Exit
                {
                    PMRExit NewExit = new PMRExit();
                    NewExit.ParentAreaID = i.from.GetArea();
                    NewExit.ID = i.to.GetArea();
                    NewExit.Logic = LogicLine;
                    MasterData.MacroExits.Add(NewExit);
                }
            }

            Debug.WriteLine("============================");
            Debug.WriteLine("ITEM LOCATIONS:");
            foreach(var i in MasterData.itemLocations)
            {
                Debug.WriteLine("------------------------");
                Debug.WriteLine($"ID:{i.ID}");
                Debug.WriteLine($"Name:{i.Name}");
                Debug.WriteLine($"Area:{i.Area}");
                Debug.WriteLine($"Logic:{i.Logic}");
            }
            Debug.WriteLine("============================");
            Debug.WriteLine("EXITS:");
            foreach (var i in MasterData.MacroExits)
            {
                //Debug.WriteLine("------------------------");
                //Debug.WriteLine($"ID:{i.ParentAreaID} -> {i.ID}");
                //Debug.WriteLine($"Logic:{i.Logic}");
            }
            Debug.WriteLine("============================");
            Debug.WriteLine("MACROS:");
            foreach (var i in MasterData.Macros)
            {
                Debug.WriteLine("------------------------");
                Debug.WriteLine($"ID:{i.ID}");
                Debug.WriteLine($"Logic:{i.Logic}");
            }

        }

        public static string GetLogicDetails(PaperMarioLogicJSON i)
        {
            List<List<string>> NewLogicSets = new List<List<string>>() { new List<string> { i.from.GetArea() } };
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
