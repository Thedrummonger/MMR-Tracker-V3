using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;

namespace MMR_Tracker_V3
{
    public static class Utility
    {
        /// <summary>
        /// Replaces a range of text in a string with a replacement string
        /// </summary>
        /// <param name="s">The source string</param>
        /// <param name="index">The starting index of the text to replace</param>
        /// <param name="length">The length of the text to replace</param>
        /// <param name="replacement">The replacement text</param>
        /// <returns></returns>
        public static string Replace(this string s, int index, int length, string replacement)
        {
            var builder = new StringBuilder();
            builder.Append(s[..index]);
            builder.Append(replacement);
            builder.Append(s[(index + length)..]);
            return builder.ToString();
        }
        /// <summary>
        /// Splits a string by a substring and trims all the resulting substrings
        /// </summary>
        /// <param name="s">The source string</param>
        /// <param name="Val">The substring to split on</param>
        /// <returns></returns>
        public static string[] TrimSplit(this string s, string Val)
        {
            return s.Split(Val).Select(x => x.Trim()).ToArray();
        }
        /// <summary>
        /// Gets a value from a dictionary at the given key and casts it to the given type
        /// </summary>
        /// <typeparam name="Y">The the object type of the dictionaries key</typeparam>
        /// <typeparam name="T">The Object the value will be cast to</typeparam>
        /// <param name="source">The source dictionary</param>
        /// <param name="Key">The key to get value of</param>
        /// <returns></returns>
        public static T GetValueAs<Y, T>(this Dictionary<Y, object> source, Y Key)
        {
            if (!source.TryGetValue(Key, out object value)) { return default; }
            return value.SerializeConvert<T>();
        }
        /// <summary>
        /// Converts and object to a given type using serialization
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="source">The source Object</param>
        /// <returns></returns>
        public static T SerializeConvert<T>(this object source)
        {
            string Serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(Serialized);
        }
        /// <summary>
        /// Creates an array containing the values of the given Enum
        /// </summary>
        /// <typeparam name="T">The Enum as a type</typeparam>
        /// <returns>An array of <Enum></returns>
        public static IEnumerable<T> EnumAsArray<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        }
        /// <summary>
        /// Creates an array containing the values of the given Enum as strings
        /// </summary>
        /// <typeparam name="T">The Enum as a type</typeparam>
        /// <returns>An array of <string></returns>
        public static IEnumerable<string> EnumAsStringArray<T>()
        {
            return EnumAsArray<T>().Select(x => x.ToString()).ToArray();
        }
        /// <summary>
        /// Splits a string by the first occurrence of the given char
        /// </summary>
        /// <param name="input">The string to split</param>
        /// <param name="Split">The Char to split at</param>
        /// <param name="LastOccurrence">Split by the last occurrence rather than the first</param>
        /// <returns>A tuple containing the the two resulting strings</returns>
        public static Tuple<string, string> SplitOnce(this string input, char Split, bool LastOccurrence = false)
        {
            int idx = LastOccurrence ? input.LastIndexOf(Split) : input.IndexOf(Split);
            Tuple<string, string> Output;
            if (idx != -1) { Output = new(input[..idx], input[(idx + 1)..]); }
            else { Output = new(input, string.Empty); }
            return Output;
        }
        /// <summary>
        /// Checks if the given object is in the given list of values
        /// </summary>
        /// <typeparam name="T">The type of objects being compared</typeparam>
        /// <param name="obj">The object to check</param>
        /// <param name="args">The list of objects that may contain the given object</param>
        /// <returns></returns>
        public static bool In<T>(this T obj, params T[] args)
        {
            return args.Contains(obj);
        }
        /// <summary>
        /// Gets the values from a list in a certain range
        /// </summary>
        /// <typeparam name="T">Type of values in the list</typeparam>
        /// <param name="list">The source list</param>
        /// <param name="range">The range of objects to grab</param>
        /// <returns></returns>
        public static List<T> GetRange<T>(this List<T> list, Range range)
        {
            var (start, length) = range.GetOffsetAndLength(list.Count);
            return list.GetRange(start, length);
        }
        /// <summary>
        /// Checks if the string represents an integer range
        /// </summary>
        /// <param name="x">The input string</param>
        /// <param name="Values">A tuple containing the min and max of the given range</param>
        /// <returns>Was the string a range?</returns>
        public static bool IsIntegerRange(this string x, out Tuple<int, int> Values)
        {
            Values = new(-1, -1);
            if (!x.Contains('-')) { return false; }
            var Segments = x.Split('-');
            if (Segments.Length != 2) { return false; }
            if (!int.TryParse(Segments[0], out int RawInt1)) { return false; }
            if (!int.TryParse(Segments[1], out int RawInt2)) { return false; }
            Values = new(Math.Min(RawInt1, RawInt2), Math.Max(RawInt1, RawInt2));
            return true;
        }
        /// <summary>
        /// Removes extra white spaces in a string so the string will never contain more than one whitespace in a row
        /// </summary>
        /// <param name="myString">String to trim</param>
        /// <returns></returns>
        public static string TrimSpaces(this string myString)
        {
            return Regex.Replace(myString, @"\s+", " ");
        }
        /// <summary>
        /// Converts a string representing a version to a version object
        /// </summary>
        /// <param name="version">Version String</param>
        /// <returns></returns>
        public static Version AsVersion(this string version)
        {
            if (!version.Any(x => char.IsDigit(x))) { version = "0"; }
            if (!version.Contains('.')) { version += ".0"; }
            return new Version(string.Join("", version.Where(x => char.IsDigit(x) || x == '.')));
        }
        /// <summary>
        /// Picks a random element from a list
        /// </summary>
        /// <typeparam name="T">Type of object the list contains</typeparam>
        /// <param name="source">Source List</param>
        /// <returns></returns>
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }
        /// <summary>
        /// If the given key is not present in the dictionary, add the given value at the given key
        /// </summary>
        /// <typeparam name="T">The type of the key object in the dictionary</typeparam>
        /// <typeparam name="V">The type of the value object in the dictionary</typeparam>
        /// <param name="Dict">The source dictionary</param>
        /// <param name="Value">The key to check for</param>
        /// <param name="Default">The value to assign to the given key</param>
        public static void SetIfEmpty<T, V>(this Dictionary<T, V> Dict, T Value, V Default)
        {
            if (!Dict.ContainsKey(Value)) { Dict[Value] = Default; }
        }
        /// <summary>
        /// Picks a number of random elements from the given list
        /// </summary>
        /// <typeparam name="T">Type of object the list contains</typeparam>
        /// <param name="source">The source list</param>
        /// <param name="count">The amount of items to take</param>
        /// <returns></returns>
        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }
        /// <summary>
        /// Shuffles an collection
        /// </summary>
        /// <typeparam name="T">Type of objects in the collection</typeparam>
        /// <param name="source">Source collection</param>
        /// <returns></returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }
        /// <summary>
        /// Serialized the given object and prints it to the debug window
        /// </summary>
        /// <param name="o">Source object</param>
        public static void PrintObjectToConsole(object o)
        {
            Debug.WriteLine(o.ToFormattedJson());
        }
        /// <summary>
        /// Serialized the given object and return it as a string
        /// </summary>
        /// <param name="o">The source object</param>
        /// <returns>The serialized object</returns>
        public static string ToFormattedJson(this object o)
        {
            return JsonConvert.SerializeObject(o, DefaultSerializerSettings);
        }

        public readonly static Newtonsoft.Json.JsonSerializerSettings DefaultSerializerSettings = new()
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            Converters = { new Newtonsoft.Json.Converters.StringEnumConverter(), new IPConverter() }
        };
        public class IPConverter : JsonConverter<IPAddress>
        {
            public override void WriteJson(JsonWriter writer, IPAddress value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }

            public override IPAddress ReadJson(JsonReader reader, Type objectType, IPAddress existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var s = (string)reader.Value;
                return IPAddress.Parse(s);
            }
        }
        /// <summary>
        /// Checks if the given string is enclosed in single quotes
        /// </summary>
        /// <param name="ID">The source string</param>
        /// <param name="CleanedID">the source string with quotes removed</param>
        /// <returns></returns>
        public static bool IsLiteralID(this string ID, out string CleanedID)
        {
            bool Literal = false;
            CleanedID = ID.Trim();
            if (ID.StartsWith("'") && ID.EndsWith("'"))
            {
                Literal = true;
                CleanedID = ID[1..^1];
            }
            return Literal;
        }
        /// <summary>
        /// Checks if the given serialized object can deserialize to the given type
        /// </summary>
        /// <typeparam name="T">Type to test the object against</typeparam>
        /// <param name="Json">The source json string</param>
        /// <returns></returns>
        public static bool isJsonTypeOf<T>(string Json)
        {
            try
            {
                T test = JsonConvert.DeserializeObject<T>(Json, DefaultSerializerSettings);
                return test != null;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Deep Clones the Requirements and Conditionals from a MMR JsonFormattedLogicObject
        /// </summary>
        /// <param name="Requirements">Source Requirements</param>
        /// <param name="Conditionals">Source Conditionals</param>
        /// <param name="NewRequirements">Cloned Requirements</param>
        /// <param name="NewConditionals">Cloned Conditionals</param>
        public static void DeepCloneLogic(List<string> Requirements, List<List<string>> Conditionals, out List<string> NewRequirements, out List<List<string>> NewConditionals)
        {

            NewRequirements = Requirements.ConvertAll(o => (string)o.Clone());
            NewConditionals = Conditionals.ConvertAll(p => p.ConvertAll(o => (string)o.Clone()));
        }

        /// <summary>
        /// Tracks code execution Timing
        /// </summary>
        /// <param name="stopwatch">The source Stopwatch Object</param>
        /// <param name="CodeTimed">Description of the code being timed</param>
        /// <param name="Action">The action to perform 0 = Start the stopwatch, 1 = Print Time and Restart, 2 = Print Time and stop timer</param>
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
        /// <summary>
        /// Checks if the given dynamic object has the given property
        /// </summary>
        /// <param name="Object">The source object</param>
        /// <param name="name">The name of the property to check for</param>
        /// <returns></returns>
        public static bool DynamicPropertyExist(dynamic Object, string name)
        {
            if (Object is null) { return false; }
            if (Object is ExpandoObject)
                return ((IDictionary<string, object>)Object).ContainsKey(name);

            var type = Object.GetType();
            return type.GetProperty(name) != null;
        }
        /// <summary>
        /// Checks if the given dynamic object has the given method
        /// </summary>
        /// <param name="Object">The source object</param>
        /// <param name="methodName">The name of the method to check for</param>
        /// <returns></returns>
        public static bool DynamicMethodExists(dynamic Object, string methodName)
        {
            if (Object is null) { return false; }
            var type = Object.GetType();
            return type.GetMethod(methodName) != null;
        }
        /// <summary>
        /// Check if the given thread is alive and the object still exists
        /// </summary>
        /// <param name="thread">The source thread</param>
        /// <param name="Obj">The source object</param>
        /// <returns></returns>
        public static bool OBJIsThreadSafe(Thread thread, dynamic Obj)
        {
            bool IsWinformSafe = Obj is not null && (!Utility.DynamicPropertyExist(Obj, "IsHandleCreated") || Obj.IsHandleCreated);
            return thread is not null && thread.IsAlive && Obj is not null && IsWinformSafe;
        }
        /// <summary>
        /// Capitalizes the first letter of words in the given string
        /// </summary>
        /// <param name="Input">Source string</param>
        /// <returns></returns>
        public static string ConvertToCamelCase(string Input)
        {
            string NiceName = Input.ToLower();
            TextInfo cultInfo = new CultureInfo("en-US", false).TextInfo;
            NiceName = cultInfo.ToTitleCase(NiceName);
            return NiceName;
        }

    }
    public static class GenericCopier<T>
    {
        public static T DeepCopy(object objectToCopy)
        {
            string Serialized = JsonConvert.SerializeObject(objectToCopy);
            return JsonConvert.DeserializeObject<T>(Serialized);
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
        public class CompressedSave(string Save)
        {
            public byte[] Bytes { get; } = Compress(Save);

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
            using var memoryStream = new MemoryStream();
            using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.SmallestSize))
            {
                gzipStream.Write(bytes, 0, bytes.Length);
            }
            return memoryStream.ToArray();
        }
        private static byte[] DecompressByte(byte[] bytes)
        {
            using var memoryStream = new MemoryStream(bytes);

            using var outputStream = new MemoryStream();
            using (var decompressStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            {
                decompressStream.CopyTo(outputStream);
            }
            return outputStream.ToArray();
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

        public static SaveType TestSaveFileType(string FilePath, InstanceData.TrackerInstance instance)
        {
            var Options = instance?.StaticOptions?.OptionFile;
            if (Options is null && File.Exists(References.Globalpaths.OptionFile))
            {
                Options = JsonConvert.DeserializeObject<OptionFile>(File.ReadAllText(References.Globalpaths.OptionFile));
            }
            string Content = File.ReadAllText(FilePath);
            var ByteContent = File.ReadAllBytes(FilePath);
            if (Options?.CompressSave ?? false)
            {
                if (TestCompressedByteSave(ByteContent)) { return SaveType.CompressedByte; };
                if (TestStandardSave(Content)) { return SaveType.Standard; };
            }
            else
            {
                if (TestStandardSave(Content)) { return SaveType.Standard; };
                if (TestCompressedByteSave(ByteContent)) { return SaveType.CompressedByte; };
            }
            if (TestCompressedSave(Content)) { return SaveType.Compressed; }; //This type is never used but check for it just incase
            return SaveType.error;
        }

        public static bool TestStandardSave(string FileContent)
        {
            return Utility.isJsonTypeOf<InstanceData.TrackerInstance>(FileContent);
        }
        public static bool TestCompressedSave(string FileContent)
        {
            try
            {
                var DecompSave = Decompress(FileContent);
                return Utility.isJsonTypeOf<InstanceData.TrackerInstance>(DecompSave);
            }
            catch
            {
                return false;
            }
        }
        public static bool TestCompressedByteSave(byte[] FileContent)
        {
            try
            {
                var DecompSave = Decompress(FileContent);
                return Utility.isJsonTypeOf<InstanceData.TrackerInstance>(DecompSave);
            }
            catch
            {
                return false;
            }
        }

        public static string GetSaveStringFromFile(string FilePath, InstanceData.TrackerInstance instance)
        {
            switch (SaveCompressor.TestSaveFileType(FilePath, instance))
            {
                case SaveCompressor.SaveType.Standard:
                    return File.ReadAllText(FilePath);
                case SaveCompressor.SaveType.Compressed:
                    var Decomp = SaveCompressor.Decompress(File.ReadAllText(FilePath));
                    return (Decomp);
                case SaveCompressor.SaveType.CompressedByte:
                    var ByteDecomp = SaveCompressor.Decompress(File.ReadAllBytes(FilePath));
                    return (ByteDecomp);
            }
            return string.Empty;
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

        public static readonly HashSet<char> LogicChars = ['&', '|', '+', '*', '(', ')'];

        public static List<string> GetEntriesFromLogicString(string input)
        {
            List<string> BrokenString = [];
            string currentItem = "";
            bool InEscapeChar = false;
            foreach (var i in input)
            {
                if (i == '\\' && !InEscapeChar) { InEscapeChar = true; continue; }
                if (LogicChars.Contains(i) && !InEscapeChar)
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
                if (InEscapeChar) { InEscapeChar = false; }
            }
            if (currentItem != "") { BrokenString.Add(currentItem); }
            return BrokenString;
        }

        public static bool FilterSearch(InstanceData.TrackerInstance Instance, object InObject, string searchTerm, string NameToCompare)
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
            while (searchTerm.Length > 0 && GlobalModifiers.Contains(searchTerm[0]))
            {
                if (searchTerm[0] == '*') { StarredOnly = true; }
                searchTerm = searchTerm[1..];
            }
            if (StarredOnly && !searchObject.Starred) { return false; }
            if (string.IsNullOrWhiteSpace(searchTerm)) { return true; }

            List<string> ExpandedExptression = GetEntriesFromLogicString(searchTerm);

            for (var i = 0; i < ExpandedExptression.Count; i++)
            {
                ExpandedExptression[i] = PerformLogicCheck(ExpandedExptression[i], searchObject, NameToCompare);
            }

            string Expression = string.Join("", ExpandedExptression);
            //Debug.WriteLine($"Expression = {Expression}");
            try
            {
                DataTable dt = new();
                var Solution = dt.Compute(Expression, "");
                if (!int.TryParse(Solution.ToString(), out int Result)) { return true; }
                return Result > 0;
            }
            catch { CurrentStringIsError = true; return false; }
        }

        private static string PerformLogicCheck(string i, SearchObject logic, string NameToCompare)
        {
            if (i.Length == 1 && LogicChars.Contains(i[0])) { return i; }

            char[] Modifiers = new char[] { '!', '=' };

            bool Inverse = false;
            bool Perfect = false;
            var subterm = i;
            if (subterm == "") { return "1"; }
            while (subterm.Length > 0 && Modifiers.Contains(subterm[0]))
            {
                if (subterm[0] == '!') { Inverse = true; }
                if (subterm[0] == '=') { Perfect = true; }
                subterm = subterm[1..];
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
                    if (Perfect && logic.Area.Equals(subterm[1..], StringComparison.CurrentCultureIgnoreCase) == Inverse) { return "0"; }
                    else if (!Perfect && logic.Area.Contains(subterm[1..], StringComparison.CurrentCultureIgnoreCase) == Inverse) { return "0"; }
                    break;
                case '@'://Search By Item Type
                    if (subterm[1..] == "") { return "1"; }
                    if (logic.ValidItemTypes == null) { return "0"; }
                    if (Perfect && logic.ValidItemTypes.Select(x => x.ToLower()).Contains(subterm[1..].ToLower()) == Inverse) { return "0"; }
                    else if ((!Perfect && logic.ValidItemTypes.Select(x => x.ToLower()).Any(x => x.Contains(subterm[1..], StringComparison.CurrentCultureIgnoreCase))) == Inverse) { return "0"; }
                    break;
                case '~'://Search By Dictionary Name
                    if (subterm[1..] == "") { return "1"; }
                    if (logic.ID == null) { return "0"; }
                    if (Perfect && logic.ID.Equals(subterm[1..], StringComparison.CurrentCultureIgnoreCase) == Inverse) { return "0"; }
                    else if ((!Perfect && logic.ID.Contains(subterm[1..], StringComparison.CurrentCultureIgnoreCase)) == Inverse) { return "0"; }
                    break;
                case '$'://Search By Original Item Name
                    if (subterm[1..] == "") { return "1"; }
                    if (logic.OriginalItem == null) { return "0"; }
                    if (Perfect && logic.OriginalItem.Equals(subterm[1..], StringComparison.CurrentCultureIgnoreCase) == Inverse) { return "0"; }
                    else if (!Perfect && logic.OriginalItem.Contains(subterm[1..], StringComparison.CurrentCultureIgnoreCase) == Inverse) { return "0"; }
                    break;
                case '%'://Search By Location Name
                    if (subterm[1..] == "") { return "1"; }
                    if (logic.Name == null) { return "0"; }
                    if (Perfect && logic.Name.Equals(subterm[1..], StringComparison.CurrentCultureIgnoreCase) == Inverse) { return "0"; }
                    else if (!Perfect && logic.Name.Contains(subterm[1..], StringComparison.CurrentCultureIgnoreCase) == Inverse) { return "0"; }
                    break;
                default: //Search By "NameToCompare" variable
                    if (Perfect && NameToCompare.Equals(subterm, StringComparison.CurrentCultureIgnoreCase) == Inverse) { return "0"; }
                    else if (!Perfect && (NameToCompare.Contains(subterm, StringComparison.CurrentCultureIgnoreCase) == Inverse)) { return "0"; }
                    break;
            }
            return "1";
        }

        public static SearchObject CreateSearchableObject(object Object, InstanceData.TrackerInstance instance)
        {
            SearchObject OutObject = new();
            if (Object is LocationData.LocationObject locationObject)
            {
                var DictData = locationObject.GetDictEntry();
                OutObject.ID = locationObject.ID;
                OutObject.Area = DictData.Area;
                OutObject.Name = DictData.GetName();
                OutObject.OriginalItem = DictData.OriginalItem;
                OutObject.Randomizeditem = locationObject.Randomizeditem.Item;
                OutObject.Starred = locationObject.Starred;
                OutObject.ValidItemTypes = DictData.ValidItemTypes;
            }
            else if (Object is LocationData.LocationProxy locationProxy)
            {
                var LocReference = instance.GetLocationByID(locationProxy.ReferenceID);
                var DictData = LocReference.GetDictEntry();
                OutObject.ID = locationProxy.ID;
                OutObject.Area = locationProxy.GetDictEntry().Area;
                OutObject.Name = locationProxy.GetDictEntry().Name;
                OutObject.OriginalItem = DictData.OriginalItem;
                OutObject.Randomizeditem = LocReference?.Randomizeditem.Item;
                OutObject.Starred = LocReference?.Starred ?? false;
                OutObject.ValidItemTypes = DictData.ValidItemTypes;
            }
            else if (Object is ItemData.ItemObject ItemObject)
            {
                var DictData = ItemObject.GetDictEntry();
                OutObject.ID = ItemObject.ID;
                OutObject.Area = "";
                if (ItemObject.AmountInStartingpool > 0) { OutObject.Area += "starting "; }
                if (ItemObject.AmountAquiredOnline.Any(x => x.Value > 0)) { OutObject.Area += "online "; }
                OutObject.Area += "item";
                OutObject.Name = DictData.GetName();
                OutObject.OriginalItem = DictData.GetName();
                OutObject.Randomizeditem = DictData.GetName();
                OutObject.Starred = false;
                OutObject.ValidItemTypes = DictData.ItemTypes;
            }
            else if (Object is HintData.HintObject HintObject)
            {
                var DictData = HintObject.GetDictEntry();
                OutObject.ID = HintObject.ID;
                OutObject.Area = "hints";
                OutObject.Name = DictData.Name;
                OutObject.OriginalItem = DictData.Name;
                OutObject.Randomizeditem = DictData.Name;
                OutObject.Starred = false;
                OutObject.ValidItemTypes = ["hint"];
            }
            else if (Object is MacroObject MacroObject)
            {
                bool Istrick = MacroObject.isTrick();
                var DictData = MacroObject.GetDictEntry();
                var LogicData = instance.GetLogic(MacroObject.ID, false);
                OutObject.ID = MacroObject.ID;
                OutObject.Area = Istrick ? (LogicData.TrickCategory ?? "misc") : "macro";
                OutObject.Name = DictData.Name ?? MacroObject.ID;
                OutObject.OriginalItem = MacroObject.ID;
                OutObject.Randomizeditem = MacroObject.ID;
                OutObject.Starred = Istrick;
                List<string> ItemTypes = ["macro"];
                if (Istrick) { ItemTypes.Add("trick"); }
                OutObject.ValidItemTypes = [.. ItemTypes];
            }
            else if (Object is EntranceData.EntranceRandoExit ExitObject)
            {
                OutObject.ID = ExitObject.ID;
                OutObject.Area = ExitObject.DisplayArea();
                OutObject.Name = ExitObject.ExitID;
                OutObject.OriginalItem = ExitObject.EntrancePair == null ? "One Way" : $"{ExitObject.EntrancePair.Area} To {ExitObject.EntrancePair.Exit}";
                OutObject.Randomizeditem = ExitObject.DestinationExit == null ? null : $"{ExitObject.DestinationExit.region} From {ExitObject.DestinationExit.from}";
                OutObject.Starred = ExitObject.Starred;
                List<string> ItemTypes = ["exit"];
                if (ExitObject.EntrancePair == null) { ItemTypes.Add("One Way"); }
                OutObject.ValidItemTypes = [.. ItemTypes];
            }
            else if (Object is OptionData.ChoiceOption ChoiceOptionObject)
            {
                OutObject.ID = ChoiceOptionObject.ID;
                OutObject.Area = "Options";
                OutObject.Name = ChoiceOptionObject.Name;
                OutObject.OriginalItem = ChoiceOptionObject.Value;
                OutObject.Randomizeditem = ChoiceOptionObject.Value;
                OutObject.Starred = true;
                List<string> ItemTypes = ["Option", "Choice"];
                OutObject.ValidItemTypes = [.. ItemTypes];
            }
            else if (Object is OptionData.ToggleOption ToggleOptionObject)
            {
                OutObject.ID = ToggleOptionObject.ID;
                OutObject.Area = "Options";
                OutObject.Name = ToggleOptionObject.Name;
                OutObject.OriginalItem = ToggleOptionObject.Value;
                OutObject.Randomizeditem = ToggleOptionObject.Value;
                OutObject.Starred = true;
                List<string> ItemTypes = ["Option", "Toggle"];
                OutObject.ValidItemTypes = [.. ItemTypes];
            }
            else if (Object is OptionData.IntOption IntOptionObject)
            {
                OutObject.ID = IntOptionObject.ID;
                OutObject.Area = "Options";
                OutObject.Name = IntOptionObject.Name;
                OutObject.OriginalItem = IntOptionObject.Value.ToString();
                OutObject.Randomizeditem = IntOptionObject.Value.ToString();
                OutObject.Starred = true;
                List<string> ItemTypes = ["Option", "int"];
                OutObject.ValidItemTypes = [.. ItemTypes];
            }
            else if (Object is EntranceData.EntranceRandoDestination DestinationObject)
            {
                OutObject.ID = DestinationObject.region;
                OutObject.Area = DestinationObject.from;
                OutObject.Name = DestinationObject.region;
                OutObject.OriginalItem = DestinationObject.from;
                OutObject.Randomizeditem = DestinationObject.from;
                OutObject.Starred = true;
                List<string> ItemTypes = ["Destination"];
                OutObject.ValidItemTypes = [.. ItemTypes];
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
                OutObject.ValidItemTypes = [ObjectString];
            }
            return OutObject;
        }
    }
}
