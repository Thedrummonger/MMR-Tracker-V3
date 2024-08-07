﻿using MMR_Tracker_V3;
using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.SpoilerLogHandling;
using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static TDMUtils.EnumerableUtilities;
using static TDMUtils.MiscUtilities;

namespace Windows_Form_Frontend
{
    public partial class MainInterface : Form
    {
        public static InstanceContainer InstanceContainer = new InstanceContainer();
        public static MainInterface CurrentProgram;
        public static bool IsSubForm = false;
        Pathfinder MainInterfacepathfinder = new Pathfinder();
        private bool FormIsMaximized = false;
        Thread MainInterfaceItemDisplayThread = null;
        ItemDisplay MainInterfaceItemDisplayForm = null;
        public NetClient CurrentNetClientForm;
        private Dictionary<string, ToolStripMenuItem> MenuItemParentTree = new Dictionary<string, ToolStripMenuItem>();

        List<Control> TLPLocationsControls = new List<Control>();
        List<Control> TLPCheckedControls = new List<Control>();
        List<Control> TLPEntranceControls = new List<Control>();
        List<Control> TLPPathfinderControls = new List<Control>();

        MiscData.DisplayListType ViewFocus = DisplayListType.Locations;

        public MainInterface(bool _SubForm = false)
        {
            IsSubForm = _SubForm;
            InitializeComponent();

            //Since only one instance of the main interface should ever be open, We can store that instance in a variable to be called from static code.
            if (CurrentProgram != null) { Debug.WriteLine("Main interface was loaded when it already exits"); Close(); return; }
            CurrentProgram = this;

            //Ensure the current directory is always the base directory in case the application is opened from a MMRTSave file elsewhere on the system
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Debug.WriteLine($"Setting Current Diretory to: {AppDomain.CurrentDomain.BaseDirectory}");

            if (!Directory.Exists(References.Globalpaths.BaseAppdataPath))
            {
                Directory.CreateDirectory(References.Globalpaths.BaseAppdataPath);
            }

            foreach (var i in tlpLocations.Controls) { TLPLocationsControls.Add((Control)i); }
            foreach (var i in tlpChecked.Controls) { TLPCheckedControls.Add((Control)i); }
            foreach (var i in tlpEntrances.Controls) { TLPEntranceControls.Add((Control)i); }
            foreach (var i in tlpPathFinder.Controls) { TLPPathfinderControls.Add((Control)i); }
        }

        //MainForm Actions

        public void MainInterface_Load(object sender, EventArgs e)
        {
            if (!DoUpdateCheck())
            {
                this.Close();
                return;
            }
            UpdateUI();
            AlignUIElements();
            WinFormInstanceCreation.ApplyUserPretLogic();
            ParseCLIArgs();
        }

        private void ParseCLIArgs()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            if (arguments.Length <= 1) { return; }
            var file = arguments[1];
            if (Path.GetExtension(file).EndsWith("mmrtsav"))
            {
                LoadSAVFile(file);
            }
            else if (Path.GetExtension(file).ToLower().EndsWith("json"))
            {
                LoadLogicFile(file);
            }
        }

        public bool DoUpdateCheck()
        {
            References.TrackerVersionStatus = UpdateManager.GetTrackerVersionStatus();
            if (References.TrackerVersionStatus.VersionStatus == UpdateManager.versionStatus.outdated)
            {
                var Download = MessageBox.Show($"Your tracker version {References.trackerVersion} is out of Date. Would you like to download the latest version {References.TrackerVersionStatus.LatestVersion.TagName}?\n\nTo disable this message click \"cancel\" or disable \"Check for Updates\" in the options file", "Tracker Out of Date", MessageBoxButtons.YesNoCancel);
                if (Download == DialogResult.Yes) { { Process.Start(new ProcessStartInfo(References.TrackerVersionStatus.LatestVersion.HtmlUrl) { UseShellExecute = true }); return false; } }
                else if (Download == DialogResult.Cancel) { UpdateManager.DisableUpdateChecks(); }
            }
            return true;
        }

        private void MainInterface_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!PromptSave()) { e.Cancel = true; }
            if (OBJIsThreadSafe(MainInterfaceItemDisplayThread, MainInterfaceItemDisplayForm))
            {
                MainInterfaceItemDisplayForm?.Invoke(new MethodInvoker(delegate { MainInterfaceItemDisplayForm.CloseThread(); }));
            }
            if (CurrentNetClientForm is not null)
            {
                CurrentNetClientForm.SkipCloseConfirmation = true;
                CurrentNetClientForm.Close();
            }
        }

        //Menu Strip

        private void UndoRedo_Click(object sender, EventArgs e)
        {
            if (sender == undoToolStripMenuItem)
            {
                InstanceContainer.DoUndo();
            }
            else if (sender == redoToolStripMenuItem)
            {
                InstanceContainer.DoRedo();
            }

            InstanceContainer.logicCalculation.CalculateLogic();
            UpdateUI();
            AlignUIElements();
        }

        //Menu Strip => File

        public void NewToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            fileToolMenuStrip.HideDropDown();
            if (!PromptSave()) { return; }
            OpenFileDialog fileDialog = new OpenFileDialog();
            var Result = fileDialog.ShowDialog();
            if (Result == DialogResult.Cancel || !File.Exists(fileDialog.FileName)) { return; }
            LoadLogicFile(fileDialog.FileName);
        }

        private void LoadLogicFile(string filename)
        {
            var LogicData = LogicFileParser.GetLogicData(filename);
            if (LogicData is null) { MessageBox.Show("Invalid File\nPlease select either a logic file or MMR spoiler log"); return; }
            string Logic = string.Join("", LogicData);
            WinFormInstanceCreation.CreateWinFormInstance(Logic);
        }

        private void SavetoolStripMenuItem1_Click(object sender, EventArgs e)
        {
            bool SaveAs = (sender is ToolStripMenuItem item && item == SaveAsToolStripMenuItem);
            if (File.Exists(InstanceContainer.CurrentSavePath) && !SaveAs)
            {
                InstanceContainer.UnsavedChanges = false;
                InstanceContainer.SaveInstance(InstanceContainer.CurrentSavePath);
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "MMRT Save File|*.mmrtsav";
                saveFileDialog.Title = "Save Tracker";
                saveFileDialog.ShowDialog();
                if (saveFileDialog.FileName != "")
                {
                    InstanceContainer.UnsavedChanges = false;
                    InstanceContainer.SaveInstance(saveFileDialog.FileName);
                    InstanceContainer.CurrentSavePath = saveFileDialog.FileName;
                }
            }
            UpdateUI();
        }

        private void LoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!PromptSave()) { return; }
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MMRT Save File|*.mmrtsav";
            openFileDialog.Title = "Load Tracker Save";
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName != "")
            {
                LoadSAVFile(openFileDialog.FileName);
            }
        }

        private void LoadSAVFile(string Path)
        {
            if (!InstanceContainer.LoadInsanceFromFile(Path))
            {
                MessageBox.Show("Save File Not Valid");
                return;
            }
            InstanceContainer.CurrentSavePath = Path;
            InstanceContainer.logicCalculation.CalculateLogic();
            UpdateUI();
            UpdateDynamicUserOptions();
            AlignUIElements();
        }

        //Menu Strip => Options

        private void logicOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstanceContainer.logicCalculation.ResetAutoObtainedItems();
            string CurrentState = InstanceContainer.Instance.ToJson(MiscData.JSONType.UTF8);
            RandomizedStateEditor editor = new RandomizedStateEditor(InstanceContainer.Instance);
            editor.ShowDialog();
            if (editor.ChangesMade) { SaveTrackerState(CurrentState); }

            InstanceContainer.logicCalculation.CalculateLogic();
            UpdateUI();
            AlignUIElements();
        }

        //Menu Strip => Tools

        private void importSpoilerLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (InstanceContainer.Instance.SpoilerLog is null)
            {
                if (InstanceContainer.Instance.LogicDictionary.SpoilerLogInstructions is not null)
                {
                    SpoilerTools.ApplySpoilerLog(InstanceContainer, WinFormUtils.SelectAndReadFile);
                }
                else
                {
                    MessageBox.Show("Spoiler log importing is not implemented for this Game");
                }
            }
            else
            {
                SaveTrackerState();
                SpoilerTools.RemoveSpoilerData(InstanceContainer.Instance);
            }
            InstanceContainer.logicCalculation.CalculateLogic();
            UpdateUI();
            AlignUIElements();
        }

        //Menu Strip => Dev


        private void CodeTestingToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        //ListBoxes

        public void GetListObjectData(ListBox LB, dynamic entry, out MiscData.CheckState checkState, out bool starred, out bool Available)
        {
            if (entry is LocationData.LocationProxy po)
            {
                checkState = po.GetReferenceLocation().CheckState;
                starred = po.Starred;
                Available = po.ProxyAvailable();
            }
            else if (entry is MiscData.Areaheader ah)
            {
                DisplayListType? displayList = null;
                if (LB == LBValidLocations) { displayList = DisplayListType.Locations; }
                if (LB == LBValidEntrances) { displayList = DisplayListType.Entrances; }
                if (LB == LBCheckedLocations) { displayList = DisplayListType.Checked; }
                bool IsMinimizedArea = displayList is not null && ah.IsMinimized((DisplayListType)displayList, InstanceContainer.Instance.StaticOptions);
                checkState = MiscData.CheckState.Marked;
                starred = IsMinimizedArea;
                Available = !IsMinimizedArea;
            }
            else
            {
                checkState = DynamicPropertyExist(entry, "CheckState") ? entry.CheckState : MiscData.CheckState.Checked;
                starred = DynamicPropertyExist(entry, "Starred") ? entry.Starred : false;
                Available = DynamicPropertyExist(entry, "Available") ? entry.Available : true;
            }
        }

        private void LBValidLocations_DrawItem(object sender, DrawItemEventArgs e)
        {
            var LB = sender as ListBox;
            if (e.Index < 0) { return; }
            e.DrawBackground();
            Font F = InstanceContainer.Instance.StaticOptions.OptionFile.GetFont();
            Brush brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? Brushes.White : Brushes.Black;

            GetListObjectData(LB, LB.Items[e.Index], out MiscData.CheckState checkState, out bool starred, out bool available);
            if (checkState == MiscData.CheckState.Marked && !available && starred)
            { F = new Font(F.FontFamily, F.Size, FontStyle.Bold | FontStyle.Strikeout); }
            else if (starred)
            { F = new Font(F.FontFamily, F.Size, FontStyle.Bold); }
            else if (checkState == MiscData.CheckState.Marked && !available)
            { F = new Font(F.FontFamily, F.Size, FontStyle.Strikeout); }

            e.Graphics.DrawString(LB.Items[e.Index].ToString(), F, brush, e.Bounds);

            e.DrawFocusRectangle();
        }

        private void LBPathFinder_DrawItem(object sender, DrawItemEventArgs e)
        {
            var LB = sender as ListBox;
            if (e.Index < 0) { return; }
            e.DrawBackground();
            Font F = InstanceContainer.Instance.StaticOptions.OptionFile.GetFont();
            Brush brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? Brushes.White : Brushes.Black;

            e.Graphics.DrawString(LB.Items[e.Index].ToString(), F, brush, e.Bounds);

            e.DrawFocusRectangle();
        }

        private void LBValidEntrances_DoubleClick(object sender, EventArgs e)
        {
            MiscData.CheckState action = MiscData.CheckState.Unchecked;
            if (sender == LBValidLocations || sender == LBValidEntrances) { action = MiscData.CheckState.Checked; }
            if (sender == LBCheckedLocations) { action = MiscData.CheckState.Unchecked; }

            HandleItemSelect((sender as ListBox).SelectedItems.Cast<object>().ToList(), action, LB: sender);
        }

        private void LBValidLocations_MouseUp(object sender, MouseEventArgs e)
        {
            var LB = sender as ListBox;
            DisplayListType? displayList = null;
            if (LB == LBValidLocations) { displayList = DisplayListType.Locations; }
            if (LB == LBValidEntrances) { displayList = DisplayListType.Entrances; }
            if (LB == LBCheckedLocations) { displayList = DisplayListType.Checked; }
            int index = LB.IndexFromPoint(e.Location);
            if (index < 0) { return; }
            if (e.Button == MouseButtons.Middle)
            {
                if (LB.Items[index] is MiscData.Areaheader ah && displayList is not null)
                {
                    bool IsMinimized = ah.IsMinimized((DisplayListType)displayList, InstanceContainer.Instance.StaticOptions);
                    ah.SetMinimized((DisplayListType)displayList, InstanceContainer.Instance.StaticOptions, !IsMinimized);
                    PrintToListBox();
                    return;
                }

                if ((ModifierKeys & Keys.Control) != Keys.Control) { LB.SelectedItems.Clear(); }
                LB.SetSelected(index, true);

                MiscData.CheckState action = MiscData.CheckState.Unchecked;
                if (sender == LBValidLocations || sender == LBValidEntrances) { action = MiscData.CheckState.Marked; }
                if (sender == LBCheckedLocations) { action = MiscData.CheckState.Marked; }

                HandleItemSelect((sender as ListBox).SelectedItems.Cast<object>().ToList(), action, LB: sender);

            }
            else if (e.Button == MouseButtons.Right)
            {
                if ((ModifierKeys & Keys.Control) != Keys.Control) { LB.SelectedItems.Clear(); }
                LB.SetSelected(index, true);
                //ShowContextMenu(LB);
                WinformContextMenu.BuildContextMenu(this, LB);
            }
        }

        private void UpdateToolTip(object sender, MouseEventArgs e)
        {
            ListBox lb = sender as ListBox;
            if (!InstanceContainer.Instance.StaticOptions.OptionFile.WinformData.ShowEntryNameTooltip) { toolTip1.Hide(lb); return; }
            int index = lb.IndexFromPoint(e.Location);
            if (index < 0) { return; }
            string DisplayName = lb.Items[index].ToString();
            if (lb.Items[index] is MiscData.Divider || lb.Items[index] is MiscData.Areaheader) { toolTip1.Hide(lb); return; }
            if (toolTip1.GetToolTip(lb) == DisplayName) { return; }
            toolTip1.SetToolTip(lb, DisplayName);
        }

        //TextBoxes

        private void TXTLocSearch_TextChanged(object sender, EventArgs e)
        {
            List<ListBox> ToUpdate = new List<ListBox>();
            if (sender == TXTLocSearch) { ToUpdate.Add(LBValidLocations); }
            if (sender == TXTEntSearch) { ToUpdate.Add(LBValidEntrances); }
            if (sender == TXTCheckedSearch) { ToUpdate.Add(LBCheckedLocations); }
            PrintToListBox(ToUpdate);
        }

        private void TXTLocSearch_MouseUp(object sender, MouseEventArgs e)
        {
            var TB = sender as TextBox;
            if (e.Button == MouseButtons.Middle) { TB.Clear(); }
        }

        //UI Updates

        public void SetTrackerTitle()
        {
            string GameCode = InstanceContainer?.Instance?.LogicFile?.GameCode is null ? "MMR" : InstanceContainer.Instance.LogicFile.GameCode;
            string GameName = InstanceContainer?.Instance?.LogicDictionary.GameName is null ? null : InstanceContainer.Instance.LogicDictionary.GameName;
            string UsavedChanges = InstanceContainer?.UnsavedChanges is not null && InstanceContainer.UnsavedChanges ? "*" : "";
            string TrackerStatus = "";
            if (References.TrackerVersionStatus.VersionStatus == UpdateManager.versionStatus.outdated) { TrackerStatus = " (outdated)"; }
            else if (References.TrackerVersionStatus.VersionStatus == UpdateManager.versionStatus.dev) { TrackerStatus = " (dev)"; }
            TrackerStatus = References.TrackerVersionStatus.DoUpdateCheck ? TrackerStatus : "";
            string DisplayName = string.IsNullOrWhiteSpace(GameName) ? GameCode : GameName;
            this.Text = DisplayName + " Tracker" + UsavedChanges + TrackerStatus;
        }

        public void UpdateUI()
        {
            FormatMenuItems();
            SetTrackerTitle();

            if (InstanceContainer is null || InstanceContainer.Instance is null) { tlpMaster.Visible = false; return; }
            tlpMaster.Visible = true;

            PathFinderToolStripMenuItem.Visible = InstanceContainer.Instance.GetAllRandomizableExits().Count > 0;

            redoToolStripMenuItem.Enabled = InstanceContainer.RedoStringList.Any();
            undoToolStripMenuItem.Enabled = InstanceContainer.UndoStringList.Any();

            string CurrentStart = (string)CMBStart.SelectedItem ?? "";
            string CurrentEnd = (string)CMBEnd.SelectedItem ?? "";
            CMBStart.DataSource = Pathfinder.GetValidStartingAreas(InstanceContainer.Instance).OrderBy(x => x).ToArray();
            CMBEnd.DataSource = Pathfinder.GetValidDestinationAreas(InstanceContainer.Instance).OrderBy(x => x).ToArray();
            if (CMBStart.Items.Contains(CurrentStart)) { CMBStart.SelectedIndex = CMBStart.Items.IndexOf(CurrentStart); }
            if (CMBEnd.Items.Contains(CurrentEnd)) { CMBEnd.SelectedIndex = CMBEnd.Items.IndexOf(CurrentEnd); }

            UpdateDynamicUserOptions();
            PrintToListBox();
            SendDataToItemDisplay();
            this.Refresh();
        }

        public void AlignUIElements()
        {
            tlpLocations.Controls.Clear();
            tlpEntrances.Controls.Clear();
            tlpChecked.Controls.Clear();
            tlpPathFinder.Controls.Clear();

            if (InstanceContainer == null || InstanceContainer.Instance == null)
            {
                SetObjectVisibility(false, false, false, false);
                return;
            }

            float LeftSize = InstanceContainer.Instance.StaticOptions.OptionFile.WinformData.ColumnSize;
            float RightSize = 100F - LeftSize;
            float TopSize = InstanceContainer.Instance.StaticOptions.OptionFile.WinformData.RowSize;
            float BottomSize = 100F - TopSize;
            if (InstanceContainer.Instance.StaticOptions.OptionFile.WinformData.UILayout == UILayout.Compact)
            {
                if (InstanceContainer.Instance.CombineEntrancesWithLocations() && ViewFocus == DisplayListType.Entrances) { ViewFocus = DisplayListType.Locations; }

                tlpMaster.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 100F);
                tlpMaster.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 0F);
                tlpMaster.RowStyles[0] = new RowStyle(SizeType.Percent, 100F);
                tlpMaster.RowStyles[1] = new RowStyle(SizeType.Percent, 0F);
                switch (ViewFocus)
                {
                    case DisplayListType.Locations:
                        tlpLocations.Controls.AddRange([.. TLPLocationsControls]);
                        SetObjectVisibility(true, false, false, false);
                        break;
                    case DisplayListType.Checked:
                        tlpLocations.Controls.AddRange([.. TLPCheckedControls]);
                        SetObjectVisibility(false, true, false, false);
                        break;
                    case DisplayListType.Entrances:
                        tlpLocations.Controls.AddRange([.. TLPEntranceControls]);
                        SetObjectVisibility(false, false, true, false);
                        break;
                }
            }
            else if (!InstanceContainer.Instance.CombineEntrancesWithLocations())
            {
                SetObjectVisibility(true, true, true, true);
                tlpMaster.RowStyles[0] = new RowStyle(SizeType.Percent, TopSize);
                tlpMaster.RowStyles[1] = new RowStyle(SizeType.Percent, BottomSize);
                tlpMaster.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, LeftSize);
                tlpMaster.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, RightSize);
                if (InstanceContainer.Instance.StaticOptions.OptionFile.WinformData.UILayout == UILayout.Horizontal)
                {
                    tlpLocations.Controls.AddRange([.. TLPLocationsControls]);
                    tlpChecked.Controls.AddRange([.. TLPEntranceControls]);
                    tlpEntrances.Controls.AddRange([.. TLPCheckedControls]);
                    tlpPathFinder.Controls.AddRange([.. TLPPathfinderControls]);
                }
                else
                {
                    tlpLocations.Controls.AddRange([.. TLPLocationsControls]);
                    tlpChecked.Controls.AddRange([.. TLPCheckedControls]);
                    tlpEntrances.Controls.AddRange([.. TLPEntranceControls]);
                    tlpPathFinder.Controls.AddRange([.. TLPPathfinderControls]);
                }
            }
            else
            {
                SetObjectVisibility(true, true, false, false);
                if (InstanceContainer.Instance.StaticOptions.OptionFile.WinformData.UILayout == UILayout.Horizontal)
                {
                    tlpMaster.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, LeftSize);
                    tlpMaster.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, RightSize);
                    tlpMaster.RowStyles[0] = new RowStyle(SizeType.Percent, 100F);
                    tlpMaster.RowStyles[1] = new RowStyle(SizeType.Percent, 0F);
                    tlpLocations.Controls.AddRange([.. TLPLocationsControls]);
                    tlpEntrances.Controls.AddRange([.. TLPCheckedControls]);
                }
                else
                {
                    tlpMaster.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 100F);
                    tlpMaster.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 0F);
                    tlpMaster.RowStyles[0] = new RowStyle(SizeType.Percent, TopSize);
                    tlpMaster.RowStyles[1] = new RowStyle(SizeType.Percent, BottomSize);
                    tlpLocations.Controls.AddRange([.. TLPLocationsControls]);
                    tlpChecked.Controls.AddRange([.. TLPCheckedControls]);
                }
            }
        }

        public void SetObjectVisibility(bool Available, bool Checked, bool Entrance, bool Pathfinder)
        {
            TLPLocationsControls.ForEach(x => x.Visible = Available);
            TLPCheckedControls.ForEach(x => x.Visible = Checked);
            TLPEntranceControls.ForEach(x => x.Visible = Entrance);
            TLPPathfinderControls.ForEach(x => x.Visible = Pathfinder);
        }

        public void PrintToListBox(List<ListBox> ToUpdate = null)
        {
            var lbLocTop = LBValidLocations.TopIndex;
            var lbEntTop = LBValidEntrances.TopIndex;
            var lbCheckTop = LBCheckedLocations.TopIndex;

            ToUpdate ??= [LBCheckedLocations, LBValidEntrances, LBValidLocations];

            foreach (var i in ToUpdate)
            {
                i.Items.Clear();
                i.Font = InstanceContainer.Instance.StaticOptions.OptionFile.GetFont();
                i.ItemHeight = Convert.ToInt32(i.Font.Size * 1.8);
                i.BeginUpdate();
            }

            var dataset = TrackerDataHandling.CreateDataSets(InstanceContainer.Instance);
            var Categories = CategoryFileHandling.GetCategoriesFromFile(InstanceContainer.Instance);

            bool InMinimized = false;

            if (ToUpdate.Contains(LBValidLocations))
            {
                var Data = new TrackerLocationDataList(WinFormUtils.CreateDivider(LBValidLocations), InstanceContainer, TXTLocSearch.Text, dataset, Categories).ShowUnavailable(CHKShowAll.Checked);
                Data.WriteLocations(MiscData.CheckState.Unchecked, false).WriteLocations(MiscData.CheckState.Unchecked, true);
                if (InstanceContainer.Instance.CombineEntrancesWithLocations()) { Data.WriteEntrances(MiscData.CheckState.Unchecked, true); }
                Data.WriteHints(MiscData.CheckState.Unchecked);
                if (InstanceContainer.Instance.StaticOptions.ShowOptionsInListBox == DisplayListType.Locations) { Data.WriteOptions(); }
                lblAvailableLocation.Text = $"Available Locations: {Data.ItemsDisplayed}" + (Data.LocationsFiltered ? $"/{Data.ItemsFound}" : "");
                foreach (var i in Data.FinalData)
                {
                    if (i is MiscData.Areaheader area) { InMinimized = area.IsMinimized(DisplayListType.Locations, InstanceContainer.Instance.StaticOptions); }
                    else if (InMinimized) { continue; }
                    LBValidLocations.Items.Add(i);
                }
                LBValidLocations.TopIndex = lbLocTop;
            }
            if (ToUpdate.Contains(LBValidEntrances) && !InstanceContainer.Instance.CombineEntrancesWithLocations())
            {
                var Data = new TrackerLocationDataList(WinFormUtils.CreateDivider(LBValidEntrances), InstanceContainer, TXTEntSearch.Text, dataset, Categories).ShowUnavailable(CHKShowAll.Checked);
                Data.WriteEntrances(MiscData.CheckState.Unchecked, false);
                if (InstanceContainer.Instance.StaticOptions.ShowOptionsInListBox == DisplayListType.Entrances) { Data.WriteOptions(); }
                lblAvailableEntrances.Text = $"Available Entrances: {Data.ItemsDisplayed}" + (Data.LocationsFiltered ? $"/{Data.ItemsFound}" : "");
                foreach (var i in Data.FinalData)
                {
                    if (i is MiscData.Areaheader area) { InMinimized = area.IsMinimized(DisplayListType.Entrances, InstanceContainer.Instance.StaticOptions); }
                    else if (InMinimized) { continue; }
                    LBValidEntrances.Items.Add(i);
                }
                LBValidEntrances.TopIndex = lbEntTop;
            }
            if (ToUpdate.Contains(LBCheckedLocations))
            {
                TrackerLocationDataList Data = new(WinFormUtils.CreateDivider(LBCheckedLocations), InstanceContainer, TXTCheckedSearch.Text, dataset, Categories);
                Data.WriteLocations(MiscData.CheckState.Checked, false).WriteLocations(MiscData.CheckState.Checked, true)
                    .WriteEntrances(MiscData.CheckState.Checked, true).WriteHints(MiscData.CheckState.Checked)
                    .WriteStartingItems().WriteOnlineItems().WriteRemoteItemHints();
                if (InstanceContainer.Instance.StaticOptions.ShowOptionsInListBox == DisplayListType.Checked) { Data.WriteOptions(); }
                lblCheckedLocation.Text = $"Checked Locations: {Data.ItemsDisplayed}" + (Data.LocationsFiltered ? $"/{Data.ItemsFound}" : "");
                foreach (var i in Data.FinalData)
                {
                    if (i is MiscData.Areaheader area) { InMinimized = area.IsMinimized(DisplayListType.Checked, InstanceContainer.Instance.StaticOptions); }
                    else if (InMinimized) { continue; }
                    LBCheckedLocations.Items.Add(i);
                }
                LBCheckedLocations.TopIndex = lbCheckTop;
            }

            foreach (var i in ToUpdate) { i.EndUpdate(); }
        }

        public void FormatMenuItems()
        {
            OptionstoolStripMenuItem.Visible = (InstanceContainer.Instance != null);
            toolsToolStripMenuItem.Visible = (InstanceContainer.Instance != null);
            undoToolStripMenuItem.Visible = (InstanceContainer.Instance != null && InstanceContainer.Instance.StaticOptions.OptionFile.MaxUndo > 0);
            redoToolStripMenuItem.Visible = (InstanceContainer.Instance != null && InstanceContainer.Instance.StaticOptions.OptionFile.MaxUndo > 0);
            refreshToolStripMenuItem.Visible = (InstanceContainer.Instance != null);
            SavetoolStripMenuItem.Visible = (InstanceContainer.Instance != null);
            SaveAsToolStripMenuItem.Visible = (InstanceContainer.Instance != null) && !string.IsNullOrWhiteSpace(InstanceContainer.CurrentSavePath);
            spoilerLogToolsToolStripMenuItem.Visible = (InstanceContainer.Instance != null);
            importSpoilerLogToolStripMenuItem.Visible = (InstanceContainer.Instance != null) && InstanceContainer.Instance.SpoilerLogSupported();
            PathFinderToolStripMenuItem.Visible = (InstanceContainer.Instance != null && InstanceContainer.Instance.GetAllRandomizableExits().Count > 0);
            netClientToolStripMenuItem.Visible = (InstanceContainer.Instance != null && InstanceContainer.Instance.LogicDictionary.NetPlaySupported);

            viewToolStripMenuItem.Visible = (InstanceContainer.Instance != null && InstanceContainer.Instance.StaticOptions.OptionFile.WinformData.UILayout == UILayout.Compact);

            if (InstanceContainer.Instance == null) { return; }

            entrancesToolStripMenuItem.Visible = !InstanceContainer.Instance.CombineEntrancesWithLocations();

            locationsToolStripMenuItem.Checked = ViewFocus == DisplayListType.Locations;
            entrancesToolStripMenuItem.Checked = ViewFocus == DisplayListType.Entrances;
            checkedToolStripMenuItem.Checked = ViewFocus == DisplayListType.Checked;

            SaveAsToolStripMenuItem.Visible = (File.Exists(InstanceContainer.CurrentSavePath));
            importSpoilerLogToolStripMenuItem.Text = (InstanceContainer.Instance.SpoilerLog != null) ? "Remove Spoiler Log" : "Import Spoiler Log";

        }

        WinFormUtils.DropDownOptionTree RootTree = new WinFormUtils.DropDownOptionTree();
        private void UpdateDynamicUserOptions()
        {
            RandomizerOptionsToolStripMenuItem1.DropDownItems.Clear();
            RootTree = new WinFormUtils.DropDownOptionTree() { MenuItem = RandomizerOptionsToolStripMenuItem1 };

            List<DisplayListType?> EnumList = Enum.GetValues(typeof(MiscData.DisplayListType)).Cast<MiscData.DisplayListType?>().ToList();
            List<DisplayListType?> DisplayListEnumList = [null, .. EnumList];
            List<string> DisplayListStringList = ["None", .. Enum.GetValues(typeof(MiscData.DisplayListType)).Cast<MiscData.DisplayListType?>().Select(x => x.ToString())];

            //Create List Box Toggle
            ToolStripComboBox ListBoxDisplayOptions = new();
            ListBoxDisplayOptions.Items.AddRange(DisplayListStringList.ToArray());
            if (InstanceContainer.Instance.StaticOptions.ShowOptionsInListBox is null) { ListBoxDisplayOptions.SelectedIndex = 0; }
            else { ListBoxDisplayOptions.SelectedIndex = EnumList.IndexOf(InstanceContainer.Instance.StaticOptions.ShowOptionsInListBox)+1; }
            ListBoxDisplayOptions.SelectedIndex = DisplayListEnumList.IndexOf(InstanceContainer.Instance.StaticOptions.ShowOptionsInListBox);
            ListBoxDisplayOptions.SelectedIndexChanged += delegate (object sender, EventArgs e)
            {
                InstanceContainer.Instance.StaticOptions.ShowOptionsInListBox = DisplayListEnumList[((ToolStripComboBox)sender).SelectedIndex];
                PrintToListBox();
            };
            ToolStripMenuItem ListBoxDisplay = new() { Text = "Display In List Box" };
            ListBoxDisplay.DropDownItems.Add(ListBoxDisplayOptions);
            RandomizerOptionsToolStripMenuItem1.DropDownItems.Add(ListBoxDisplay);
            RandomizerOptionsToolStripMenuItem1.DropDownItems.Add(new ToolStripSeparator());

            List<dynamic> AllOptions = new List<object>();
            AllOptions.AddRange(InstanceContainer.Instance.ChoiceOptions.Values);
            AllOptions.AddRange(InstanceContainer.Instance.MultiSelectOptions.Values);
            AllOptions.AddRange(InstanceContainer.Instance.ToggleOptions.Values);
            AllOptions.AddRange(InstanceContainer.Instance.IntOptions.Values);
            IEnumerable<object> OrderedOptions = AllOptions.OrderBy(x => x.Priority);

            //Create the option subgroup trees
            foreach (dynamic i in OrderedOptions)
            {
                string Category = i.SubCategory;
                if (string.IsNullOrWhiteSpace(Category)) { continue; }
                string[] CategoryTree = Category.Split('/');
                var CurrentTree = RootTree;
                foreach (var c in CategoryTree)
                {
                    CurrentTree.SubGroups.SetIfEmpty(c, new WinFormUtils.DropDownOptionTree() { GroupID = c, Parent = CurrentTree });
                    CurrentTree = CurrentTree.SubGroups[c];
                }
            }

            //Create options and add them to their subgroup
            foreach (dynamic o in AllOptions)
            {
                if (o is OptionData.ChoiceOption ChoiceOption)
                {
                    var OptionTree = GetOptionTree(o);
                    if (!InstanceContainer.logicCalculation.ConditionalsMet(ChoiceOption.Conditionals)) { continue; }
                    if (ChoiceOption.ValueList.Count < 2) { continue; }

                    ToolStripComboBox toolStripComboBox = new();
                    foreach (var j in ChoiceOption.ValueList)
                    {
                        MiscData.OptionComboboxItem optionComboboxItem = new MiscData.OptionComboboxItem() { Text = ChoiceOption.getValueName(j.Key), Value = j.Key };
                        toolStripComboBox.Items.Add(optionComboboxItem);
                    }
                    foreach (MiscData.OptionComboboxItem ind in toolStripComboBox.Items)
                    {
                        if (ind.Value as string == ChoiceOption.Value) { toolStripComboBox.SelectedIndex = toolStripComboBox.Items.IndexOf(ind); break; }
                    }
                    toolStripComboBox.SelectedIndexChanged += delegate (object sender, EventArgs e)
                    {
                        SaveTrackerState(InstanceContainer.Instance.ToJson(JSONType.UTF8));
                        ChoiceOption.SetValue((toolStripComboBox.SelectedItem as MiscData.OptionComboboxItem).Value as string);
                        if (!InstanceContainer.logicCalculation.ReCompileLogicOnCalculation) { InstanceContainer.logicCalculation.CompileOptionActionEdits(); }
                        LocationChecker.TriggerUserOptionUpdatedEvent([ChoiceOption], InstanceContainer.Instance);
                        InstanceContainer.logicCalculation.CalculateLogic();
                        UpdateUI();
                    };
                    ToolStripMenuItem menuItem = new() { Name = $"{ChoiceOption.ID}Menu", Text = ChoiceOption.getOptionName() };
                    menuItem.ToolTipText = ChoiceOption.Description;
                    menuItem.AutoToolTip = true;
                    menuItem.DropDownItems.Add(toolStripComboBox);
                    OptionTree.MenuItems.Add(menuItem);
                }
                else if (o is OptionData.MultiSelectOption MultiSelectOption)
                {
                    var OptionTree = GetOptionTree(o);
                    if (!InstanceContainer.logicCalculation.ConditionalsMet(MultiSelectOption.Conditionals)) { continue; }
                    ToolStripMenuItem menuItem = new() { Name = $"{MultiSelectOption.ID}Menu", Text = MultiSelectOption.getOptionName() };
                    menuItem.ToolTipText = MultiSelectOption.Description;
                    menuItem.AutoToolTip = true;
                    foreach (var i in MultiSelectOption.ValueList)
                    {
                        ToolStripMenuItem SubMenuItem = new() { Name = $"{MultiSelectOption.ID}{i.Key}Menu", Checked = MultiSelectOption.EnabledValues.Contains(i.Key), Text = i.Value.Name };
                        SubMenuItem.Click += delegate (object sender, EventArgs e)
                        {
                            SaveTrackerState(InstanceContainer.Instance.ToJson(JSONType.UTF8));
                            MultiSelectOption.ToggleValue(i.Key);
                            if (!InstanceContainer.logicCalculation.ReCompileLogicOnCalculation) { InstanceContainer.logicCalculation.CompileOptionActionEdits(); }
                            LocationChecker.TriggerUserOptionUpdatedEvent([MultiSelectOption], InstanceContainer.Instance);
                            InstanceContainer.logicCalculation.CalculateLogic();
                            UpdateUI();
                        };
                        menuItem.DropDownItems.Add(SubMenuItem);
                    }
                    OptionTree.MenuItems.Add(menuItem);
                }
                else if (o is OptionData.ToggleOption ToggleOption)
                {
                    var OptionTree = GetOptionTree(o);
                    if (!InstanceContainer.logicCalculation.ConditionalsMet(ToggleOption.Conditionals)) { continue; }
                    ToolStripMenuItem menuItem = new() { Name = $"{ToggleOption.ID}Menu", Checked = ToggleOption.IsEnabled(), Text = ToggleOption.getOptionName() };
                    menuItem.ToolTipText = ToggleOption.Description;
                    menuItem.AutoToolTip = true;
                    menuItem.Click += delegate (object sender, EventArgs e)
                    {
                        SaveTrackerState(InstanceContainer.Instance.ToJson(JSONType.UTF8));
                        ToggleOption.ToggleValue();
                        if (!InstanceContainer.logicCalculation.ReCompileLogicOnCalculation) { InstanceContainer.logicCalculation.CompileOptionActionEdits(); }
                        LocationChecker.TriggerUserOptionUpdatedEvent([ToggleOption], InstanceContainer.Instance);
                        InstanceContainer.logicCalculation.CalculateLogic();
                        UpdateUI();
                    };
                    OptionTree.MenuItems.Add(menuItem);
                }
                else if (o is OptionData.IntOption IntOption)
                {
                    var OptionTree = GetOptionTree(o);
                    if (!InstanceContainer.logicCalculation.ConditionalsMet(IntOption.Conditionals)) { continue; }
                    string DisplayName = IntOption.ToString();
                    ToolStripMenuItem menuItem = new() { Name = $"{IntOption.ID}Menu", Text = IntOption.ToString() };
                    menuItem.ToolTipText = IntOption.Description;
                    menuItem.AutoToolTip = true;
                    menuItem.Click += delegate (object sender, EventArgs e)
                    {
                        HandleItemSelect(new List<OptionData.IntOption> { IntOption }, MiscData.CheckState.Checked);
                    };
                    OptionTree.MenuItems.Add(menuItem);
                }
            }

            ApplyMenuItems(RootTree);

            void ApplyMenuItems(WinFormUtils.DropDownOptionTree Tree)
            {
                foreach (var i in Tree.MenuItems)
                {
                    Tree.MenuItem.DropDownItems.Add(i);
                }
                foreach (var i in Tree.SubGroups)
                {
                    if (!i.Value.MenuItems.Any() && !i.Value.SubGroups.Any()) { continue; }
                    ToolStripMenuItem SubMenu = new ToolStripMenuItem() { Text = i.Key };
                    Tree.MenuItem.DropDownItems.Add(SubMenu);
                    i.Value.MenuItem = SubMenu;
                    ApplyMenuItems(i.Value);
                }
            }

            WinFormUtils.DropDownOptionTree GetOptionTree(dynamic option)
            {
                string Category = option.SubCategory;
                if (string.IsNullOrWhiteSpace(Category)) { return RootTree; }
                string[] CategoryTree = Category.Split('/');
                var CurrentTree = RootTree;
                foreach (var c in CategoryTree)
                {
                    CurrentTree = CurrentTree.SubGroups[c];
                }
                return CurrentTree;
            }

        }

        //ListboxObject Handeling

        public void HandleItemSelect(IEnumerable<object> Items, MiscData.CheckState checkState, bool EnforceMarkAction = false, object LB = null)
        {
            if (Items.Count() == 1)
            {
                if (Items.First() is MiscData.Divider) { return; }
                else if (Items.First() is MiscData.Areaheader header && LB != null)
                {
                    if (LB == LBValidLocations) { TXTLocSearch.Text = "#" + header.Area; }
                    else if (LB == LBValidEntrances) { TXTEntSearch.Text = "#" + header.Area; }
                    else if (LB == LBCheckedLocations) { TXTCheckedSearch.Text = "#" + header.Area; }
                    return;
                }
            }
            string CurrentState = InstanceContainer.Instance.ToJson(MiscData.JSONType.UTF8);

            var CheckObjectOptions = new MiscData.CheckItemSetting(checkState)
                .SetEnforceMarkAction(EnforceMarkAction)
                .SetCheckUnassignedLocations(HandleUnassignedChecks)
                .SetCheckUnassignedEntrances(HandleUnassignedChecks)
                .SetCheckUnassignedHints(HandleUnassignedVariables)
                .SetCheckChoiceOptions(HandleUnassignedChecks)
                .SetCheckIntOptions(HandleUnassignedVariables);

            var UpdatedObjects = LocationChecker.CheckSelectedItems(Items, InstanceContainer, CheckObjectOptions);

            if (UpdatedObjects.Any())
            {
                SaveTrackerState(CurrentState);
            }
            UpdateUI();
        }

        private List<ManualCheckObjectResult> HandleUnassignedChecks(IEnumerable<object> Checks, MMR_Tracker_V3.TrackerObjects.InstanceData.InstanceContainer Instance)
        {
            CheckItemForm checkItemForm = new CheckItemForm(Checks, Instance);
            checkItemForm.ShowDialog();
            return checkItemForm._Result;
        }
        private List<ManualCheckObjectResult> HandleUnassignedVariables(IEnumerable<object> Checks, MMR_Tracker_V3.TrackerObjects.InstanceData.InstanceContainer Instance)
        {
            VariableInputWindow variableInputWindow = new VariableInputWindow(Checks, Instance);
            variableInputWindow.ShowDialog();
            return variableInputWindow._Result;
        }

        private void CHKShowAll_CheckedChanged(object sender, EventArgs e)
        {
            PrintToListBox();
        }

        //Other

        private void SaveTrackerState(string PreviousState = null)
        {
            InstanceContainer.SaveState(PreviousState);
            redoToolStripMenuItem.Enabled = InstanceContainer.RedoStringList.Any();
            undoToolStripMenuItem.Enabled = InstanceContainer.UndoStringList.Any();
            InstanceContainer.UnsavedChanges = true;
        }

        public bool PromptSave()
        {
            if (InstanceContainer.Instance == null || !InstanceContainer.UnsavedChanges) { return true; }
            var result = MessageBox.Show("You have unsaved Changes. Would you Like to Save?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            switch (result)
            {
                case DialogResult.No:
                    return true;
                case DialogResult.Yes:
                    SavetoolStripMenuItem1_Click(SavetoolStripMenuItem, null);
                    return true;
                default:
                    return false;
            }
        }

        private void BTNSetItem_Click(object sender, EventArgs e)
        {
            ListBox LB = (sender == BTNSetItem) ? LBValidLocations : LBValidEntrances;
            if (LB.SelectedItems.Count < 1) { return; }
            HandleItemSelect(LB.SelectedItems.Cast<object>().ToList(), MiscData.CheckState.Marked, LB: LB);
        }

        private void BTNSetEntrance_MouseUp(object sender, MouseEventArgs e)
        {
            ListBox LB = (sender == BTNSetItem) ? LBValidLocations : LBValidEntrances;
            if (e.Button == MouseButtons.Right)
            {
                ContextMenuStrip contextMenuStrip = new();
                ToolStripItem MarkOnly = contextMenuStrip.Items.Add("Mark Only");
                MarkOnly.Click += (sender, e) =>
                {
                    HandleItemSelect(LB.SelectedItems.Cast<object>().ToList(), MiscData.CheckState.Marked, true, LB);
                };
                ToolStripItem UnMarkOnly = contextMenuStrip.Items.Add("UnMark Only");
                UnMarkOnly.Click += (sender, e) =>
                {
                    HandleItemSelect(LB.SelectedItems.Cast<object>().ToList(), MiscData.CheckState.Unchecked, LB: LB);
                };
                contextMenuStrip.Show(Cursor.Position);
            }
        }

        private void MainInterface_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.S:
                        SavetoolStripMenuItem1_Click(SavetoolStripMenuItem, e);
                        break;
                    case Keys.Z:
                        UndoRedo_Click(undoToolStripMenuItem, e);
                        break;
                    case Keys.Y:
                        UndoRedo_Click(redoToolStripMenuItem, e);
                        break;
                }
            }
            if (e.KeyCode == Keys.F5)
            {
                UndoRedo_Click(refreshToolStripMenuItem, e);
            }
        }

        //Key Press Handeling

        private void LB_KeyDown(object sender, KeyEventArgs e)
        {
            var OptionFile = InstanceContainer.Instance.StaticOptions.OptionFile;
            var SizeMod = ModifierKeys.HasFlag(Keys.Shift) ? 5 : 1;
            var ActiveControl = this.ActiveControl;
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                switch (e.KeyCode)
                {
                    case Keys.A:
                        (sender as ListBox).BeginUpdate();
                        for (int i = 0; i < (sender as ListBox).Items.Count; i++) { (sender as ListBox).SetSelected(i, true); }
                        (sender as ListBox).EndUpdate();
                        break;
                    case Keys.Left:
                        OptionFile.SetColumnSize(OptionFile.WinformData.ColumnSize - SizeMod);
                        AlignUIElements();
                        break;
                    case Keys.Right:
                        OptionFile.SetColumnSize(OptionFile.WinformData.ColumnSize + SizeMod);
                        AlignUIElements();
                        break;
                    case Keys.Up:
                        OptionFile.SetRowSize(OptionFile.WinformData.RowSize - SizeMod);
                        AlignUIElements();
                        break;
                    case Keys.Down:
                        OptionFile.SetRowSize(OptionFile.WinformData.RowSize + SizeMod);
                        AlignUIElements();
                        break;
                }
            }
            this.ActiveControl = ActiveControl;
        }

        private void preventKeyShortcuts(object sender, KeyPressEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control) &&
                this.ActiveControl != TXTLocSearch &&
                this.ActiveControl != TXTEntSearch &&
                this.ActiveControl != TXTCheckedSearch)
            { e.Handled = true; }
        }

        private void miscOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StaticOptionSelect staticOptionSelect = new StaticOptionSelect(InstanceContainer.Instance);
            staticOptionSelect.ShowDialog();
            UpdateUI();
            AlignUIElements();
        }

        private void BTNFindPath_Click(object sender, EventArgs e)
        {
            if (CMBStart.SelectedIndex < 0 || CMBEnd.SelectedIndex < 0 || CMBStart.SelectedItem.ToString() == CMBEnd.SelectedItem.ToString()) { return; }
            LBPathFinder.Font = InstanceContainer.Instance.StaticOptions.OptionFile.GetFont();
            LBPathFinder.ItemHeight = Convert.ToInt32(LBPathFinder.Font.Size * 1.8);
            LBPathFinder.DataSource = new List<string> { "Finding path" };
            MainInterfacepathfinder = new Pathfinder();
            MainInterfacepathfinder.FindPath(InstanceContainer.Instance, (string)CMBStart.SelectedItem, (string)CMBEnd.SelectedItem);
            MainInterfacepathfinder.FinalPath = MainInterfacepathfinder.FinalPath.OrderBy(x => x.Count).ToList();
            PopoutPathfinder.PrintPaths(InstanceContainer.Instance, MainInterfacepathfinder, LBPathFinder);
        }

        private void LBPathFinder_DoubleClick(object sender, EventArgs e)
        {
            if (((ListBox)sender).SelectedItem is Pathfinder.PathfinderPath path)
            {
                PopoutPathfinder.PrintPaths(InstanceContainer.Instance, MainInterfacepathfinder, LBPathFinder, path.Focused ? -1 : path.Index);
            }
        }

        private void PathFinderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopoutPathfinder popoutPathfinder = new PopoutPathfinder(InstanceContainer.Instance);
            popoutPathfinder.Show();
        }

        private void lblSwapPathfinder_Click(object sender, EventArgs e)
        {
            if (CMBStart.SelectedIndex < 0 || CMBEnd.SelectedIndex < 0 || CMBStart.SelectedItem.ToString() == CMBEnd.SelectedItem.ToString()) { return; }
            string Start = (string)CMBStart.SelectedItem;
            string end = (string)CMBEnd.SelectedItem;
            CMBStart.SelectedItem = end;
            CMBEnd.SelectedItem = Start;
        }

        private void spoilerLogToolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SpoilerLogLookUp spoilerLogLookUp = new SpoilerLogLookUp(InstanceContainer.Instance);
            spoilerLogLookUp.Show();
        }

        private void visualItemTrackerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MainInterfaceItemDisplayThread == null || !MainInterfaceItemDisplayThread.IsAlive || MainInterfaceItemDisplayForm == null)
            {

                MainInterfaceItemDisplayForm = new ItemDisplay(this, WinFormImageUtils.GetImageSheet());
                MainInterfaceItemDisplayThread = new Thread(new ThreadStart(() => MainInterfaceItemDisplayForm.ShowDialog()));
                MainInterfaceItemDisplayThread.Start();
                while (!OBJIsThreadSafe(MainInterfaceItemDisplayThread, MainInterfaceItemDisplayForm)) { Thread.Sleep(10); }
                SendDataToItemDisplay();
            }
            else
            {
                MainInterfaceItemDisplayForm.Invoke(new Action(delegate
                {
                    MainInterfaceItemDisplayForm.Activate();
                }));
                SendDataToItemDisplay();
            }
        }

        private async void SendDataToItemDisplay()
        {
            if (!OBJIsThreadSafe(MainInterfaceItemDisplayThread, MainInterfaceItemDisplayForm)) { return; }
            var newState = WinFormImageUtils.CaptureTrackerState(InstanceContainer.Instance);
            await Task.Run(() => MainInterfaceItemDisplayForm.Invoke(new MethodInvoker(delegate { MainInterfaceItemDisplayForm.UpdateData(newState); })));
        }

        private void PathfinderCMB_DropDown(object sender, EventArgs e)
        {
            WinFormUtils.AdjustComboBoxWidth(sender as System.Windows.Forms.ComboBox);
        }

        private void MainInterface_Resize(object sender, EventArgs e)
        {
            LBValidLocations.Refresh();
            LBValidEntrances.Refresh();
            LBCheckedLocations.Refresh();
            LBPathFinder.Refresh();
        }

        private void ViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender == locationsToolStripMenuItem) { ViewFocus = DisplayListType.Locations; }
            else if (sender == entrancesToolStripMenuItem) { ViewFocus = DisplayListType.Entrances; }
            else if (sender == checkedToolStripMenuItem) { ViewFocus = DisplayListType.Checked; }
            UpdateUI();
            AlignUIElements();
        }

        private void netClientToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentNetClientForm is not null) { CurrentNetClientForm.Focus(); }
            else
            {
                CurrentNetClientForm = new NetClient(this, InstanceContainer);
                CurrentNetClientForm.Show();
            }
        }
    }
}
