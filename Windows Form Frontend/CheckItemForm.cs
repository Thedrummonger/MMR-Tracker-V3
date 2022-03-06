using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Windows_Form_Frontend
{
    public partial class CheckItemForm : Form
    {
        List<LocationData.LocationObject> _CheckList;
        LogicObjects.TrackerInstance _Instance;
        public CheckItemForm(List<LocationData.LocationObject> ManualChecks, LogicObjects.TrackerInstance Instance)
        {
            InitializeComponent();
            _CheckList = ManualChecks;
            _Instance = Instance;
        }

        private void CheckItemForm_Load(object sender, EventArgs e)
        {
            if (!_CheckList.Any())
            {
                this.Close();
                return;
            }

            bool Multiworld = false;
            if (!Multiworld)
            {
                int OldSearchLength = textBox1.Width;
                textBox1.Width = listBox1.Width;
                button1.Location = new Point(button1.Location.X + (textBox1.Width - OldSearchLength), button1.Location.Y);
                numericUpDown1.Visible = false;
                label2.Visible = false;
            }

            SelectNextItem();
        }

        private void SelectNextItem()
        {
            this.Text = "Select Item at " + _CheckList[0].GetDictEntry(_Instance).Name ?? _CheckList[0].ID;
            writeItems();
        }

        private void writeItems()
        {
            var Names = new List<string>();
            var EnteredItems = new List<ItemData.ItemObject>();
            foreach (var i in _Instance.ItemPool)
            {
                if (string.IsNullOrWhiteSpace(i.GetDictEntry(_Instance).Name) || !i.GetDictEntry(_Instance).Name.ToLower().Contains(textBox1.Text.ToLower())) { continue; }
                i.DisplayName = i.GetDictEntry(_Instance).Name;
                if (i.CanBePlaced(_Instance) && i.GetDictEntry(_Instance).ItemTypes.Intersect(_CheckList[0].GetDictEntry(_Instance).ValidItemTypes).Any() && !EnteredItems.Contains(i) && !Names.Contains(i.ToString()))
                {
                    Names.Add(i.ToString());
                    EnteredItems.Add(i);
                }
            }
            listBox1.DataSource = EnteredItems.OrderBy(x => x.GetDictEntry(_Instance).Name).ToList();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            _CheckList[0].Randomizeditem.Item = ((ItemData.ItemObject)listBox1.SelectedItem).Id;
            _CheckList.RemoveAt(0);
            if (_CheckList.Any())
            {
                SelectNextItem();
            }
            else
            {
                this.Close();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            writeItems();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _CheckList[0].Randomizeditem.Item = "JUNK";
            _CheckList.RemoveAt(0);
            if (_CheckList.Any())
            {
                SelectNextItem();
            }
            else
            {
                this.Close();
            }
        }
    }
}
