﻿using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjects;

namespace CLIFrontEnd
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                CLISelectMenu MenuSelect = new(["New (From File)", "New (From Preset)", "Load", "Exit"]);

                InstanceData.InstanceContainer container = new();

                bool ShouldExit = false;
                bool Success = false;
                string Error = null;
                while (!Success)
                {
                    MenuSelect.Run(string.IsNullOrWhiteSpace(Error) ? [] : [Error]);
                    switch (MenuSelect.selectedLineIndex)
                    {
                        case 0:
                            (Success, Error) = LoadFromLogicFile(container);
                            break;
                        case 1:
                            (Success, Error) = LoadFromTemplate(container);
                            break;
                        case 2:
                            (Success, Error) = LoadFromSaveFile(container);
                            break;
                        case 3:
                            Success = true;
                            ShouldExit = true;
                            break;
                    }
                }
                if (ShouldExit) { break; }
                MainDisplay mainDisplay = new(container);
                mainDisplay.Display();
            }
        }

        private static (bool, string) LoadFromSaveFile(InstanceData.InstanceContainer container)
        {
            var Result = NativeFileDialogSharp.Dialog.FileOpen("mmrtsav");
            if (Result.IsCancelled) { return (false, ""); }
            if (Result.IsError) { return (false, Result.ErrorMessage); }
            if (!Path.Exists(Result.Path)) { return (false, $"{Result.Path} was not a valid path"); }
            var Applied = container.LoadInsanceFromFile(Result.Path);
            return (Applied, "Could not Load Save");
        }
        private static (bool, string) LoadFromLogicFile(InstanceData.InstanceContainer container)
        {
            var Result = NativeFileDialogSharp.Dialog.FileOpen("txt,json");
            if (Result.IsCancelled) { return (false, ""); }
            if (Result.IsError) { return (false, Result.ErrorMessage); }
            if (!Path.Exists(Result.Path)) { return (false, $"{Result.Path} was not a valid path"); }
            var Applied = container.GenerateInstance(File.ReadAllText(Result.Path));
            return Applied switch
            {
                TrackerInstanceCreation.InstanceState.Success => (true, "success"),
                TrackerInstanceCreation.InstanceState.LogicFailure => (false, "Logic File was invalid"),
                TrackerInstanceCreation.InstanceState.DictionaryFailure => (false, "Could not find a valid dictionary for logic file"),
                _ => (false, "Failed to create Instance"),
            };
        }

        private static (bool, string) LoadFromTemplate(InstanceData.InstanceContainer container)
        {
            Console.Clear();
            Console.WriteLine("Getting Presets...");
            var Templates = LogicPresetHandeling.GetLogicPresets();
            if (Templates.Count < 1) { return (false, "No Presets Available"); }
            var MenuSelect = new CLISelectMenu(Templates);
            var SelectedTemplate = (LogicPresetHandeling.PresetlogicData)MenuSelect.Run();
            var Applied = container.GenerateInstance(SelectedTemplate.LogicString, SelectedTemplate.DictionaryString);
            return Applied switch
            {
                TrackerInstanceCreation.InstanceState.Success => (true, "success"),
                TrackerInstanceCreation.InstanceState.LogicFailure => (false, "Logic File was invalid"),
                TrackerInstanceCreation.InstanceState.DictionaryFailure => (false, "Could not find a valid dictionary for logic file"),
                _ => (false, "Failed to create Instance"),
            };
        }
    }
}