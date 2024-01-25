using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Diagnostics;
using System.IO;

namespace MMR_Tracker_V3.Logic
{
    public class LogicFileParser
    {
        /// <summary>
        /// Reads the logic data from a logic file.
        /// </summary>
        /// <param name="File">The lines of the spoiler log as an array</param>
        /// <param name="WasSpoilerLog">returns true if the file was a spoiler log and conatined spoiler data</param>
        /// <returns>The Logic data as a string array</returns>
        public static string[] GetLogicData(string[] File)
        {
            return ParseFile(File);
        }
        /// <summary>
        /// Reads the logic data from a logic file.
        /// </summary>
        /// <param name="LogicFile">Either the file path to the logicfile/spoiler log or the contents of the file as a string.</param>
        /// <param name="WasSpoilerLog">returns true if the file was a spoiler log and conatined spoiler data</param>
        /// <returns>The Logic data as a string array</returns>
        public static string[] GetLogicData(string LogicFile)
        {
            if (File.Exists(LogicFile))
            {
                return ParseFile(File.ReadAllLines(LogicFile));
            }
            else
            {
                string[] lines = LogicFile.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                return ParseFile(lines);
            }
        }

        private static string[] ParseFile(string[] File)
        {
            if (TestLogicFileValid(File))
            {
                Debug.WriteLine("Entry Was Logic File");
                return File;
            }
            return null;
        }

        private static bool TestLogicFileValid(string[] LogFile)
        {
            try
            {
                var LogicFile = MMRData.LogicFile.FromJson(string.Join("", LogFile));
                return LogicFile is not null;
            }
            catch { return false; }
        }
    }
}
