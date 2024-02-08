using MMR_Tracker_V3.TrackerObjects;
using System.Text;

namespace CLIFrontEnd
{
    internal static class CLIUtility
    {
        public static MiscData.Divider CreateDivider(int KeyPadding = 0, string key = "=")
        {
            int Width = Console.WindowWidth - KeyPadding;
            string Divider = "";
            for (var i = 0; i < Width; i++) { Divider += key; }
            return new MiscData.Divider(Divider);
        }

        public class DisplayAction(Action action, string description)
        {
            public Action action = action;
            public string Description = description;
        }

        public enum CLIDisplayListType
        {
            Locations,
            Checked,
            Entrances,
            Options,
            Pathfinder
        }
        public class InputPrefixData
        {
            private HashSet<char> ValidPrefixes = ['\\', '#', '$', '%', '!', '@', '$', '^', '*'];
            public string ParsedInput = "";
            public List<int> Indexes = [];
            public HashSet<char> Prefixes = [];
            public InputPrefixData(string? Input = null, char[]? ValidPrefixChars = null)
            {
                if (ValidPrefixChars is not null) { ValidPrefixes = [.. ValidPrefixChars]; }
                StringBuilder stringBuilder = new();
                Input ??= Console.ReadLine()??"";
                foreach(var i in Input)
                {
                    if (ValidPrefixes.Contains(i)) { Prefixes.Add(i); }
                    else { stringBuilder.Append(i); }
                }
                ParsedInput = stringBuilder.ToString();
                Indexes = InputParser.ParseIndicesString(ParsedInput);
            }
        }


        public static MiscData.DisplayListType? ToStandardDisplayListType(this CLIDisplayListType CLIDLT)
        {
            return CLIDLT switch
            {
                CLIDisplayListType.Locations => MiscData.DisplayListType.Locations,
                CLIDisplayListType.Checked => MiscData.DisplayListType.Checked,
                CLIDisplayListType.Entrances => MiscData.DisplayListType.Entrances,
                _ => null
            };
        }
    }
}
