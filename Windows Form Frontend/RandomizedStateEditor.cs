using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
            cmbLocationType.Items.Add("Item Locations");
            cmbLocationType.SelectedIndex = 0;

            PrintToLocationList(); 
            PrintStartingItemData();
            PrinttrickData();
            UpdatedSettingStrings();
        }

        private void UpdatedSettingStrings()
        {
            var LocationList = _Instance.LocationPool.Values.Where(x => !x.GetDictEntry(_Instance).IgnoreForSettingString ?? true).ToList();
            var StartingItems = SpoilerLogTools.GetStartingItemList(_Instance).ToList();
            var TrickList = _Instance.MacroPool.Values.Where(x => x.isTrick(_Instance)).ToList();
            txtLocString.Text = SpoilerLogTools.CreateSettingString(LocationList, LocationList.Where(x => !x.IsUnrandomized()).ToList());
            txtjunkString.Text = SpoilerLogTools.CreateSettingString(LocationList, LocationList.Where(x => x.RandomizedState == MiscData.RandomizedState.ForcedJunk).ToList());
            txtStartString.Text = SpoilerLogTools.CreateSettingString(StartingItems, StartingItems.Where(x => x.AmountInStartingpool > 0).ToList());
            //txtTrickString.Text = CreateSettingString(TrickList, TrickList.Where(x => x.TrickEnabled).ToList());
        }

        private void PrintToLocationList()
        {
            Updating = true;
            lvLocationList.ShowItemToolTips = true;
            lvLocationList.Items.Clear();
            if (cmbLocationType.SelectedItem is string selection && selection == "Item Locations")
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
                if (i.Value.RandomizedState == MiscData.RandomizedState.Randomized && !chkShowRand.Checked) { continue; }
                if (i.Value.RandomizedState == MiscData.RandomizedState.Unrandomized && !chkShowUnrand.Checked) { continue; }
                if (i.Value.RandomizedState == MiscData.RandomizedState.UnrandomizedManual && !chkShowManual.Checked) { continue; }
                if (i.Value.RandomizedState == MiscData.RandomizedState.ForcedJunk && !chkShowJunk.Checked) { continue; }
                i.Value.DisplayName = i.Value.GetDictEntry(_Instance).Name ?? i.Key;
                if (!SearchStringParser.FilterSearch(_Instance, i.Value, TxtLocationSearch.Text, i.Value.DisplayName)) { continue; }
                string VanillaItemText = "";
                if (i.Value.GetDictEntry(_Instance).OriginalItem != null)
                {
                    var VanillaItem = i.Value.GetDictEntry(_Instance).OriginalItem;
                    if (_Instance.InstanceReference.ItemDictionaryMapping.ContainsKey(VanillaItem))
                    {
                        var VanillaItemDictIndex = _Instance.InstanceReference.ItemDictionaryMapping[VanillaItem];
                        var VanillaItemObject = _Instance.LogicDictionary.ItemList[VanillaItemDictIndex];
                        VanillaItemText  = $"{VanillaItemObject.GetItemName(_Instance)} [{VanillaItem}])";
                    }
                    else
                    {
                        VanillaItemText = $"{VanillaItem})";
                    }
                }

                string[] row = { i.Value.DisplayName, VanillaItemText, i.Value.RandomizedState.GetDescription() };
                ListViewItem listViewItem = new ListViewItem(row) { Tag = i.Value };
                listViewItem.Checked = CheckedLocationItems.Contains(i.Value);
                listViewItem.ToolTipText = $"Location: {i.Value.DisplayName} \nVanilla Item: {VanillaItemText} \nRandomized State: {i.Value.RandomizedState}";
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

            foreach(var i in _DataSets.AvailableStartingItems.Where(x => x.CanBePlaced(_Instance)))
            {
                i.DisplayName = i.GetDictEntry(_Instance).GetItemName(_Instance) ?? i.Id;
                if (!SearchStringParser.FilterSearch(_Instance, i, txtSearchAvailableStarting.Text, i.DisplayName)) { continue; }
                lbAvailableStarting.Items.Add(i);
            }
            foreach (var i in _DataSets.CurrentStartingItems)
            {
                i.DisplayName = (i.GetDictEntry(_Instance).GetItemName(_Instance) ?? i.Id) + $": X{i.AmountInStartingpool}";
                if (!SearchStringParser.FilterSearch(_Instance, i, txtSearchCurrentStarting.Text, i.DisplayName)) { continue; }
                lbCurrentStarting.Items.Add(i);
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
                if (!SearchStringParser.FilterSearch(_Instance, i, txtTrickSearch.Text, i.ID)) { continue; }
                if (CurrentCategory != _Instance.GetOriginalLogic(i.ID).TrickCategory)
                {
                    lvTricks.Items.Add(WinFormUtils.CreateDivider(lvTricks, _Instance.GetOriginalLogic(i.ID).TrickCategory.ToUpper()));
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
            UpdatedSettingStrings();
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
            UpdatedSettingStrings();
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
            UpdatedSettingStrings();
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
                if (button == btnSetUnRandomized) 
                { 
                    if (!i.CanBeUnrandomized(_Instance)) { Debug.WriteLine($"{i.ID} Could not be unrandomized"); continue; }
                    i.RandomizedState = MiscData.RandomizedState.Unrandomized; 
                }
                if (button == btnSetManual)
                {
                    if (!i.CanBeUnrandomized(_Instance)) { Debug.WriteLine($"{i.ID} Could not be unrandomized"); continue; }
                    i.RandomizedState = MiscData.RandomizedState.UnrandomizedManual; 
                }
                if (button == btnSetJunk) { i.RandomizedState = MiscData.RandomizedState.ForcedJunk; }
            }
            CheckedLocationItems.Clear();
            
            UpdateItemSets();
            PrintToLocationList();
            PrintStartingItemData();
            UpdatedSettingStrings();
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

        private void button8_Click(object sender, EventArgs e)
        {
            var SuccessLoc = SpoilerLogTools.ApplyLocationString(txtLocString.Text, _Instance);
            var SuncessJunk = SpoilerLogTools.ApplyJunkString(txtjunkString.Text, _Instance);
            var SuncessStart = SpoilerLogTools.ApplyStartingItemString(txtStartString.Text, _Instance);

            UpdateItemSets();

            PrintToLocationList();
            PrintStartingItemData();
            PrinttrickData();

            UpdatedSettingStrings();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            var Result = fileDialog.ShowDialog();
            if (Result == DialogResult.Cancel || !File.Exists(fileDialog.FileName)) { return; }
            try
            {
                MMRData.SpoilerLogData configuration = Newtonsoft.Json.JsonConvert.DeserializeObject<MMRData.SpoilerLogData>(File.ReadAllText(fileDialog.FileName));
                if (configuration.GameplaySettings == null)
                {
                    MessageBox.Show("Setting File Invalid!");
                    return;
                }
                txtLocString.Text = configuration.GameplaySettings.CustomItemListString;
                txtjunkString.Text = configuration.GameplaySettings.CustomJunkLocationsString;
                txtStartString.Text = configuration.GameplaySettings.CustomStartingItemListString;
                _Instance.ApplyRandoSettings(configuration);

                UpdateItemSets();

                PrintToLocationList();
                PrintStartingItemData();
                PrinttrickData();

                UpdatedSettingStrings();
            }
            catch { MessageBox.Show("Setting File Invalid!"); }
        }
    }
}
