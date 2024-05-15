using System.Diagnostics;
using System.Dynamic;
using System.Formats.Asn1;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using Newtonsoft.Json;

namespace TDMUtils
{
    public static class Utility
    {
        public static T DeepClone<T>(this T obj)
        {
            return obj.SerializeConvert <T>();
        }
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
        /// 
        public static string[] SplitAtNewLine(this string s)
        {
            return s.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }
        /// <summary>
        /// Splits a string and trims the values
        /// </summary>
        /// <param name="s">The source string</param>
        /// <param name="Val">The substring to split on</param>
        /// <returns></returns>
        /// 
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
            return JsonConvert.SerializeObject(o, NewtonsoftExtensions.DefaultSerializerSettings);
        }

        public static string ToYamlString(this object e)
        {
            var serializer = new YamlDotNet.Serialization.SerializerBuilder().Build();
            return serializer.Serialize(e);
        }
        /// <summary>
        /// Checks if an object represents a value that could be interpreted as a boolean
        /// </summary>
        /// <param name="val"></param>
        /// <param name="Default">The value to return if the object can not be parsed as a boolean. Throws an exception on failed parse if null</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// 
        public static string ConvertYamlStringToJsonString(string YAML, bool Format = false)
        {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();
            object yamlIsDumb = deserializer.Deserialize<object>(YAML);
            if (Format) { return JsonConvert.SerializeObject(yamlIsDumb, NewtonsoftExtensions.DefaultSerializerSettings); }
            return JsonConvert.SerializeObject(yamlIsDumb);
        }
        public static string ConvertCsvFileToJsonObject(string[] lines)
        {
            var csv = new List<string[]>();

            var properties = lines[0].Split(',');

            foreach (string line in lines)
            {
                var LineData = line.Split(',');
                csv.Add(LineData);
            }

            var listObjResult = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) { continue; }
                var objResult = new Dictionary<string, string>();
                for (int j = 0; j < properties.Length; j++)
                    objResult.Add(properties[j].Trim(), csv[i][j].Trim());

                listObjResult.Add(objResult);
            }

            return JsonConvert.SerializeObject(listObjResult, NewtonsoftExtensions.DefaultSerializerSettings);
        }
        public static T DeserializeJsonFile<T>(string Path)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(Path));
        }
        public static T DeserializeCSVFile<T>(string Path)
        {
            var Json = Utility.ConvertCsvFileToJsonObject(File.ReadAllLines(Path));
            return JsonConvert.DeserializeObject<T>(Json);
        }

        public static T DeserializeYAMLFile<T>(string Path)
        {
            var Json = Utility.ConvertYamlStringToJsonString(File.ReadAllText(Path), true);
            return JsonConvert.DeserializeObject<T>(Json);
        }
        public static bool IsTruthy(this object val, bool? Default = null)
        {
            var result = Default;
            if (val is bool boolVal) { result = (boolVal); }
            else if (val is int IntBoolVal) { result = (IntBoolVal > 0); }
            else if (val is Int64 Int64BoolVal) { result = (Int64BoolVal > 0); }
            else if (val is float FloatBoolVal) { result = (FloatBoolVal > 0); }
            else if (val is double DoubleBoolVal) { result = (DoubleBoolVal > 0); }
            else if (val is decimal DecimalBoolVal) { result = (DecimalBoolVal > 0); }
            else if (val is string StringBoolVal && bool.TryParse(StringBoolVal, out bool rb)) { result = (rb); }

            if (result is null) { throw new Exception($"{val.GetType().Name} {val} Was not a valid truthy Value"); }
            return (bool)result;
        }

        public static int AsIntValue(this object _Value)
        {
            if (_Value is int i1) { return (i1); }
            else if (_Value is Int64 i64) { return (Convert.ToInt32(i64)); }
            else if (_Value is string str && int.TryParse(str, out int istr)) { return (istr); }
            else if (_Value is bool bval) { return (bval ? 1 : 0); }
            else { throw new Exception($"{_Value.GetType().Name} {_Value} Could not be applied to an int option"); }
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
                T test = JsonConvert.DeserializeObject<T>(Json, NewtonsoftExtensions.DefaultSerializerSettings);
                return test != null;
            }
            catch
            {
                return false;
            }
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
        public static string ConvertToCamelCase(this string Input)
        {
            string NiceName = Input.ToLower();
            TextInfo cultInfo = new CultureInfo("en-US", false).TextInfo;
            NiceName = cultInfo.ToTitleCase(NiceName);
            return NiceName;
        }

        public static string AddWordSpacing(this string Input)
        {
            return Regex.Replace(Input, "([a-z])([A-Z])", "$1 $2");
        }

    }
}
