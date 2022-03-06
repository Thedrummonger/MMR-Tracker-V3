using Microsoft.VisualBasic;
using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
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
    public partial class MainInterface : Form
    {
        public static MMR_Tracker_V3.LogicObjects.TrackerInstance MainUITrackerInstance = new LogicObjects.TrackerInstance();
        public static List<string> UndoStringList = new();
        public static List<string> RedoStringList = new();

        public static MainInterface CurrentProgram;

        private bool FormIsMaximized = false;

        public MainInterface()
        {
            InitializeComponent();
        }

        private void MainInterface_Load(object sender, EventArgs e)
        {
            //Since only one instance of the main interface should ever be open, We can store that instance in a variable to be called from static code.
            if (CurrentProgram != null) { Close(); return; }
            CurrentProgram = this;

            //Ensure the current directory is always the base directory in case the application is opened from a MMRTSave file elsewhere on the system
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            Testing.ISDebugging = ((Control.ModifierKeys != Keys.Control) && Debugger.IsAttached);
            Testing.ViewAsUserMode = ((Control.ModifierKeys == Keys.Control) && Debugger.IsAttached);

            UpdateUI();
        }

        private void UpdateUI()
        {
            var UpperLeftLBL = label1;
            var UpperRightLBL = label3;
            var LowerLeftLBL = label2;
            var LowerRightLBL = label4;
            var LowerRight2LBL = label5;
            var LowerRight3LBL = label6;
            var Menuhieght = menuStrip1.Height;
            var FormHeight = this.Height - 40 - Menuhieght;
            var FormWidth = this.Width - 18;
            var FormHalfHeight = FormHeight / 2;
            var FormHalfWidth = FormWidth / 2;
            var locX = 2;
            var locY = 2 + Menuhieght;

            FormatMenuItems();

            redoToolStripMenuItem.Enabled = RedoStringList.Any();
            undoToolStripMenuItem.Enabled = UndoStringList.Any();

            if (MainUITrackerInstance.LogicFile == null || MainUITrackerInstance.LogicFile.Version < 0)
            {
                Debug.WriteLine("InstanceNull");
                SetObjectVisibility(false, false);
                return;
            }
            else if (MainUITrackerInstance.Options.EntranceRadnoEnabled)
            {
                SetObjectVisibility(true, true);
                UpperLeftLBL.Location = new Point(locX, locY + 2);
                BTNSetItem.Location = new Point(FormHalfWidth - BTNSetItem.Width, Menuhieght + 1);
                TXTLocSearch.Location = new Point(locX, locY + UpperLeftLBL.Height + 6);
                TXTLocSearch.Width = FormHalfWidth - 2;
                LBValidLocations.Location = new Point(locX, locY + UpperLeftLBL.Height + TXTLocSearch.Height + 8);
                LBValidLocations.Width = FormHalfWidth - 2;
                LBValidLocations.Height = FormHalfHeight - UpperLeftLBL.Height - TXTLocSearch.Height - 14;

                UpperRightLBL.Location = new Point(FormHalfWidth + locX, locY + 2);
                BTNSetEntrance.Location = new Point(FormWidth - BTNSetEntrance.Width, Menuhieght + 1);
                TXTEntSearch.Location = new Point(FormHalfWidth + locX, locY + UpperRightLBL.Height + 6);
                TXTEntSearch.Width = FormHalfWidth - 2;
                LBValidEntrances.Location = new Point(FormHalfWidth + locX, locY + UpperRightLBL.Height + TXTEntSearch.Height + 8);
                LBValidEntrances.Width = FormHalfWidth - 2;
                LBValidEntrances.Height = FormHalfHeight - UpperRightLBL.Height - TXTEntSearch.Height - 14;

                LowerLeftLBL.Location = new Point(locX, FormHalfHeight + locY - 2);
                CHKShowAll.Location = new Point(FormHalfWidth - CHKShowAll.Width, Menuhieght + FormHalfHeight - 2);
                TXTCheckedSearch.Location = new Point(locX, locY + UpperLeftLBL.Height + 2 + FormHalfHeight);
                TXTCheckedSearch.Width = FormHalfWidth - 2;
                LBCheckedLocations.Location = new Point(locX, locY + UpperLeftLBL.Height + TXTCheckedSearch.Height + 4 + FormHalfHeight);
                LBCheckedLocations.Width = FormHalfWidth - 2;
                LBCheckedLocations.Height = FormHalfHeight - UpperLeftLBL.Height - TXTCheckedSearch.Height - 8;

                LowerRightLBL.Location = new Point(FormHalfWidth + locX, FormHalfHeight + locY - 2);
                BTNFindPath.Location = new Point(FormWidth - BTNFindPath.Width, Menuhieght + FormHalfHeight - 3);
                LowerRight2LBL.Location = new Point(FormHalfWidth + locX, FormHalfHeight + locY + LowerRightLBL.Height + 6);
                lblSwapPathfinder.Location = new Point(LowerRight2LBL.Location.X + LowerRight2LBL.Width + 4, LowerRight2LBL.Location.Y - 3);
                LowerRight3LBL.Location = new Point(FormHalfWidth + locX, FormHalfHeight + locY + LowerRightLBL.Height + 7 + CMBStart.Height);
                CMBStart.Location = new Point(FormHalfWidth + locX + LowerRight3LBL.Width + 2, FormHalfHeight + locY + LowerRightLBL.Height + 2);
                CMBEnd.Location = new Point(FormHalfWidth + locX + LowerRight3LBL.Width + 2, FormHalfHeight + locY + LowerRightLBL.Height + CMBStart.Height + 5);
                CMBStart.Width = FormHalfWidth - LowerRight3LBL.Width - 4;
                CMBEnd.Width = FormHalfWidth - LowerRight3LBL.Width - 4;
                LBPathFinder.Location = new Point(locX + FormHalfWidth, FormHalfHeight + locY + 8 + LowerRightLBL.Height + CMBStart.Height + CMBEnd.Height);
                LBPathFinder.Width = FormHalfWidth - 2;
                LBPathFinder.Height = LBCheckedLocations.Height - CMBEnd.Height - 5;
            }
            else
            {
                SetObjectVisibility(true, false);
                if (MainUITrackerInstance.Options.WinformData.HorizontalLayout)
                {
                    UpperLeftLBL.Location = new Point(locX, locY + 2);
                    BTNSetItem.Location = new Point(FormHalfWidth - BTNSetItem.Width, Menuhieght + 1);
                    TXTLocSearch.Location = new Point(locX, locY + UpperLeftLBL.Height + 6);
                    TXTLocSearch.Width = FormHalfWidth - 2;
                    LBValidLocations.Location = new Point(locX, locY + UpperLeftLBL.Height + TXTLocSearch.Height + 8);
                    LBValidLocations.Width = FormHalfWidth - 2;
                    LBValidLocations.Height = FormHeight - UpperLeftLBL.Height - TXTLocSearch.Height - 14;

                    LowerLeftLBL.Location = new Point(FormHalfWidth + locX, locY + 2);
                    CHKShowAll.Location = new Point(FormWidth - CHKShowAll.Width, Menuhieght + 3);
                    TXTCheckedSearch.Location = new Point(FormHalfWidth + locX, locY + UpperRightLBL.Height + 6);
                    TXTCheckedSearch.Width = FormHalfWidth - 2;
                    LBCheckedLocations.Location = new Point(FormHalfWidth + locX, locY + UpperRightLBL.Height + TXTEntSearch.Height + 8);
                    LBCheckedLocations.Width = FormHalfWidth - 2;
                    LBCheckedLocations.Height = FormHeight - UpperRightLBL.Height - TXTEntSearch.Height - 14;
                }
                else
                {
                    UpperLeftLBL.Location = new Point(locX, locY + 2);
                    BTNSetItem.Location = new Point(FormWidth - BTNSetItem.Width, Menuhieght + 1);
                    TXTLocSearch.Location = new Point(locX, locY + UpperLeftLBL.Height + 6);
                    TXTLocSearch.Width = FormWidth - 2;
                    LBValidLocations.Location = new Point(locX, locY + UpperLeftLBL.Height + TXTLocSearch.Height + 8);
                    LBValidLocations.Width = FormWidth - 2;
                    LBValidLocations.Height = FormHalfHeight - UpperLeftLBL.Height - TXTLocSearch.Height - 14;

                    LowerLeftLBL.Location = new Point(locX, FormHalfHeight + locY - 2);
                    CHKShowAll.Location = new Point(FormWidth - CHKShowAll.Width, Menuhieght + FormHalfHeight - 2);
                    TXTCheckedSearch.Location = new Point(locX, locY + UpperLeftLBL.Height + 2 + FormHalfHeight);
                    TXTCheckedSearch.Width = FormWidth - 2;
                    LBCheckedLocations.Location = new Point(locX, locY + UpperLeftLBL.Height + TXTCheckedSearch.Height + 4 + FormHalfHeight);
                    LBCheckedLocations.Width = FormWidth - 2;
                    LBCheckedLocations.Height = FormHalfHeight - UpperLeftLBL.Height - TXTCheckedSearch.Height - 8;
                }
            }
            PrintToListBox();
            this.Refresh();
        }

        public void SetObjectVisibility(bool item, bool location)
        {
            var UpperLeftLBL = label1;
            var UpperRightLBL = label3;
            var LowerLeftLBL = label2;
            var LowerRightLBL = label4;
            var LowerRight2LBL = label5;
            var LowerRight3LBL = label6;

            UpperLeftLBL.Visible = item;
            BTNSetItem.Visible = item;
            TXTLocSearch.Visible = item;
            LBValidLocations.Visible = item;
            LowerLeftLBL.Visible = item;
            CHKShowAll.Visible = item;
            TXTCheckedSearch.Visible = item;
            LBCheckedLocations.Visible = item;

            UpperRightLBL.Visible = location;
            BTNSetEntrance.Visible = location;
            TXTEntSearch.Visible = location;
            LBValidEntrances.Visible = location;
            LowerRightLBL.Visible = location;
            BTNFindPath.Visible = location;
            LowerRight2LBL.Visible = location;
            LowerRight3LBL.Visible = location;
            CMBStart.Visible = location;
            CMBEnd.Visible = location;
            LBPathFinder.Visible = location;
            lblSwapPathfinder.Visible = location;
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void MainInterface_ResizeEnd(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void MainInterface_Resize(object sender, EventArgs e)
        {
            //Maximizing and unmaximizing does not trigger ResizeEnd which should be used normally since it doesn't constantly run while resizing.
            //so run this code only if the form becomes maximized or becomes un maximized.
            if (WindowState == FormWindowState.Maximized)
            {
                UpdateUI();
                FormIsMaximized = true;
            }
            else if (FormIsMaximized)
            {
                UpdateUI();
                FormIsMaximized = false;
            }
        }

        private void PrintToListBox(List<ListBox> ToUpdate = null)
        {
            var lbLocTop = LBValidLocations.TopIndex;
            var lbEntTop = LBValidEntrances.TopIndex;
            var lbCheckTop = LBCheckedLocations.TopIndex;

            if (ToUpdate == null) { ToUpdate = new List<ListBox> { LBCheckedLocations, LBValidEntrances, LBValidLocations }; }

            foreach (var i in ToUpdate)
            {
                i.Items.Clear();
                i.ItemHeight = Convert.ToInt32(WinFormUtils.GetFontFromString(MainUITrackerInstance.Options.WinformData.FormFont).Size * 1.8);
                i.Font = WinFormUtils.GetFontFromString(MainUITrackerInstance.Options.WinformData.FormFont);
            }

            Dictionary<string, int> Groups = Utility.GetCategoriesFromFile(MainUITrackerInstance);

            var DataSets = TrackerDataHandeling.PopulateDataSets(MainUITrackerInstance);

            var AvailableLocations = DataSets.AvailableLocations;
            if (TXTLocSearch.Text.StartsWith("^") || CHKShowAll.Checked)
            {
                AvailableLocations = DataSets.UncheckedLocations;
            }

            var CheckedLocations = DataSets.CheckedLocations;

            AvailableLocations = AvailableLocations
                .OrderBy(x => (Groups.ContainsKey(x.GetDictEntry(MainUITrackerInstance).Area.ToLower().Trim()) ? Groups[x.GetDictEntry(MainUITrackerInstance).Area.ToLower().Trim()] : DataSets.AvailableLocations.Count() + 1))
                .ThenBy(x => x.GetDictEntry(MainUITrackerInstance).Area)
                .ThenBy(x => x.DisplayName).ToList();
            CheckedLocations = CheckedLocations
                .OrderBy(x => (Groups.ContainsKey(x.GetDictEntry(MainUITrackerInstance).Area.ToLower().Trim()) ? Groups[x.GetDictEntry(MainUITrackerInstance).Area.ToLower().Trim()] : DataSets.CheckedLocations.Count() + 1))
                .ThenBy(x => x.GetDictEntry(MainUITrackerInstance).Area)
                .ThenBy(x => x.DisplayName).ToList();

            string CurrentLocation = "";
            foreach(var i in AvailableLocations)
            {
                if (!ToUpdate.Contains(LBValidLocations)) { break; }

                if (i.RandomizedState == MiscData.RandomizedState.ForcedJunk) { continue; }

                i.DisplayName = i.GetDictEntry(MainUITrackerInstance).Name ?? i.ID;
                if (i.CheckState == MiscData.CheckState.Marked)
                {
                    string RandomizedItemDisplay = i.Randomizeditem.Item;
                    var RandomizedItem = MainUITrackerInstance.GetItemByID(i.Randomizeditem.Item);
                    if (RandomizedItem != null) { RandomizedItemDisplay = RandomizedItem.GetDictEntry(MainUITrackerInstance).Name ?? RandomizedItem.Id; }
                    i.DisplayName += $": {RandomizedItemDisplay}";
                }
                if (!Utility.FilterSearch(MainUITrackerInstance, i, TXTLocSearch.Text, i.DisplayName)) { continue; }

                if (CurrentLocation != i.GetDictEntry(MainUITrackerInstance).Area)
                {
                    if (LBValidLocations.Items.Count > 0) { LBValidLocations.Items.Add(WinFormUtils.CreateDivider(LBValidLocations)); }
                    LBValidLocations.Items.Add(new MiscData.Areaheader { Area = i.GetDictEntry(MainUITrackerInstance).Area });
                    CurrentLocation = i.GetDictEntry(MainUITrackerInstance).Area;
                }
                LBValidLocations.Items.Add(i);
            }
            CurrentLocation = "";
            foreach (var i in CheckedLocations)
            {
                if (!ToUpdate.Contains(LBCheckedLocations)) { break; }

                if (i.RandomizedState == MiscData.RandomizedState.Unrandomized) { continue; }

                string RandomizedItemDisplay = i.Randomizeditem.Item;
                var RandomizedItem = MainUITrackerInstance.GetItemByID(i.Randomizeditem.Item);
                if (RandomizedItem != null) { RandomizedItemDisplay = RandomizedItem.GetDictEntry(MainUITrackerInstance).Name ?? RandomizedItem.Id; }
                i.DisplayName = $"{RandomizedItemDisplay}: {i.GetDictEntry(MainUITrackerInstance).Name ?? i.ID}";

                if (!Utility.FilterSearch(MainUITrackerInstance, i, TXTCheckedSearch.Text, i.DisplayName)) { continue; }

                if (CurrentLocation != i.GetDictEntry(MainUITrackerInstance).Area)
                {
                    if (LBCheckedLocations.Items.Count > 0) { LBCheckedLocations.Items.Add(WinFormUtils.CreateDivider(LBCheckedLocations)); }
                    LBCheckedLocations.Items.Add(new MiscData.Areaheader { Area = i.GetDictEntry(MainUITrackerInstance).Area });
                    CurrentLocation = i.GetDictEntry(MainUITrackerInstance).Area;
                }
                LBCheckedLocations.Items.Add(i);
            }

            var AvailableHints = DataSets.AvailableHints;
            if (TXTLocSearch.Text.StartsWith("^") || CHKShowAll.Checked)
            {
                AvailableHints = DataSets.UnheckedHints;
            }

            if (AvailableHints.Any())
            {
                bool DividerCreated = false;
                foreach (var i in AvailableHints)
                {
                    i.DisplayName = (i.CheckState == MiscData.CheckState.Marked) ? $"{i.GetDictEntry(MainUITrackerInstance).Name}: {i.HintText}" : i.GetDictEntry(MainUITrackerInstance).Name;
                    if (!i.DisplayName.ToLower().Contains(TXTLocSearch.Text.ToLower())) { continue; }
                    if (!DividerCreated)
                    {
                        if (LBValidLocations.Items.Count > 0) { LBValidLocations.Items.Add(WinFormUtils.CreateDivider(LBValidLocations)); }
                        LBValidLocations.Items.Add("HINTS:");
                        DividerCreated = true;
                    }
                    LBValidLocations.Items.Add(i);
                }
            }
            if (DataSets.CheckedHints.Any())
            {
                bool DividerCreated = false;
                foreach (var i in DataSets.CheckedHints)
                {
                    i.DisplayName = $"{i.GetDictEntry(MainUITrackerInstance).Name}: {i.HintText}";
                    if (!i.DisplayName.ToLower().Contains(TXTCheckedSearch.Text.ToLower())) { continue; }
                    if (!DividerCreated)
                    {
                        if (LBCheckedLocations.Items.Count > 0) { LBCheckedLocations.Items.Add(WinFormUtils.CreateDivider(LBCheckedLocations)); }
                        LBCheckedLocations.Items.Add("HINTS:");
                        DividerCreated = true;
                    }
                    LBCheckedLocations.Items.Add(i);
                }
            }

            if (ToUpdate.Contains(LBValidLocations)) { LBValidLocations.TopIndex = lbLocTop; }
            if (ToUpdate.Contains(LBValidEntrances)) { LBValidEntrances.TopIndex = lbEntTop; }
            if (ToUpdate.Contains(LBCheckedLocations)) { LBCheckedLocations.TopIndex = lbCheckTop; }
        }

        private void NewToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            var Result = fileDialog.ShowDialog();
            if (Result == DialogResult.Cancel || !File.Exists(fileDialog.FileName)) { return; }
            string Logic = File.ReadAllText(fileDialog.FileName);

            if (!WinFormInstanceCreation.CreateWinFormInstance(Logic))
            {
                MainUITrackerInstance = new LogicObjects.TrackerInstance();
                return;
            }

            LogicCalculation.CalculateLogic(MainUITrackerInstance);
            UpdateUI();
            PopulateTrackerOptions();
        }

        private void PopulateTrackerOptions()
        {
            RandomizerOptionsToolStripMenuItem1.DropDownItems.Clear();
            foreach (var i in MainUITrackerInstance.TrackerOptions)
            {
                if (i.IsToggleOption())
                {
                    ToolStripMenuItem menuItem = new ToolStripMenuItem();
                    menuItem.Checked = i.CurrentValue == "enabled";
                    menuItem.Text = i.DisplayName;
                    menuItem.Click += delegate (object sender, EventArgs e) { ToggleRandomizerOption_Click(sender, e, i); };
                    RandomizerOptionsToolStripMenuItem1.DropDownItems.Add(menuItem);
                }
                else if (i.Values.Keys.Count > 1)
                {
                    ToolStripMenuItem menuItem = new ToolStripMenuItem();
                    menuItem.Text = i.DisplayName;
                    RandomizerOptionsToolStripMenuItem1.DropDownItems.Add(menuItem);

                    ToolStripComboBox toolStripComboBox = new();
                    foreach (var j in i.Values)
                    {
                        toolStripComboBox.Items.Add(j.Key);
                    }
                    toolStripComboBox.SelectedIndex = toolStripComboBox.Items.IndexOf(i.CurrentValue);
                    toolStripComboBox.SelectedIndexChanged += delegate (object sender, EventArgs e) 
                    {
                        ToggleRandomizerOption_Click(sender, e, i, toolStripComboBox.SelectedItem); 
                    };
                    menuItem.DropDownItems.Add(toolStripComboBox);
                }
            }
        }

        private void ToggleRandomizerOption_Click(object sender, EventArgs e, OptionData.TrackerOption Option, Object Selection = null)
        {
            if (Selection == null)
            {
                Option.ToggleOption();
                ((ToolStripMenuItem)sender).Checked = Option.CurrentValue == "enabled";
            }
            else
            {
                Option.CurrentValue = (string)Selection;
            }
            LogicCalculation.CalculateLogic(MainUITrackerInstance);
            UpdateUI();
        }

        private void LBValidLocations_DrawItem(object sender, DrawItemEventArgs e)
        {
            var LB = sender as ListBox;
            if (e.Index < 0) { return; }
            e.DrawBackground();
            Font F = WinFormUtils.GetFontFromString(MainUITrackerInstance.Options.WinformData.FormFont);
            Brush brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? Brushes.White : Brushes.Black;
            if (LB.Items[e.Index] is LocationData.LocationObject ListEntry && sender != LBPathFinder)
            {
                var item = ListEntry;
                if (item.CheckState == MiscData.CheckState.Marked && !item.Available && item.Starred) 
                { F = new Font(F.FontFamily, F.Size, FontStyle.Bold | FontStyle.Strikeout); }
                else if (item.Starred) 
                { F = new Font(F.FontFamily, F.Size, FontStyle.Bold); }
                else if (item.CheckState == MiscData.CheckState.Marked && !item.Available) 
                { F = new Font(F.FontFamily, F.Size, FontStyle.Strikeout); }
            }
            e.Graphics.DrawString(LB.Items[e.Index].ToString(), F, brush, e.Bounds);

            e.DrawFocusRectangle();
        }

        private void LBCheckedLocations_DrawItem(object sender, DrawItemEventArgs e)
        {
            var LB = sender as ListBox;
            if (e.Index < 0) { return; }
            e.DrawBackground();
            Font F = WinFormUtils.GetFontFromString(MainUITrackerInstance.Options.WinformData.FormFont);
            Brush brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? Brushes.White : Brushes.Black;
            if (LB.Items[e.Index] is LocationData.LocationObject ListEntry && sender != LBPathFinder)
            {
                var item = ListEntry;
                if (!item.Available && item.Starred)
                { F = new Font(F.FontFamily, F.Size, FontStyle.Bold | FontStyle.Strikeout); }
                else if (item.Starred)
                { F = new Font(F.FontFamily, F.Size, FontStyle.Bold); }
                else if (!item.Available)
                { F = new Font(F.FontFamily, F.Size, FontStyle.Strikeout); }
            }
            e.Graphics.DrawString(LB.Items[e.Index].ToString(), F, brush, e.Bounds);

            e.DrawFocusRectangle();
        }

        private void LBValidEntrances_DoubleClick(object sender, EventArgs e)
        {
            MiscData.CheckState action = MiscData.CheckState.Unchecked;
            if (sender == LBValidLocations || sender == LBValidEntrances) { action = MiscData.CheckState.Checked; }
            if (sender == LBCheckedLocations) { action = MiscData.CheckState.Unchecked; }

            HandleItemSelect((sender as ListBox).SelectedItems.Cast<object>().ToList(), action);
        }

        private void LBValidLocations_MouseUp(object sender, MouseEventArgs e)
        {
            var LB = sender as ListBox;
            int index = LB.IndexFromPoint(e.Location);
            if (index < 0) { return; }
            if (e.Button == MouseButtons.Middle)
            {
                if ((ModifierKeys & Keys.Control) != Keys.Control) { LB.SelectedItems.Clear(); }
                LB.SetSelected(index, true);

                MiscData.CheckState action = MiscData.CheckState.Unchecked;
                if (sender == LBValidLocations || sender == LBValidEntrances) { action = MiscData.CheckState.Marked; }
                if (sender == LBCheckedLocations) { action = MiscData.CheckState.Marked; }

                HandleItemSelect((sender as ListBox).SelectedItems.Cast<object>().ToList(), action);

            }
            else if (e.Button == MouseButtons.Right)
            {
                LB.SelectedItems.Clear();
                LB.SetSelected(index, true);
                ShowContextMenu(LB);
            }
        }

        private void ShowContextMenu(ListBox listBox)
        {
            ContextMenuStrip contextMenuStrip = new();
            //Refresh List Box
            ToolStripItem RefreshContextItem = contextMenuStrip.Items.Add("Refresh");
            RefreshContextItem.Click += (sender, e) => { PrintToListBox(new List<ListBox> { listBox }); };

            if (listBox.SelectedItem is LocationData.LocationObject)
            {
                var itemObject = (listBox.SelectedItem as LocationData.LocationObject);

                //Star Item
                string StarFunction = itemObject.Starred ? "UnStar Location" : "Star Location";
                ToolStripItem StarContextItem = contextMenuStrip.Items.Add(StarFunction);
                StarContextItem.Click += (sender, e) => { itemObject.Starred = !itemObject.Starred; PrintToListBox(new List<ListBox> { listBox }); };

                //ShowLogic
                ToolStripItem ShowLogicFunction = contextMenuStrip.Items.Add("Show Logic");
                ShowLogicFunction.Click += (sender, e) => 
                {
                    ShowLogic showLogic = new ShowLogic(itemObject.ID, MainUITrackerInstance);
                    showLogic.Show();
                };

                if (listBox == LBValidLocations)
                {
                    //Check Item
                    ToolStripItem CheckContextItem = contextMenuStrip.Items.Add("Check Location");
                    CheckContextItem.Click += (sender, e) => { HandleItemSelect(new List<object> { listBox.SelectedItem }, MiscData.CheckState.Checked); };
                    //Mark\UnMark Item
                    string MarkFunction = itemObject.CheckState == MiscData.CheckState.Marked ? "UnMark Location" : "Mark Location";
                    ToolStripItem MarkContextItem = contextMenuStrip.Items.Add(MarkFunction);
                    MarkContextItem.Click += (sender, e) => { HandleItemSelect(new List<object> { listBox.SelectedItem }, MiscData.CheckState.Marked); };
                }
                else if (listBox == LBCheckedLocations)
                {
                    //UnCheck Item
                    ToolStripItem CheckContextItem = contextMenuStrip.Items.Add("UnCheck Location");
                    CheckContextItem.Click += (sender, e) => { HandleItemSelect(new List<object> { listBox.SelectedItem }, MiscData.CheckState.Unchecked); };
                    //Uncheck and Mark Item
                    ToolStripItem MarkContextItem = contextMenuStrip.Items.Add("UnCheck and Mark Location");
                    MarkContextItem.Click += (sender, e) => { HandleItemSelect(new List<object> { listBox.SelectedItem }, MiscData.CheckState.Marked); };
                }
            }

            if (contextMenuStrip.Items.Count > 0)
            {
                contextMenuStrip.Show(Cursor.Position);
            }
        }

        private void HandleItemSelect(List<object> Items, MiscData.CheckState checkState)
        {
            Stopwatch TimeItemSelect = new Stopwatch();
            Stopwatch TimeTotalItemSelect = new Stopwatch();

            string CurrentState = Utf8Json.JsonSerializer.ToJsonString(MainUITrackerInstance);
            Utility.TimeCodeExecution(TimeItemSelect, "Saving Tracker State (UTF8)", 1);

            Debug.WriteLine("===================================");

            Utility.TimeCodeExecution(TimeTotalItemSelect);

            Utility.TimeCodeExecution(TimeItemSelect);
            List<LocationData.LocationObject> locationObjects = Items.Where(x => x is LocationData.LocationObject).Select(x => x as LocationData.LocationObject).ToList();
            List<HintData.HintObject> HintObjects = Items.Where(x => x is HintData.HintObject).Select(x => x as HintData.HintObject).ToList();
            Utility.TimeCodeExecution(TimeItemSelect, "Get Location Data", 1);

            //Items =====================================
            List<LocationData.LocationObject> ManualChecks = new List<LocationData.LocationObject>();
            foreach (LocationData.LocationObject LocationObject in locationObjects)
            {
                if (LocationObject.CheckState != MiscData.CheckState.Unchecked) { continue; }
                if (LocationObject.Randomizeditem.Item == null)
                {
                    LocationObject.Randomizeditem.Item = LocationObject.GetItemAtCheck(MainUITrackerInstance);
                    if (LocationObject.Randomizeditem.Item == null)
                    {
                        ManualChecks.Add(LocationObject);
                    }
                }
            }
            Utility.TimeCodeExecution(TimeItemSelect, "Get Manual Checks", 1);
            TimeTotalItemSelect.Stop();

            if (ManualChecks.Any())
            {
                CheckItemForm checkItemForm = new CheckItemForm(ManualChecks, MainUITrackerInstance);
                checkItemForm.ShowDialog();
            }
            Utility.TimeCodeExecution(TimeItemSelect, "Check Item Form", 1);
            TimeTotalItemSelect.Start();

            foreach (LocationData.LocationObject LocationObject in locationObjects)
            {
                var Action = (checkState == MiscData.CheckState.Marked && LocationObject.CheckState == MiscData.CheckState.Marked) ? MiscData.CheckState.Unchecked : checkState;
                LocationObject.ToggleChecked(Action, MainUITrackerInstance);
            }
            Utility.TimeCodeExecution(TimeItemSelect, "Check Items", 1);

            //Hints======================================

            foreach (HintData.HintObject hintObject in HintObjects)
            {
                if (hintObject.CheckState == MiscData.CheckState.Unchecked)
                {
                    if (hintObject.SpoilerHintText == null)
                    {
                        string input = Interaction.InputBox("Input Hint Text", hintObject.GetDictEntry(MainUITrackerInstance).Name);
                        hintObject.HintText = input;
                    }
                    else
                    {
                        hintObject.HintText = hintObject.SpoilerHintText;
                    }
                }
                var CheckAction = (checkState == MiscData.CheckState.Marked && hintObject.CheckState == MiscData.CheckState.Marked) ? MiscData.CheckState.Unchecked : checkState;
                hintObject.CheckState = CheckAction;
                hintObject.HintText = CheckAction == MiscData.CheckState.Unchecked ? null : hintObject.HintText;
            }
            Utility.TimeCodeExecution(TimeItemSelect, "Check Hints", 1);

            UpdateUndoList(CurrentState);
            Utility.TimeCodeExecution(TimeItemSelect, "Adding to undo List", 1);

            LogicCalculation.CalculateLogic(MainUITrackerInstance);
            Utility.TimeCodeExecution(TimeItemSelect, "Logic Calculation", 1);
            UpdateUI();
            Utility.TimeCodeExecution(TimeItemSelect, "Update UI", -1);
            Utility.TimeCodeExecution(TimeTotalItemSelect, "Total Check Item Action", -1);

        }

        private void TXTLocSearch_TextChanged(object sender, EventArgs e)
        {
            List<ListBox> ToUpdate = new List<ListBox>();
            if (sender == TXTLocSearch) { ToUpdate.Add(LBValidLocations); }
            if (sender == TXTEntSearch) { ToUpdate.Add(LBValidEntrances); }
            if (sender == TXTCheckedSearch) { ToUpdate.Add(LBCheckedLocations); }
            PrintToListBox(ToUpdate);
        }

        private void logicOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RandomizedStateEditor editor = new RandomizedStateEditor(MainUITrackerInstance);
            editor.ShowDialog();

            LogicCalculation.CalculateLogic(MainUITrackerInstance);
            UpdateUI();

            foreach(var i in MainUITrackerInstance.LocationPool)
            {
                if (i.RandomizedState == MiscData.RandomizedState.Unrandomized)
                {
                    Debug.WriteLine(i.ID + " Is Unrandomized");
                }
            }
        }

        public void FormatMenuItems()
        {
            SaveAsToolStripMenuItem1.Visible = (References.CurrentSavePath != "");
            importSpoilerLogToolStripMenuItem.Text = (Utility.CheckforSpoilerLog(MainUITrackerInstance)) ? "Remove Spoiler Log" : "Import Spoiler Log";
            OptionstoolStripMenuItem.Visible = (MainUITrackerInstance.LogicFile.Version > 0);
            undoToolStripMenuItem.Visible = (MainUITrackerInstance.LogicFile.Version > 0);
            redoToolStripMenuItem.Visible = (MainUITrackerInstance.LogicFile.Version > 0);
            refreshToolStripMenuItem.Visible = (MainUITrackerInstance.LogicFile.Version > 0);
            SavetoolStripMenuItem1.Visible = (MainUITrackerInstance.LogicFile.Version > 0);
            spoilerLogToolsToolStripMenuItem.Visible = (MainUITrackerInstance.LogicFile.Version > 0);
            importSpoilerLogToolStripMenuItem.Visible = (MainUITrackerInstance.LogicFile.Version > 0);

            //Manage Dev Menus
            devToolsToolStripMenuItem.Visible = Testing.ISDebugging || Testing.ViewAsUserMode;
            devToolsToolStripMenuItem.Text = (Testing.ViewAsUserMode) ? "Run as Dev" : "Dev Options";
            foreach (ToolStripDropDownItem i in devToolsToolStripMenuItem.DropDownItems) { i.Visible = Testing.ISDebugging; }
            viewAsUserToolStripMenuItem.Checked = Testing.ViewAsUserMode;

        }

        private void TXTLocSearch_MouseUp(object sender, MouseEventArgs e)
        {
            var TB = sender as TextBox;
            if (e.Button == MouseButtons.Middle) { TB.Clear(); }
        }

        private void UpdateUndoList(string State)
        {
            RedoStringList.Clear();
            UndoStringList.Add(State);
            redoToolStripMenuItem.Enabled = RedoStringList.Any();
            undoToolStripMenuItem.Enabled = UndoStringList.Any();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender == undoToolStripMenuItem)
            {
                string CurrentState = Utf8Json.JsonSerializer.ToJsonString(MainUITrackerInstance);
                MainUITrackerInstance = JsonConvert.DeserializeObject<LogicObjects.TrackerInstance>(UndoStringList[^1]);
                RedoStringList.Add(CurrentState);
                UndoStringList.RemoveAt(UndoStringList.Count - 1);
            }
            else if (sender == redoToolStripMenuItem)
            {
                string CurrentState = Utf8Json.JsonSerializer.ToJsonString(MainUITrackerInstance);
                MainUITrackerInstance = JsonConvert.DeserializeObject<LogicObjects.TrackerInstance>(RedoStringList[^1]);
                UndoStringList.Add(CurrentState);
                RedoStringList.RemoveAt(RedoStringList.Count - 1);
            }
            LogicCalculation.CalculateLogic(MainUITrackerInstance);
            UpdateUI();
        }

        private void CodeTestingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Testing.CodeTesting(MainUITrackerInstance);
        }

        private void CHKShowAll_CheckedChanged(object sender, EventArgs e)
        {
            PrintToListBox();
        }
    }
}
