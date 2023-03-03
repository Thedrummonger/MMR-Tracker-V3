using Octokit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3
{
    public static class ItemTracker
    {
        public class ItemStateChangeEventArgs : EventArgs
        {
            public TrackerState NewState { get; set; }
        }

        public enum TextPosition
        {
            topLeft,
            topRight,
            bottomLeft, 
            bottomRight
        }

        public enum TextType
        {
            text,
            ItemCount
        }
        public enum StaticDirecton
        {
            Horizontal,
            Vertical
        }

        public class TrackerState
        {
            public Dictionary<string, ItemCounts> ItemValues { get; set; } = new Dictionary<string, ItemCounts>();
            public Dictionary<string, bool> MacroValues { get; set; } = new Dictionary<string, bool>();
        }
        public class ItemCounts
        {
            public int Obtained { get; set; } = 0;
            public int Marked { get; set; } = 0;
        }

        public class ItemTrackerInstance
        {
            public ImageSheet imageSheet { get; set; }
            public StaticDirecton LimiterDirection { get; set; } = StaticDirecton.Horizontal;
            public int ImagesPerLimiterDirection { get; set; } = 6;
            public TrackerState trackerState { get; set; }
            public List<DisplayBox> DisplayBoxes { get; set; }

            private void X_ItemStateUpdated(object sender, ItemStateChangeEventArgs e)
            {
                trackerState = e.NewState;
            }

        }

        public class ImageSheet
        {
            public ImageSheet(string ImageSheetPath, int IndividualImageDimentions)
            {
                Image = new Bitmap(ImageSheetPath);
                ImageDimentions = IndividualImageDimentions;
            }
            public Bitmap Image { get; set; }
            public int ImageDimentions { get; set; } = 32;

            public Bitmap GetItemImage(int Column, int Row)
            {
                return Image.Clone(new Rectangle(Column * ImageDimentions, Row * ImageDimentions, ImageDimentions, ImageDimentions), PixelFormat.Format32bppPArgb);
            }
        }

        public class DisplayBox
        {
            public DisplayBox(string BoxID, Bitmap BoxDefaultImage)
            {
                ID = BoxID;
                DefaultImage = BoxDefaultImage;
                DisplayItems = new List<DisplayItem> { };
            }
            public string ID { get; set; }
            public Bitmap DefaultImage { get; set; }
            public List<DisplayItem> DisplayItems { get; set; }

            public DisplayItem GetItemToDisplay(TrackerState trackerState)
            { 
                var ValidImage = DisplayItems.FirstOrDefault(x => x.DisplayItemValid(trackerState));
                if (ValidImage == null) { return new DisplayItem("Default", DefaultImage, "true"); }
                return ValidImage;
            }
        }

        public class DisplayItem
        {
            public DisplayItem(string DefaultName, Bitmap DefaultImage, string DefaultLogicReferenceEntry) 
            {
                Name = DefaultName;
                Image = DefaultImage;
                LogicReferenceEntry= DefaultLogicReferenceEntry;
            }
            public string Name { get; set; }
            public Bitmap Image { get; set; }
            public string LogicReferenceEntry { get; }
            public string FilterString { get; }
            public List<ImageTextBox> TextDisplay { get; set; }
            public bool DisplayItemValid(TrackerState trackerState)
            {
                ParseLogicReferenceEntry(LogicReferenceEntry, out string Entry, out int Amount);
                if (trackerState.ItemValues.TryGetValue(Entry, out ItemCounts counts))
                {
                    return counts.Obtained >= Amount;
                }
                else if (trackerState.MacroValues.TryGetValue(Entry, out bool obtained))
                {
                    return obtained;
                }
                return false;
            }
        }

        public class ImageTextBox
        {
            public TextPosition Position { get; set; }
            public TextType textType { get; set; }
            public string Value { get; set; } 

            public string GetText(TrackerState trackerState)
            {
                if (textType == TextType.text)
                {
                    return Value;
                }
                else if(textType == TextType.ItemCount) 
                {
                    int ObtainedCount = 0;
                    int SeenCount = 0;
                    string[] Items = Value.Split('+').Select(x => x.Trim()).ToArray();
                    foreach(var item in Items)
                    {
                        if (trackerState.ItemValues.ContainsKey(item))
                        {
                            ObtainedCount += trackerState.ItemValues[item].Obtained;
                            SeenCount += trackerState.ItemValues[item].Marked;
                        }
                        else if (trackerState.MacroValues.ContainsKey(item) && trackerState.MacroValues[item])
                        {
                            ObtainedCount += 1;
                        }
                    }
                    if (SeenCount > 0)
                    {
                        return $"{ObtainedCount}\\{ObtainedCount + SeenCount}";
                    }
                    else { return ObtainedCount.ToString(); }
                }
                else
                {
                    return Value;
                }
            }
        }
        private static void ParseLogicReferenceEntry(string LogicReferenceEntry, out string entry, out int amount)
        {
            entry = LogicReferenceEntry;
            amount = 1;
            if (!entry.Contains(',')) { return; }
            var data = entry.Split(',').Select(x => x.Trim()).ToArray();
            entry = data[0];
            if (int.TryParse(data[1].Trim(), out int testAmount)) { amount = testAmount; }
        }

        public static TrackerState CaptureTrackerState(LogicObjects.TrackerInstance Instance)
        {
            TrackerState trackerState = new TrackerState();
            foreach (var item in Instance.ItemPool.Values)
            {
                if (!trackerState.ItemValues.ContainsKey(item.Id)) { trackerState.ItemValues[item.Id] = new ItemCounts(); }
                trackerState.ItemValues[item.Id].Obtained += item.GetTotalUsable(Instance);
            }
            foreach (var location in Instance.LocationPool.Values.Where(x => x.CheckState == TrackerObjects.MiscData.CheckState.Marked))
            {
                string MarkedItem = location.Randomizeditem.Item;
                if (!trackerState.ItemValues.ContainsKey(MarkedItem)) { trackerState.ItemValues[MarkedItem] = new ItemCounts(); }
                trackerState.ItemValues[MarkedItem].Marked += 1;
            }
            foreach(var macro in Instance.MacroPool.Values)
            {
                trackerState.MacroValues[macro.ID] = macro.Aquired;
            }
            return trackerState;    
        }
    }
}
