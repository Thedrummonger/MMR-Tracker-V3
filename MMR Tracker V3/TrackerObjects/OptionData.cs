using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class OptionData
    {
        public static readonly Dictionary<string, string> ToggleValues = new()
        {
            {"yes", "no" },
            {"enabled", "disabled" },
            {"true", "false" },
            {"on", "off" },
        };

        public static readonly string[] DisplayListBoxes = { "None", "Available Locations", "Checked Locations" };

        [Serializable]
        public class TrackerOption
        {
            public string ID { get; set; }
            public string DisplayName { get; set; }
            public string CurrentValue { get; set; } = "";
            public Dictionary<string, actions> Values { get; set; } = new Dictionary<string, actions>();
            public actions GetActions()
            {
                return Values[CurrentValue];
            }
            public bool IsToggleOption()
            {
                return 
                    Values.Count == 2 &&
                    Values.Keys.Select(x => x.ToLower()).Intersect(ToggleValues.Keys).Any() && 
                    Values.Keys.Select(x => x.ToLower()).Intersect(ToggleValues.Values).Any();
            }
            public void ToggleOption()
            {
                if (!this.IsToggleOption()) { return; }
                CurrentValue = Values.Keys.First(x => x != CurrentValue);
            }

            public override string ToString()
            {
                return DisplayName + ": " + CurrentValue;
            }
        }

        [Serializable]
        public class actions
        {
            public LogicReplacement[] LogicReplacements { get; set; } = Array.Empty<LogicReplacement>();
            public AdditionalLogic[] AdditionalLogic { get; set; } = Array.Empty<AdditionalLogic>();
            public Dictionary<string, string> ItemNameOverride { get; set; } = new Dictionary<string, string>();
            public bool LocationValid(string ID)
            {
                return LogicReplacements.Any(x => x.LocationValid(ID)) || AdditionalLogic.Any(x => x.LocationValid(ID));
            }
        }

        [Serializable]
        public class AdditionalLogic
        {
            public string[] LocationWhitelist { get; set; } = Array.Empty<string>();
            public string[] LocationBlacklist { get; set; } = Array.Empty<string>();
            public string[] AdditionalRequirements { get; set; } = Array.Empty<string>();
            public List<string>[] AdditionalConditionals { get; set; } = Array.Empty<List<string>>();
            public bool LocationValid(string ID)
            {
                return (!LocationWhitelist.Any() || LocationWhitelist.Contains(ID)) && !LocationBlacklist.Contains(ID);
            }
        }

        [Serializable]
        public class LogicReplacement
        {
            public string[] LocationWhitelist { get; set; } = Array.Empty<string>();
            public string[] LocationBlacklist { get; set; } = Array.Empty<string>();
            public LogicReplacementData[] ReplacementList { get; set; } = Array.Empty<LogicReplacementData>();
            public bool LocationValid(string ID)
            {
                return (!LocationWhitelist.Any() || LocationWhitelist.Contains(ID)) && !LocationBlacklist.Contains(ID);
            }
        }

        [Serializable]
        public class LogicReplacementData
        {
            public string Target { get; set; }
            public string Replacement { get; set; }
        }
    }
}
