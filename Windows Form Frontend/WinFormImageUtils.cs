using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Windows_Form_Frontend.WinFormImageUtils;
using System.IO;
using System.Diagnostics;
using MMR_Tracker_V3.TrackerObjects;
using MMR_Tracker_V3;
using Octokit;
using MMR_Tracker_V3.TrackerObjectExtentions;

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
            ItemCount,
            ObtainedCount,
            SeenCount,
            HasSeen
        }
        public enum StaticDirecton
        {
            Horizontal,
            Vertical
        }

        public class TrackerState
        {
            public Dictionary<string, ItemCounts> ItemValues { get; set; } = new Dictionary<string, ItemCounts>();
            public Dictionary<string, ItemCounts> ItemNameValues { get; set; } = new Dictionary<string, ItemCounts>();
            public Dictionary<string, bool> MacroValues { get; set; } = new Dictionary<string, bool>();
            public Dictionary<string, string> OptionValues { get; set; } = new Dictionary<string, string>();
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
            public void AddDisplayBox(string DisplayBoxID, int DefaultImageX, int DefaultImageY)
            {
                DisplayBox NewItem = new DisplayBox(DisplayBoxID, new Point(DefaultImageX, DefaultImageY));
                DisplayBoxes.Add(NewItem);
            }
            public void AddDisplayItem(string DisplayBoxID, string ID, int ImageX, int ImageY, string Logic, bool Invert = false, int R = 0, int G = 0, int B = 0)
            {
                DisplayItem NewItem = new DisplayItem(ID, new Point(ImageX, ImageY), Logic);
                NewItem.edits = new ImageEdits() { Invert= Invert, Reshade = new RGB(R,G,B) };
                var TargetBox = DisplayBoxes.First(x => x.ID == DisplayBoxID);
                TargetBox.DisplayItems.Add(NewItem);
            }
            public void AddTextToDisplayItem(string DisplayBoxID, string DisplayItemID, TextPosition position, TextType type, string value)
            {
                ImageTextBox imageTextBox = new ImageTextBox();
                imageTextBox.Position = position;
                imageTextBox.textType = type;
                imageTextBox.Value = value;

                var TargetBox = DisplayBoxes.First(x => x.ID == DisplayBoxID);
                var TargetItem = TargetBox.DisplayItems.First(x => x.Name == DisplayItemID);
                TargetItem.TextDisplay.Add(imageTextBox);
            }
            public void AddTextToDefaultImage(string DisplayBoxID, TextPosition position, TextType type, string value)
            {
                ImageTextBox imageTextBox = new ImageTextBox();
                imageTextBox.Position = position;
                imageTextBox.textType = type;
                imageTextBox.Value = value;

                var TargetBox = DisplayBoxes.First(x => x.ID == DisplayBoxID);
                TargetBox.DefaultTextDisplay.Add(imageTextBox);
            }
            public void AddStaticTextBox(string DisplayBoxID, TextPosition position, TextType type, string value)
            {
                AddTextToDefaultImage(DisplayBoxID, position, type, value);
                var TargetBox = DisplayBoxes.First(x => x.ID == DisplayBoxID);
                foreach(var i in TargetBox.DisplayItems)
                {
                    AddTextToDisplayItem(DisplayBoxID, i.Name, position, type, value); 
                }
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
            public DisplayBox(string BoxID, Point BoxDefaultImageLocations)
            {
                ID = BoxID;
                DefaultImageLocations = BoxDefaultImageLocations;
                DisplayItems = new List<DisplayItem> { };
            }
            public string ID { get; set; }

            public Point DefaultImageLocations { get; set; } = new Point(0, 0);
            private Bitmap DefaultImage;
            public List<ImageTextBox> DefaultTextDisplay { get; set; } = new List<ImageTextBox> { };
            public List<DisplayItem> DisplayItems { get; set; } = new List<DisplayItem>();

            public DisplayItem GetItemToDisplay(TrackerState trackerState)
            {
                var ValidImage = DisplayItems.FirstOrDefault(x => x.DisplayItemValid(trackerState));
                if (ValidImage == null) 
                {
                    var NonValid = new DisplayItem("Default", DefaultImageLocations, "true");
                    NonValid.ManualSetImage(DefaultImage);
                    return NonValid; 
                }
                return ValidImage;
            }
            public void Initialize(ImageSheet imageSheet)
            {
                DefaultImage = imageSheet.GetItemImage(DefaultImageLocations.X, DefaultImageLocations.Y);
                DisplayItems.ForEach(x => x.Initialize(imageSheet));
            }
            public Bitmap GetDeafaultImage(bool GreyScale = true)
            {
                var image = DefaultImage;
                if (GreyScale) { image = GreyImage(image); }
                return image;
            }
        }

        public class RGB
        {
            public RGB(int Ri, int Gi, int Bi) 
            {
                R = Ri;
                G = Gi;
                B = Bi;
            }
            public int R { get; set; } = 0;
            public int G { get; set; } = 0;
            public int B { get; set; } = 0;
        }

        public class ImageEdits
        {
            public bool GreyScale { get; set; } = false;
            public bool Invert { get; set; } = false;
            public RGB Reshade { get; set; } = new RGB(0,0,0);
        }

        public class DisplayItem
        {
            public DisplayItem(string DefaultName, Point ImageLocation, string DefaultLogicReferenceEntry)
            {
                Name = DefaultName;
                ImageLocations = ImageLocation;
                LogicReferenceEntry= DefaultLogicReferenceEntry;
            }
            public string Name { get; set; }
            public Point ImageLocations { get; set; } = new Point(0, 0);
            private Bitmap Image;
            public ImageEdits edits { get; set; } = new ImageEdits();
            public string LogicReferenceEntry { get; set; }
            public string FilterString { get; set; }
            public List<ImageTextBox> TextDisplay { get; set; } =  new List<ImageTextBox> { };
            public bool DisplayItemValid(TrackerState trackerState)
            {
                return ParseImageDisplayLogic(trackerState, LogicReferenceEntry);
            }


            public void Initialize(ImageSheet imageSheet)
            {
                Image = imageSheet.GetItemImage(ImageLocations.X, ImageLocations.Y);
            }
            public Bitmap GetImage()
            {
                var image = Image;
                if (edits.GreyScale) { image = GreyImage(image); }
                if (edits.Invert) { image = InvertImage(image); }
                if (edits.Reshade.R != 0 || edits.Reshade.G != 0 || edits.Reshade.B != 0) { image = ReshadeImage(image, edits.Reshade.R, edits.Reshade.G, edits.Reshade.B); }
                return image;
            }
            public void ManualSetImage(Bitmap ManualImage)
            {
                Image = ManualImage;
            }
        }

        public class ImageTextBox
        {
            public TextPosition Position { get; set; }
            public TextType textType { get; set; }
            public string Value { get; set; }

            public string GetText(TrackerState trackerState)
            {
                if (textType == TextType.text) { return Value; }

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
                    else if (trackerState.ItemNameValues.ContainsKey(item))
                    {
                        ObtainedCount += trackerState.ItemValues[item].Obtained;
                        SeenCount += trackerState.ItemValues[item].Marked;
                    }
                    else if (trackerState.MacroValues.ContainsKey(item) && trackerState.MacroValues[item])
                    {
                        ObtainedCount += 1;
                    }
                }

                if (textType == TextType.ItemCount && ObtainedCount < 1 && SeenCount < 1) { return ""; }

                return textType switch
                {
                    TextType.HasSeen => SeenCount >= 1 ? "X" : "",
                    TextType.ItemCount => SeenCount >= 1 ? $"{ObtainedCount}\\{ObtainedCount + SeenCount}" : ObtainedCount.ToString(),
                    TextType.ObtainedCount => ObtainedCount >= 1 ? ObtainedCount.ToString() : "",
                    TextType.SeenCount => SeenCount >= 1 ? SeenCount.ToString() : "",
                    _ => Value,
                };
            }
        }
        private static bool ParseImageDisplayLogic(TrackerState trackerState, string logicReferenceEntry)
        {
            return ConditionalValid(logicReferenceEntry.Split("||").Select(x => x.Split("&&").Select(x => x.Trim()).ToArray()));

            bool ConditionalValid(IEnumerable<string[]> Cond)
            {
                foreach (var cond in Cond)
                {
                    if (RequirementValid(cond)) { return true; }
                }
                return false;
            }

            bool RequirementValid(string[] Req)
            {
                return (Req.All(x => EntryValid(x)));
            }

            bool EntryValid(string Entry)
            {
                ParseAdvanceEntry(Entry, out string CleanEntry, out dynamic Param);

                if (trackerState.ItemValues.TryGetValue(CleanEntry, out ItemCounts counts))
                {
                    return counts.Obtained >= Param;
                }
                else if (trackerState.ItemNameValues.TryGetValue(CleanEntry, out ItemCounts Namecounts))
                {
                    return Namecounts.Obtained >= Param;
                }
                else if (trackerState.MacroValues.TryGetValue(CleanEntry, out bool obtained))
                {
                    return obtained;
                }
                else if (trackerState.OptionValues.TryGetValue(CleanEntry, out string OptionValue))
                {
                    return OptionValue == Param;
                }
                else
                {
                    Debug.WriteLine($"ERROR {CleanEntry}, {Param} Was Unknown)");
                    return false;
                }
            }
            void ParseAdvanceEntry(string LogicReferenceEntry, out string entry, out dynamic amount)
            {
                entry = LogicReferenceEntry;
                amount = 1;
                if (!LogicReferenceEntry.Contains(',')) { return; }
                var data = entry.Split(',').Select(x => x.Trim()).ToArray();
                entry = data[0];
                amount = int.TryParse(data[1].Trim(), out int testAmount) ? testAmount : data[1].Trim().ToString();
            }
        }

        public static TrackerState CaptureTrackerState(MMR_Tracker_V3.InstanceData.TrackerInstance Instance)
        {
            if (Instance is null) { return new TrackerState(); }
            TrackerState trackerState = new TrackerState();
            foreach (var item in Instance.ItemPool.Values)
            {
                if (!trackerState.ItemValues.ContainsKey(item.ID)) { trackerState.ItemValues[item.ID] = new ItemCounts(); }
                trackerState.ItemValues[item.ID].Obtained += item.GetTotalUsable(Instance);

                string ItemName = item.GetDictEntry(Instance).GetName(Instance);
                if (!trackerState.ItemValues.ContainsKey(ItemName)) { trackerState.ItemValues[ItemName] = new ItemCounts(); }
                trackerState.ItemValues[ItemName].Obtained += item.GetTotalUsable(Instance);
            }
            foreach (var location in Instance.LocationPool.Values.Where(x => x.CheckState == MMR_Tracker_V3.TrackerObjects.MiscData.CheckState.Marked))
            {
                string MarkedItem = location.Randomizeditem.Item;
                if (!trackerState.ItemValues.ContainsKey(MarkedItem)) { trackerState.ItemValues[MarkedItem] = new ItemCounts(); }
                trackerState.ItemValues[MarkedItem].Marked += 1;

                string ItemName = Instance.GetItemByID(MarkedItem)?.GetDictEntry(Instance)?.GetName(Instance);
                if (ItemName is not null)
                {
                    if (!trackerState.ItemValues.ContainsKey(ItemName)) { trackerState.ItemValues[ItemName] = new ItemCounts(); }
                    trackerState.ItemValues[ItemName].Marked += 1;
                }
            }
            foreach (var macro in Instance.MacroPool.Values)
            {
                trackerState.MacroValues[macro.ID] = macro.Aquired;
            }
            foreach (var option in Instance.ChoiceOptions.Values)
            {
                trackerState.OptionValues[option.ID] = option.Value;
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

        public static ItemTrackerInstance GetImageSheet()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ItemTrackerInstance>(File.ReadAllText(Path.Combine("ItemTrackerData", "MMRItemTracker.json")));
        }
    }
}
