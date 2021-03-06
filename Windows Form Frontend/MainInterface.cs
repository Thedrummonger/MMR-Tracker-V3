using Microsoft.VisualBasic;
using MMR_Tracker_V3;
using MMR_Tracker_V3.OtherGames;
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
        public static MiscData.InstanceContainer InstanceContainer = new MiscData.InstanceContainer();
        public static MainInterface CurrentProgram;
        Pathfinder MainInterfacepathfinder = new Pathfinder();
        private bool FormIsMaximized = false;

        public MainInterface()
        {
            InitializeComponent();
            InstanceContainer.logicCalculation = new LogicCalculation(InstanceContainer);
        }

        //MainForm Actions

        private void MainInterface_Load(object sender, EventArgs e)
        {
            //Since only one instance of the main interface should ever be open, We can store that instance in a variable to be called from static code.
            if (CurrentProgram != null) { Close(); return; }
            CurrentProgram = this;
            this.Text = "MMR Tracker";

            //Ensure the current directory is always the base directory in case the application is opened from a MMRTSave file elsewhere on the system
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            if (!Directory.Exists(References.Globalpaths.BaseAppdataPath))
            {
                Directory.CreateDirectory(References.Globalpaths.BaseAppdataPath);
            }
            DoUpdateCheck();
            doDevCheck();
            UpdateUI();
            WinFormInstanceCreation.ApplyUserPretLogic();
        }

        public void DoUpdateCheck()
        {
            References.TrackerVersionStatus = UpdateManager.GetTrackerVersionStatus();
            if (References.TrackerVersionStatus.VersionStatus == UpdateManager.versionStatus.outdated)
            {
                var Download = MessageBox.Show($"Your tracker version { References.trackerVersion } is out of Date. Would you like to download the latest version { References.TrackerVersionStatus.LatestVersion.TagName }?\n\nTo disable this message click \"cancel\" or disable \"Check for Updates\" in the options file", "Tracker Out of Date", MessageBoxButtons.YesNoCancel);
                if (Download == DialogResult.Yes) { { System.Diagnostics.Process.Start("explorer.exe", References.TrackerVersionStatus.LatestVersion.HtmlUrl); this.Close(); return; } }
                else if (Download == DialogResult.Cancel)
                {
                    LogicObjects.OptionFile options = new LogicObjects.OptionFile();
                    if (File.Exists(References.Globalpaths.OptionFile))
                    {
                        try { options = JsonConvert.DeserializeObject<LogicObjects.OptionFile>(File.ReadAllText(References.Globalpaths.OptionFile)); }
                        catch { Debug.WriteLine("could not parse options.txt"); }
                    }
                    options.CheckForUpdate = false;
                    References.TrackerVersionStatus.DoUpdateCheck = false;
                    try { File.WriteAllText(References.Globalpaths.OptionFile, JsonConvert.SerializeObject(options, Testing._NewtonsoftJsonSerializerOptions)); }
                    catch (Exception ex) { Debug.WriteLine($"could not write to options.txt {ex}"); }
                }
            }
        }

        private void doDevCheck(bool? forceDevState = null)
        {
            bool ForceDev = forceDevState is not null && (bool)forceDevState;
            bool ForceUser = forceDevState is not null && !(bool)forceDevState;
            bool isDevPC = File.Exists(References.Globalpaths.DevFile) || ForceDev;
            bool IsDubugger = Debugger.IsAttached || ForceDev;
            bool Modifier = ModifierKeys == Keys.Control;

            if (IsDubugger && !Modifier && !ForceUser)
            {
                Testing.DebugMode = MiscData.DebugMode.Debugging;
            }
            else if (isDevPC && Modifier && !ForceUser)
            {
                Testing.DebugMode = MiscData.DebugMode.UserView;
            }
            else
            {
                Testing.DebugMode = MiscData.DebugMode.Off;
            }
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

        private void MainInterface_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!PromptSave()) { e.Cancel = true; }
        }

        //Menu Strip

        private void UndoRedo_Click(object sender, EventArgs e)
        {
            Stopwatch TimeTotalItemSelect = new Stopwatch();
            Utility.TimeCodeExecution(TimeTotalItemSelect, "Saving Tracker State (UTF8)", 1);
            if (sender == undoToolStripMenuItem && InstanceContainer.UndoStringList.Any())
            {
                string CurrentState = Utf8Json.JsonSerializer.ToJsonString(InstanceContainer.Instance);
                InstanceContainer.Instance = LogicObjects.TrackerInstance.FromJson(InstanceContainer.UndoStringList[^1]);
                InstanceContainer.RedoStringList.Add(CurrentState);
                InstanceContainer.UndoStringList.RemoveAt(InstanceContainer.UndoStringList.Count - 1);
            }
            else if (sender == redoToolStripMenuItem && InstanceContainer.RedoStringList.Any())
            {
                string CurrentState = Utf8Json.JsonSerializer.ToJsonString(InstanceContainer.Instance);
                InstanceContainer.Instance = LogicObjects.TrackerInstance.FromJson(InstanceContainer.RedoStringList[^1]);
                InstanceContainer.UndoStringList.Add(CurrentState);
                InstanceContainer.RedoStringList.RemoveAt(InstanceContainer.RedoStringList.Count - 1);
            }
            Utility.TimeCodeExecution(TimeTotalItemSelect, "Undo/Redo Action", -1);

            InstanceContainer.logicCalculation.CalculateLogic();
            UpdateUI();
        }

        //Menu Strip => File

        private void NewToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!PromptSave()) { return; }
            OpenFileDialog fileDialog = new OpenFileDialog();
            var Result = fileDialog.ShowDialog();
            if (Result == DialogResult.Cancel || !File.Exists(fileDialog.FileName)) { return; }
            var LogicData = LogicFileParser.GetLogicData(fileDialog.FileName, out bool WasSpoilerLog);
            if (LogicData is null) { MessageBox.Show("Invalid File\nPlease select either a logic file or MMR spoiler log"); return; }
            string Logic = string.Join("", LogicData);
            WinFormInstanceCreation.CreateWinFormInstance(Logic, SpoilerLog: (WasSpoilerLog ? fileDialog.FileName : null) );
        }

        private void SavetoolStripMenuItem1_Click(object sender, EventArgs e)
        {
            bool SaveAs = (sender is ToolStripMenuItem item && item == SaveAsToolStripMenuItem1);
            if (File.Exists(InstanceContainer.CurrentSavePath) && !SaveAs)
            {
                InstanceContainer.UnsavedChanges = false;
                File.WriteAllText(InstanceContainer.CurrentSavePath, InstanceContainer.Instance.ToString());
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
                    File.WriteAllText(saveFileDialog.FileName, InstanceContainer.Instance.ToString());
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
                LogicObjects.TrackerInstance NewTrackerInstance;
                try
                {
                    NewTrackerInstance = LogicObjects.TrackerInstance.FromJson(File.ReadAllText(openFileDialog.FileName));
                }
                catch { MessageBox.Show("Save File Not Valid"); return; }
                InstanceContainer.Instance = NewTrackerInstance;
                InstanceContainer.CurrentSavePath = openFileDialog.FileName;
                InstanceContainer.logicCalculation.CalculateLogic();
                UpdateUI();
                UpdateDynamicUserOptions();
            }
        }

        //Menu Strip => Options

        private void ToggleRandomizerOption_Click(object sender, EventArgs e, OptionData.TrackerOption Option, Object Selection = null, bool update = true)
        {
            if (Selection == null)
            {
                Option.ToggleOption();
            }
            else
            {
                Option.CurrentValue = (string)Selection;
            }
            if (update)
            {
                InstanceContainer.logicCalculation.CalculateLogic();
                UpdateUI();
            }
        }

        private void logicOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string CurrentState = Utf8Json.JsonSerializer.ToJsonString(InstanceContainer.Instance);
            RandomizedStateEditor editor = new RandomizedStateEditor(InstanceContainer.Instance);
            editor.ShowDialog();
            if (editor.ChangesMade) { SaveTrackerState(CurrentState); }

            InstanceContainer.logicCalculation.CalculateLogic();
            UpdateUI();
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
                    string CurrentState = Utf8Json.JsonSerializer.ToJsonString(InstanceContainer.Instance);
                    var ToUpdate = new List<ListBox> { LBCheckedLocations, LBValidEntrances, LBValidLocations };
                    foreach (var i in ToUpdate) { WinFormUtils.PrintMessageToListBox(i, "Importing Spoiler Log \n Please Wait..."); }
                    if (SpoilerLogTools.ImportSpoilerLog(File.ReadAllLines(openFileDialog.FileName), openFileDialog.FileName, InstanceContainer.Instance))
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
        }

        //Menu Strip => Dev

        private void CodeTestingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> UnlockData = new List<string>();
            InstanceContainer.logicCalculation.checkItemArray("NonTransformationMasks", 3, UnlockData, out int Total);

            Debug.WriteLine($"Total Masks {Total}");
            Debug.WriteLine($"Unlock Data {JsonConvert.SerializeObject(UnlockData, Testing._NewtonsoftJsonSerializerOptions)}");

            var HeartPieces = InstanceContainer.Instance.ItemPool.Values.Where(x => x.GetDictEntry(InstanceContainer.Instance).GetName(InstanceContainer.Instance) == "Heart Container").Select(x => x.Id);
            //Debug.WriteLine($"Unlock Data {JsonConvert.SerializeObject(HeartPieces, Testing._NewtonsoftJsonSerializerOptions)}");
        }

        //ListBoxes

        public void GetListObjectData(dynamic entry, out MiscData.CheckState checkState, out bool starred, out bool Available)
        {
            if (entry is LocationData.LocationProxy po) { 
                checkState = InstanceContainer.Instance.GetLocationByID(po.ReferenceID)?.CheckState ?? MiscData.CheckState.Checked; 
                starred = po.Starred;
                Available = po.ProxyAvailable(InstanceContainer.Instance);
            }
            else {
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
            Font F = WinFormUtils.GetFontFromString(InstanceContainer.Instance.StaticOptions.OptionFile.WinformData.FormFont);
            Brush brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? Brushes.White : Brushes.Black;

            GetListObjectData(LB.Items[e.Index], out MiscData.CheckState checkState, out bool starred, out bool available);
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
            Font F = WinFormUtils.GetFontFromString(InstanceContainer.Instance.StaticOptions.OptionFile.WinformData.FormFont);
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
            int index = LB.IndexFromPoint(e.Location);
            if (index < 0) { return; }
            if (e.Button == MouseButtons.Middle)
            {
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
            string UsavedChanges = InstanceContainer?.UnsavedChanges is not null &&  InstanceContainer.UnsavedChanges ? "*" : "";
            string TrackerStatus = "";
            if (References.TrackerVersionStatus.VersionStatus == UpdateManager.versionStatus.outdated) { TrackerStatus = " (outdated)"; }
            else if (References.TrackerVersionStatus.VersionStatus == UpdateManager.versionStatus.dev) { TrackerStatus = " (dev)"; }
            TrackerStatus = References.TrackerVersionStatus.DoUpdateCheck ? TrackerStatus : "";
            this.Text = GameCode + " Tracker" + UsavedChanges + TrackerStatus;
        }

        public void UpdateUI()
        {
            AlignUIElements();
            FormatMenuItems();
            SetTrackerTitle();

            if (InstanceContainer is null || InstanceContainer.Instance is null) { return; }


            InstanceContainer.Instance.EntrancePool.IsEntranceRando = InstanceContainer.Instance.EntrancePool.CheckForRandomEntrances();

            PathFinderToolStripMenuItem.Visible = InstanceContainer.Instance.EntrancePool.IsEntranceRando;

            redoToolStripMenuItem.Enabled = InstanceContainer.RedoStringList.Any();
            undoToolStripMenuItem.Enabled = InstanceContainer.UndoStringList.Any();

            string CurrentStart = (string)CMBStart.SelectedItem ?? "";
            string CurrentEnd = (string)CMBEnd.SelectedItem ?? "";
            var AccessableAreas = InstanceContainer.Instance.EntrancePool.AreaList.Values.Where(x => x.ExitsAcessibleFrom > 0 && x.LoadingZoneExits.Any()).Select(x => x.ID);
            CMBStart.DataSource = AccessableAreas.OrderBy(x => x).ToList();
            CMBEnd.DataSource = AccessableAreas.OrderBy(x => x).ToList();
            if (CMBStart.Items.Contains(CurrentStart)) { CMBStart.SelectedIndex = CMBStart.Items.IndexOf(CurrentStart); }
            if (CMBEnd.Items.Contains(CurrentEnd)) { CMBEnd.SelectedIndex = CMBEnd.Items.IndexOf(CurrentEnd); }

            UpdateDynamicUserOptions();
            PrintToListBox();
            this.Refresh();
        }

        private void AlignUIElements()
        {
            var MenuHeight = menuStrip1.Height;
            var FormHeight = this.Height - 40 - MenuHeight;
            var FormWidth = this.Width - 18;
            var FormHalfHeight = FormHeight / 2;
            var FormHalfWidth = FormWidth / 2;
            var locX = 2;
            var locY = 2 + MenuHeight;
            if (InstanceContainer == null || InstanceContainer.Instance == null)
            {
                SetObjectVisibility(false, false);
                return;
            }
            else if (InstanceContainer.Instance.StaticOptions.EntranceRandoFeatures && (InstanceContainer.Instance.EntrancePool.IsEntranceRando || InstanceContainer.Instance.EntrancePool.CheckForRandomEntrances()))
            {
                SetObjectVisibility(true, true);
                lblAvailableLocation.Location = new Point(locX, locY + 2);
                BTNSetItem.Location = new Point(FormHalfWidth - BTNSetItem.Width, MenuHeight + 1);
                TXTLocSearch.Location = new Point(locX, locY + lblAvailableLocation.Height + 6);
                TXTLocSearch.Width = FormHalfWidth - 2;
                LBValidLocations.Location = new Point(locX, locY + lblAvailableLocation.Height + TXTLocSearch.Height + 8);
                LBValidLocations.Width = FormHalfWidth - 2;
                LBValidLocations.Height = FormHalfHeight - lblAvailableLocation.Height - TXTLocSearch.Height - 14;

                lblAvailableEntrances.Location = new Point(FormHalfWidth + locX, locY + 2);
                BTNSetEntrance.Location = new Point(FormWidth - BTNSetEntrance.Width, MenuHeight + 1);
                TXTEntSearch.Location = new Point(FormHalfWidth + locX, locY + lblAvailableEntrances.Height + 6);
                TXTEntSearch.Width = FormHalfWidth - 2;
                LBValidEntrances.Location = new Point(FormHalfWidth + locX, locY + lblAvailableEntrances.Height + TXTEntSearch.Height + 8);
                LBValidEntrances.Width = FormHalfWidth - 2;
                LBValidEntrances.Height = FormHalfHeight - lblAvailableEntrances.Height - TXTEntSearch.Height - 14;

                lblCheckedLocation.Location = new Point(locX, FormHalfHeight + locY - 2);
                CHKShowAll.Location = new Point(FormHalfWidth - CHKShowAll.Width, MenuHeight + FormHalfHeight - 2);
                TXTCheckedSearch.Location = new Point(locX, locY + lblAvailableLocation.Height + 2 + FormHalfHeight);
                TXTCheckedSearch.Width = FormHalfWidth - 2;
                LBCheckedLocations.Location = new Point(locX, locY + lblAvailableLocation.Height + TXTCheckedSearch.Height + 4 + FormHalfHeight);
                LBCheckedLocations.Width = FormHalfWidth - 2;
                LBCheckedLocations.Height = FormHalfHeight - lblAvailableLocation.Height - TXTCheckedSearch.Height - 8;

                label4.Location = new Point(FormHalfWidth + locX, FormHalfHeight + locY - 2);
                BTNFindPath.Location = new Point(FormWidth - BTNFindPath.Width, MenuHeight + FormHalfHeight - 3);
                label5.Location = new Point(FormHalfWidth + locX, FormHalfHeight + locY + label4.Height + 6);
                lblSwapPathfinder.Location = new Point(label5.Location.X + label5.Width + 4, label5.Location.Y - 3);
                label6.Location = new Point(FormHalfWidth + locX, FormHalfHeight + locY + label4.Height + 7 + CMBStart.Height);
                CMBStart.Location = new Point(FormHalfWidth + locX + label6.Width + 2, FormHalfHeight + locY + label4.Height + 2);
                CMBEnd.Location = new Point(FormHalfWidth + locX + label6.Width + 2, FormHalfHeight + locY + label4.Height + CMBStart.Height + 5);
                CMBStart.Width = FormHalfWidth - label6.Width - 4;
                CMBEnd.Width = FormHalfWidth - label6.Width - 4;
                LBPathFinder.Location = new Point(locX + FormHalfWidth, FormHalfHeight + locY + 8 + label4.Height + CMBStart.Height + CMBEnd.Height);
                LBPathFinder.Width = FormHalfWidth - 2;
                LBPathFinder.Height = LBCheckedLocations.Height - CMBEnd.Height - 5;
            }
            else
            {
                SetObjectVisibility(true, false);
                if (InstanceContainer.Instance.StaticOptions.OptionFile.WinformData.HorizontalLayout)
                {
                    lblAvailableLocation.Location = new Point(locX, locY + 2);
                    BTNSetItem.Location = new Point(FormHalfWidth - BTNSetItem.Width, MenuHeight + 1);
                    TXTLocSearch.Location = new Point(locX, locY + lblAvailableLocation.Height + 6);
                    TXTLocSearch.Width = FormHalfWidth - 2;
                    LBValidLocations.Location = new Point(locX, locY + lblAvailableLocation.Height + TXTLocSearch.Height + 8);
                    LBValidLocations.Width = FormHalfWidth - 2;
                    LBValidLocations.Height = FormHeight - lblAvailableLocation.Height - TXTLocSearch.Height - 14;

                    lblCheckedLocation.Location = new Point(FormHalfWidth + locX, locY + 2);
                    CHKShowAll.Location = new Point(FormWidth - CHKShowAll.Width, MenuHeight + 3);
                    TXTCheckedSearch.Location = new Point(FormHalfWidth + locX, locY + lblAvailableEntrances.Height + 6);
                    TXTCheckedSearch.Width = FormHalfWidth - 2;
                    LBCheckedLocations.Location = new Point(FormHalfWidth + locX, locY + lblAvailableEntrances.Height + TXTEntSearch.Height + 8);
                    LBCheckedLocations.Width = FormHalfWidth - 2;
                    LBCheckedLocations.Height = FormHeight - lblAvailableEntrances.Height - TXTEntSearch.Height - 14;
                }
                else
                {
                    lblAvailableLocation.Location = new Point(locX, locY + 2);
                    BTNSetItem.Location = new Point(FormWidth - BTNSetItem.Width, MenuHeight + 1);
                    TXTLocSearch.Location = new Point(locX, locY + lblAvailableLocation.Height + 6);
                    TXTLocSearch.Width = FormWidth - 2;
                    LBValidLocations.Location = new Point(locX, locY + lblAvailableLocation.Height + TXTLocSearch.Height + 8);
                    LBValidLocations.Width = FormWidth - 2;
                    LBValidLocations.Height = FormHalfHeight - lblAvailableLocation.Height - TXTLocSearch.Height - 14;

                    lblCheckedLocation.Location = new Point(locX, FormHalfHeight + locY - 2);
                    CHKShowAll.Location = new Point(FormWidth - CHKShowAll.Width, MenuHeight + FormHalfHeight - 2);
                    TXTCheckedSearch.Location = new Point(locX, locY + lblAvailableLocation.Height + 2 + FormHalfHeight);
                    TXTCheckedSearch.Width = FormWidth - 2;
                    LBCheckedLocations.Location = new Point(locX, locY + lblAvailableLocation.Height + TXTCheckedSearch.Height + 4 + FormHalfHeight);
                    LBCheckedLocations.Width = FormWidth - 2;
                    LBCheckedLocations.Height = FormHalfHeight - lblAvailableLocation.Height - TXTCheckedSearch.Height - 8;
                }
            }
        }

        public void SetObjectVisibility(bool item, bool location)
        {
            var UpperLeftLBL = lblAvailableLocation;
            var UpperRightLBL = lblAvailableEntrances;
            var LowerLeftLBL = lblCheckedLocation;
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

        private void PrintToListBox(List<ListBox> ToUpdate = null)
        {
            var lbLocTop = LBValidLocations.TopIndex;
            var lbEntTop = LBValidEntrances.TopIndex;
            var lbCheckTop = LBCheckedLocations.TopIndex;

            if (ToUpdate == null) { ToUpdate = new List<ListBox> { LBCheckedLocations, LBValidEntrances, LBValidLocations }; }

            foreach (var i in ToUpdate)
            {
                i.Items.Clear();
                i.Font = WinFormUtils.GetFontFromString(InstanceContainer.Instance.StaticOptions.OptionFile.WinformData.FormFont);
                i.ItemHeight = Convert.ToInt32(i.Font.Size * 1.8);
                i.BeginUpdate();
            }

            var dataset = TrackerDataHandeling.PopulateDataSets(InstanceContainer.Instance);

            if (ToUpdate.Contains(LBValidLocations)) 
            {
                var Entries = TrackerDataHandeling.PopulateAvailableLocationList(dataset, WinFormUtils.CreateDivider(LBValidLocations), InstanceContainer.Instance, TXTLocSearch.Text, CHKShowAll.Checked, out int x, out int y);
                lblAvailableLocation.Text = $"Available Locations: {y}" + (x != y ? $"/{x}" : "");
                foreach (var i in Entries) { LBValidLocations.Items.Add(i); }
                LBValidLocations.TopIndex = lbLocTop; 
            }
            if (ToUpdate.Contains(LBValidEntrances)) 
            {
                var Entries = TrackerDataHandeling.PopulateAvailableEntraceList(dataset, WinFormUtils.CreateDivider(LBValidEntrances), InstanceContainer.Instance, TXTEntSearch.Text, CHKShowAll.Checked, out int x, out int y);
                lblAvailableEntrances.Text = $"Available Entrances: {y}" + (x != y ? $"/{x}" : "");
                foreach (var i in Entries) { LBValidEntrances.Items.Add(i); }
                LBValidEntrances.TopIndex = lbEntTop; 
            }
            if (ToUpdate.Contains(LBCheckedLocations)) 
            {
                var Entries = TrackerDataHandeling.PopulateCheckedLocationList(dataset, WinFormUtils.CreateDivider(LBCheckedLocations), InstanceContainer.Instance, TXTCheckedSearch.Text, out int x, out int y);
                lblCheckedLocation.Text = $"Checked Locations: {y}" + (x != y ? $"/{x}" : "");
                foreach (var i in Entries) { LBCheckedLocations.Items.Add(i); }
                LBCheckedLocations.TopIndex = lbCheckTop; 
            }

            foreach (var i in ToUpdate) { i.EndUpdate(); }
        }

        public void FormatMenuItems()
        {
            OptionstoolStripMenuItem.Visible = (InstanceContainer.Instance != null);
            toolsToolStripMenuItem.Visible = (InstanceContainer.Instance != null);
            undoToolStripMenuItem.Visible = (InstanceContainer.Instance != null);
            redoToolStripMenuItem.Visible = (InstanceContainer.Instance != null);
            refreshToolStripMenuItem.Visible = (InstanceContainer.Instance != null);
            SavetoolStripMenuItem1.Visible = (InstanceContainer.Instance != null);
            SaveAsToolStripMenuItem1.Visible = (InstanceContainer.Instance != null) && !string.IsNullOrWhiteSpace(InstanceContainer.CurrentSavePath);
            spoilerLogToolsToolStripMenuItem.Visible = (InstanceContainer.Instance != null);
            importSpoilerLogToolStripMenuItem.Visible = (InstanceContainer.Instance != null);
            PathFinderToolStripMenuItem.Visible = (InstanceContainer.Instance != null && InstanceContainer.Instance.EntrancePool.IsEntranceRando);

            logicEditorToolStripMenuItem.Visible = Testing.Debugging();

            if (InstanceContainer.Instance == null) { return; }

            SaveAsToolStripMenuItem1.Visible = (File.Exists(InstanceContainer.CurrentSavePath));
            importSpoilerLogToolStripMenuItem.Text = (InstanceContainer.Instance.SpoilerLog != null) ? "Remove Spoiler Log" : "Import Spoiler Log";

            //Manage Dev Menus
            devToolsToolStripMenuItem.Visible = Testing.IsDevUser();
            devToolsToolStripMenuItem.Text = (Testing.UserView()) ? "Run as Dev" : "Dev Options";
            foreach (ToolStripDropDownItem i in devToolsToolStripMenuItem.DropDownItems) { i.Visible = Testing.Debugging(); }
            viewAsUserToolStripMenuItem.Checked = Testing.UserView();

        }

        private void UpdateDynamicUserOptions()
        {
            RandomizerOptionsToolStripMenuItem1.DropDownItems.Clear();
            //Create List Box Toggle
            ToolStripComboBox ListBoxDisplayOptions = new();
            ListBoxDisplayOptions.Items.AddRange(OptionData.DisplayListBoxes);
            ListBoxDisplayOptions.SelectedIndex = OptionData.DisplayListBoxes.ToList().IndexOf(InstanceContainer.Instance.StaticOptions.ShowOptionsInListBox);
            ListBoxDisplayOptions.SelectedIndexChanged += delegate (object sender, EventArgs e)
            {
                InstanceContainer.Instance.StaticOptions.ShowOptionsInListBox = ((ToolStripComboBox)sender).SelectedItem.ToString();
                PrintToListBox();
            };
            ToolStripMenuItem ListBoxDisplay = new() { Text = "Display In List Box" };
            ListBoxDisplay.DropDownItems.Add(ListBoxDisplayOptions);
            RandomizerOptionsToolStripMenuItem1.DropDownItems.Add(ListBoxDisplay);
            RandomizerOptionsToolStripMenuItem1.DropDownItems.Add(new ToolStripSeparator());
            //Add User Options
            foreach (var i in InstanceContainer.Instance.UserOptions.Values)
            {
                if (i.IsToggleOption())
                {
                    ToolStripMenuItem menuItem = new() { Checked = OptionData.ToggleValues.Keys.Select(x => x.ToLower()).Contains(i.CurrentValue.ToLower()), Text = i.DisplayName };
                    menuItem.Click += delegate (object sender, EventArgs e) { ToggleRandomizerOption_Click(sender, e, i); };
                    RandomizerOptionsToolStripMenuItem1.DropDownItems.Add(menuItem);
                }
                else if (i.Values.Keys.Count > 1)//If the option only has one value, it will always be active so no need to display it.
                {
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
                    ToolStripMenuItem menuItem = new() { Text = i.DisplayName };
                    menuItem.DropDownItems.Add(toolStripComboBox);
                    RandomizerOptionsToolStripMenuItem1.DropDownItems.Add(menuItem);
                }
            }
            foreach(var i in InstanceContainer.Instance.Variables.Values.Where(x => !x.Static))
            {
                ToolStripMenuItem menuItem = new() { Text = i.ToString() };
                menuItem.Click += delegate (object sender, EventArgs e) { HandleItemSelect(new List<LogicDictionaryData.TrackerVariable> { i }, MiscData.CheckState.Checked); };
                RandomizerOptionsToolStripMenuItem1.DropDownItems.Add(menuItem);
            }
            RandomizerOptionsToolStripMenuItem1.Visible = RandomizerOptionsToolStripMenuItem1.DropDownItems.Count > 1;
        }

        //Context Menus

        private void ShowContextMenu(ListBox listBox)
        {
            string LogicID = null;
            if (listBox.SelectedItem is EntranceData.EntranceRandoExit LogicexitObject) { LogicID = InstanceContainer.Instance.GetLogicNameFromExit(LogicexitObject); }
            if (listBox.SelectedItem is LocationData.LocationObject LogicLocationObject) { LogicID = LogicLocationObject.ID; }
            if (listBox.SelectedItem is HintData.HintObject LogicHintObject) { LogicID = LogicHintObject.ID; }
            if (listBox.SelectedItem is LocationData.LocationProxy LogicProxyObject) { LogicID = LogicProxyObject.LogicInheritance ?? LogicProxyObject.ReferenceID; }

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

            //Filter By Area
            if (listBox.SelectedItem is MiscData.Areaheader AreaObject)
            {
                var TagetTextBox = listBox == LBValidLocations ? TXTLocSearch : (listBox == LBValidEntrances ? TXTEntSearch : (listBox == LBCheckedLocations ? TXTCheckedSearch : null));
                ToolStripItem FilterHereContextItem = contextMenuStrip.Items.Add("Filter By this area");
                FilterHereContextItem.Click += (sender, e) => 
                {
                    TagetTextBox.Text = "#" + AreaObject.Area;
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
                ShowLogicFunction.Click += (sender, e) => { new ShowLogic(LogicID, InstanceContainer.Instance).Show(); };
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
            if (listBox.SelectedItem is OptionData.TrackerOption || listBox.SelectedItem is LogicDictionaryData.TrackerVariable)
            {
                //UnCheck Item
                ToolStripItem CheckContextItem = contextMenuStrip.Items.Add("Edit");
                CheckContextItem.Click += (sender, e) => { HandleItemSelect(new List<object> { listBox.SelectedItem }, MiscData.CheckState.Unchecked, LB: listBox); };
            }

            //Object Price Edit
            dynamic Target = null;
            if (listBox.SelectedItem is LocationData.LocationProxy ProxyPriceSet) { Target = ProxyPriceSet.GetLogicInheritance(InstanceContainer.Instance); }
            else if (listBox.SelectedItem is LocationData.LocationObject LocPriceSet) { Target = LocPriceSet; }
            if (Utility.DynamicPropertyExist(Target, "Price"))
            {
                string SetPriceText = Target.Price > -1 ? "Clear Price" : "Set Price";
                ToolStripItem SetPrice = contextMenuStrip.Items.Add(SetPriceText);
                SetPrice.Click += (sender, e) =>
                {
                    if (Target.Price > -1) { Target.Price = -1; }
                    else { SetCheckPrice(Target); }
                };
            }

            //Debug Tools
            if (Testing.Debugging())
            {
                //DevData
                ToolStripItem ShowDevData = contextMenuStrip.Items.Add("Show Dev Data");
                ShowDevData.Click += (sender, e) =>
                {
                    Debug.WriteLine(JsonConvert.SerializeObject(listBox.SelectedItem, Testing._NewtonsoftJsonSerializerOptions));
                    if (listBox.SelectedItem is LocationData.LocationObject DebugLocObj)
                    {
                        Debug.WriteLine(JsonConvert.SerializeObject(DebugLocObj.GetDictEntry(InstanceContainer.Instance), Testing._NewtonsoftJsonSerializerOptions));
                    }
                    if (listBox.SelectedItem is LocationData.LocationProxy DebugProxyObj)
                    {
                        var ProxyRef = InstanceContainer.Instance.GetLocationByID(DebugProxyObj.ReferenceID);
                        Debug.WriteLine(JsonConvert.SerializeObject(ProxyRef, Testing._NewtonsoftJsonSerializerOptions));
                        Debug.WriteLine(JsonConvert.SerializeObject(ProxyRef?.GetDictEntry(InstanceContainer.Instance), Testing._NewtonsoftJsonSerializerOptions));
                    }
                };
            }

            if (contextMenuStrip.Items.Count > 0)
            {
                contextMenuStrip.Show(Cursor.Position);
            }
        }

        private void SetCheckPrice(dynamic Object)
        {
            var DictEntry = Object.GetDictEntry(InstanceContainer.Instance);
            var PriceContainer = new List<LogicDictionaryData.TrackerVariable>() { new() { ID = DictEntry.Name ?? DictEntry.ID, Value = 0 } };
            VariableInputWindow PriceInput = new(PriceContainer, InstanceContainer.Instance);
            PriceInput.ShowDialog();
            Object.Price = (int)PriceContainer.First().Value;
            InstanceContainer.logicCalculation.CalculateLogic();
            UpdateUI();
        }

        private void ShowUnlockData(string iD)
        {
            if (!InstanceContainer.logicCalculation.LogicUnlockData.ContainsKey(iD)) { return; }
            var AdvancedUnlockData = PlaythroughTools.GetAdvancedUnlockData(iD, InstanceContainer.logicCalculation.LogicUnlockData, InstanceContainer.Instance);
            var DataDisplay = PlaythroughTools.FormatAdvancedUnlockData(AdvancedUnlockData, InstanceContainer.logicCalculation.LogicUnlockData);

            List<dynamic> Items = new List<dynamic>();
            foreach(var i in DataDisplay)
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

        private void HandleItemSelect(IEnumerable<object> Items, MiscData.CheckState checkState, bool EnforceMarkAction = false, object LB = null)
        {
            Debug.WriteLine("Starting Check Action===================================");
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
            Stopwatch FullCodeTimer = new Stopwatch();
            Utility.TimeCodeExecution(FullCodeTimer);
            Stopwatch CodeTimer = new Stopwatch();
            Utility.TimeCodeExecution(CodeTimer);
            string CurrentState = Utf8Json.JsonSerializer.ToJsonString(InstanceContainer.Instance);
            Utility.TimeCodeExecution(CodeTimer, "Saving tracker State", 1);

            bool ChangesMade = TrackerDataHandeling.CheckSelectedItems(Items, checkState, InstanceContainer, HandleUnassignedChecks, HandleUnassignedVariables, EnforceMarkAction, FullCodeTimer);
            Utility.TimeCodeExecution(CodeTimer, "---TOTAL Checking Selected Objects", 1);

            if (ChangesMade) { SaveTrackerState(CurrentState); }
            Utility.TimeCodeExecution(CodeTimer, "Commiting Save State", 1);
            UpdateUI();
            Utility.TimeCodeExecution(CodeTimer, "Updating UI", -1);
            Utility.TimeCodeExecution(FullCodeTimer, "Full Check Action", -1);
        }

        private bool HandleUnassignedChecks(IEnumerable<object> Checks, LogicObjects.TrackerInstance Instance)
        {
            CheckItemForm checkItemForm = new CheckItemForm(Checks, Instance);
            checkItemForm.ShowDialog();
            return true;
        }
        private bool HandleUnassignedVariables(IEnumerable<object> Checks, LogicObjects.TrackerInstance Instance)
        {
            VariableInputWindow variableInputWindow = new VariableInputWindow(Checks, Instance);
            variableInputWindow.ShowDialog();
            return true;
        }

        private void CHKShowAll_CheckedChanged(object sender, EventArgs e)
        {
            PrintToListBox();
        }

        private void UpdateUndoList(string State)
        {
            InstanceContainer.RedoStringList.Clear();
            InstanceContainer.UndoStringList.Add(State);
            redoToolStripMenuItem.Enabled = InstanceContainer.RedoStringList.Any();
            undoToolStripMenuItem.Enabled = InstanceContainer.UndoStringList.Any();
        }

        //Other

        private void SaveTrackerState(string PreviousState = null)
        {
            if (PreviousState == null) { UpdateUndoList(Utf8Json.JsonSerializer.ToJsonString(InstanceContainer.Instance)); }
            else { UpdateUndoList(PreviousState); }
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
                    SavetoolStripMenuItem1_Click(SavetoolStripMenuItem1, null);
                    return true;
                default:
                    return false;
            }
        }

        private void BTNSetItem_Click(object sender, EventArgs e)
        {
            if (TXTLocSearch.Text == "toggledev")
            {
                if (Testing.IsDevUser()) { doDevCheck(false); }
                else if (Testing.StandardUser()) { doDevCheck(true); }
                TXTLocSearch.Text = "";
                UpdateUI();
                return;
            }
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
                        SavetoolStripMenuItem1_Click(SavetoolStripMenuItem1, e);
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
            if (Control.ModifierKeys == Keys.Control)
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
            if (Control.ModifierKeys == Keys.Control &&
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
        }

        private void BTNFindPath_Click(object sender, EventArgs e)
        {
            if (CMBStart.SelectedIndex < 0 || CMBEnd.SelectedIndex < 0 || CMBStart.SelectedItem.ToString() == CMBEnd.SelectedItem.ToString()) { return; }
            LBPathFinder.Font = WinFormUtils.GetFontFromString(InstanceContainer.Instance.StaticOptions.OptionFile.WinformData.FormFont);
            LBPathFinder.ItemHeight = Convert.ToInt32(LBPathFinder.Font.Size * 1.8);
            LBPathFinder.DataSource = new List<string> { "Finding path" };
            MainInterfacepathfinder = new Pathfinder();
            MainInterfacepathfinder.FindPath(InstanceContainer.Instance, (string)CMBStart.SelectedItem, (string)CMBEnd.SelectedItem, new List<string>(), new Dictionary<string, string>());
            MainInterfacepathfinder.FinalPath = MainInterfacepathfinder.FinalPath.OrderBy(x => x.Count).ToList();
            if (!MainInterfacepathfinder.FinalPath.Any()) { LBPathFinder.DataSource = new List<string> { "No Path Found" }; }
            else { PrintPaths(); }
        }

        private void PrintPaths()
        {
            List<object> Results = new List<object>();
            int index = 0;
            foreach (var i in MainInterfacepathfinder.FinalPath)
            {
                if (index >= 20) { break; }
                Results.Add(WinFormUtils.CreateDivider(LBPathFinder));
                Results.Add(new Pathfinder.PathfinderPath { Display = $"Path {index+1}: {i.Count - 1} Stops", Index = index });
                var PathList = i.Select(x => new Pathfinder.PathfinderPath { Display = x.Value == "" ? x.Key : $"{x.Key} => {x.Value}", Index = index });
                Results = Results.Concat(PathList).ToList();
                index++;
            }
            LBPathFinder.DataSource = Results;
        }

        private void LBPathFinder_DoubleClick(object sender, EventArgs e)
        {
            if (((ListBox)sender).SelectedItem is Pathfinder.PathfinderPath path)
            {
                if (path.Focused) { PrintPaths(); }
                else
                {
                    List<object> Results = new List<object>();
                    Results.Add(new Pathfinder.PathfinderPath { Display = $"Path {path.Index + 1}: {MainInterfacepathfinder.FinalPath[path.Index].Count - 1} Stops", Index = path.Index });
                    Results = Results.Concat(MainInterfacepathfinder.FinalPath[path.Index].Select(x => new Pathfinder.PathfinderPath { Display = x.Value == "" ? x.Key : $"{x.Key} => {x.Value}", Index = path.Index, Focused = true })).ToList();
                    LBPathFinder.DataSource = Results.ToList();
                }
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

        private void ExportCheckedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstanceContainer.LogicRecreation.SaveTrackerState(InstanceContainer);
            MessageBox.Show("Instance Saved");
        }

        private void ImportCheckedToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            InstanceContainer.LogicRecreation.LoadTrackerState(InstanceContainer);
            UpdateUI();
        }

        private void spoilerLogToolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SpoilerLogLookUp spoilerLogLookUp = new SpoilerLogLookUp(InstanceContainer.Instance);
            spoilerLogLookUp.Show();
        }

        private void viewAsUserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Testing.DebugMode = MiscData.DebugMode.UserView;
            UpdateUI();
        }

        private void devToolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(!Testing.UserView()) { return; }
            Testing.DebugMode = MiscData.DebugMode.Debugging;
            UpdateUI();
        }
    }
}
