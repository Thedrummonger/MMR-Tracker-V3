using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Windows_Form_Frontend
{
    public partial class CheckItemForm : Form
    {
        List<object> _CheckList;
        LogicObjects.TrackerInstance _Instance;
        public CheckItemForm(IEnumerable<object> ManualChecks, LogicObjects.TrackerInstance Instance)
        {
            InitializeComponent();
            _CheckList = ManualChecks.ToList();
            _Instance = Instance;
        }

        private void CheckItemForm_Load(object sender, EventArgs e)
        {
            WriteNextItem();
        }

        private void CheckItemForm_Shown(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox1.Focus();
        }

        private void FormatUIItems(bool Multiworld, bool Button, string ButtonText)
        {
            if (!Multiworld)
            {
                int OldSearchLength = textBox1.Width;
                textBox1.Width = listBox1.Width;
                button1.Location = new Point(button1.Location.X + (textBox1.Width - OldSearchLength), button1.Location.Y);
                numericUpDown1.Value = -1;
                numericUpDown1.Visible = false;
                label2.Visible = false;
            }
            button1.Visible = Button;
            button1.Text = ButtonText;
        }

        private void WriteNextItem()
        {
            if (!_CheckList.Any())
            {
                this.Close();
                return;
            }
            textBox1.Text = "";
            textBox1.Focus();
            PrintItems();
        }

        private void PrintItems()
        {
            if (_CheckList[0] is LocationData.LocationObject LocationObject)
            {
                writeItemObjectsAtLocation(LocationObject);
            }
            else if (_CheckList[0] is OptionData.TrackerOption OptionObject)
            {
                WriteTrackerOption(OptionObject);
            }
            else if (_CheckList[0] is EntranceData.EntranceRandoExit ExitObject)
            {
                WriteTrackerExits(ExitObject);
            }
            else
            {
                _CheckList.RemoveAt(0);
                WriteNextItem();
            }
        }

        private void WriteTrackerExits(EntranceData.EntranceRandoExit exitObject)
        {
            FormatUIItems(false, false, "");
            this.Text = $"Select Destination of Exit {exitObject.ParentAreaID} -> {exitObject.ID}";
            List<EntranceData.EntranceRandoDestination> EnteredItems = _Instance.GetAllLoadingZoneDestinations(textBox1.Text);
            listBox1.DataSource = EnteredItems;
        }

        private void WriteTrackerOption(OptionData.TrackerOption Option)
        {
            FormatUIItems(false, false, "");
            var EnteredItems = new List<string>();
            this.Text = "Select Value for Option " + Option.DisplayName;
            foreach(var i in Option.Values.Keys)
            {
                EnteredItems.Add(i);
            }
            listBox1.DataSource = EnteredItems;
        }

        private void writeItemObjectsAtLocation(LocationData.LocationObject Location)
        {
            FormatUIItems(false, true, "Set Junk");
            this.Text = "Select Item at " + Location.GetDictEntry(_Instance).GetName(_Instance);
            List<ItemData.ItemObject> EnteredItems = _Instance.GetValidItemsForLocation(Location, textBox1.Text);
            listBox1.DataSource = EnteredItems;
        }

        private void ApplySelection(bool ButtonClick = false)
        {
            if ((listBox1.SelectedIndex < 0 || listBox1.SelectedItem is MiscData.Areaheader || listBox1.SelectedItem is MiscData.Divider) && !ButtonClick) { return; }
            if (_CheckList[0] is LocationData.LocationObject LocationObject)
            {
                if (ButtonClick)
                {
                    LocationObject.Randomizeditem.Item = "JUNK";
                }
                else
                {
                    LocationObject.Randomizeditem.Item = ((ItemData.ItemObject)listBox1.SelectedItem).Id;
                    LocationObject.Randomizeditem.OwningPlayer = (int)numericUpDown1.Value;
                }
            }
            else if (_CheckList[0] is OptionData.TrackerOption OptionObject)
            {
                OptionObject.CurrentValue = listBox1.SelectedItem.ToString();
            }
            else if (_CheckList[0] is EntranceData.EntranceRandoExit ExitObject)
            {
                ExitObject.DestinationExit = (EntranceData.EntranceRandoDestination)listBox1.SelectedItem;
            }
            _CheckList.RemoveAt(0);
            WriteNextItem();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            PrintItems();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            ApplySelection();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ApplySelection(true);
        }

        private void textBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                textBox1.Text = "";
            }
        }
    }
}
