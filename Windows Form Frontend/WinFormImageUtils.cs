using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows_Form_Frontend
{
    public class WinFormImageUtils
    {
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
            public string GameCode { get; set; } = "MMR";
            public int LogicVersion { get; set; } = 1;
            public ImageSheet imageSheet { get; set; }
            public StaticDirecton LimiterDirection { get; set; } = StaticDirecton.Horizontal;
            public int ImagesPerLimiterDirection { get; set; } = 6;
            public TrackerState trackerState { get; set; }
            public List<DisplayBox> DisplayBoxes { get; set; } = new List<DisplayBox>();

            public void Initialize()
            {
                imageSheet.Initialize();
                DisplayBoxes.ForEach(x => x.Initialize(imageSheet));
            }

        }

        public class ImageSheet
        {
            public string ImageSheetPath { get; set; }
            private Bitmap Image;
            public int ImageDimentions { get; set; } = 32;

            public void Initialize()
            {
                Image = new Bitmap(ImageSheetPath);
            }

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

            public Point DefaultImageLocations { get; set; } = new Point(0, 0);
            private Bitmap DefaultImage;
            public List<DisplayItem> DisplayItems { get; set; } = new List<DisplayItem>();

            public DisplayItem GetItemToDisplay(TrackerState trackerState)
            {
                var ValidImage = DisplayItems.FirstOrDefault(x => x.DisplayItemValid(trackerState));
                if (ValidImage == null) { return new DisplayItem("Default", DefaultImage, "true"); }
                return ValidImage;
            }
            public void Initialize(ImageSheet imageSheet)
            {
                DefaultImage = imageSheet.GetItemImage(DefaultImageLocations.X, DefaultImageLocations.Y);
                DisplayItems.ForEach(x => x.Initialize(imageSheet));
            }
            public Bitmap GetImage(bool GreyScale = false, bool Invert = false, int? ReshadeR = null, int? ReshadeG = null, int? ReshadeB = null)
            {
                var image = DefaultImage;
                if (GreyScale) { image = GreyImage(image); }
                if (Invert) { image = InvertImage(image); }
                if (ReshadeR is not null || ReshadeG is not null || ReshadeB is not null) { image = ReshadeImage(image, ReshadeR??0, ReshadeG??0, ReshadeB??0); }
                return image;
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
            public Point ImageLocations { get; set; } = new Point(0, 0);
            private Bitmap Image;
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
            public void Initialize(ImageSheet imageSheet)
            {
                Image = imageSheet.GetItemImage(ImageLocations.X, ImageLocations.Y);
            }
            public Bitmap GetImage(bool GreyScale = false, bool Invert = false, int? ReshadeR = null, int? ReshadeG = null, int? ReshadeB = null)
            {
                var image = Image;
                if (GreyScale) { image = GreyImage(image); }
                if (Invert) { image = InvertImage(image); }
                if (ReshadeR is not null || ReshadeG is not null || ReshadeB is not null) { image = ReshadeImage(image, ReshadeR??0, ReshadeG??0, ReshadeB??0); }
                return image;
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
                else if (textType == TextType.ItemCount)
                {
                    int ObtainedCount = 0;
                    int SeenCount = 0;
                    string[] Items = Value.Split('+').Select(x => x.Trim()).ToArray();
                    foreach (var item in Items)
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

        public static TrackerState CaptureTrackerState(MMR_Tracker_V3.LogicObjects.TrackerInstance Instance)
        {
            if (Instance is null) { return new TrackerState(); }
            TrackerState trackerState = new TrackerState();
            foreach (var item in Instance.ItemPool.Values)
            {
                if (!trackerState.ItemValues.ContainsKey(item.Id)) { trackerState.ItemValues[item.Id] = new ItemCounts(); }
                trackerState.ItemValues[item.Id].Obtained += item.GetTotalUsable(Instance);
            }
            foreach (var location in Instance.LocationPool.Values.Where(x => x.CheckState == MMR_Tracker_V3.TrackerObjects.MiscData.CheckState.Marked))
            {
                string MarkedItem = location.Randomizeditem.Item;
                if (!trackerState.ItemValues.ContainsKey(MarkedItem)) { trackerState.ItemValues[MarkedItem] = new ItemCounts(); }
                trackerState.ItemValues[MarkedItem].Marked += 1;
            }
            foreach (var macro in Instance.MacroPool.Values)
            {
                trackerState.MacroValues[macro.ID] = macro.Aquired;
            }
            return trackerState;
        }
        public static Bitmap GreyImage(Bitmap image)
        {
            Bitmap grayScale = new Bitmap(image.Width, image.Height);

            for (Int32 y = 0; y < grayScale.Height; y++)
                for (Int32 x = 0; x < grayScale.Width; x++)
                {
                    Color c = image.GetPixel(x, y);

                    Int32 gs = (Int32)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);

                    grayScale.SetPixel(x, y, Color.FromArgb(gs, gs, gs));
                }
            return grayScale;
        }
        public static Bitmap InvertImage(Bitmap pic)
        {
            for (int y = 0; (y <= (pic.Height - 1)); y++)
            {
                for (int x = 0; (x <= (pic.Width - 1)); x++)
                {
                    Color inv = pic.GetPixel(x, y);
                    if (inv.R == 0 && inv.G == 0 && inv.B == 0) { continue; }
                    inv = Color.FromArgb(inv.A, (255 - inv.R), (255 - inv.G), (255 - inv.B));
                    pic.SetPixel(x, y, inv);
                }
            }
            return pic;
        }
        private static Bitmap ReshadeImage(Bitmap pic, int R, int G, int B)
        {
            pic = pic.Clone() as Bitmap;
            for (int y = 0; (y <= (pic.Height - 1)); y++)
            {
                for (int x = 0; (x <= (pic.Width - 1)); x++)
                {
                    Color inv = pic.GetPixel(x, y);
                    if (inv.R == 0 && inv.G == 0 && inv.B == 0) { continue; }
                    int NewR = (inv.R + R > 255) ? 255 : ((inv.R + R < 0) ? 0 : inv.R + R);
                    int NewB = (inv.B + B > 255) ? 255 : ((inv.B + B < 0) ? 0 : inv.B + B);
                    int NewG = (inv.G + G > 255) ? 255 : ((inv.G + G < 0) ? 0 : inv.G + G);
                    inv = Color.FromArgb(inv.A, NewR, NewG, NewB);
                    pic.SetPixel(x, y, inv);
                }
            }
            return pic;
        }
    }
}
