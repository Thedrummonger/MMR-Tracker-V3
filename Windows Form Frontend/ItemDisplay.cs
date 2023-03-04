using MathNet.Numerics;
using MMR_Tracker_V3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;
using static Windows_Form_Frontend.WinFormImageUtils;

namespace Windows_Form_Frontend
{
    public partial class ItemDisplay : Form
    {
        private MainInterface _ParentForm;
        private WinFormImageUtils.ItemTrackerInstance _Instance;
        private Dictionary<string, PictureBox> PictureBoxes = new Dictionary<string, PictureBox>();
        private Dictionary<string, List<Label>> Labels = new Dictionary<string, List<Label>>();
        WinFormImageUtils.TrackerState trackerState = new WinFormImageUtils.TrackerState();
        public ItemDisplay(MainInterface ParentForm, WinFormImageUtils.ItemTrackerInstance Instance)
        {
            _ParentForm = ParentForm;
            _Instance = Instance;
            _Instance.Initialize();
            InitializeComponent();
        }

        //Form Functions =================================================================
        private void ItemDisplay_Load(object sender, EventArgs e)
        {
            DrawingControl.SuspendDrawing(this);
            CreateDisplayBoxes();
            refreshPage();
            EnforceWindowSize();
            DrawingControl.ResumeDrawing(this);
        }

        private void ItemDisplay_Shown(object sender, EventArgs e)
        {

        }

        private void ItemDisplay_ResizeBegin(object sender, EventArgs e)
        {
            DrawingControl.SuspendDrawing(this);
        }

        private void ItemDisplay_ResizeEnd(object sender, EventArgs e)
        {
            refreshPage();
            EnforceWindowSize();
            DrawingControl.ResumeDrawing(this);
        }

        private void EnforceWindowSize()
        {
            Rectangle screenRectangle = this.RectangleToScreen(this.ClientRectangle);
            int titleHeight = screenRectangle.Top - this.Top;
            int RowCount = (int)Math.Ceiling((decimal)PictureBoxes.Keys.Count / (decimal)_Instance.ImagesPerLimiterDirection);
            var PBSize = GetImageBoxSize() + 2;
            if (_Instance.LimiterDirection == StaticDirecton.Horizontal)
            {
                this.Height = RowCount * PBSize + titleHeight;
            }
            else
            {
                this.Width = RowCount * PBSize + 8;
            }
        }

        //handle picture Boxes =================================================================

        private void CreateDisplayBoxes()
        {
            int PBSize = GetImageBoxSize();
            int Position = 0;
            foreach (var i in _Instance.DisplayBoxes)
            {
                var PB = new PictureBox
                {
                    Name = i.ID,
                    BorderStyle = BorderStyle.Fixed3D,
                    Image = i.GetDeafaultImage(),
                    Size = new Size(PBSize, PBSize),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Location = GetPBpositionByIndex(Position),
                    BackColor = Color.Transparent,
                    BackgroundImageLayout = ImageLayout.Stretch
                };
                PictureBoxes.Add(i.ID, PB);
                Position++;
            }
        }

        private void addPictureBoxesToScreen()
        {
            this.Controls.Clear();
            int PBSize = GetImageBoxSize();
            int Position = 0;
            foreach (var i in PictureBoxes.Values)
            {
                i.Location = GetPBpositionByIndex(Position);
                i.Size = new Size(PBSize, PBSize);
                this.Controls.Add(i);
                Position++;
            }
        }

        //handle picture Box Content =================================================================

        private void UpdateImages()
        {
            foreach (var i in _Instance.DisplayBoxes)
            {
                if (!PictureBoxes.ContainsKey(i.ID)) { Debug.WriteLine($"WARNING {i.ID} WAS ORPHANED"); continue; }
                var CurrentPB = PictureBoxes[i.ID];
                var ValidEntry = i.DisplayItems.FirstOrDefault(x => x.DisplayItemValid(trackerState));
                CurrentPB.Image = ValidEntry is null ? i.GetDeafaultImage() : ValidEntry.GetImage();
                ManageLabels(i, ValidEntry, CurrentPB);
            }
        }

        private void ManageLabels(DisplayBox ParentDisplay, DisplayItem ValidEntry, PictureBox FormPB)
        {
            if (Labels.ContainsKey(ParentDisplay.ID))
            {
                foreach (var lbl in Labels[ParentDisplay.ID]) { this.Controls.Remove(lbl); }
                Labels[ParentDisplay.ID].Clear();
            }
            if (ValidEntry is not null && ValidEntry.TextDisplay.Any())
            {
                foreach (var text in ValidEntry.TextDisplay)
                {
                    AddLabel(text);
                }
            }
            else if (ValidEntry is null && ParentDisplay.DefaultTextDisplay.Any())
            {
                foreach (var text in ParentDisplay.DefaultTextDisplay)
                {
                    AddLabel(text);
                }
            }

            void AddLabel(ImageTextBox text)
            {
                string LabelText = text.GetText(trackerState);
                if (string.IsNullOrWhiteSpace(LabelText)) { return; }
                Label lbl = new Label();
                lbl.Text = LabelText;
                lbl.BackColor = Color.Black;
                lbl.ForeColor = Color.White;
                lbl.Parent= this;
                lbl.AutoSize = true;
                lbl.Location = getLableLocation(text, FormPB, lbl);
                this.Controls.Add(lbl);
                lbl.BringToFront();
                if (!Labels.ContainsKey(ParentDisplay.ID)) { Labels.Add(ParentDisplay.ID, new List<Label>()); }
                Labels[ParentDisplay.ID].Add(lbl);
            }
        }

        //Cross Thread Functions =================================================================
        public void UpdateData(WinFormImageUtils.TrackerState newState)
        {
            trackerState = newState;
            UpdateImages();
        }

        public void CloseThread()
        {
            this.Close();
        }

        //Utility Functions =================================================================

        private int GetImageBoxSize()
        {
            Rectangle screenRectangle = this.RectangleToScreen(this.ClientRectangle);
            int titleHeight = screenRectangle.Top - this.Top;
            return _Instance.LimiterDirection == WinFormImageUtils.StaticDirecton.Horizontal ? (this.Width-16) / _Instance.ImagesPerLimiterDirection : (this.Height - titleHeight - 4) / _Instance.ImagesPerLimiterDirection;
        }

        private Point GetPBpositionByIndex(int index)
        {
            int PBSize = GetImageBoxSize();
            int offset = index / _Instance.ImagesPerLimiterDirection;
            int position = index % _Instance.ImagesPerLimiterDirection;
            if (_Instance.LimiterDirection == WinFormImageUtils.StaticDirecton.Horizontal)
            {
                return new Point(position * PBSize, offset * PBSize);
            }
            else
            {
                return new Point(offset * PBSize, position * PBSize);
            }
        }

        private Point getLableLocation(ImageTextBox ITB, PictureBox ParentPB, Label self)
        {
            int LeftAnchor = ParentPB.Location.X + 2;
            int RightAnchor = LeftAnchor + ParentPB.Width - self.Width - 4;
            int TopAnchor = ParentPB.Location.Y + 2;
            int BottomAnchor = TopAnchor + ParentPB.Height - self.Height - 4;
            switch (ITB.Position)
            {
                case TextPosition.topLeft:
                    return new Point(LeftAnchor, TopAnchor);
                case TextPosition.topRight:
                    return new Point(RightAnchor, TopAnchor);
                case TextPosition.bottomLeft:
                    return new Point(LeftAnchor, BottomAnchor);
                case TextPosition.bottomRight:
                    return new Point(RightAnchor, BottomAnchor);
            }
            return new Point(LeftAnchor, TopAnchor);
        }

        private void refreshPage()
        {
            addPictureBoxesToScreen();
            UpdateImages();
        }
    }
    class DrawingControl
    {
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);

        private const int WM_SETREDRAW = 11;

        public static void SuspendDrawing(System.Windows.Forms.Control parent)
        {
            SendMessage(parent.Handle, WM_SETREDRAW, false, 0);
        }

        public static void ResumeDrawing(System.Windows.Forms.Control parent)
        {
            SendMessage(parent.Handle, WM_SETREDRAW, true, 0);
            parent.Refresh();
        }
    }
}
