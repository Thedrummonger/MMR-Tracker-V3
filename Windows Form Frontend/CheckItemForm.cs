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
            if (!_CheckList.Any())
            {
                this.Close();
                return;
            }
            WriteNextItem();
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
            var Names = new List<string>();
            var EnteredItems = new List<object>();
            foreach (var area in _Instance.EntrancePool.AreaList.Values.Where(x => x.LoadingZoneExits.Any()).ToList().SelectMany(x => x.LoadingZoneExits).OrderBy(x => x.Value.ID))
            {
                var Entry = new EntranceData.EntranceRandoDestination
                {
                    region = area.Value.ID,
                    from = area.Value.ParentAreaID,
                };
                if (!SearchStringParser.FilterSearch(_Instance, Entry, textBox1.Text, Entry.ToString())) { continue; }
                EnteredItems.Add(Entry);
            }
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
            this.Text = "Select Item at " + Location.GetDictEntry(_Instance).Name ?? Location.ID;
            var Names = new List<string>();
            var EnteredItems = new List<ItemData.ItemObject>();
            foreach (var i in _Instance.ItemPool.Values)
            {
                if (string.IsNullOrWhiteSpace(i.GetDictEntry(_Instance).GetItemName(_Instance))) { continue; }
                i.DisplayName = i.GetDictEntry(_Instance).GetItemName(_Instance);
                if (!SearchStringParser.FilterSearch(_Instance, i, textBox1.Text, i.DisplayName)) { continue; }
                if (i.CanBePlaced(_Instance) && i.GetDictEntry(_Instance).ItemTypes.Intersect(Location.GetDictEntry(_Instance).ValidItemTypes).Any() && !EnteredItems.Contains(i) && !Names.Contains(i.ToString()))
                {
                    Names.Add(i.ToString());
                    EnteredItems.Add(i);
                }
            }
            listBox1.DataSource = EnteredItems.OrderBy(x => x.GetDictEntry(_Instance).GetItemName(_Instance)).ToList();
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
            WriteNextItem();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            ApplySelection();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ApplySelection(true);
        }
    }
}
