using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net;

namespace MMR_Tracker_V3.OtherGames.OOTMMRCOMBO
{

    public static class ReadAndParseData
    {
        public static string TestFolder = References.TestingPaths.GetDevTestingPath();
        public static string SpoilerLogs = Path.Combine(TestFolder, @"MM-OOT");
        public static string[] AllSpoilerLogs = Directory.GetFiles(SpoilerLogs);

        public static string CodeFolder = References.TestingPaths.GetDevCodePath();
        public static string OOTMMCode = Path.Combine(CodeFolder, @"MMR Tracker V3", "OtherGames", "OOTMMRCOMBO");
        public static string OOTMMChecks = Path.Combine(OOTMMCode, @"checks.json");
        public static string OOTMMItems = Path.Combine(OOTMMCode, @"items.json");

        public static Dictionary<string, dynamic> OOTRCheckDict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(File.ReadAllText(OOTMMChecks));
        public static Dictionary<string, dynamic> OOTRItemsDict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(File.ReadAllText(OOTMMItems));

        public class MMROOTLocation
        {
            public string location;
            public string scene;
            public string item;
        }

        public static void GetLocationsAndItems()
        {

        }

        public static void GetSpoilerLog()
        {
            Dictionary<string, string> CheckList = new Dictionary<string, string>();
            Dictionary<string, string> ItemList = new Dictionary<string, string>();

            foreach (var LogicFile in AllSpoilerLogs)
            {
                Debug.WriteLine(LogicFile);
                if (!Path.GetFileNameWithoutExtension(LogicFile).StartsWith("spoiler-")) { continue; }
                var Data = File.ReadAllLines(LogicFile);
                var ReleventData = GetReleventSpoilerLines(Data);
                foreach (var i in ReleventData)
                {
                    var entryData = i.Split(':').Select(x => x.Trim()).ToArray();
                    CheckList[entryData[0]] = "";
                    ItemList[entryData[1]] = "";
                }
            }
            Debug.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(CheckList.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value), Testing._NewtonsoftJsonSerializerOptions));
            Debug.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(ItemList.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value), Testing._NewtonsoftJsonSerializerOptions));
        }

        public static List<string> GetReleventSpoilerLines(string[] Data)
        {
            var ReleventSpoilerlines = new List<string>();
            bool AtSphereData = false;
            bool CurrentLineEmpty = false;
            bool AtLocationData = false;
            foreach (var x in Data)
            {
                bool SphereLine;
                if (x.StartsWith("Sphere ")) { AtSphereData = true; SphereLine = true; }
                else { SphereLine = false; }
                bool PreviousLineEmpty = CurrentLineEmpty;
                if (string.IsNullOrWhiteSpace(x)) { CurrentLineEmpty = true; }
                else { CurrentLineEmpty = false; }
                if (!CurrentLineEmpty && !SphereLine && PreviousLineEmpty && AtSphereData) { AtLocationData = true; }
                if (AtLocationData) { ReleventSpoilerlines.Add(x); }
            }
            return ReleventSpoilerlines;
        }

        public static void ScrapeLocationData()
        {
            System.Net.WebClient client = new System.Net.WebClient();

            string[] MMPoolWebData = client.DownloadString("https://raw.githubusercontent.com/OoTMM/core/develop/data/mm/pool.csv").Split( new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToArray();
            var mmPool = ConvertCsvFileToJsonObject(MMPoolWebData.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());
            var mmPoolObj = JsonConvert.DeserializeObject<List<MMROOTLocation>>(mmPool);

            string[] OOTPoolWebData = client.DownloadString("https://raw.githubusercontent.com/OoTMM/core/develop/data/oot/pool.csv").Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToArray();
            var ootPool = ConvertCsvFileToJsonObject(OOTPoolWebData.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());
            var ootPoolObj = JsonConvert.DeserializeObject<List<MMROOTLocation>>(ootPool);

            foreach(var i in OOTRCheckDict.Keys)
            {
                var OOTCheck = ootPoolObj.Find(x => $"OOT {x.location}" == i);
                var MMRCheck = mmPoolObj.Find(x => $"MM {x.location}" == i);
                if (OOTCheck is not null)
                    OOTRCheckDict[i] = new string[] { OOTCheck.scene, $"OOT_{OOTCheck.item}" };
                else if (MMRCheck is not null)
                    OOTRCheckDict[i] = new string[] { MMRCheck.scene, $"MM_{MMRCheck.item}" };
            }

            TrackerObjects.MMRData.LogicFile logicFile = new TrackerObjects.MMRData.LogicFile();
            logicFile.Version = 1;
            logicFile.GameCode = "OOTMM";
            logicFile.Logic = new List<TrackerObjects.MMRData.JsonFormatLogicItem>();

            TrackerObjects.LogicDictionaryData.LogicDictionary logicDictionary = new TrackerObjects.LogicDictionaryData.LogicDictionary();
            logicDictionary.LogicVersion = 1;
            logicDictionary.GameCode = "OOTMM";
            logicDictionary.LocationList = new List<TrackerObjects.LogicDictionaryData.DictionaryLocationEntries>();
            logicDictionary.ItemList = new List<TrackerObjects.LogicDictionaryData.DictionaryItemEntries>();

            foreach (var i in OOTRCheckDict.Keys)
            {
                string[] DictValue = OOTRCheckDict[i] as string[];
                logicFile.Logic.Add(new TrackerObjects.MMRData.JsonFormatLogicItem() { Id = i });
                TrackerObjects.LogicDictionaryData.DictionaryLocationEntries dictEntry = new TrackerObjects.LogicDictionaryData.DictionaryLocationEntries();
                dictEntry.ID = i;
                dictEntry.SpoilerData = new TrackerObjects.MMRData.SpoilerlogReference { SpoilerLogNames = new string[] { i } };
                dictEntry.Name = i;
                dictEntry.ValidItemTypes = new string[] { "item" };
                dictEntry.Area = DictValue[0];
                dictEntry.OriginalItem = DictValue[1];
                logicDictionary.LocationList.Add(dictEntry);
            }
            foreach(var i in OOTRItemsDict.Keys)
            {
                if (OOTRItemsDict[i] is bool ib && !ib) { continue; }
                TrackerObjects.LogicDictionaryData.DictionaryItemEntries dictItem = new TrackerObjects.LogicDictionaryData.DictionaryItemEntries();
                string ItemName = i;
                List<string> ExtraSpoilerNames = new List<string>();
                if (OOTRItemsDict[i] is System.String) { ItemName = (string)OOTRItemsDict[i]; }
                else 
                { 
                    ItemName = OOTRItemsDict[i][0];
                    ExtraSpoilerNames = OOTRItemsDict[i][1].ToObject<List<string>>();
                }

                if (i.StartsWith("OOT_"))
                {
                    ItemName = $"OOT {ItemName}";
                }
                else if (i.StartsWith("MM_"))
                {
                    ItemName = $"MM {ItemName}";
                }


                dictItem.Name = ItemName;
                dictItem.MaxAmountInWorld = -1;
                dictItem.ValidStartingItem = true;
                dictItem.ID = i;
                dictItem.SpoilerData = new TrackerObjects.MMRData.SpoilerlogReference { SpoilerLogNames = new string[] { i } };
                ExtraSpoilerNames.ForEach(x => dictItem.SpoilerData.SpoilerLogNames = dictItem.SpoilerData.SpoilerLogNames.Append(x).ToArray());
                dictItem.ItemTypes = new string[] { "item" };
                logicDictionary.ItemList.Add(dictItem);
            }

            File.WriteAllText(Path.Combine(TestFolder, @"OOTMMLogic.json"), JsonConvert.SerializeObject(logicFile, Testing._NewtonsoftJsonSerializerOptions));
            File.WriteAllText(Path.Combine(TestFolder, @"OOTMMDictionary.json"), JsonConvert.SerializeObject(logicDictionary, Testing._NewtonsoftJsonSerializerOptions));

        }

        public static string ConvertCsvFileToJsonObject(string[] lines)
        {
            var csv = new List<string[]>();

            var properties = lines[0].Split(',');

            foreach (string line in lines)
            {
                var LineData = line.Split(',');
                csv.Add(LineData);
            }

            var listObjResult = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                var objResult = new Dictionary<string, string>();
                for (int j = 0; j < properties.Length; j++)
                    objResult.Add(properties[j].Trim(), csv[i][j].Trim());

                listObjResult.Add(objResult);
            }

            return JsonConvert.SerializeObject(listObjResult, Testing._NewtonsoftJsonSerializerOptions);
        }
    }
}
