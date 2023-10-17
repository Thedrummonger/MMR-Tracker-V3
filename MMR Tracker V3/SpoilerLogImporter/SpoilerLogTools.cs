using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static FParsec.ErrorMessage;
using static MMR_Tracker_V3.TrackerObjects.HintData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.SpoilerLogImporter
{
    public static class SpoilerLogTools
    {
        public static string GetSpoilerLogFilter(InstanceData.TrackerInstance Instance)
        {
            return Instance.LogicFile.GameCode switch
            {
                "OOTR" => "Ocarina of Time Rando Spoiler Log (*.json)|*.json",
                "OOTMM" => "OOT X MM Combo Rando Spoiler Log (*.txt)|*.txt",
                "TPR" => "Twilight Princess Rando Spoiler Log|*.json",
                "PMR" => "Paper Mario Rando Spoiler Log|*.txt",
                _ => "Majoras Mask Randomizer Text Spoiler Log|*.txt",
            };
        }

        public static bool ImportSpoilerLog(string spoilerLog, InstanceContainer container)
        {
            if (File.Exists(spoilerLog))
            {
                return ImportSpoilerLog(File.ReadLines(spoilerLog).ToArray(), spoilerLog, container);
            }
            else
            {
                string[] lines = spoilerLog.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                return ImportSpoilerLog(lines, spoilerLog, container);
            }
        }
        public static bool ImportSpoilerLog(string spoilerLog, string OriginalFile, InstanceContainer container)
        {
            if (File.Exists(spoilerLog))
            {
                return ImportSpoilerLog(File.ReadLines(spoilerLog).ToArray(), OriginalFile, container);
            }
            else
            {
                string[] lines = spoilerLog.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                return ImportSpoilerLog(lines, OriginalFile, container);
            }
        }

        public static bool ImportSpoilerLog(string[] spoilerLog, string OriginalFile, InstanceContainer container)
        {
            container.logicCalculation.ResetAutoObtainedItems();
            switch (container.Instance.LogicFile.GameCode)
            {
                case "OOTMM":
                    container.Instance.SpoilerLog = new InstanceData.SpoilerLogFileData { FileName = OriginalFile, Log = spoilerLog };
                    OOTMMSpoilerLogTools.readAndApplySpoilerLog(container.Instance);
                    container.Instance.EntrancePool.IsEntranceRando = container.Instance.EntrancePool.CheckForRandomEntrances(container.Instance);
                    container.Instance.SpoilerLog.GetStaticPlaythrough(container.Instance);
                    return true;
                case "MMR":
                    container.Instance.SpoilerLog = new InstanceData.SpoilerLogFileData { FileName = OriginalFile, Log = spoilerLog };
                    MMRSpoilerLogTools.ReadAndApplySpoilerLog(container.Instance);
                    container.Instance.EntrancePool.IsEntranceRando = container.Instance.EntrancePool.CheckForRandomEntrances(container.Instance);
                    container.Instance.SpoilerLog.GetStaticPlaythrough(container.Instance);
                    return true;
                case "TPR":
                    container.Instance.SpoilerLog = new InstanceData.SpoilerLogFileData { FileName = OriginalFile, Log = spoilerLog };
                    TPRSpoilerLogTools.readAndApplySpoilerLog(container.Instance);
                    container.Instance.SpoilerLog.GetStaticPlaythrough(container.Instance);
                    return true;
                case "PMR":
                    container.Instance.SpoilerLog = new InstanceData.SpoilerLogFileData { FileName = OriginalFile, Log = spoilerLog };
                    PMRSpoilerLogTools.ParseSpoiler(container.Instance);
                    container.Instance.SpoilerLog.GetStaticPlaythrough(container.Instance);
                    return true;
                default:
                    return false;
            }
        }

        public static void RemoveSpoilerData(this InstanceData.TrackerInstance instance)
        {
            foreach (var i in instance.LocationPool.Values)
            {
                i.Randomizeditem.SpoilerLogGivenItem = null;
                i.SetPrice(-1);
            }
            foreach (var i in instance.MacroPool.Values)
            {
                i.SetPrice(-1);
            }
            foreach (var i in instance.HintPool.Values)
            {
                i.SpoilerHintText = null;
            }
        }
    }
}
