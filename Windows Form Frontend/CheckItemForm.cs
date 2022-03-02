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
            SelectNextItem();
        }

        private void SelectNextItem()
        {
            this.Text = "Select Item at " + _CheckList[0].UIData.LocationName ?? _CheckList[0].LogicData.Id;
            writeItems();
        }

        private void writeItems()
        {
            var Names = new List<string>();
            var EnteredItems = new List<MMR_Tracker_V3.TrackerObjects.ItemData.ItemObject>();
            foreach (var i in _Instance.ItemPool.CurrentPool)
            {
                if (string.IsNullOrWhiteSpace(i.ItemName) || !i.ItemName.ToLower().Contains(textBox1.Text.ToLower())) { continue; }
                if (i.CanBePlaced(_Instance) && i.ItemTypes.Intersect(_CheckList[0].TrackerData.ValidItemTypes).Any() && !EnteredItems.Contains(i) && !Names.Contains(i.ToString()))
                {
                    Names.Add(i.ToString());
                    EnteredItems.Add(i);
                }
            }
            listBox1.DataSource = EnteredItems.OrderBy(x => x.ItemName).ToList();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            _CheckList[0].TrackerData.RandomizedItem = ((ItemData.ItemObject)listBox1.SelectedItem).Id;
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
