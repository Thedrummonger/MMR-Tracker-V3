using MMR_Tracker_V3;
using MMR_Tracker_V3.SpoilerLogImporter;
using MMR_Tracker_V3.TrackerObjectExtentions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
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
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using MMR_Tracker_V3.Logic;
using Microsoft.VisualBasic;

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
            DoUpdateCheck();
            UpdateUI();
            AlignUIElements();
            WinFormInstanceCreation.ApplyUserPretLogic();
        }

        public void DoUpdateCheck()
        {
            References.TrackerVersionStatus = UpdateManager.GetTrackerVersionStatus();
            if (References.TrackerVersionStatus.VersionStatus == UpdateManager.versionStatus.outdated)
            {
                var Download = MessageBox.Show($"Your tracker version {References.trackerVersion} is out of Date. Would you like to download the latest version {References.TrackerVersionStatus.LatestVersion.TagName}?\n\nTo disable this message click \"cancel\" or disable \"Check for Updates\" in the options file", "Tracker Out of Date", MessageBoxButtons.YesNoCancel);
                if (Download == DialogResult.Yes) { { System.Diagnostics.Process.Start("explorer.exe", References.TrackerVersionStatus.LatestVersion.HtmlUrl); this.Close(); return; } }
                else if (Download == DialogResult.Cancel) { UpdateManager.DisableUpdateChecks(); }
            }
        }

        private void MainInterface_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!PromptSave()) { e.Cancel = true; }
            if (Utility.OBJIsThreadSafe(MainInterfaceItemDisplayThread, MainInterfaceItemDisplayForm))
            {
                MainInterfaceItemDisplayForm.Invoke(new MethodInvoker(delegate { MainInterfaceItemDisplayForm.CloseThread(); }));
            }
        }

        //Menu Strip

        private void UndoRedo_Click(object sender, EventArgs e)
        {
            Stopwatch TimeTotalItemSelect = new Stopwatch();
            Utility.TimeCodeExecution(TimeTotalItemSelect, "Saving Tracker State (UTF8)", 1);
            if (sender == undoToolStripMenuItem)
            {
                InstanceContainer.DoUndo();
            }
            else if (sender == redoToolStripMenuItem)
            {
                InstanceContainer.DoRedo();
            }
            Utility.TimeCodeExecution(TimeTotalItemSelect, "Undo/Redo Action", -1);

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
            var LogicData = LogicFileParser.GetLogicData(fileDialog.FileName);
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
                if (!InstanceContainer.LoadInsanceFromFile(openFileDialog.FileName))
                {
                    MessageBox.Show("Save File Not Valid");
                    return;
                }
                InstanceContainer.CurrentSavePath = openFileDialog.FileName;
                InstanceContainer.logicCalculation.CalculateLogic();
                UpdateUI();
                UpdateDynamicUserOptions();
                AlignUIElements();
            }
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
            if (InstanceContainer.Instance.SpoilerLog == null)
            {
                OpenFileDialog openFileDialog = new()
                {
                    Filter = SpoilerLogTools.GetSpoilerLogFilter(InstanceContainer.Instance),
                    Title = "Load Text Spoiler Log"
                };
                openFileDialog.ShowDialog();
                if (openFileDialog.FileName != "" && File.Exists(openFileDialog.FileName))
                {
                    string CurrentState = InstanceContainer.Instance.ToJson(MiscData.JSONType.UTF8);
                    var ToUpdate = new List<ListBox> { LBCheckedLocations, LBValidEntrances, LBValidLocations };
                    foreach (var i in ToUpdate) { WinFormUtils.PrintMessageToListBox(i, "Importing Spoiler Log \n Please Wait..."); }
                    if (SpoilerLogTools.ImportSpoilerLog(File.ReadAllLines(openFileDialog.FileName), openFileDialog.FileName, InstanceContainer))
                    {
                        SaveTrackerState(CurrentState);
                    }
                }
            }
            else
            {
                SaveTrackerState();
                SpoilerLogTools.RemoveSpoilerData(InstanceContainer.Instance);
                InstanceContainer.Instance.SpoilerLog = null;
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
                checkState = InstanceContainer.Instance.GetLocationByID(po.ReferenceID)?.CheckState ?? MiscData.CheckState.Checked;
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
                checkState = Utility.DynamicPropertyExist(entry, "CheckState") ? entry.CheckState : MiscData.CheckState.Checked;
                starred = Utility.DynamicPropertyExist(entry, "Starred") ? entry.Starred : false;
                Available = Utility.DynamicPropertyExist(entry, "Available") ? entry.Available : true;
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
                    if (IsMinimized) { ah.RemoveMinimized((DisplayListType)displayList, InstanceContainer.Instance.StaticOptions); }
                    else { ah.SetMinimized((DisplayListType)displayList, InstanceContainer.Instance.StaticOptions); }
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
                LB.SelectedItems.Clear();
                LB.SetSelected(index, true);
                ShowContextMenu(LB);
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
            string UsavedChanges = InstanceContainer?.UnsavedChanges is not null && InstanceContainer.UnsavedChanges ? "*" : "";
            string TrackerStatus = "";
            if (References.TrackerVersionStatus.VersionStatus == UpdateManager.versionStatus.outdated) { TrackerStatus = " (outdated)"; }
            else if (References.TrackerVersionStatus.VersionStatus == UpdateManager.versionStatus.dev) { TrackerStatus = " (dev)"; }
            TrackerStatus = References.TrackerVersionStatus.DoUpdateCheck ? TrackerStatus : "";
            this.Text = GameCode + " Tracker" + UsavedChanges + TrackerStatus;
        }

        public void UpdateUI()
        {
            FormatMenuItems();
            SetTrackerTitle();

            if (InstanceContainer is null || InstanceContainer.Instance is null) { tlpMaster.Visible = false; return; }
            tlpMaster.Visible = true;

            InstanceContainer.Instance.EntrancePool.IsEntranceRando = InstanceContainer.Instance.EntrancePool.CheckForRandomEntrances();

            PathFinderToolStripMenuItem.Visible = InstanceContainer.Instance.EntrancePool.IsEntranceRando;

            redoToolStripMenuItem.Enabled = InstanceContainer.RedoStringList.Any();
            undoToolStripMenuItem.Enabled = InstanceContainer.UndoStringList.Any();

            string CurrentStart = (string)CMBStart.SelectedItem ?? "";
            string CurrentEnd = (string)CMBEnd.SelectedItem ?? "";
            var AccessableAreas = InstanceContainer.Instance.EntrancePool.AreaList.Values.Where(x => x.ExitsAcessibleFrom > 0 && (x.RandomizableExits().Any() || InstanceContainer.Instance.StaticOptions.OptionFile.ShowMacroExitsPathfinder)).Select(x => x.ID);
            CMBStart.DataSource = AccessableAreas.OrderBy(x => x).ToList();
            CMBEnd.DataSource = AccessableAreas.OrderBy(x => x).ToList();
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
            else if (InstanceContainer.Instance.StaticOptions.OptionFile.WinformData.UILayout == UILayout.Compact)
            {
                if (InstanceContainer.Instance.CombineEntrancesWithLocations() && ViewFocus == DisplayListType.Entrances) { ViewFocus = DisplayListType.Locations; }

                SetObjectVisibility(ViewFocus == DisplayListType.Locations, ViewFocus == DisplayListType.Checked, ViewFocus == DisplayListType.Entrances, false);
                tlpMaster.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 100F);
                tlpMaster.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 0F);
                tlpMaster.RowStyles[0] = new RowStyle(SizeType.Percent, 100F);
                tlpMaster.RowStyles[1] = new RowStyle(SizeType.Percent, 0F);
                switch (ViewFocus)
                {
                    case DisplayListType.Locations:
                        tlpLocations.Controls.AddRange(TLPLocationsControls.ToArray());
                        break;
                    case DisplayListType.Checked:
                        tlpLocations.Controls.AddRange(TLPCheckedControls.ToArray());
                        break;
                    case DisplayListType.Entrances:
                        tlpLocations.Controls.AddRange(TLPEntranceControls.ToArray());
                        break;
                }
            }
            else if (!InstanceContainer.Instance.CombineEntrancesWithLocations())
            {
                SetObjectVisibility(true, true, true, true);
                tlpMaster.RowStyles[0] = new RowStyle(SizeType.Percent, 50F);
                tlpMaster.RowStyles[1] = new RowStyle(SizeType.Percent, 50F);
                tlpMaster.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 50F);
                tlpMaster.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 50F);
                if (InstanceContainer.Instance.StaticOptions.OptionFile.WinformData.UILayout == UILayout.Horizontal)
                {
                    tlpLocations.Controls.AddRange(TLPLocationsControls.ToArray());
                    tlpChecked.Controls.AddRange(TLPEntranceControls.ToArray());
                    tlpEntrances.Controls.AddRange(TLPCheckedControls.ToArray());
                    tlpPathFinder.Controls.AddRange(TLPPathfinderControls.ToArray());
                }
                else
                {
                    tlpLocations.Controls.AddRange(TLPLocationsControls.ToArray());
                    tlpChecked.Controls.AddRange(TLPCheckedControls.ToArray());
                    tlpEntrances.Controls.AddRange(TLPEntranceControls.ToArray());
                    tlpPathFinder.Controls.AddRange(TLPPathfinderControls.ToArray());
                }
            }
            else
            {
                SetObjectVisibility(true, true, false, false);
                if (InstanceContainer.Instance.StaticOptions.OptionFile.WinformData.UILayout == UILayout.Horizontal)
                {
                    tlpMaster.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 50F);
                    tlpMaster.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 50F);
                    tlpMaster.RowStyles[0] = new RowStyle(SizeType.Percent, 100F);
                    tlpMaster.RowStyles[1] = new RowStyle(SizeType.Percent, 0F);
                    tlpLocations.Controls.AddRange(TLPLocationsControls.ToArray());
                    tlpEntrances.Controls.AddRange(TLPCheckedControls.ToArray());
                }
                else
                {
                    tlpMaster.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 100F);
                    tlpMaster.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 0F);
                    tlpMaster.RowStyles[0] = new RowStyle(SizeType.Percent, 50F);
                    tlpMaster.RowStyles[1] = new RowStyle(SizeType.Percent, 50F);
                    tlpLocations.Controls.AddRange(TLPLocationsControls.ToArray());
                    tlpChecked.Controls.AddRange(TLPCheckedControls.ToArray());
                }
            }
        }

        public void SetObjectVisibility(bool Available, bool Checked, bool Entrance, bool Pathfinder)
        {
            var UpperLeftLBL = lblAvailableLocation;
            var UpperRightLBL = lblAvailableEntrances;
            var LowerLeftLBL = lblCheckedLocation;
            var LowerRightLBL = label4;
            var LowerRight2LBL = label5;
            var LowerRight3LBL = label6;

            UpperLeftLBL.Visible = Available;
            BTNSetItem.Visible = Available;
            TXTLocSearch.Visible = Available;
            LBValidLocations.Visible = Available;

            LowerLeftLBL.Visible = Checked;
            CHKShowAll.Visible = Checked;
            TXTCheckedSearch.Visible = Checked;
            LBCheckedLocations.Visible = Checked;

            UpperRightLBL.Visible = Entrance;
            BTNSetEntrance.Visible = Entrance;
            TXTEntSearch.Visible = Entrance;
            LBValidEntrances.Visible = Entrance;
            LowerRightLBL.Visible = Entrance;
            BTNFindPath.Visible = Entrance;

            LowerRight2LBL.Visible = Pathfinder;
            LowerRight3LBL.Visible = Pathfinder;
            CMBStart.Visible = Pathfinder;
            CMBEnd.Visible = Pathfinder;
            LBPathFinder.Visible = Pathfinder;
            lblSwapPathfinder.Visible = Pathfinder;
        }

        private void PrintToListBox(List<ListBox> ToUpdate = null)
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

            bool InMinimized = false;

            if (ToUpdate.Contains(LBValidLocations))
            {
                var Data = new TrackerLocationDataList(WinFormUtils.CreateDivider(LBValidLocations), InstanceContainer, TXTLocSearch.Text, dataset).ShowUnavailable(CHKShowAll.Checked);
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
                var Data = new TrackerLocationDataList(WinFormUtils.CreateDivider(LBValidEntrances), InstanceContainer, TXTEntSearch.Text, dataset).ShowUnavailable(CHKShowAll.Checked);
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
                TrackerLocationDataList Data = new(WinFormUtils.CreateDivider(LBCheckedLocations), InstanceContainer, TXTCheckedSearch.Text, dataset);
                Data.WriteLocations(MiscData.CheckState.Checked, false).WriteLocations(MiscData.CheckState.Checked, true)
                    .WriteEntrances(MiscData.CheckState.Checked, true).WriteHints(MiscData.CheckState.Checked).WriteStartingItems().WriteOnlineItems();
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
            importSpoilerLogToolStripMenuItem.Visible = (InstanceContainer.Instance != null);
            PathFinderToolStripMenuItem.Visible = (InstanceContainer.Instance != null && InstanceContainer.Instance.EntrancePool.IsEntranceRando);

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
                    if (!InstanceContainer.logicCalculation.ConditionalsMet(ChoiceOption.Conditionals, new List<string>())) { continue; }
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
                    menuItem.DropDownItems.Add(toolStripComboBox);
                    OptionTree.MenuItems.Add(menuItem);
                }
                else if (o is OptionData.MultiSelectOption MultiSelectOption)
                {
                    var OptionTree = GetOptionTree(o);
                    if (!InstanceContainer.logicCalculation.ConditionalsMet(MultiSelectOption.Conditionals, new List<string>())) { continue; }
                    ToolStripMenuItem menuItem = new() { Name = $"{MultiSelectOption.ID}Menu", Text = MultiSelectOption.getOptionName() };
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
                    if (!InstanceContainer.logicCalculation.ConditionalsMet(ToggleOption.Conditionals, new List<string>())) { continue; }
                    ToolStripMenuItem menuItem = new() { Name = $"{ToggleOption.ID}Menu", Checked = ToggleOption.Enabled.ID == ToggleOption.Value, Text = ToggleOption.getOptionName() };
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
                    if (!InstanceContainer.logicCalculation.ConditionalsMet(IntOption.Conditionals, new List<string>())) { continue; }
                    string DisplayName = IntOption.ToString();
                    ToolStripMenuItem menuItem = new() { Name = $"{IntOption.ID}Menu", Text = IntOption.ToString() };
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

        //Context Menus

        private void ShowContextMenu(ListBox listBox)
        {
            string LogicID = null;
            DisplayListType? displayList = null;
            if (listBox.SelectedItem is EntranceData.EntranceRandoExit LogicexitObject) { LogicID = LogicexitObject.ID; }
            if (listBox.SelectedItem is LocationData.LocationObject LogicLocationObject) { LogicID = LogicLocationObject.ID; }
            if (listBox.SelectedItem is HintData.HintObject LogicHintObject) { LogicID = LogicHintObject.ID; }
            if (listBox.SelectedItem is LocationData.LocationProxy LogicProxyObject) { LogicID = LogicProxyObject.GetDictEntry().LogicInheritance ?? LogicProxyObject.ReferenceID; }

            if (listBox == LBValidLocations) { displayList = DisplayListType.Locations; }
            if (listBox == LBValidEntrances) { displayList = DisplayListType.Entrances; }
            if (listBox == LBCheckedLocations) { displayList = DisplayListType.Checked; }


            ContextMenuStrip contextMenuStrip = new();

            //Refresh List Box
            ToolStripItem RefreshContextItem = contextMenuStrip.Items.Add("Refresh");
            RefreshContextItem.Click += (sender, e) => { PrintToListBox(new List<ListBox> { listBox }); };

            //Navigate to Area
            if (listBox.SelectedItem is MiscData.Areaheader NavAreaObject &&
                InstanceContainer.Instance.EntrancePool.AreaList.ContainsKey(NavAreaObject.Area) &&
                InstanceContainer.Instance.EntrancePool.IsEntranceRando)
            {
                ToolStripItem NavigateHereContextItem = contextMenuStrip.Items.Add("Navigate To this area");
                NavigateHereContextItem.Click += (sender, e) => { CMBEnd.SelectedItem = NavAreaObject.Area; };
            }

            //Area Object Functions
            if (listBox.SelectedItem is MiscData.Areaheader AreaObject && displayList is not null)
            {
                //Filter By Area
                var TagetTextBox = listBox == LBValidLocations ? TXTLocSearch : (listBox == LBValidEntrances ? TXTEntSearch : (listBox == LBCheckedLocations ? TXTCheckedSearch : null));
                ToolStripItem FilterHereContextItem = contextMenuStrip.Items.Add("Filter By this area");
                FilterHereContextItem.Click += (sender, e) =>
                {
                    TagetTextBox.Text = "#" + AreaObject.Area;
                };
                bool IsMinimized = AreaObject.IsMinimized((DisplayListType)displayList, InstanceContainer.Instance.StaticOptions);
                string HideAction = IsMinimized ? "Expand Area" : "Minimize Area";
                ToolStripItem HideAreaContextItem = contextMenuStrip.Items.Add(HideAction);
                HideAreaContextItem.Click += (sender, e) =>
                {
                    if (IsMinimized) { AreaObject.RemoveMinimized((DisplayListType)displayList, InstanceContainer.Instance.StaticOptions); }
                    else { AreaObject.SetMinimized((DisplayListType)displayList, InstanceContainer.Instance.StaticOptions); }
                    PrintToListBox();
                };
            }

            //Star Object
            if (Utility.DynamicPropertyExist(listBox.SelectedItem as dynamic, "Starred"))
            {
                dynamic obj = listBox.SelectedItem;
                string StarFunction = obj.Starred ? "UnStar Location" : "Star Location";
                ToolStripItem StarContextItem = contextMenuStrip.Items.Add(StarFunction);
                StarContextItem.Click += (sender, e) => { obj.Starred = !obj.Starred; PrintToListBox(new List<ListBox> { listBox }); };
            }

            //Show Logic
            if (LogicID is not null && InstanceContainer.Instance.GetLogic(LogicID) is not null)
            {
                ToolStripItem ShowLogicFunction = contextMenuStrip.Items.Add("Show Logic");
                ShowLogicFunction.Click += (sender, e) => { new ShowLogic(LogicID, InstanceContainer).Show(); };
            }

            //Show Unlock Data
            if (LogicID is not null && InstanceContainer.logicCalculation.LogicUnlockData.ContainsKey(LogicID))
            {
                ToolStripItem WhatUnlockedThis = contextMenuStrip.Items.Add("What Unlocked This");
                WhatUnlockedThis.Click += (sender, e) => { ShowUnlockData(LogicID); };
            }

            //Show Whats Missing
            if (LogicID is not null && !InstanceContainer.logicCalculation.LogicUnlockData.ContainsKey(LogicID) && InstanceContainer.Instance.SpoilerLog is not null)
            {
                ToolStripItem WhatsMissing = contextMenuStrip.Items.Add("Show Missing Items");
                WhatsMissing.Click += (sender, e) =>
                {
                    var missingitems = PlaythroughTools.GetMissingItems(LogicID, InstanceContainer.Instance);
                    if (missingitems == null) { MessageBox.Show($"{LogicID} Can not be obtained"); return; }
                    MessageBox.Show($"The Following items are still needed for {LogicID}\n\n{string.Join("\n", missingitems)}");
                };
            }

            //Objects Check Functions
            if ((LogicID is not null) && (listBox == LBValidLocations || listBox == LBValidEntrances))
            {
                dynamic TargetLocationObject = listBox.SelectedItem;
                if (listBox.SelectedItem is LocationData.LocationProxy tp) { TargetLocationObject = InstanceContainer.Instance.GetLocationByID(tp.ReferenceID); }
                //Check Item
                ToolStripItem CheckContextItem = contextMenuStrip.Items.Add("Check Location");
                CheckContextItem.Click += (sender, e) => { HandleItemSelect(new List<object> { listBox.SelectedItem }, MiscData.CheckState.Checked, LB: listBox); };
                //Mark\UnMark Item
                string MarkFunction = "Toggle Checked";
                if (Utility.DynamicPropertyExist(TargetLocationObject, "CheckState"))
                { MarkFunction = TargetLocationObject.CheckState == MiscData.CheckState.Marked ? "UnMark Location" : "Mark Location"; }
                ToolStripItem MarkContextItem = contextMenuStrip.Items.Add(MarkFunction);
                MarkContextItem.Click += (sender, e) => { HandleItemSelect(new List<object> { listBox.SelectedItem }, MiscData.CheckState.Marked, LB: listBox); };
            }

            //Objects Uncheck Functions
            if ((LogicID is not null) && listBox == LBCheckedLocations)
            {
                //UnCheck Item
                ToolStripItem CheckContextItem = contextMenuStrip.Items.Add("UnCheck entry");
                CheckContextItem.Click += (sender, e) => { HandleItemSelect(new List<object> { listBox.SelectedItem }, MiscData.CheckState.Unchecked, LB: listBox); };
                //Uncheck and Mark Item
                ToolStripItem MarkContextItem = contextMenuStrip.Items.Add("Mark entry");
                MarkContextItem.Click += (sender, e) => { HandleItemSelect(new List<object> { listBox.SelectedItem }, MiscData.CheckState.Marked, LB: listBox); };
            }

            //Variable and Option check Functions
            if (listBox.SelectedItem is OptionData.ChoiceOption || listBox.SelectedItem is OptionData.ToggleOption || listBox.SelectedItem is OptionData.IntOption)
            {
                //UnCheck Item
                ToolStripItem CheckContextItem = contextMenuStrip.Items.Add("Edit");
                CheckContextItem.Click += (sender, e) => { HandleItemSelect(new List<object> { listBox.SelectedItem }, MiscData.CheckState.Unchecked, LB: listBox); };
            }

            //Object Price Edit
            dynamic Target = listBox.SelectedItem;
            if (Utility.DynamicPropertyExist(Target, "Price") || PriceRando.TestForPriceData(Target))
            {
                Target.GetPrice(out int p, out char c);
                string SetPriceText = p > -1 ? "Clear Price" : "Set Price";
                ToolStripItem SetPrice = contextMenuStrip.Items.Add(SetPriceText);
                SetPrice.Click += (sender, e) =>
                {
                    if (p > -1) { Target.SetPrice(-1); }
                    else { SetCheckPrice(Target); }
                    InstanceContainer.logicCalculation.CompileOptionActionEdits();
                    InstanceContainer.logicCalculation.CalculateLogic();
                    UpdateUI();
                };
            }

            //Hide Item
            if (Utility.DynamicPropertyExist(listBox.SelectedItem, "Hidden"))
            {
                dynamic HiddenItem = listBox.SelectedItem;
                string HiddenText = HiddenItem.Hidden ? "Unhide" : "Hide";
                ToolStripItem SetHidden = contextMenuStrip.Items.Add(HiddenText);
                SetHidden.Click += (sender, e) => { HiddenItem.Hidden = !HiddenItem.Hidden; PrintToListBox(new List<ListBox> { listBox }); };

            }

            if (contextMenuStrip.Items.Count > 0)
            {
                contextMenuStrip.Show(Cursor.Position);
            }
        }

        private void SetCheckPrice(dynamic Object)
        {
            var DictEntry = Object.GetDictEntry();
            var PriceContainer = new List<OptionData.IntOption>() { new() { ID = DictEntry.Name ?? DictEntry.ID, Value = 0 } };
            VariableInputWindow PriceInput = new(PriceContainer, InstanceContainer);
            PriceInput.ShowDialog();
            var ResultPrice = PriceInput._Result.First().GetItem<int>();
            Object.SetPrice(ResultPrice);
        }

        private void ShowUnlockData(string iD)
        {
            if (!InstanceContainer.logicCalculation.LogicUnlockData.ContainsKey(iD)) { return; }
            var AdvancedUnlockData = PlaythroughTools.GetAdvancedUnlockData(iD, InstanceContainer.logicCalculation.LogicUnlockData, InstanceContainer.Instance);
            var DataDisplay = PlaythroughTools.FormatAdvancedUnlockData(AdvancedUnlockData, InstanceContainer.logicCalculation.LogicUnlockData);

            List<dynamic> Items = new List<dynamic>();
            foreach (var i in DataDisplay)
            {
                var FLI = new MiscData.StandardListBoxItem
                {
                    Display = i is MiscData.Divider DVIx ? DVIx.Display : i.ToString(),
                    tag = i is MiscData.Divider DVIy ? DVIy : i.ToString(),
                    tagFunc = i is MiscData.Divider ? ShowUnlockSubFunction : null
                };
                Items.Add(FLI);
            }

            BasicDisplay basicDisplay = new BasicDisplay(Items);
            basicDisplay.Text = $"Unlock Data for {iD}";
            basicDisplay.Show();
        }

        private dynamic ShowUnlockSubFunction(dynamic dynamic)
        {
            if (dynamic is not ValueTuple<List<ValueTuple<object, bool>>, object> TO || TO.Item2 is not MiscData.Divider DIV) { return null; }
            List<ValueTuple<object, bool>> Return = new();
            bool Toggleing = false;
            foreach (var i in TO.Item1)
            {
                bool IsDivider = i.Item1 is MiscData.StandardListBoxItem FLI && FLI.tag is MiscData.Divider;
                Toggleing = IsDivider ? ((i.Item1 as MiscData.StandardListBoxItem).tag as MiscData.Divider).Display == DIV.Display : Toggleing;
                bool Shown = (Toggleing ? !i.Item2 : i.Item2) || IsDivider;
                Return.Add((i.Item1, Shown));
            }
            return Return;
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
                .SetCheckCoiceOptions(HandleUnassignedChecks)
                .SetCheckIntOPtions(HandleUnassignedVariables);

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
            if (ModifierKeys == Keys.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.A:
                        (sender as ListBox).BeginUpdate();
                        for (int i = 0; i < (sender as ListBox).Items.Count; i++) { (sender as ListBox).SetSelected(i, true); }
                        (sender as ListBox).EndUpdate();
                        break;
                }
            }
        }

        private void preventKeyShortcuts(object sender, KeyPressEventArgs e)
        {
            if (ModifierKeys == Keys.Control &&
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
                while (!Utility.OBJIsThreadSafe(MainInterfaceItemDisplayThread, MainInterfaceItemDisplayForm)) { Thread.Sleep(10); }
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
            if (!Utility.OBJIsThreadSafe(MainInterfaceItemDisplayThread, MainInterfaceItemDisplayForm)) { return; }
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
    }
}
