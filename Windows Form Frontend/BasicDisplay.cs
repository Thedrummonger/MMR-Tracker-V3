using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Windows_Form_Frontend
{
    public partial class BasicDisplay : Form
    {
        List<ValueTuple<dynamic, bool>> _displayItems = new List<ValueTuple<dynamic, bool>>();
        public BasicDisplay(List<ValueTuple<dynamic, bool>> Display)
        {
            InitializeComponent();
            CreateDisplayItems(Display);
        }
        public BasicDisplay(List<dynamic> Display)
        {
            InitializeComponent();
            CreateDisplayItems(Display);
        }

        private void CreateDisplayItems(dynamic In)
        {
            if (In is List<ValueTuple<dynamic, bool>> LT) { _displayItems = LT; }
            else if (In is List<dynamic> GT)
            {
                _displayItems.Clear();
                GT.ForEach(i => _displayItems.Add(new ValueTuple<dynamic, bool>(i, true)));
            }
        }

        private void updateDisplay()
        {
            int TopInd = listBox1.TopIndex;
            int selectedInd = listBox1.SelectedIndex;
            listBox1.DataSource = _displayItems.Where(x => x.Item2).Select(x => x.Item1).ToList();
            listBox1.SelectedIndex = selectedInd;
            listBox1.TopIndex = TopInd;
        }

        private void BasicDisplay_Shown(object sender, EventArgs e)
        {
            ResizeListBox();
            listBox1.HorizontalScrollbar = true;
            updateDisplay();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is MiscData.StandardListBoxItem FLI && FLI.tagFunc is not null)
            {
                var output = FLI.tagFunc(ValueTuple.Create(_displayItems, FLI.Tag));
                CreateDisplayItems(output);
                updateDisplay();
            }
        }

        private void BasicDisplay_ResizeEnd(object sender, EventArgs e)
        {
            ResizeListBox();
        }

        private void ResizeListBox()
        {
            Rectangle screenRectangle = this.RectangleToScreen(this.ClientRectangle);
            int titleHeight = screenRectangle.Top - this.Top;
            listBox1.Location = new Point(4, 4);
            listBox1.Height = this.Height - titleHeight - 16;
            listBox1.Width = this.Width - 24;
        }
    }
}
