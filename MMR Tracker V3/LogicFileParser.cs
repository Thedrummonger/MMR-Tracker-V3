using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3
{
    public class LogicFileParser
    {
        /// <summary>
        /// Reads the logic data from a logic file or spoiler log.
        /// </summary>
        /// <param name="File">The lines of the spoiler log as an array</param>
        /// <param name="WasSpoilerLog">returns true if the file was a spoiler log and conatined spoiler data</param>
        /// <returns>The Logic data as a string array</returns>
        public static string[] GetLogicData(string[] File, out bool WasSpoilerLog)
        {
            return ParseFile(File, out WasSpoilerLog);
        }
        /// <summary>
        /// Reads the logic data from a logic file or spoiler log.
        /// </summary>
        /// <param name="LogicFile">Either the file path to the logicfile/spoiler log or the contents of the file as a string.</param>
        /// <param name="WasSpoilerLog">returns true if the file was a spoiler log and conatined spoiler data</param>
        /// <returns>The Logic data as a string array</returns>
        public static string[] GetLogicData(string LogicFile, out bool WasSpoilerLog)
        {
            if (File.Exists(LogicFile))
            {
                return ParseFile(File.ReadAllLines(LogicFile), out WasSpoilerLog);
            }
            else
            {
                string[] lines = LogicFile.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                return ParseFile(lines, out WasSpoilerLog);
            }
        }

        private static string[] ParseFile(string[] File, out bool WasSpoilerLog)
        {
            WasSpoilerLog = false;
            if (TestForSpoilerLogLogic(File, out string[] Logic))
            {
                Debug.WriteLine("Entry Was Spoiler Log");
                WasSpoilerLog = true;
                return Logic;
            }
            else if (TestLogicFileValid(File))
            {
                Debug.WriteLine("Entry Was Logic File");
                return File;
            }
            return null;
        }
        private static bool TestForSpoilerLogLogic(string[] LogFile, out string[] Logic)
        {
            Logic = null;
            MMRData.SpoilerLogData LogData = SpoilerLogTools.ReadSpoilerLog(LogFile);
            if ( LogData is null || LogData.GameplaySettings is null || LogData.GameplaySettings.LogicMode is null) { return false; }
            if (LogData.GameplaySettings.LogicMode == "UserLogic")
            {
                if (!File.Exists(LogData.GameplaySettings.UserLogicFileName)) { return false; }
                var UserLogicFile = File.ReadAllLines(LogData.GameplaySettings.UserLogicFileName);
                if (TestLogicFileValid(UserLogicFile)) { Logic = UserLogicFile; return true; }
                return false;
            }
            else if (LogData.GameplaySettings.LogicMode == "Casual")
            {
                WebClient wc = new WebClient();
                try
                {
                    string Paste = wc.DownloadString("https://raw.githubusercontent.com/ZoeyZolotova/mm-rando/dev/MMR.Randomizer/Resources/REQ_CASUAL.txt");
                    var UserLogicFile = Paste.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                    if (TestLogicFileValid(UserLogicFile)) { Logic = UserLogicFile; return true; }
                    return false;
                }
                catch { return false; }
            }
            else if (LogData.GameplaySettings.LogicMode == "Glitched")
            {
                WebClient wc = new WebClient();
                try
                {
                    string Paste = wc.DownloadString("https://raw.githubusercontent.com/ZoeyZolotova/mm-rando/dev/MMR.Randomizer/Resources/REQ_GLITCH.txt");
                    var UserLogicFile = Paste.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                    if (TestLogicFileValid(UserLogicFile)) { Logic = UserLogicFile; return true; }
                    return false;
                }
                catch { return false; }
            }
            return false;
        }
        private static bool TestLogicFileValid(string[] LogFile)
        {
            try
            {
                var LogicFile = MMRData.LogicFile.FromJson(String.Join("", LogFile));
                return true;
            }
            catch { return false; }
        }
    }
}
