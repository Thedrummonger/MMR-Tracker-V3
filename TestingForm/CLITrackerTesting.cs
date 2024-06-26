﻿using System.Diagnostics;

namespace TestingForm
{
    public class CLITrackerTesting
    {
        public static TextWriterTraceListener CLIDebugListener = null;
        public static bool CLITrackerActive = false;

        public static void OpenCLITracker()
        {
            CLITrackerActive = true;
            RemoveCLIDebugListener();
            DLLImport.ShowWindow(DLLImport.GetConsoleWindow(), DLLImport.SW_SHOW);
            TestingForm.CurrentForm.UpdateDebugActions();
            Task.Run(() =>
            {
                CLIFrontEnd.Program.Main(Environment.GetCommandLineArgs());
                //freezes thread while CLI is running. Continues here when CLI exits
                Console.Clear();
                DLLImport.ShowWindow(DLLImport.GetConsoleWindow(), DLLImport.SW_HIDE);
                CLITrackerActive = false;
                TestingForm.CurrentForm.Invoke(new MethodInvoker(delegate ()
                {
                    TestingForm.CurrentForm.UpdateDebugActions();
                }));
            });
        }

        public static bool IsCLIActive() { return CLITrackerActive; }
        public static bool IsCLIInactive() { return !CLITrackerActive; }

        public static void AddCLIDebugListener()
        {
            if (!Trace.Listeners.Contains(CLIDebugListener) && IsCLIInactive()) {  
                Trace.Listeners.Add(CLIDebugListener);
                DLLImport.ShowWindow(DLLImport.GetConsoleWindow(), DLLImport.SW_SHOW);
            }
        }
        public static void RemoveCLIDebugListener()
        {
            if (Trace.Listeners.Contains(CLIDebugListener)) {
                Trace.Listeners.Remove(CLIDebugListener); 
                if (IsCLIInactive()) {
                    DLLImport.ShowWindow(DLLImport.GetConsoleWindow(), DLLImport.SW_HIDE);
                }
            }
        }
        public static bool IsCLIDebugListenerActive()
        {
            return Trace.Listeners.Contains(CLIDebugListener) || IsCLIActive();
        }
        public static bool IsCLIDebugListenerInactive()
        {
            return !Trace.Listeners.Contains(CLIDebugListener) && IsCLIInactive();
        }
    }
}
