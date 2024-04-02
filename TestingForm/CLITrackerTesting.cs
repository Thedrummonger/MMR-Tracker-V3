
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows_Form_Frontend;

namespace TestingForm
{
    public class CLITrackerTesting
    {
        public static bool CLIActive = false;

        public static void OpenCLITracker()
        {
            DLLImport.AllocConsole();
            DLLImport.ShowWindow(DLLImport.GetConsoleWindow(), DLLImport.SW_SHOW);
            CLIFrontEnd.Program.Main(Environment.GetCommandLineArgs());
            CLIActive = true;
        }
        public static void Program_CloseForm()
        {
            CLIActive = false;
            if (!CLIInUse())
            {

            }
        }

        public static bool CLIInUse()
        {
            return CLIActive || MainInterface.CurrentProgram is not null;
        }
    }
}
