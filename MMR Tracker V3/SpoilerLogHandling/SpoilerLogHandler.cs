using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TDMUtils;

namespace MMR_Tracker_V3.SpoilerLogHandling
{
    public class GenericSpoilerLog
    {
        public Dictionary<string, string> LocationAssignment = [];
        public Dictionary<string, int> LocationPlayerAssignment = [];
        public Dictionary<string, (string From, string Region)> EntranceAssignments = [];
        public Dictionary<string, string> HintAssignment = [];
        public Dictionary<string, Dictionary<string, string>> HintLocationDataAssignment = [];
        public Dictionary<string, int> IntOptionAssignment = [];
        public Dictionary<string, bool> ToggleOptionAssignment = [];
        public Dictionary<string, string> ChoiceOptionAssignment = [];
        public Dictionary<string, string[]> MutliOptionAssignment = [];
        public Dictionary<string, int> StartingItems = [];
        public List<string> JunkLocations = [];
        public List<string> JunkEntrances = [];
        public List<string> UnrandomizedLocations = [];
        public List<string> UnrandomizedEntrances = [];
        public List<string> ManualLocations = [];
        public List<string> ManualEntrances = [];
        public List<string> JunkHints = [];
        public Dictionary<string, (int? Price, char? Currency)> LocationPricing = [];
        public Dictionary<string, (int? Price, char? Currency)> EntrancePricing = [];
        public Dictionary<string, (int? Price, char? Currency)> HintPricing = [];
        public Dictionary<string, (int? Price, char? Currency)> MacroPricing = [];
        public List<string> EnabledTricks = [];
        public bool ItemHasSpoilerData(string ID)
        {
            return LocationAssignment.ContainsKey(ID) || 
                JunkLocations.Contains(ID) || 
                ManualLocations.Contains(ID) || 
                UnrandomizedLocations.Contains(ID); 
        }
        public static GenericSpoilerLog CreateFromInstanceState(InstanceData.TrackerInstance instance)
        {
            GenericSpoilerLog Result = new GenericSpoilerLog();
            Result.LocationAssignment = instance.LocationPool.ToDictionary(x => x.Key, x => x.Value.Randomizeditem.SpoilerLogGivenItem);
            Result.LocationPlayerAssignment = instance.LocationPool.ToDictionary(x => x.Key, x => x.Value.Randomizeditem.OwningPlayer);
            Result.LocationPricing = instance.LocationPool.ToDictionary(x => x.Key, x => (x.Value.Price, x.Value.Currency));
            Result.JunkLocations = instance.LocationPool.Where(x => x.Value.IsJunk()).Select(x => x.Key).ToList();
            Result.UnrandomizedLocations = instance.LocationPool.Where(x => x.Value.IsUnrandomized(MiscData.UnrandState.Unrand)).Select(x => x.Key).ToList();
            Result.ManualLocations = instance.LocationPool.Where(x => x.Value.IsUnrandomized(MiscData.UnrandState.Manual)).Select(x => x.Key).ToList();

            foreach (var i in instance.ExitPool.Where(x => x.Value.SpoilerDefinedDestinationExit is not null))
            {
                var Dest = i.Value.SpoilerDefinedDestinationExit;
                Result.EntranceAssignments.Add(i.Key, (Dest.from, Dest.region));
            }
            Result.EntrancePricing = instance.ExitPool.ToDictionary(x => x.Key, x => (x.Value.Price, x.Value.Currency));
            Result.JunkEntrances = instance.ExitPool.Where(x => x.Value.IsJunk()).Select(x => x.Key).ToList();
            Result.UnrandomizedEntrances = instance.ExitPool.Where(x => x.Value.IsUnrandomized(MiscData.UnrandState.Unrand)).Select(x => x.Key).ToList();
            Result.ManualEntrances = instance.ExitPool.Where(x => x.Value.IsUnrandomized(MiscData.UnrandState.Manual)).Select(x => x.Key).ToList();

            Result.HintAssignment = instance.HintPool.ToDictionary(x => x.Key, x => x.Value.SpoilerHintText);
            Result.HintLocationDataAssignment = instance.HintPool.ToDictionary(x => x.Key, x => x.Value.ParsedHintData);
            Result.HintPricing = instance.HintPool.ToDictionary(x => x.Key, x => (x.Value.Price, x.Value.Currency));
            Result.JunkHints = instance.HintPool.Where(x => x.Value.IsJunk()).Select(x => x.Key).ToList();

            Result.ChoiceOptionAssignment = instance.ChoiceOptions.ToDictionary(x => x.Key, x => x.Value.Value);
            Result.IntOptionAssignment = instance.IntOptions.ToDictionary(x => x.Key, x => x.Value.Value);
            Result.ToggleOptionAssignment = instance.ToggleOptions.ToDictionary(x => x.Key, x => x.Value.Value);
            Result.MutliOptionAssignment = instance.MultiSelectOptions.ToDictionary(x => x.Key, x => x.Value.EnabledValues.ToArray());

            Result.StartingItems = instance.ItemPool.ToDictionary(x => x.Key, x => x.Value.AmountInStartingpool);
            Result.EnabledTricks = instance.MacroPool.Where(x => x.Value.isTrick() && x.Value.TrickEnabled).Select(x => x.Key).ToList();
            Result.MacroPricing = instance.MacroPool.ToDictionary(x => x.Key, x => (x.Value.Price, x.Value.Currency));
            return Result;
        }
    }

    public class Parser
    {
        public static void ApplyGenericSpoilerLog(InstanceData.InstanceContainer IC, GenericSpoilerLog spoilerLog)
        {
            ResetInstance(IC);
            foreach (var i in spoilerLog.LocationAssignment) { IC.Instance.LocationPool[i.Key].Randomizeditem.SpoilerLogGivenItem = i.Value; }
            foreach (var i in spoilerLog.LocationPlayerAssignment) { IC.Instance.LocationPool[i.Key].Randomizeditem.OwningPlayer = i.Value; }
            foreach (var i in spoilerLog.LocationPricing) { IC.Instance.LocationPool[i.Key].SetPrice(i.Value.Price ?? -1, i.Value.Currency ?? '\0'); }
            foreach (var i in spoilerLog.JunkLocations) { IC.Instance.LocationPool[i].SetRandomizedState(MiscData.RandomizedState.ForcedJunk); }
            foreach (var i in spoilerLog.UnrandomizedLocations) { IC.Instance.LocationPool[i].SetRandomizedState(MiscData.RandomizedState.Unrandomized); }
            foreach (var i in spoilerLog.ManualLocations) { IC.Instance.LocationPool[i].SetRandomizedState(MiscData.RandomizedState.UnrandomizedManual); }

            foreach (var i in spoilerLog.EntranceAssignments) { IC.Instance.ExitPool[i.Key].SpoilerDefinedDestinationExit = 
                    new EntranceData.EntranceRandoDestination { from = i.Value.From, region = i.Value.Region }; }
            foreach (var i in spoilerLog.EntrancePricing) { IC.Instance.ExitPool[i.Key].SetPrice(i.Value.Price ?? -1, i.Value.Currency ?? '\0'); }
            foreach (var i in spoilerLog.JunkEntrances) { IC.Instance.ExitPool[i].SetRandomizedState(MiscData.RandomizedState.ForcedJunk); }
            foreach (var i in spoilerLog.UnrandomizedEntrances) { IC.Instance.ExitPool[i].SetRandomizedState(MiscData.RandomizedState.Unrandomized); }
            foreach (var i in spoilerLog.ManualEntrances) { IC.Instance.ExitPool[i].SetRandomizedState(MiscData.RandomizedState.UnrandomizedManual); }

            foreach (var i in spoilerLog.HintAssignment) { IC.Instance.HintPool[i.Key].SpoilerHintText = i.Value; }
            foreach (var i in spoilerLog.HintLocationDataAssignment) { IC.Instance.HintPool[i.Key].ParsedHintData = i.Value; }
            foreach (var i in spoilerLog.JunkHints) { IC.Instance.HintPool[i].SetRandomizedState(MiscData.RandomizedState.ForcedJunk); }
            foreach (var i in spoilerLog.HintPricing) { IC.Instance.HintPool[i.Key].SetPrice(i.Value.Price ?? -1, i.Value.Currency ?? '\0'); }

            foreach (var i in spoilerLog.IntOptionAssignment) { IC.Instance.IntOptions[i.Key].SetValue(i.Value); }
            foreach (var i in spoilerLog.ChoiceOptionAssignment) { IC.Instance.ChoiceOptions[i.Key].SetValue(i.Value); }
            foreach (var i in spoilerLog.ToggleOptionAssignment) { IC.Instance.ToggleOptions[i.Key].SetValue(i.Value); }
            foreach (var i in spoilerLog.MutliOptionAssignment) { IC.Instance.MultiSelectOptions[i.Key].SetValues(i.Value); }

            foreach (var i in spoilerLog.StartingItems) { IC.Instance.ItemPool[i.Key].AmountInStartingpool = i.Value; }
            foreach (var i in spoilerLog.EnabledTricks) { IC.Instance.MacroPool[i].TrickEnabled = true; }
            foreach (var i in spoilerLog.MacroPricing) { IC.Instance.MacroPool[i.Key].SetPrice(i.Value.Price ?? -1, i.Value.Currency ?? '\0'); }
            IC.logicCalculation.CompileOptionActionEdits();
            IC.Instance.SpoilerLog = new InstanceData.SpoilerLogFileData() { Log = spoilerLog, };
            IC.Instance.SpoilerLog.GetStaticPlaythrough(IC);
        }
        public static void ResetInstance(InstanceData.InstanceContainer IC)
        {
            IC.logicCalculation.ResetAutoObtainedItems();
            foreach (var i in IC.Instance.LocationPool.Values) { i.SetRandomizedState(MiscData.RandomizedState.Randomized); }
            foreach (var i in IC.Instance.LocationPool.Values) { i.Randomizeditem.SpoilerLogGivenItem = null; }
            foreach (var i in IC.Instance.LocationPool.Values) { i.Randomizeditem.OwningPlayer = -1; }
            foreach (var i in IC.Instance.LocationPool.Values) { i.SetPrice(-1, '\0'); }

            foreach (var i in IC.Instance.ExitPool.Values) { i.SetRandomizedState(MiscData.RandomizedState.Randomized); }
            foreach (var i in IC.Instance.ExitPool.Values) { i.SpoilerDefinedDestinationExit = null; }
            foreach (var i in IC.Instance.ExitPool.Values) { i.SetPrice(-1, '\0'); }

            foreach (var i in IC.Instance.HintPool.Values) { i.SetRandomizedState(MiscData.RandomizedState.Randomized); }
            foreach (var i in IC.Instance.HintPool.Values) { i.SpoilerHintText = null; }
            foreach (var i in IC.Instance.HintPool.Values) { i.ParsedHintData = []; }
            foreach (var i in IC.Instance.HintPool.Values) { i.SetPrice(-1, '\0'); }

            foreach (var i in IC.Instance.IntOptions.Values) { i.SetValue(i.GetDictEntry().Value); }
            foreach (var i in IC.Instance.ChoiceOptions.Values) { i.SetValue(i.GetDictEntry().Value); }
            foreach (var i in IC.Instance.ToggleOptions.Values) { i.SetValue(i.GetDictEntry().Value); }
            foreach (var i in IC.Instance.MultiSelectOptions.Values) { i.SetValues(i.GetDictEntry().EnabledValues); }

            foreach (var i in IC.Instance.ItemPool.Values) { i.AmountInStartingpool = 0; }
            foreach (var i in IC.Instance.MacroPool.Values.Where(x => x.isTrick())) { i.TrickEnabled = false; }
            foreach (var i in IC.Instance.MacroPool.Values) { i.SetPrice(-1, '\0'); }
        }
    }
    public class SpoilerLogInstructions
    {
        /// <summary>
        /// Each key in this dict will prompt for a file, read the file and save the contents to the data store under the given key
        /// </summary>
        public Dictionary<string, (string Description, string[] FileExtentions)> FileImports;
        /// <summary>
        /// The path to the external parse that will be given the serialized data store and return a generic Spoiler log. Alternatively, supply a parser ID to use a hardcoded Parser
        /// </summary>
        public string ParserPath;
        /// <summary>
        /// Each key in this dict will prompt for a file, read the file and save the contents to the data store under the given key
        /// </summary>
        public Dictionary<string, (string Description, string[] FileExtentions)> ArchipelagoFileImports;
        /// <summary>
        /// The path to the external parse that will be given the serialized data store and return a generic Spoiler log. Alternatively, supply a parser ID to use a hardcoded Parser
        /// </summary>
        public string ArchipelagoParserPath;
        public Dictionary<string, object> CreateDataStore(Func<string[], string, string[]> FileSelectorReader)
        {
            Dictionary<string, object> DataStore = [];
            foreach(var file in FileImports ?? []) { DataStore.Add(file.Key, FileSelectorReader(file.Value.FileExtentions, file.Value.Description)); }
            return DataStore;
        }
        public Dictionary<string, object> CreateDataStore(Func<string[], string, string[]> FileSelectorReader, Archipelago.GenericAPSpoiler APData)
        {
            Dictionary<string, object> DataStore = new() { { "Archipelago", APData } };
            foreach (var file in ArchipelagoFileImports ?? []) { DataStore.Add(file.Key, FileSelectorReader(file.Value.FileExtentions, file.Value.Description)); }
            return DataStore;
        }

        public bool SupportsSpoiler()
        {
            return !ParserPath.IsNullOrWhiteSpace() && FileImports.Count > 0;
        }
        public bool SupportsAPSpoiler()
        {
            return !ArchipelagoParserPath.IsNullOrWhiteSpace();
        }
    }
    public static class SpoilerTools
    {
        public static bool ApplySpoilerLog(InstanceData.InstanceContainer IC, Func<string[], string, string[]> FileSelectorReader)
        {
            bool IsArchipellago = IC.netConnection.IsConnected() && IC.netConnection.OnlineMode == NetCode.NetData.OnlineMode.Archipelago;
            SpoilerLogInstructions instructions = IC.Instance.LogicDictionary.SpoilerLogInstructions;
            if (instructions is null) { return false; }
            Dictionary<string, object> DataStore;
            string ParserPath;
            if (IsArchipellago)
            {
                ParserPath = instructions.ArchipelagoParserPath;
                if (string.IsNullOrEmpty(ParserPath) ) { Debug.WriteLine("No Valid Parser Path"); return false; }
                var APSpoiler = Archipelago.CreateGenericSpoilerLog(IC);
                DataStore = instructions.CreateDataStore(FileSelectorReader, Archipelago.CreateGenericSpoilerLog(IC));
            }
            else
            {
                ParserPath = instructions.ParserPath;
                if (string.IsNullOrEmpty(ParserPath)) { Debug.WriteLine("No Valid Parser Path"); return false; }
                DataStore = instructions.CreateDataStore(FileSelectorReader);
            }
            var GenericSpoilerLog = HardCodedParser(IC, ParserPath, DataStore);
            if (GenericSpoilerLog is not null) 
            { 
                Parser.ApplyGenericSpoilerLog(IC, GenericSpoilerLog);
                return true;
            }
            GenericSpoilerLog = PythonParse(instructions, ParserPath, DataStore);
            if (GenericSpoilerLog is not null) 
            { 
                Parser.ApplyGenericSpoilerLog(IC, GenericSpoilerLog);
                return true;
            }
            return false;
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

        private static GenericSpoilerLog HardCodedParser(InstanceData.InstanceContainer IC, string ParserPath, Dictionary<string, object> DataStore)
        {
            return ParserPath switch
            {
                "MMR" => HardCodedParsers.MMR.ReadAndApplySpoilerLog(IC, DataStore),
                "WWR" => HardCodedParsers.WWR.ParseSpoiler(IC.Instance, DataStore),
                "PMR" => HardCodedParsers.PMR.ParseSpoiler(IC.Instance, DataStore),
                "TPR" => HardCodedParsers.TPR.readAndApplySpoilerLog(IC.Instance, DataStore),
                "MC" => HardCodedParsers.Minecraft.ReadSpoiler(IC.Instance, DataStore),
                "BT" => HardCodedParsers.BanjoTooie.ReadSpoiler(IC.Instance, DataStore),
                "OOTMM" => HardCodedParsers.OOTMMSpoilerLogTools.readAndApplySpoilerLog(IC.Instance, DataStore),
                "PIK2" => HardCodedParsers.Pikmin2.ReadSpoiler(IC.Instance, DataStore),
                _ => null,
            };
        }

        private static GenericSpoilerLog? PythonParse(SpoilerLogInstructions instructions, string ParserPath, Dictionary<string, object> DataStore)
        {
            if (!File.Exists(ParserPath)) { return null; }
            throw new NotImplementedException();
        }

    }
}
