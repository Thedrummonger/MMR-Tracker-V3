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

namespace Windows_Form_Frontend
{
    public partial class ItemDisplay : Form
    {
        private MainInterface _ParentForm;
        private WinFormImageUtils.ItemTrackerInstance _Instance;
        public ItemDisplay(MainInterface ParentForm, WinFormImageUtils.ItemTrackerInstance Instance)
        {
            _ParentForm = ParentForm;
            _Instance = Instance;
            _Instance.Initialize();
            InitializeComponent();
        }

        public void UpdateData(WinFormImageUtils.TrackerState newState)
        {
            Debug.WriteLine($"Data Recieved containing {newState.ItemValues.Count} items and {newState.MacroValues.Count} macros");
        }

        private int GetImageBoxSize()
        {
            return _Instance.LimiterDirection == WinFormImageUtils.StaticDirecton.Horizontal ? (this.Width-16) / _Instance.ImagesPerLimiterDirection : (this.Height-16) / _Instance.ImagesPerLimiterDirection;
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

        private void button1_Click(object sender, EventArgs e)
        {
            DrawingControl.SuspendDrawing(this);
            foreach (System.Windows.Forms.Control item in this.Controls.OfType<PictureBox>().ToList())
            {
                this.Controls.Remove(item);
            }
            int PBSize = GetImageBoxSize();

            int[] testPoints = {0, 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23 };

            foreach(var point in testPoints)
            {
                var PB = new PictureBox
                {
                    Name = $"TestPoint{point}",
                    BorderStyle = BorderStyle.Fixed3D,
                    Image = _Instance.imageSheet.GetItemImage(2, 3),
                    Size = new Size(PBSize, PBSize),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Location = GetPBpositionByIndex(point),
                    BackColor = Color.Transparent,
                    BackgroundImageLayout = ImageLayout.Stretch
                };
                this.Controls.Add(PB);
            }
            DrawingControl.ResumeDrawing(this);
        }

        private void ItemDisplay_ResizeBegin(object sender, EventArgs e)
        {
            DrawingControl.SuspendDrawing(this);
        }

        private void ItemDisplay_ResizeEnd(object sender, EventArgs e)
        {
            DrawingControl.ResumeDrawing(this);
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
