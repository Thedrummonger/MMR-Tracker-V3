using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.SpoilerLogHandling.Python
{
    public class Test
    {
        public static string ReadFromPyFile(string ParserFile)
        {
            Process process = new();
            process.StartInfo.FileName = @"python.exe";
            process.StartInfo.Arguments = $"\"{ParserFile}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.WaitForExit();
            return process.StandardOutput.ReadToEnd();
        }
    }
}
