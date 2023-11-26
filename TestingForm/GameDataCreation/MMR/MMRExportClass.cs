using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace TestingForm.GameDataCreation.MMR
{
    internal class MMRExportClass
    {
        public class MMRData
        {
            public List<MMRLocation> Locations { get; set; } = new List<MMRLocation>();
            public List<MMRMacro> Macros { get; set; } = new List<MMRMacro>();
            public List<MMRHint> Hints { get; set; } = new List<MMRHint>();
            public List<MMRItem> Items { get; set; } = new List<MMRItem>();
            public List<MMREntrance> Entrances { get; set; } = new List<MMREntrance>();
            public List<MMRAreaClear> AreaClear { get; set; } = new List<MMRAreaClear>();
        }

        public class MMREntrance
        {
            public string ID { get; set; }
            public string Name { get; set; }
        }
        public class MMRAreaClear
        {
            public string ID { get; set; }
        }

        public class MMRItem
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string? ProgressiveGroup { get; set; }
            public bool StartingItem { get; set; }
            public string[] Tags { get; set; }
        }

        public class MMRHint
        {
            public string ID { get; set; }
        }

        public class MMRMacro
        {
            public string ID { get; set; }
            public List<string> PriceNames { get; set; }
        }

        public class MMRLocation
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Area { get; set; }
            public string[] Tags { get; set; }
            public List<string> PriceNames { get; set; }
            public List<LocationProxy> Proxies { get; set; }
        }

        public class LocationProxy
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Area { get; set; }
            public string Logic { get; set; }
        }
    }
}
