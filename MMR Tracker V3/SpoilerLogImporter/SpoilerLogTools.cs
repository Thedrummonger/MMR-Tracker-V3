using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;

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
                "LAS" => "Links Awakening Switch Rando Spoiler Log|*.txt",
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
            bool LogImported = true;
            switch (container.Instance.LogicFile.GameCode)
            {
                case "OOTMM":
                    container.Instance.SpoilerLog = new InstanceData.SpoilerLogFileData { FileName = OriginalFile, Log = spoilerLog };
                    OOTMMSpoilerLogTools.readAndApplySpoilerLog(container.Instance);
                    //container.Instance.SpoilerLog.GetStaticPlaythrough(container.Instance);
                    break;
                case "MMR":
                    container.Instance.SpoilerLog = new InstanceData.SpoilerLogFileData { FileName = OriginalFile, Log = spoilerLog };
                    MMRSpoilerLogTools.ReadAndApplySpoilerLog(container);
                    //container.Instance.SpoilerLog.GetStaticPlaythrough(container.Instance);
                    break;
                case "TPR":
                    container.Instance.SpoilerLog = new InstanceData.SpoilerLogFileData { FileName = OriginalFile, Log = spoilerLog };
                    TPRSpoilerLogTools.readAndApplySpoilerLog(container.Instance);
                    //container.Instance.SpoilerLog.GetStaticPlaythrough(container.Instance);
                    break;
                case "PMR":
                    container.Instance.SpoilerLog = new InstanceData.SpoilerLogFileData { FileName = OriginalFile, Log = spoilerLog };
                    PMRSpoilerLogTools.ParseSpoiler(container.Instance);
                    //container.Instance.SpoilerLog.GetStaticPlaythrough(container.Instance);
                    break;
                case "LAS":
                    container.Instance.SpoilerLog = new InstanceData.SpoilerLogFileData { FileName = OriginalFile, Log = spoilerLog };
                    LASSpoilerLogTools.ParseSpoiler(container.Instance);
                    //container.Instance.SpoilerLog.GetStaticPlaythrough(container.Instance);
                    break;
                default:
                    LogImported = false; break;
            }
            container.logicCalculation.CompileOptionActionEdits();
            return LogImported;
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
            instance.SpoilerLog = null;
        }

        public static bool CheckForSpoilerLog(InstanceData.TrackerInstance logic)
        {
            return logic.LocationPool.Values.Any(x => !string.IsNullOrWhiteSpace(x.Randomizeditem.SpoilerLogGivenItem));
        }
    }
}
