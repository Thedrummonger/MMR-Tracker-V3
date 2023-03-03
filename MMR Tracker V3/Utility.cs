using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MMR_Tracker_V3
{
    public class Utility
    {
        public static Dictionary<string, int> GetCategoriesFromFile(LogicObjects.TrackerInstance Instance)
        {
            Dictionary<string, int> Groups = new();
            if (File.Exists(References.Globalpaths.CategoryTextFile))
            {
                bool AtGame = true;
                foreach (var i in File.ReadAllLines(References.Globalpaths.CategoryTextFile))
                {
                    var Line = i.ToLower().Trim();
                    if (string.IsNullOrWhiteSpace(Line) || Line.StartsWith("//")) { continue; }
                    if (Line.StartsWith("#gamecodestart:"))
                    {
                        AtGame = Line.Replace("#gamecodestart:", "").Trim().Split(',')
                            .Select(y => y.Trim()).Contains(Instance.LogicFile.GameCode.ToLower());
                        continue;
                    }
                    if (Line.StartsWith("#gamecodeend:")) { AtGame = true; continue; }

                    if (!Groups.ContainsKey(Line) && AtGame)
                    {
                        Groups.Add(Line.Trim(), Groups.Count);
                    }
                }
                return Groups;
            }
            else { return new Dictionary<string, int>(); }
        }

        public static void DeepCloneLogic(List<string> Requirements, List<List<string>> Conditionals, out List<string> NewRequirements, out List<List<string>> NewConditionals)
        {
            NewRequirements = Requirements.ConvertAll(o => (string)o.Clone());
            NewConditionals = Conditionals.ConvertAll(p => p.ConvertAll(o => (string)o.Clone()));
        }

        public static bool CheckforSpoilerLog(LogicObjects.TrackerInstance logic)
        {
            return logic.LocationPool.Values.Any(x => !string.IsNullOrWhiteSpace(x.Randomizeditem.SpoilerLogGivenItem));
        }

        public static void TimeCodeExecution(Stopwatch stopwatch, string CodeTimed = "", int Action = 0)
        {
            if (Action == 0)
            {
                stopwatch.Start();
            }
            else
            {
                Debug.WriteLine($"{CodeTimed} took {stopwatch.ElapsedMilliseconds} m/s");
                stopwatch.Stop();
                stopwatch.Reset();
                if (Action == 1) { stopwatch.Start(); }
            }
        }

        public static MMRData.JsonFormatLogicItem CreateInaccessableLogic(string ID)
        {
            return new MMRData.JsonFormatLogicItem()
            {
                Id = ID,
                RequiredItems = new List<string> { "false" },
                ConditionalItems = new List<List<string>>(),
                IsTrick = false
            };
        }

        public static string GetLocationDisplayName(dynamic obj, LogicObjects.TrackerInstance instance)
        {
            dynamic PriceData;
            LocationData.LocationObject Location;
            string LocationDisplay, RandomizedItemDisplay, PriceDisplay, StarredDisplay;
            bool Available;
            if (obj is LocationData.LocationObject lo) 
            {
                Location = lo;
                PriceData = lo;
                LocationDisplay = Location.GetDictEntry(instance)?.GetName(instance);
                StarredDisplay = lo.Starred ? "*" : "";
            }
            else if (obj is LocationData.LocationProxy po)
            {
                Location = po.GetReferenceLocation(instance);
                PriceData = po.GetLogicInheritance(instance);
                LocationDisplay = po.Name ?? Location.GetDictEntry(instance)?.GetName(instance);
                StarredDisplay = po.Starred ? "*" : "";
            }
            else { return obj.ToString(); }

            if (Utility.DynamicPropertyExist(PriceData, "Available")) { Available = PriceData.Available; }
            else if(Utility.DynamicPropertyExist(PriceData, "Aquired")) { Available = PriceData.Aquired; }
            else { Available = false; ; }


            PriceData.GetPrice(out int p, out char c);
            PriceDisplay = p < 0 || (!Available) ? "" : $" [{c}{p}]";
            RandomizedItemDisplay = instance.GetItemByID(Location.Randomizeditem.Item)?.GetDictEntry(instance)?.GetName(instance) ?? Location.Randomizeditem.Item;

            return Location.CheckState switch
            {
                MiscData.CheckState.Marked => $"{LocationDisplay}: {RandomizedItemDisplay}{StarredDisplay}{PriceDisplay}",
                MiscData.CheckState.Unchecked => $"{LocationDisplay}{StarredDisplay}",
                MiscData.CheckState.Checked => $"{RandomizedItemDisplay}{PriceDisplay}: {LocationDisplay}{StarredDisplay}",
                _ => obj.ToString(),
            };
        }

        public static string GetEntranceDisplayName(EntranceData.EntranceRandoExit ExitObjectObject, LogicObjects.TrackerInstance instance)
        {
            var Destination = ExitObjectObject.GetDestinationAtExit(instance);
            string StarredDisplay = ExitObjectObject.Starred ? "*" : "";
            string RandomizedExitDisplay = Destination is null ? "" : $"{Destination.region} <- {Destination.from}";

            return ExitObjectObject.CheckState switch
            {
                MiscData.CheckState.Marked => $"{ExitObjectObject.ID}: {RandomizedExitDisplay}{StarredDisplay}",
                MiscData.CheckState.Unchecked => $"{ExitObjectObject.ID}{StarredDisplay}",
                MiscData.CheckState.Checked => $"{RandomizedExitDisplay}: {ExitObjectObject.ID}{StarredDisplay}",
                _ => ExitObjectObject.ToString(),
            };
        }

        public static List<string> GetAllWalletLogicEntries(LogicObjects.TrackerInstance instance)
        {
            bool CurrentInitializedState = instance.PriceData.Initialized;
            instance.PriceData.Initialized = false;

            List<string> WalletEntries = new List<string>();
            foreach (var i in instance.ItemPool)
            {
                if (i.Value.GetDictEntry(instance).WalletCapacity != null)
                {
                    WalletEntries.Add(i.Key);
                }
            }

            while (true)
            {
                if (!ScanMacros()) { break; }
            }

            instance.PriceData.Initialized = CurrentInitializedState;

            return WalletEntries;

            bool ScanMacros()
            {
                bool NewWalletFound = false;
                foreach (var i in instance.MacroPool)
                {
                    if (WalletEntries.Contains(i.Key)) { continue; }
                    var Logic = instance.GetLogic(i.Key);
                    if (IsWalletmacro(Logic))
                    {
                        WalletEntries.Add(i.Key);
                        NewWalletFound = true;
                    }
                }
                return NewWalletFound;
            }

            bool IsWalletmacro(MMRData.JsonFormatLogicItem Logic)
            {
                if (!Logic.RequiredItems.Any() && !Logic.ConditionalItems.Any()) { return false; }
                foreach (var i in Logic.RequiredItems)
                {
                    if (!WalletEntries.Contains(i)) { return false; }
                }
                foreach (var cond in Logic.ConditionalItems)
                {
                    foreach (var i in cond)
                    {
                        if (!WalletEntries.Contains(i)) { return false; }
                    }
                }
                return true;
            }

        }

        public static bool DynamicPropertyExist(dynamic Object, string name)
        {
            if (Object is null) { return false; }
            if (Object is ExpandoObject)
                return ((IDictionary<string, object>)Object).ContainsKey(name);

            return Object.GetType().GetProperty(name) != null;
        }

        public static bool TestForPriceData(dynamic Object)
        {
            try
            {
                Object.GetPrice(out int p, out char c);
                Debug.WriteLine("Had Price Function");
                return true;
            }
            catch (Exception e) {

                Debug.WriteLine($"NOT Had Price Function {e}"); 
                return false; 
            }
        }

        public static bool IsLogicEqual(MMRData.JsonFormatLogicItem logicItem1, MMRData.JsonFormatLogicItem logicItem2)
        {
            bool ReqEqual = logicItem1.RequiredItems.SequenceEqual(logicItem2.RequiredItems);
            bool ConEqual = logicItem1.ConditionalItems.SelectMany(x => x).SequenceEqual(logicItem2.ConditionalItems.SelectMany(x => x));
            return ReqEqual && ConEqual;
        }

        public static bool OBJIsThreadSafe(Thread thread, dynamic Obj)
        {
            bool IsWinformSafe = Obj is not null && (!Utility.DynamicPropertyExist(Obj, "IsHandleCreated") || Obj.IsHandleCreated);
            return thread is not null && thread.IsAlive && Obj is not null && IsWinformSafe;
        }

    }
    public static class GenericCopier<T>
    {
        public static T DeepCopy(object objectToCopy)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, objectToCopy);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }
    }

    public static class SaveCompressor
    {
        public enum SaveType
        {
            Standard,
            Compressed,
            CompressedByte,
            error
        }
        public class CompressedSave
        {
            public byte[] Bytes { get; }
            public CompressedSave(string Save)
            {
                Bytes = Compress(Save);
            }
            public override string ToString()
            {
                return GetBytesAsString(Bytes);
            }
        }

        public static string Decompress(byte[] dataToDeCompress)
        {
            byte[] decompressedData = DecompressByte(dataToDeCompress);
            return Encoding.UTF8.GetString(decompressedData);
        }

        public static string Decompress(string String)
        {
            return Decompress(Convert.FromBase64String(String));
        }

        private static byte[] CompressByte(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.SmallestSize))
                {
                    gzipStream.Write(bytes, 0, bytes.Length);
                }
                return memoryStream.ToArray();
            }
        }
        private static byte[] DecompressByte(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {

                using (var outputStream = new MemoryStream())
                {
                    using (var decompressStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                    {
                        decompressStream.CopyTo(outputStream);
                    }
                    return outputStream.ToArray();
                }
            }
        }

        private static byte[] Compress(string String)
        {
            byte[] dataToCompress = Encoding.UTF8.GetBytes(String);
            byte[] compressedData = CompressByte(dataToCompress);
            return compressedData;
        }

        private static string GetBytesAsString(byte[] byteData)
        {
            return Convert.ToBase64String(byteData);
        }

        public static SaveType TestFileType(string FilePath)
        {
            try
            {
                Debug.WriteLine("Trying to load Standard Save");
                var Instance = LogicObjects.TrackerInstance.FromJson(File.ReadAllText(FilePath));
                Debug.WriteLine("Success!");
                return SaveType.Standard;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Failure! {e}");
            }
            try
            {
                Debug.WriteLine("Trying to load Compressed Save");
                var DecompSave = Decompress(File.ReadAllText(FilePath));
                var Instance = LogicObjects.TrackerInstance.FromJson(DecompSave);
                Debug.WriteLine("Success!");
                return SaveType.Compressed;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Failure! {e}");
            }
            try
            {
                Debug.WriteLine("Trying to load Compressed Byte Save");
                var DecompSave = Decompress(File.ReadAllBytes(FilePath));
                var Instance = LogicObjects.TrackerInstance.FromJson(DecompSave);
                Debug.WriteLine("Success!");
                return SaveType.CompressedByte;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Failure! {e}");
            }
            return SaveType.error;
        }
    }

    public static class SearchStringParser
    {
        private static string CurrentString = "";
        private static bool CurrentStringIsError = false;

        public class SearchObject
        {
            public bool Starred { get; set; } = false;
            public string ID { get; set; } = "";
            public string Name { get; set; } = "";
            public string OriginalItem { get; set; } = "";
            public string Randomizeditem { get; set; } = "";
            public string Area { get; set; } = "";
            public string[] ValidItemTypes { get; set; } = null;
        }

        public static readonly char[] LogicChars = new char[] { '&', '|', '+', '*', '(', ')' };

        public static List<string> GetEntriesFromLogicString(string input)
        {
            List<string> BrokenString = new List<string>();
            string currentItem = "";
            foreach (var i in input)
            {
                if (LogicChars.Contains(i))
                {
                    if (currentItem != "")
                    {
                        BrokenString.Add(currentItem);
                        currentItem = "";
                    }
                    if (i == '&') { BrokenString.Add("*"); }
                    else if (i == '|') { BrokenString.Add("+"); }
                    else { BrokenString.Add(i.ToString()); }
                }
                else { currentItem += i.ToString(); }
            }
            if (currentItem != "") { BrokenString.Add(currentItem); }
            return BrokenString;
        }

        public static bool FilterSearch(LogicObjects.TrackerInstance Instance, object InObject, string searchTerm, string NameToCompare)
        {
            var searchObject = CreateSearchableObject(InObject, Instance);
            if (searchObject == null) { return false; }
            //Debug.WriteLine($"{searchObject.ID}: {InObject.GetType()}" );

            //Since filter search is usually called a large number of times at once, we can cut down on lag by checking first if we've already compared against the given string
            //If we have and that string was a malformed term, skip all subsequent searches until the text changes.
            if (searchTerm != CurrentString)
            {
                CurrentString = searchTerm;
                CurrentStringIsError = false;
            }
            else if (CurrentStringIsError) { return false; ; }

            if (string.IsNullOrWhiteSpace(searchTerm)) { return true; }

            bool StarredOnly = false;
            char[] GlobalModifiers = new char[] { '^', '*' };
            while (searchTerm.Count() > 0 && GlobalModifiers.Contains(searchTerm[0]))
            {
                if (searchTerm[0] == '*') { StarredOnly = true; }
                searchTerm = searchTerm.Substring(1);
            }
            if (StarredOnly && !searchObject.Starred) { return false; }
            if (string.IsNullOrWhiteSpace(searchTerm)) { return true; }

            List<string> ExpandedExptression = GetEntriesFromLogicString(searchTerm);

            for (var i = 0; i < ExpandedExptression.Count; i++)
            {
                ExpandedExptression[i] = PerformLogicCheck(ExpandedExptression[i], searchObject, NameToCompare);
            }

            string Expression = string.Join("", ExpandedExptression);
            //Console.WriteLine($"Expression = {Expression}");
            try
            {
                DataTable dt = new();
                var Solution = dt.Compute(Expression, "");
                if (!int.TryParse(Solution.ToString(), out int Result)) { return true; }
                return Result > 0;
            }
            catch { CurrentStringIsError = true; return true; }
        }

        private static string PerformLogicCheck(string i, SearchObject logic, string NameToCompare)
        {
            if (LogicChars.Contains(i[0])) { return i; }

            char[] Modifiers = new char[] { '!', '=' };

            bool Inverse = false;
            bool Perfect = false;
            var subterm = i;
            if (subterm == "") { return "1"; }
            while (subterm.Length > 0 && Modifiers.Contains(subterm[0]))
            {
                if (subterm[0] == '!') { Inverse = true; }
                if (subterm[0] == '=') { Perfect = true; }
                subterm = subterm.Substring(1);
            }
            if (subterm == "") { return "1"; }
            if (string.IsNullOrWhiteSpace(subterm)) { return ""; }

            subterm = subterm.Trim();

            switch (subterm[0])
            {
                case '_': //Search By Randomized Item
                    if (subterm[1..] == "") { return "1"; }
                    if (logic.Randomizeditem == null) { return "0"; }
                    if (Perfect && logic.Randomizeditem.ToLower() == subterm[1..].ToLower() == Inverse) { return "0"; }
                    else if (!Perfect && logic.Randomizeditem.ToLower().Contains(subterm[1..].ToLower()) == Inverse) { return "0"; }
                    break;
                case '#'://Search By Location Area
                    if (subterm[1..] == "") { return "1"; }
                    if (logic.Area == null) { return "0"; }
                    if (Perfect && logic.Area.ToLower() == subterm[1..].ToLower() == Inverse) { return "0"; }
                    else if (!Perfect && logic.Area.ToLower().Contains(subterm[1..].ToLower()) == Inverse) { return "0"; }
                    break;
                case '@'://Search By Item Type
                    if (subterm[1..] == "") { return "1"; }
                    if (logic.ValidItemTypes == null) { return "0"; }
                    if (Perfect && logic.ValidItemTypes.Select(x => x.ToLower()).Contains(subterm[1..].ToLower()) == Inverse) { return "0"; }
                    else if ((!Perfect && logic.ValidItemTypes.Select(x => x.ToLower()).Any(x => x.Contains(subterm[1..].ToLower()))) == Inverse) { return "0"; }
                    break;
                case '~'://Search By Dictionary Name
                    if (subterm[1..] == "") { return "1"; }
                    if (logic.ID == null) { return "0"; }
                    if (Perfect && logic.ID.ToLower() == subterm[1..].ToLower() == Inverse) { return "0"; }
                    else if ((!Perfect && logic.ID.ToLower().Contains(subterm[1..].ToLower())) == Inverse) { return "0"; }
                    break;
                case '$'://Search By Original Item Name
                    if (subterm[1..] == "") { return "1"; }
                    if (logic.OriginalItem == null) { return "0"; }
                    if (Perfect && logic.OriginalItem.ToLower() == subterm[1..].ToLower() == Inverse) { return "0"; }
                    else if (!Perfect && logic.OriginalItem.ToLower().Contains(subterm[1..].ToLower()) == Inverse) { return "0"; }
                    break;
                case '%'://Search By Location Name
                    if (subterm[1..] == "") { return "1"; }
                    if (logic.Name == null) { return "0"; }
                    if (Perfect && logic.Name.ToLower() == subterm[1..].ToLower() == Inverse) { return "0"; }
                    else if (!Perfect && logic.Name.ToLower().Contains(subterm[1..].ToLower()) == Inverse) { return "0"; }
                    break;
                default: //Search By "NameToCompare" variable
                    if (Perfect && NameToCompare.ToLower() == subterm.ToLower() == Inverse) { return "0"; }
                    else if (!Perfect && (NameToCompare.ToLower().Contains(subterm.ToLower()) == Inverse)) { return "0"; }
                    break;
            }
            return "1";
        }

        public static SearchObject CreateSearchableObject(object Object, LogicObjects.TrackerInstance instance)
        {
            SearchObject OutObject = new();
            if (Object is LocationData.LocationObject locationObject)
            {
                var DictData = locationObject.GetDictEntry(instance);
                OutObject.ID = locationObject.ID;
                OutObject.Area = DictData.Area;
                OutObject.Name = DictData.GetName(instance);
                OutObject.OriginalItem = DictData.OriginalItem;
                OutObject.Randomizeditem = locationObject.Randomizeditem.Item;
                OutObject.Starred = locationObject.Starred;
                OutObject.ValidItemTypes = DictData.ValidItemTypes;
            }
            else if (Object is LocationData.LocationProxy locationProxy)
            {
                var LocReference = instance.GetLocationByID(locationProxy.ReferenceID);
                var DictData = LocReference.GetDictEntry(instance);
                OutObject.ID = locationProxy.ID;
                OutObject.Area = locationProxy.Area;
                OutObject.Name = locationProxy.Name;
                OutObject.OriginalItem = DictData.OriginalItem;
                OutObject.Randomizeditem = LocReference?.Randomizeditem.Item;
                OutObject.Starred = LocReference?.Starred??false;
                OutObject.ValidItemTypes = DictData.ValidItemTypes;
            }
            else if (Object is ItemData.ItemObject ItemObject)
            {
                var DictData = ItemObject.GetDictEntry(instance);
                OutObject.ID = ItemObject.Id;
                OutObject.Area = "";
                if (ItemObject.AmountInStartingpool > 0) { OutObject.Area += "starting "; }
                if (ItemObject.AmountAquiredOnline.Any(x => x.Value > 0)) { OutObject.Area += "online "; }
                OutObject.Area += "item";
                OutObject.Name = DictData.GetName(instance);
                OutObject.OriginalItem = DictData.GetName(instance);
                OutObject.Randomizeditem = DictData.GetName(instance);
                OutObject.Starred = false;
                OutObject.ValidItemTypes = DictData.ItemTypes;
            }
            else if (Object is HintData.HintObject HintObject)
            {
                var DictData = HintObject.GetDictEntry(instance);
                OutObject.ID = HintObject.ID;
                OutObject.Area = "hints";
                OutObject.Name = DictData.Name;
                OutObject.OriginalItem = DictData.Name;
                OutObject.Randomizeditem = DictData.Name;
                OutObject.Starred = false;
                OutObject.ValidItemTypes = new string[] { "hint" };
            }
            else if (Object is MacroObject MacroObject)
            {
                bool Istrick = MacroObject.isTrick(instance);
                var DictData = MacroObject.GetDictEntry(instance);
                var LogicData = instance.GetLogic(MacroObject.ID, false);
                OutObject.ID = MacroObject.ID;
                OutObject.Area = Istrick ? (LogicData.TrickCategory??"misc") : "macro";
                OutObject.Name = DictData.Name ?? MacroObject.ID;
                OutObject.OriginalItem = MacroObject.ID;
                OutObject.Randomizeditem = MacroObject.ID;
                OutObject.Starred = Istrick;
                List<string> ItemTypes = new() { "macro" };
                if (Istrick) { ItemTypes.Add("trick"); }
                OutObject.ValidItemTypes = ItemTypes.ToArray();
            }
            else if (Object is EntranceData.EntranceRandoExit ExitObject)
            {
                OutObject.ID = instance.GetLogicNameFromExit(ExitObject);
                OutObject.Area = ExitObject.Area??ExitObject.ParentAreaID;
                OutObject.Name = ExitObject.ID;
                OutObject.OriginalItem = ExitObject.EntrancePair == null ? "One Way" : $"{ExitObject.EntrancePair.Area} To {ExitObject.EntrancePair.Exit}";
                OutObject.Randomizeditem = ExitObject.DestinationExit == null ? null : $"{ExitObject.DestinationExit.region} From {ExitObject.DestinationExit.from}";
                OutObject.Starred = ExitObject.Starred;
                List<string> ItemTypes = new() { "exit" };
                if (ExitObject.EntrancePair == null) { ItemTypes.Add("One Way"); }
                OutObject.ValidItemTypes = ItemTypes.ToArray();
            }
            else if (Object is OptionData.TrackerOption OptionObject)
            {
                OutObject.ID = OptionObject.ID;
                OutObject.Area = "Options";
                OutObject.Name = OptionObject.DisplayName;
                OutObject.OriginalItem = OptionObject.CurrentValue;
                OutObject.Randomizeditem = OptionObject.CurrentValue;
                OutObject.Starred = OptionObject.IsToggleOption();
                List<string> ItemTypes = new() { "Option" };
                if (OptionObject.IsToggleOption()) { ItemTypes.Add("Toggle"); }
                OutObject.ValidItemTypes = ItemTypes.ToArray();
            }
            else if (Object is LogicDictionaryData.TrackerVariable VariableObject)
            {
                OutObject.ID = VariableObject.ID;
                OutObject.Area = "Variable";
                OutObject.Name = VariableObject.Name;
                OutObject.OriginalItem = VariableObject.ValueToString();
                OutObject.Randomizeditem = VariableObject.ValueToString();
                OutObject.Starred = !VariableObject.Static;
                List<string> ItemTypes = new() { "Variable" };
                if (VariableObject.Value is string) { ItemTypes.Add("string"); }
                if (VariableObject.Value is Int64) { ItemTypes.Add("Number"); }
                if (VariableObject.Value is bool) { ItemTypes.Add("Bool"); }
                if (VariableObject.Value is List<string>) { ItemTypes.Add("List"); }
                OutObject.ValidItemTypes = ItemTypes.ToArray();
            }
            else if (Object is EntranceData.EntranceRandoDestination DestinationObject)
            {
                OutObject.ID = DestinationObject.region;
                OutObject.Area = DestinationObject.from;
                OutObject.Name = DestinationObject.region;
                OutObject.OriginalItem = DestinationObject.from;
                OutObject.Randomizeditem = DestinationObject.from;
                OutObject.Starred = true;
                List<string> ItemTypes = new() { "Destination" };
                OutObject.ValidItemTypes = ItemTypes.ToArray();
            }
            else
            {
                string ObjectString = Object.ToString();
                OutObject.ID = ObjectString;
                OutObject.Area = ObjectString;
                OutObject.Name = ObjectString;
                OutObject.OriginalItem = ObjectString;
                OutObject.Randomizeditem = ObjectString;
                OutObject.Starred = true;
                OutObject.ValidItemTypes = new string[] { ObjectString };
            }
            return OutObject;
        }
    }
}
