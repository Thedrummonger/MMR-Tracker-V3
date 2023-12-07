
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingForm
{
    internal class CLITrackerTesting
    {

        public static void OpenCLITracker()
        {
            DLLImport.AllocConsole();
            DLLImport.ShowWindow(DLLImport.GetConsoleWindow(), DLLImport.SW_SHOW);
            //CLITracker.Main(Environment.GetCommandLineArgs());
        }

        public static void HideCLI()
        {
            Debug.WriteLine("Hiding CLI");
            DLLImport.ShowWindow(DLLImport.GetConsoleWindow(), DLLImport.SW_HIDE);
        }
    }
}
