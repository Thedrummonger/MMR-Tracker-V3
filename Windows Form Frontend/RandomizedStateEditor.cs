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
        public bool ChangesMade = false;
        public RandomizedStateEditor(LogicObjects.TrackerInstance instance)
        {
            InitializeComponent();
            _Instance = instance;
            _DataSets = TrackerDataHandeling.PopulateDataSets(instance);
        }

        private void RandomizedStateEditor_Load(object sender, EventArgs e)
        {
            cmbLocationType.Items.Add("Item Locations");
            if (_Instance.EntrancePool.AreaList.Any())
            {
                cmbLocationType.Items.Add("Entrances");
            }
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
            txtjunkString.Text = SpoilerLogTools.CreateSettingString(LocationList, LocationList.Where(x => x.IsJunk()).ToList());
            txtStartString.Text = SpoilerLogTools.CreateSettingString(StartingItems, StartingItems.Where(x => x.AmountInStartingpool > 0).ToList());
            //txtTrickString.Text = CreateSettingString(TrickList, TrickList.Where(x => x.TrickEnabled).ToList());
        }

        private void PrintToLocationList()
        {
            Updating = true;
            lvLocationList.ShowItemToolTips = true;
            lvLocationList.Items.Clear();
            if (cmbLocationType.SelectedItem is string selection)
            {
                if (selection == "Item Locations")
                {
                    PrintLocationData();
                }
                else if (selection == "Entrances")
                {
                    PrintExitData();
                }
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
                if (i.Value.IsRandomized() && !chkShowRand.Checked) { continue; }
                if (i.Value.IsUnrandomized(1) && !chkShowUnrand.Checked) { continue; }
                if (i.Value.IsUnrandomized(2) && !chkShowManual.Checked) { continue; }
                if (i.Value.IsJunk() && !chkShowJunk.Checked) { continue; }
                i.Value.DisplayName = i.Value.GetDictEntry(_Instance).Name ?? i.Key;
                if (!SearchStringParser.FilterSearch(_Instance, i.Value, TxtLocationSearch.Text, i.Value.DisplayName)) { continue; }
                string VanillaItemText = "";
                if (i.Value.GetDictEntry(_Instance).OriginalItem != null)
                {
                    var VanillaItem = i.Value.GetDictEntry(_Instance).OriginalItem;
                    if (_Instance.GetItemByID(VanillaItem) != null)
                    {
                        var VanillaItemObject = _Instance.GetItemByID(VanillaItem).GetDictEntry(_Instance);
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
                listViewItem.ToolTipText = $"Exit: {i.Value.DisplayName} \nVanilla Destination: {VanillaItemText} \nRandomized State: {i.Value.RandomizedState}";
                TempList.Add(listViewItem);
            }
            lvLocationList.Items.AddRange(TempList.ToArray());
            lvLocationList.CheckBoxes = true;
        }

        private List<EntranceData.EntranceRandoExit> CheckedExitItems = new List<EntranceData.EntranceRandoExit>();
        private void PrintExitData()
        {
            List<ListViewItem> TempList = new List<ListViewItem>();
            foreach (var area in _Instance.EntrancePool.AreaList)
            {
                foreach(var i in area.Value.LoadingZoneExits)
                {
                    if (i.Value.IsRandomized() && !chkShowRand.Checked) { continue; }
                    if (i.Value.IsUnrandomized(1) && !chkShowUnrand.Checked) { continue; }
                    if (i.Value.IsUnrandomized(2) && !chkShowManual.Checked) { continue; }
                    if (i.Value.IsJunk() && !chkShowJunk.Checked) { continue; }
                    i.Value.DisplayName = i.Value.ParentAreaID + " => " + i.Value.ID;
                    if (!SearchStringParser.FilterSearch(_Instance, i.Value, TxtLocationSearch.Text, i.Value.DisplayName)) { continue; }
                    string VanillaItemText = i.Value.ID + " <= " + i.Value.ParentAreaID;

                    string[] row = { i.Value.DisplayName, VanillaItemText, i.Value.RandomizedState.GetDescription() };
                    ListViewItem listViewItem = new ListViewItem(row) { Tag = i.Value };
                    listViewItem.Checked = CheckedExitItems.Contains(i.Value);
                    listViewItem.ToolTipText = $"Location: {i.Value.DisplayName} \nVanilla Item: {VanillaItemText} \nRandomized State: {i.Value.RandomizedState}";
                    TempList.Add(listViewItem);
                }
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
            var TrickList = _DataSets.Tricks.OrderBy(x => _DataSets.Tricks.IndexOf(_DataSets.Tricks.First(y => _Instance.GetLogic(y.ID, false).TrickCategory == _Instance.GetLogic(x.ID, false).TrickCategory)));
            foreach (var i in TrickList)
            {
                var DictEntry = i.GetDictEntry(_Instance);
                string DisplayName = DictEntry.Name ?? i.ID;
                if (!SearchStringParser.FilterSearch(_Instance, i, txtTrickSearch.Text, DisplayName)) { continue; }
                if (CurrentCategory != (_Instance.GetLogic(i.ID, false).TrickCategory??""))
                {
                    lvTricks.Items.Add(WinFormUtils.CreateDivider(lvTricks, _Instance.GetLogic(i.ID, false).TrickCategory.ToUpper()).ToString());
                    CurrentCategory = _Instance.GetLogic(i.ID, false).TrickCategory;
                }

                string[] row = { DisplayName };
                ListViewItem listViewItem = new ListViewItem(row) { Tag = i };
                listViewItem.Checked = i.TrickEnabled;
                listViewItem.ToolTipText = _Instance.GetLogic(i.ID, false).TrickTooltip;
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
            ChangesMade = true;
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
            ChangesMade = true;
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
            ChangesMade = true;
        }

        private void lvLocationList_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (Updating) { return; }
            if (e.Item.Tag is LocationData.LocationObject LocationObject)
            {
                if (e.Item.Checked) { CheckedLocationItems.Add(LocationObject); }
                else { CheckedLocationItems.Remove(LocationObject); }
            }
            else if (e.Item.Tag is EntranceData.EntranceRandoExit ExitObject)
            {
                if (e.Item.Checked) { CheckedExitItems.Add(ExitObject); }
                else { CheckedExitItems.Remove(ExitObject); }
            }

        }

        private void TxtLocationSearch_TextChanged(object sender, EventArgs e)
        {
            PrintToLocationList();
        }

        private void ChangeRandomizationState(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            foreach (var i in CheckedLocationItems)
            {
                if (button == btnSetRandomized) { i.SetRandomizedState(MiscData.RandomizedState.Randomized, _Instance); }
                if (button == btnSetUnRandomized) 
                { 
                    if (!i.CanBeUnrandomized(_Instance)) { Debug.WriteLine($"{i.ID} Could not be unrandomized"); continue; }
                    i.SetRandomizedState(MiscData.RandomizedState.Unrandomized, _Instance);
                }
                if (button == btnSetManual)
                {
                    if (!i.CanBeUnrandomized(_Instance)) { Debug.WriteLine($"{i.ID} Could not be unrandomized"); continue; }
                    i.SetRandomizedState(MiscData.RandomizedState.UnrandomizedManual, _Instance);
                }
                if (button == btnSetJunk) { i.SetRandomizedState(MiscData.RandomizedState.ForcedJunk, _Instance); }
            }
            CheckedLocationItems.Clear();
            foreach (var i in CheckedExitItems)
            {
                if (button == btnSetRandomized) { i.RandomizedState = MiscData.RandomizedState.Randomized; }
                if (button == btnSetUnRandomized) { i.RandomizedState = MiscData.RandomizedState.Unrandomized; }
                if (button == btnSetManual) { i.RandomizedState = MiscData.RandomizedState.UnrandomizedManual; }
                if (button == btnSetJunk) { i.RandomizedState = MiscData.RandomizedState.ForcedJunk; }
            }
            CheckedExitItems.Clear();

            UpdateItemSets();
            PrintToLocationList();
            PrintStartingItemData();
            UpdatedSettingStrings();
            ChangesMade = true;
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

        private void btnApplySettingStrings_Click(object sender, EventArgs e)
        {
            SpoilerLogTools.ApplyLocationString(txtLocString.Text, _Instance);
            SpoilerLogTools.ApplyJunkString(txtjunkString.Text, _Instance);
            SpoilerLogTools.ApplyStartingItemString(txtStartString.Text, _Instance);

            UpdateItemSets();
            PrintToLocationList();
            PrintStartingItemData();
            PrinttrickData();
            UpdatedSettingStrings();
            ChangesMade = true;
        }

        private void btnLoadSettingFile_Click(object sender, EventArgs e)
        {
            MMRData.SpoilerLogData configuration;
            OpenFileDialog fileDialog = new OpenFileDialog();
            var Result = fileDialog.ShowDialog();
            if (Result == DialogResult.Cancel || !File.Exists(fileDialog.FileName)) { return; }
            //Parse as MMR Settings File
            try { configuration = Newtonsoft.Json.JsonConvert.DeserializeObject<MMRData.SpoilerLogData>(File.ReadAllText(fileDialog.FileName)); }
            catch
            {   //Parse as Spoiler Log File
                try { configuration = SpoilerLogTools.ReadSpoilerLog(File.ReadAllLines(fileDialog.FileName)); }
                catch { MessageBox.Show("Setting File Invalid!"); return; }
            }
            if (configuration.GameplaySettings == null) { MessageBox.Show("Setting File Invalid!"); return; }
            txtLocString.Text = configuration.GameplaySettings.CustomItemListString;
            txtjunkString.Text = configuration.GameplaySettings.CustomJunkLocationsString;
            txtStartString.Text = configuration.GameplaySettings.CustomStartingItemListString;
            _Instance.ApplyMMRandoSettings(configuration);

            UpdateItemSets();
            PrintToLocationList();
            PrintStartingItemData();
            PrinttrickData();
            UpdatedSettingStrings();
            ChangesMade = true;
        }
    }
}
