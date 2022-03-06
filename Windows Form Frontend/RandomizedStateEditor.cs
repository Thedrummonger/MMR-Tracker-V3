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
    public partial class RandomizedStateEditor : Form
    {
        LogicObjects.TrackerInstance _Instance;
        private TrackerDataHandeling.DataSets _DataSets;
        private bool Updating = false;
        public RandomizedStateEditor(LogicObjects.TrackerInstance instance)
        {
            InitializeComponent();
            _Instance = instance;
            _DataSets = TrackerDataHandeling.PopulateDataSets(instance);
        }

        private void RandomizedStateEditor_Load(object sender, EventArgs e)
        {
            var DataSets = TrackerDataHandeling.PopulateDataSets(_Instance);

            cmbLocationType.Items.Add("Item Locations");
            cmbLocationType.SelectedIndex = 0;

            PrintToLocationList(); 
            PrintStartingItemData();
            PrinttrickData();
        }

        private void PrintToLocationList()
        {
            Updating = true;
            lvLocationList.ShowItemToolTips = true;
            lvLocationList.Items.Clear();
            if (cmbLocationType.SelectedItem == "Item Locations")
            {
                PrintLocationData();
            }
            Updating = false;
        }

        private void UpdateItemSets()
        {
            _DataSets = TrackerDataHandeling.PopulateDataSets(_Instance);
        }

        private List<LocationData.LocationObject> CheckedLocationItems = new List<LocationData.LocationObject>();
        private void PrintLocationData()
        {
            List<ListViewItem> TempList = new List<ListViewItem>();
            foreach (var i in _Instance.LocationPool)
            {
                if (i.RandomizedState == MiscData.RandomizedState.Randomized && !chkShowRand.Checked) { continue; }
                if (i.RandomizedState == MiscData.RandomizedState.Unrandomized && !chkShowUnrand.Checked) { continue; }
                if (i.RandomizedState == MiscData.RandomizedState.UnrandomizedManual && !chkShowManual.Checked) { continue; }
                if (i.RandomizedState == MiscData.RandomizedState.ForcedJunk && !chkShowJunk.Checked) { continue; }
                i.DisplayName = i.GetDictEntry(_Instance).Name ?? i.ID;
                if (!Utility.FilterSearch(_Instance, i, TxtLocationSearch.Text, i.DisplayName)) { continue; }
                string VanillaItemText = "";
                if (i.GetDictEntry(_Instance).OriginalItem != null)
                {
                    var VanillaItem = i.GetDictEntry(_Instance).OriginalItem;
                    if (_Instance.InstanceReference.ItemDictionaryMapping.ContainsKey(VanillaItem))
                    {
                        var VanillaItemDictIndex = _Instance.InstanceReference.ItemDictionaryMapping[VanillaItem];
                        var VanillaItemObject = _Instance.LogicDictionary.ItemList[VanillaItemDictIndex];
                        VanillaItemText  = $"{VanillaItemObject.Name} [{VanillaItem}])";
                    }
                    else
                    {
                        VanillaItemText = $"{VanillaItem})";
                    }
                }

                string[] row = { i.DisplayName, VanillaItemText, i.RandomizedState.GetDescription() };
                ListViewItem listViewItem = new ListViewItem(row) { Tag = i };
                listViewItem.Checked = CheckedLocationItems.Contains(i);
                listViewItem.ToolTipText = $"Location: {i.DisplayName} \nVanilla Item: {VanillaItemText} \nRandomized State: {i.RandomizedState}";
                TempList.Add(listViewItem);
            }
            lvLocationList.Items.AddRange(TempList.ToArray());
            lvLocationList.CheckBoxes = true;
        }
        private void PrintStartingItemData()
        {
            Updating = true;
            lbAvailableStarting.Items.Clear();
            lbCurrentStarting.Items.Clear();

            foreach(var i in _DataSets.AvailableStartingItems)
            {
                if (i.CanBePlaced(_Instance))
                {
                    i.DisplayName = i.GetDictEntry(_Instance).Name ?? i.Id;
                    if (i.DisplayName.ToLower().Contains(txtSearchAvailableStarting.Text.ToLower()))
                    {
                        lbAvailableStarting.Items.Add(i);
                    }
                }
            }
            foreach (var i in _DataSets.CurrentStartingItems)
            {
                i.DisplayName = (i.GetDictEntry(_Instance).Name ?? i.Id) + $": X{i.AmountInStartingpool}";
                if (i.DisplayName.ToLower().Contains(txtSearchCurrentStarting.Text.ToLower()))
                {
                    lbCurrentStarting.Items.Add(i);
                }
            }

            Updating = false;
        }
        private void PrinttrickData()
        {
            Updating = true;
            lvTricks.Items.Clear();
            lvTricks.CheckBoxes = true;
            lvTricks.ShowItemToolTips = true;
            string CurrentCategory = string.Empty;
            foreach (var i in _DataSets.Tricks.OrderBy(x => _DataSets.Tricks.IndexOf(_DataSets.Tricks.First(y => _Instance.GetOriginalLogic(y.ID).TrickCategory == _Instance.GetOriginalLogic(x.ID).TrickCategory))))
            {
                if (!i.ID.ToLower().Contains(txtTrickSearch.Text.ToLower())) { continue; }
                if (CurrentCategory != _Instance.GetOriginalLogic(i.ID).TrickCategory)
                {
                    lvTricks.Items.Add(_Instance.GetOriginalLogic(i.ID).TrickCategory.ToUpper());
                    CurrentCategory = _Instance.GetOriginalLogic(i.ID).TrickCategory;
                }

                string[] row = { i.ID };
                ListViewItem listViewItem = new ListViewItem(row) { Tag = i };
                listViewItem.Checked = i.TrickEnabled;
                listViewItem.ToolTipText = _Instance.GetOriginalLogic(i.ID).TrickTooltip;
                lvTricks.Items.Add(listViewItem);
            }
            Updating = false;
        }
        private void lvTricks_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (Updating) { return; }
            if (!(e.Item.Tag is MacroObject)) 
            {
                Updating = true;
                e.Item.Checked = false;
                Updating = false;
                return; 
            }
            var trick = (MacroObject)e.Item.Tag;
            trick.TrickEnabled = e.Item.Checked;
            UpdateItemSets();
        }
        private void btnAddStartingItem_Click(object sender, EventArgs e)
        {
            foreach(var i in lbAvailableStarting.SelectedItems)
            {
                ItemData.ItemObject Item = i as ItemData.ItemObject;
                Item.AmountInStartingpool++;
            }
            UpdateItemSets();
            PrintStartingItemData();
        }

        private void btnRemoveStartingItem_Click(object sender, EventArgs e)
        {
            foreach (var i in lbCurrentStarting.SelectedItems)
            {
                ItemData.ItemObject Item = i as ItemData.ItemObject;
                Item.AmountInStartingpool--;
            }
            UpdateItemSets();
            PrintStartingItemData();
        }

        private void lvLocationList_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (Updating) { return; }
            var item = (LocationData.LocationObject)e.Item.Tag;
            if (e.Item.Checked)
            {
                CheckedLocationItems.Add(item);
            }
            else
            {
                CheckedLocationItems.Remove(item);
            }
        }

        private void TxtLocationSearch_TextChanged(object sender, EventArgs e)
        {
            PrintToLocationList();
        }

        private void btnSetManual_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            foreach (var i in CheckedLocationItems)
            {
                if (button == btnSetRandomized) { i.RandomizedState = MiscData.RandomizedState.Randomized; }
                if (button == btnSetUnRandomized) { i.RandomizedState = MiscData.RandomizedState.Unrandomized; }
                if (button == btnSetManual) { i.RandomizedState = MiscData.RandomizedState.UnrandomizedManual; }
                if (button == btnSetJunk) { i.RandomizedState = MiscData.RandomizedState.ForcedJunk; }
            }
            CheckedLocationItems.Clear();
            UpdateItemSets();
            PrintToLocationList();
        }

        private void cmbLocationType_SelectedValueChanged(object sender, EventArgs e)
        {
            PrintToLocationList();
        }

        private void txtSearchAvailableStarting_TextChanged(object sender, EventArgs e)
        {
            PrintStartingItemData();
        }

        private void txtSearchCurrentStarting_TextChanged(object sender, EventArgs e)
        {
            PrintStartingItemData();
        }

        private void txtTrickSearch_TextChanged(object sender, EventArgs e)
        {
            PrinttrickData();
        }
    }
}
