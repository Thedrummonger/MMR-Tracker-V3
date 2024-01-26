using MMR_Tracker_V3.TrackerObjects;

namespace CLIFrontEnd
{
    internal static class CLIUtility
    {
        public static MiscData.Divider CreateDivider(string key = "=")
        {
            string Divider = "";
            for (var i = 0; i < Console.WindowWidth; i++) { Divider += key; }
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
        public enum SelectedItemAction
        {
            Filter,
            Check,
            Mark,
            Price,
            Hide
        }
        public class InputPrefixData
        {
            public string ParsedInput = "";
            public SelectedItemAction itemAction = SelectedItemAction.Check;
            public List<int> Indexes = [];
            public InputPrefixData(string? Input)
            {
                if (Input is null)
                {
                    ParsedInput = "";
                }
                else if (Input.StartsWith('\\'))
                {
                    ParsedInput = Input[1..];
                    itemAction = SelectedItemAction.Filter;
                }
                else if (Input.StartsWith('#'))
                {
                    ParsedInput = Input[1..];
                    itemAction = SelectedItemAction.Mark;
                }
                else if (Input.StartsWith('$'))
                {
                    ParsedInput = Input[1..];
                    itemAction = SelectedItemAction.Price;
                }
                else if (Input.StartsWith('%'))
                {
                    ParsedInput = Input[1..];
                    itemAction = SelectedItemAction.Hide;
                }
                else
                {
                    ParsedInput = Input;
                    itemAction = SelectedItemAction.Check;
                }
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
