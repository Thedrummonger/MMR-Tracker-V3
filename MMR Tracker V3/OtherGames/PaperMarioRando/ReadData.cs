using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.OtherGames.PaperMarioRando
{
    public class PMREdge
    {
        public MapPoint from;
        public MapPoint to;
        public object[][] reqs;
        public string[] pseudoitems;
        public Tuple<string, int> ParsedReq(object j)
        {
            string Item;
            int Count;
            if (j is System.String) { Item = (string)j; Count = 1; }
            else
            {
                var json = JsonConvert.SerializeObject(j);
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
                Item = dictionary.First().Key;
                Count = dictionary.First().Value;
            }
            return new(Item, Count);
        }
    }

    public class MapPoint
    {
        public string map;
        public object id;
        public int IntID()
        {
            if (!int.TryParse(StringID(), out int val)) { return -1; }
            return val;
        }
        public string StringID()
        {
            return id.ToString();
        }
    }

    public class ReadData
    {

        public static void ReadEadges()
        {
            string CodePath = Path.Combine(References.TestingPaths.GetDevCodePath(), "MMR Tracker V3");
            string PMRFiles = Path.Combine(CodePath, "OtherGames", "PaperMarioRando");
            string EdgesFile = Path.Combine(PMRFiles, "Edges.json");

            string TestingFoler = Path.Combine(References.TestingPaths.GetDevTestingPath(), "PMRTesting");

            var Edges = JsonConvert.DeserializeObject<List<PMREdge>>(File.ReadAllText(EdgesFile));

            TrackerObjects.LogicDictionaryData.LogicDictionary PMRDict = new TrackerObjects.LogicDictionaryData.LogicDictionary()
            {
                RootArea = "Spawn",
                GameCode = "PMR",
                WinCondition = "YOUWIN"
            };

            Dictionary<string, string> ExitLogic = new Dictionary<string, string>();
            foreach(var edge in Edges)
            {
                string FromID = $"{edge.from.map}_{edge.from.StringID()}";
                string ToID = $"{edge.to.map}_{edge.to.StringID()}";
                if (FromID == ToID) { continue; }
                string EntranceID = $"{FromID} => {ToID}";

                if (!ExitLogic.ContainsKey(EntranceID))
                {
                    PMRDict.EntranceList.Add(EntranceID, new TrackerObjects.LogicDictionaryData.DictionaryEntranceEntries { ID = EntranceID, Area = FromID, Exit = ToID });
                }

            }

            File.WriteAllText(Path.Combine(TestingFoler, "PMR v1.json"), JsonConvert.SerializeObject(PMRDict, Formatting.Indented));

            List<Tuple<string, int>> Reqs = new List<Tuple<string, int>>();

            foreach (var edge in Edges)
            {
                if (edge.reqs is not null && edge.reqs.Any()) 
                { 
                    foreach(var i in edge.reqs)
                    {
                        foreach(var j in i)
                        {
                            Reqs.Add(edge.ParsedReq(j));
                        }
                    }
                }
            }
            Reqs.Sort();
            Testing.PrintObjectToConsole(Reqs.Distinct());
        }

    }
}
