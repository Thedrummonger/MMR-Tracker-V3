using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class OptionData
    {
        public class TrackerOptions
        {
            public List<TrackerOption> Options { get; set; } = new List<TrackerOption>();
        }
        public class TrackerOption
        {
            public string ID { get; set; }
            public string DisplayName { get; set; }
            public bool Enabled { get; set; } = false;
            public string[] LocationWhitelist { get; set; } = Array.Empty<string>();
            public string[] LocationBlacklist { get; set; } = Array.Empty<string>();
            public LogicReplacement[] LogicReplacements { get; set; } = Array.Empty<LogicReplacement>();
            public string[] AdditionalRequirements { get; set; } = Array.Empty<string>();
            public List<string>[] AdditionalConditionals { get; set; } = Array.Empty<List<string>>();

            public bool LocationValid(string ID)
            {
                return (!LocationWhitelist.Any() || LocationWhitelist.Contains(ID)) && !LocationBlacklist.Contains(ID);
            }
        }

        public class LogicReplacement
        {
            public string Target { get; set; }
            public string Replacement { get; set; }
        }
    }
}
