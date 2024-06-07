using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Windows.Forms;
using TDMUtils;

namespace Windows_Form_Frontend
{
    public partial class RandomizedStateEditor : Form
    {
        MMR_Tracker_V3.TrackerObjects.InstanceData.TrackerInstance _Instance;
        private TrackerDataHandling.DataSets _DataSets;
        private bool Updating = false;
        public bool ChangesMade = false;
        public RandomizedStateEditor(MMR_Tracker_V3.TrackerObjects.InstanceData.TrackerInstance instance)
        {
            InitializeComponent();
            _Instance = instance;
            _DataSets = TrackerDataHandling.CreateDataSets(instance);
        }

        private void RandomizedStateEditor_Load(object sender, EventArgs e)
        {
            cmbLocationType.Items.Add("Item Locations");
            if (_Instance.GetAllRandomizableExits().Count > 0)
            {
                cmbLocationType.Items.Add("Entrances");
            }
            if (_Instance.HintPool.Any())
            {
                cmbLocationType.Items.Add("Hints");
            }
            cmbLocationType.SelectedIndex = 0;

            PrintToLocationList();
            PrintStartingItemData();
            PrinttrickData();
            UpdatedSettingStrings();
        }

        private void UpdatedSettingStrings()
        {
            var LocationList = _Instance.LocationPool.Values.Where(x => !x.GetDictEntry().IgnoreForSettingString ?? true).ToList();
            var StartingItems = MMR_Tracker_V3.SpoilerLogHandling.SettingStringHandler.GetStartingItemList(_Instance).ToList();
            var TrickList = _Instance.MacroPool.Values.Where(x => x.isTrick()).ToList();
            txtLocString.Text = MMR_Tracker_V3.SpoilerLogHandling.SettingStringHandler.CreateSettingString(LocationList, LocationList.Where(x => !x.IsUnrandomized()).ToList());
            txtjunkString.Text = MMR_Tracker_V3.SpoilerLogHandling.SettingStringHandler.CreateSettingString(LocationList, LocationList.Where(x => x.IsJunk()).ToList());
            txtStartString.Text = MMR_Tracker_V3.SpoilerLogHandling.SettingStringHandler.CreateSettingString(StartingItems, StartingItems.Where(x => x.AmountInStartingpool > 0).ToList());
            //txtTrickString.Text = CreateSettingString(TrickList, TrickList.Where(x => x.TrickEnabled).ToList());
        }

        private void PrintToLocationList()
        {
            Updating = true;

            btnSetUnRandomized.Enabled = true;
            btnSetManual.Enabled = true;
            chkShowManual.Enabled = true;
            chkShowUnrand.Enabled = true;

            lvLocationList.Font = _Instance.StaticOptions.OptionFile.GetFont();
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
                else if (selection == "Hints")
                {
                    btnSetUnRandomized.Enabled = false;
                    btnSetManual.Enabled = false;
                    chkShowManual.Enabled = false;
                    chkShowUnrand.Enabled = false;
                    PrintHintData();
                }
            }
            Updating = false;
        }
        private List<HintData.HintObject> CheckedhintItems = new List<HintData.HintObject>();
        private void PrintHintData()
        {
            List<ListViewItem> TempList = new List<ListViewItem>();
            foreach (var i in _Instance.HintPool)
            {
                if (i.Value.RandomizedState == MiscData.RandomizedState.Randomized && !chkShowRand.Checked) { continue; }
                if (i.Value.RandomizedState == MiscData.RandomizedState.ForcedJunk && !chkShowJunk.Checked) { continue; }
                i.Value.DisplayName = i.Value.GetDictEntry().Name;
                if (!SearchStringParser.FilterSearch(_Instance, i.Value, TxtLocationSearch.Text, i.Value.DisplayName)) { continue; }

                string[] row = { i.Value.DisplayName, "", i.Value.RandomizedState.GetDescription() };
                ListViewItem listViewItem = new(row)
                {
                    Tag = i.Value,
                    Checked = CheckedhintItems.Contains(i.Value),
                    ToolTipText = $"Hint: {i.Value.DisplayName} \nRandomized State: {i.Value.RandomizedState}"
                };
                TempList.Add(listViewItem);
            }
            lvLocationList.Items.AddRange(TempList.ToArray());
            lvLocationList.CheckBoxes = true;
        }

        private void UpdateItemSets()
        {
            _DataSets = TrackerDataHandling.CreateDataSets(_Instance);
        }

        private List<LocationData.LocationObject> CheckedLocationItems = new List<LocationData.LocationObject>();
        private void PrintLocationData()
        {
            List<ListViewItem> TempList = new List<ListViewItem>();
            foreach (var i in _Instance.LocationPool)
            {
                if (i.Value.IsRandomized() && !chkShowRand.Checked) { continue; }
                if (i.Value.IsUnrandomized(MiscData.UnrandState.Unrand) && !chkShowUnrand.Checked) { continue; }
                if (i.Value.IsUnrandomized(MiscData.UnrandState.Manual) && !chkShowManual.Checked) { continue; }
                if (i.Value.IsJunk() && !chkShowJunk.Checked) { continue; }
                i.Value.DisplayName = i.Value.GetDictEntry().GetName();
                if (!SearchStringParser.FilterSearch(_Instance, i.Value, TxtLocationSearch.Text, i.Value.DisplayName)) { continue; }
                string VanillaItemText = "";
                if (i.Value.GetDictEntry().OriginalItem != null)
                {
                    var VanillaItem = i.Value.GetDictEntry().OriginalItem;
                    if (_Instance.GetItemByID(VanillaItem) != null)
                    {
                        var VanillaItemObject = _Instance.GetItemByID(VanillaItem).GetDictEntry();
                        VanillaItemText = $"{VanillaItemObject.GetName()} [{VanillaItem}]";
                    }
                    else
                    {
                        VanillaItemText = $"{VanillaItem}";
                    }
                }

                string[] row = { i.Value.DisplayName, VanillaItemText, i.Value.RandomizedState.GetDescription() };
                ListViewItem listViewItem = new(row)
                {
                    Tag = i.Value,
                    Checked = CheckedLocationItems.Contains(i.Value),
                    ToolTipText = $"Location: {i.Value.DisplayName} \nVanilla Item: {VanillaItemText} \nRandomized State: {i.Value.RandomizedState}"
                };
                TempList.Add(listViewItem);
            }
            lvLocationList.Items.AddRange(TempList.ToArray());
            lvLocationList.CheckBoxes = true;
        }

        private List<EntranceData.EntranceRandoExit> CheckedExitItems = new List<EntranceData.EntranceRandoExit>();
        private void PrintExitData()
        {
            List<ListViewItem> TempList = new List<ListViewItem>();
            foreach (var i in _Instance.GetAllRandomizableExits())
            {
                if (i.IsRandomized() && !chkShowRand.Checked) { continue; }
                if (i.IsUnrandomized(MiscData.UnrandState.Unrand) && !chkShowUnrand.Checked) { continue; }
                if (i.IsUnrandomized(MiscData.UnrandState.Manual) && !chkShowManual.Checked) { continue; }
                if (i.IsJunk() && !chkShowJunk.Checked) { continue; }
                i.DisplayName = i.GetParentArea().ID + " => " + i.ExitID;
                if (!SearchStringParser.FilterSearch(_Instance, i, TxtLocationSearch.Text, i.DisplayName)) { continue; }
                string VanillaItemText = i.ExitID + " <= " + i.GetParentArea().ID;

                string[] row = { i.DisplayName, VanillaItemText, i.RandomizedState.GetDescription() };
                ListViewItem listViewItem = new(row)
                {
                    Tag = i,
                    Checked = CheckedExitItems.Contains(i),
                    ToolTipText = $"Exit: {i.DisplayName} \nVanilla Destination: {VanillaItemText} \nRandomized State: {i.RandomizedState}"
                };
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
            lbAvailableStarting.Font = _Instance.StaticOptions.OptionFile.GetFont();
            lbCurrentStarting.Font = _Instance.StaticOptions.OptionFile.GetFont();

            foreach (var i in _DataSets.AvailableStartingItems.Where(x => x.CanBePlaced()))
            {
                i.DisplayName = i.GetDictEntry().GetName() ?? i.ID;
                if (!SearchStringParser.FilterSearch(_Instance, i, txtSearchAvailableStarting.Text, i.DisplayName)) { continue; }
                lbAvailableStarting.Items.Add(i);
            }
            foreach (var i in _DataSets.CurrentStartingItems)
            {
                i.DisplayName = (i.GetDictEntry().GetName() ?? i.ID) + $": X{i.AmountInStartingpool}";
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
            lvTricks.Font = _Instance.StaticOptions.OptionFile.GetFont();
            string CurrentCategory = string.Empty;
            var TrickList = _DataSets.Tricks.OrderBy(x => _DataSets.Tricks.IndexOf(_DataSets.Tricks.First(y => _Instance.GetLogic(y.ID, false).TrickCategory == _Instance.GetLogic(x.ID, false).TrickCategory)));
            foreach (var i in TrickList)
            {
                var DictEntry = i.GetDictEntry();
                string DisplayName = DictEntry.Name ?? i.ID;
                if (_Instance.GetLogic(i.ID, false).TrickUrl is not null) { DisplayName = "ⓘ" + DisplayName; }
                if (!SearchStringParser.FilterSearch(_Instance, i, txtTrickSearch.Text, DisplayName)) { continue; }
                if (CurrentCategory != (_Instance.GetLogic(i.ID, false).TrickCategory ?? ""))
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
            foreach (var i in lbAvailableStarting.SelectedItems)
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
            else if (e.Item.Tag is HintData.HintObject HintObject)
            {
                if (e.Item.Checked) { CheckedhintItems.Add(HintObject); }
                else { CheckedhintItems.Remove(HintObject); }
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
                if (button == btnSetRandomized) { i.SetRandomizedState(MiscData.RandomizedState.Randomized); }
                if (button == btnSetUnRandomized)
                {
                    if (!i.CanBeUnrandomized()) { Debug.WriteLine($"{i.ID} Could not be unrandomized"); continue; }
                    i.SetRandomizedState(MiscData.RandomizedState.Unrandomized);
                }
                if (button == btnSetManual)
                {
                    if (!i.CanBeUnrandomized()) { Debug.WriteLine($"{i.ID} Could not be unrandomized"); continue; }
                    i.SetRandomizedState(MiscData.RandomizedState.UnrandomizedManual);
                }
                if (button == btnSetJunk) { i.SetRandomizedState(MiscData.RandomizedState.ForcedJunk); }
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
            foreach (var i in CheckedhintItems)
            {
                if (button == btnSetRandomized) { i.RandomizedState = MiscData.RandomizedState.Randomized; }
                if (button == btnSetJunk) { i.RandomizedState = MiscData.RandomizedState.ForcedJunk; }
            }
            CheckedhintItems.Clear();

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
            MMR_Tracker_V3.SpoilerLogHandling.SettingStringHandler.ApplyLocationString(txtLocString.Text, _Instance);
            MMR_Tracker_V3.SpoilerLogHandling.SettingStringHandler.ApplyJunkString(txtjunkString.Text, _Instance);
            MMR_Tracker_V3.SpoilerLogHandling.SettingStringHandler.ApplyStartingItemString(txtStartString.Text, _Instance);

            UpdateItemSets();
            PrintToLocationList();
            PrintStartingItemData();
            PrinttrickData();
            UpdatedSettingStrings();
            ChangesMade = true;
        }

        private void btnLoadSettingFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            var Result = fileDialog.ShowDialog();
            if (Result == DialogResult.Cancel || !File.Exists(fileDialog.FileName)) { return; }
            //Parse as MMR Settings File

            UpdateItemSets();
            PrintToLocationList();
            PrintStartingItemData();
            PrinttrickData();
            UpdatedSettingStrings();
            ChangesMade = true;
        }

        private void lvTricks_MouseUp(object sender, MouseEventArgs e)
        {
            ListView lv = sender as ListView;
            Point localPoint = lv.PointToClient(Cursor.Position);
            ListViewItem index = lv.GetItemAt(localPoint.X, localPoint.Y);
            if (index == null) { return; }
            if (e.Button == MouseButtons.Right)
            {
                //if ((ModifierKeys & Keys.Control) != Keys.Control) { lv.SelectedItems.Clear(); }
                lv.SelectedItems.Clear();
                index.Selected = true;
                //ShowContextMenu(LB);
                ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
                if (index.Tag is MacroObject MO)
                {
                    var Logic = _Instance.GetLogic(MO.ID, false);
                    Debug.WriteLine(Logic.ToFormattedJson());
                    var ToggleItem = new ToolStripMenuItem
                    {
                        Text = "Toggle"
                    };
                    ToggleItem.Click += (sender, e) => { index.Checked = !index.Checked; };
                    contextMenuStrip.Items.Add(ToggleItem);
                    if (!string.IsNullOrWhiteSpace(Logic?.TrickUrl))
                    {
                        var TrickVideo = new ToolStripMenuItem
                        {
                            Text = "Reference Link"
                        };
                        TrickVideo.Click += (sender, e) => { Process.Start(new ProcessStartInfo(Logic.TrickUrl) { UseShellExecute = true }); };
                        contextMenuStrip.Items.Add(TrickVideo);
                    }
                }
                if (contextMenuStrip.Items.Count > 0)
                {
                    contextMenuStrip.Show(Cursor.Position);
                }
            }
        }
    }
}
