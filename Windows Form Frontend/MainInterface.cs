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
        public static LogicObjects.TrackerInstance CurrentTrackerInstance;
        public static List<string> UndoStringList = new();
        public static List<string> RedoStringList = new();
        public static MainInterface CurrentProgram;
        private bool FormIsMaximized = false;
        private bool UnsavedChanges = false;

        public MainInterface()
        {
            InitializeComponent();
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

            Testing.ISDebugging = ((Control.ModifierKeys != Keys.Control) && Debugger.IsAttached);
            Testing.ViewAsUserMode = ((Control.ModifierKeys == Keys.Control) && Debugger.IsAttached);

            UpdateUI();
            WinFormInstanceCreation.ApplyUserPretLogic();
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
            if (sender == undoToolStripMenuItem && UndoStringList.Any())
            {
                string CurrentState = Utf8Json.JsonSerializer.ToJsonString(CurrentTrackerInstance);
                CurrentTrackerInstance = LogicObjects.TrackerInstance.FromJson(UndoStringList[^1]);
                RedoStringList.Add(CurrentState);
                UndoStringList.RemoveAt(UndoStringList.Count - 1);
            }
            else if (sender == redoToolStripMenuItem && RedoStringList.Any())
            {
                string CurrentState = Utf8Json.JsonSerializer.ToJsonString(CurrentTrackerInstance);
                CurrentTrackerInstance = LogicObjects.TrackerInstance.FromJson(RedoStringList[^1]);
                UndoStringList.Add(CurrentState);
                RedoStringList.RemoveAt(RedoStringList.Count - 1);
            }
            Utility.TimeCodeExecution(TimeTotalItemSelect, "Undo/Redo Action", -1);

            LogicCalculation.CalculateLogic(CurrentTrackerInstance);
            UpdateUI();
        }

        //Menu Strip => File

        private void NewToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!PromptSave()) { return; }
            OpenFileDialog fileDialog = new OpenFileDialog();
            var Result = fileDialog.ShowDialog();
            if (Result == DialogResult.Cancel || !File.Exists(fileDialog.FileName)) { return; }
            string Logic = File.ReadAllText(fileDialog.FileName);
            WinFormInstanceCreation.CreateWinFormInstance(Logic);
        }

        private void SavetoolStripMenuItem1_Click(object sender, EventArgs e)
        {
            bool SaveAs = (sender is ToolStripMenuItem item && item == SaveAsToolStripMenuItem1);
            if (File.Exists(References.CurrentSavePath) && !SaveAs)
            {
                UnsavedChanges = false;
                File.WriteAllText(References.CurrentSavePath, CurrentTrackerInstance.ToString());
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "MMRT Save File|*.mmrtsav";
                saveFileDialog.Title = "Save Tracker";
                saveFileDialog.ShowDialog();
                if (saveFileDialog.FileName != "")
                {
                    UnsavedChanges = false;
                    File.WriteAllText(saveFileDialog.FileName, CurrentTrackerInstance.ToString());
                    References.CurrentSavePath = saveFileDialog.FileName;
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
                CurrentTrackerInstance = NewTrackerInstance;
                References.CurrentSavePath = openFileDialog.FileName;
                LogicCalculation.CalculateLogic(CurrentTrackerInstance);
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
                LogicCalculation.CalculateLogic(CurrentTrackerInstance);
                UpdateUI();
            }
        }

        private void logicOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string CurrentState = Utf8Json.JsonSerializer.ToJsonString(CurrentTrackerInstance);
            RandomizedStateEditor editor = new RandomizedStateEditor(CurrentTrackerInstance);
            editor.ShowDialog();
            if (editor.ChangesMade) { SaveTrackerState(CurrentState); }

            LogicCalculation.CalculateLogic(CurrentTrackerInstance);
            UpdateUI();
        }

        //Menu Strip => Tools

        private void importSpoilerLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentTrackerInstance.SpoilerLog == null)
            {
                OpenFileDialog openFileDialog = new()
                {
                    Filter = "MMR Text Spoiler Log|*.txt|Json Spoiler Log (*.json)|*.json",
                    Title = "Load Text Spoiler Log"
                };
                openFileDialog.ShowDialog();
                if (openFileDialog.FileName != "" && File.Exists(openFileDialog.FileName))
                {
                    if (CurrentTrackerInstance.LogicFile.GameCode == "OOTR")
                    {
                        OOTRTools.HandleOOTRSpoilerLog(File.ReadAllText(openFileDialog.FileName), CurrentTrackerInstance);
                        return;
                    }
                    else
                    {
                        SaveTrackerState();
                        var ToUpdate = new List<ListBox> { LBCheckedLocations, LBValidEntrances, LBValidLocations };
                        foreach (var i in ToUpdate) { i.Items.Clear(); i.Items.Add("Importing Spoiler Log"); i.Items.Add("Please Wait..."); i.Items.Add("Reading Spoiler Log.."); i.Refresh(); }
                        CurrentTrackerInstance.SpoilerLog = SpoilerLogTools.ReadSpoilerLog(File.ReadAllLines(openFileDialog.FileName));
                        foreach (var i in ToUpdate) { i.Items.Add("Applying Settings..."); i.Refresh(); }
                        SpoilerLogTools.ApplyMMRandoSettings(CurrentTrackerInstance, CurrentTrackerInstance.SpoilerLog);
                        foreach (var i in ToUpdate) { i.Items.Add("Applying Spoiler Data..."); i.Refresh(); }
                        SpoilerLogTools.ApplyMMRandoSpoilerLog(CurrentTrackerInstance, CurrentTrackerInstance.SpoilerLog);
                    }
                }
            }
            else
            {
                SaveTrackerState();
                SpoilerLogTools.RemoveSpoilerData(CurrentTrackerInstance);
                CurrentTrackerInstance.SpoilerLog = null;
            }
            LogicCalculation.CalculateLogic(CurrentTrackerInstance);
            UpdateUI();
        }

        //Menu Strip => Dev

        private void CodeTestingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MMR_Tracker_V3.OtherGames.OOTRTools.ReadEntranceRefFile(out string Logic, out string Dict);
            WinFormInstanceCreation.CreateWinFormInstance(Logic, Dict);
            UpdateUI();
            Testing.CodeTesting(CurrentTrackerInstance);

        }

        //ListBoxes

        private void LBValidLocations_DrawItem(object sender, DrawItemEventArgs e)
        {
            var LB = sender as ListBox;
            if (e.Index < 0) { return; }
            e.DrawBackground();
            Font F = WinFormUtils.GetFontFromString(CurrentTrackerInstance.StaticOptions.OptionFile.WinformData.FormFont);
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
            Font F = WinFormUtils.GetFontFromString(CurrentTrackerInstance.StaticOptions.OptionFile.WinformData.FormFont);
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

        private void LBValidEntrances_DrawItem(object sender, DrawItemEventArgs e)
        {
            var LB = sender as ListBox;
            if (e.Index < 0) { return; }
            e.DrawBackground();
            Font F = WinFormUtils.GetFontFromString(CurrentTrackerInstance.StaticOptions.OptionFile.WinformData.FormFont);
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
            if (!CurrentTrackerInstance.StaticOptions.OptionFile.WinformData.ShowEntryNameTooltip) { toolTip1.Hide(lb); return; }
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

        public void UpdateUI()
        {
            AlignUIElements();
            FormatMenuItems();
            if (CurrentTrackerInstance == null) { return; }

            this.Text = CurrentTrackerInstance.LogicFile.GameCode + " Tracker" + (UnsavedChanges ? "*" : "");

            CurrentTrackerInstance.EntrancePool.IsEntranceRando = CurrentTrackerInstance.EntrancePool.CheckForRandomEntrances();
            redoToolStripMenuItem.Enabled = RedoStringList.Any();
            undoToolStripMenuItem.Enabled = UndoStringList.Any();

            var AccessableAreas = CurrentTrackerInstance.EntrancePool.AreaList.Values.Where(x => x.ExitsAcessibleFrom > 0).Select(x => x.ID);
            CMBStart.DataSource = AccessableAreas.ToList();
            CMBEnd.DataSource = AccessableAreas.ToList();

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
            if (CurrentTrackerInstance == null)
            {
                SetObjectVisibility(false, false);
                return;
            }
            else if (CurrentTrackerInstance.StaticOptions.EntranceRandoFeatures && CurrentTrackerInstance.EntrancePool.IsEntranceRando)
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
                if (CurrentTrackerInstance.StaticOptions.OptionFile.WinformData.HorizontalLayout)
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
                i.Font = WinFormUtils.GetFontFromString(CurrentTrackerInstance.StaticOptions.OptionFile.WinformData.FormFont);
                i.ItemHeight = Convert.ToInt32(i.Font.Size * 1.8);
                i.BeginUpdate();
            }

            if (ToUpdate.Contains(LBValidLocations)) 
            {
                var Entries = TrackerDataHandeling.PopulateAvailableLocationList(WinFormUtils.CreateDivider(LBValidLocations), CurrentTrackerInstance, TXTLocSearch.Text, CHKShowAll.Checked, out int x, out int y);
                lblAvailableLocation.Text = $"Available Locations: {y}" + (x != y ? $"/{x}" : "");
                foreach (var i in Entries) { LBValidLocations.Items.Add(i); }
                LBValidLocations.TopIndex = lbLocTop; 
            }
            if (ToUpdate.Contains(LBValidEntrances)) 
            {
                var Entries = TrackerDataHandeling.PopulateAvailableEntraceList(WinFormUtils.CreateDivider(LBValidEntrances), CurrentTrackerInstance, TXTEntSearch.Text, CHKShowAll.Checked, out int x, out int y);
                lblAvailableEntrances.Text = $"Available Entrances: {y}" + (x != y ? $"/{x}" : "");
                foreach (var i in Entries) { LBValidEntrances.Items.Add(i); }
                LBValidEntrances.TopIndex = lbEntTop; 
            }
            if (ToUpdate.Contains(LBCheckedLocations)) 
            {
                var Entries = TrackerDataHandeling.PopulateCheckedLocationList(WinFormUtils.CreateDivider(LBCheckedLocations), CurrentTrackerInstance, TXTCheckedSearch.Text, out int x, out int y);
                lblCheckedLocation.Text = $"Checked Locations: {y}" + (x != y ? $"/{x}" : "");
                foreach (var i in Entries) { LBCheckedLocations.Items.Add(i); }
                LBCheckedLocations.TopIndex = lbCheckTop; 
            }

            foreach (var i in ToUpdate) { i.EndUpdate(); }
        }

        public void FormatMenuItems()
        {
            OptionstoolStripMenuItem.Visible = (CurrentTrackerInstance != null);
            undoToolStripMenuItem.Visible = (CurrentTrackerInstance != null);
            redoToolStripMenuItem.Visible = (CurrentTrackerInstance != null);
            refreshToolStripMenuItem.Visible = (CurrentTrackerInstance != null);
            SavetoolStripMenuItem1.Visible = (CurrentTrackerInstance != null);
            spoilerLogToolsToolStripMenuItem.Visible = (CurrentTrackerInstance != null);
            importSpoilerLogToolStripMenuItem.Visible = (CurrentTrackerInstance != null);

            if (CurrentTrackerInstance == null) { return; }

            SaveAsToolStripMenuItem1.Visible = (File.Exists(References.CurrentSavePath));
            importSpoilerLogToolStripMenuItem.Text = (CurrentTrackerInstance.SpoilerLog != null) ? "Remove Spoiler Log" : "Import Spoiler Log";

            //Manage Dev Menus
            devToolsToolStripMenuItem.Visible = Testing.ISDebugging || Testing.ViewAsUserMode;
            devToolsToolStripMenuItem.Text = (Testing.ViewAsUserMode) ? "Run as Dev" : "Dev Options";
            foreach (ToolStripDropDownItem i in devToolsToolStripMenuItem.DropDownItems) { i.Visible = Testing.ISDebugging; }
            viewAsUserToolStripMenuItem.Checked = Testing.ViewAsUserMode;

        }

        private void UpdateDynamicUserOptions()
        {
            RandomizerOptionsToolStripMenuItem1.DropDownItems.Clear();
            //Create List Box Toggle
            ToolStripComboBox ListBoxDisplayOptions = new();
            ListBoxDisplayOptions.Items.AddRange(OptionData.DisplayListBoxes);
            ListBoxDisplayOptions.SelectedIndex = OptionData.DisplayListBoxes.ToList().IndexOf(CurrentTrackerInstance.StaticOptions.ShowOptionsInListBox);
            ListBoxDisplayOptions.SelectedIndexChanged += delegate (object sender, EventArgs e)
            {
                CurrentTrackerInstance.StaticOptions.ShowOptionsInListBox = ((ToolStripComboBox)sender).SelectedItem.ToString();
                PrintToListBox();
            };
            ToolStripMenuItem ListBoxDisplay = new() { Text = "Display In List Box" };
            ListBoxDisplay.DropDownItems.Add(ListBoxDisplayOptions);
            RandomizerOptionsToolStripMenuItem1.DropDownItems.Add(ListBoxDisplay);
            RandomizerOptionsToolStripMenuItem1.DropDownItems.Add(new ToolStripSeparator());
            //Add User Options
            foreach (var i in CurrentTrackerInstance.UserOptions.Values)
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
            foreach(var i in CurrentTrackerInstance.Variables.Values.Where(x => !x.Static))
            {
                ToolStripMenuItem menuItem = new() { Text = i.ToString() };
                menuItem.Click += delegate (object sender, EventArgs e) { HandleItemSelect(new List<LogicDictionaryData.TrackerVariable> { i }, MiscData.CheckState.Checked); };
                RandomizerOptionsToolStripMenuItem1.DropDownItems.Add(menuItem);
            }
        }

        //Context Menus

        private void SetCheckPrice(LocationData.LocationObject itemObject)
        {
            string Identifier = itemObject.GetDictEntry(CurrentTrackerInstance).Name + " Price";
            var PriceContainer = new List<LogicDictionaryData.TrackerVariable>() { new() { ID = Identifier, Value = 0 } };
            VariableInputWindow PriceInput = new(PriceContainer, CurrentTrackerInstance);
            PriceInput.ShowDialog();
            itemObject.CheckPrice = (int)PriceContainer.First().Value;
            LogicCalculation.CalculateLogic(CurrentTrackerInstance);
            UpdateUI();
        }

        private void ShowContextMenu(ListBox listBox)
        {
            ContextMenuStrip contextMenuStrip = new();
            //Refresh List Box
            ToolStripItem RefreshContextItem = contextMenuStrip.Items.Add("Refresh");
            RefreshContextItem.Click += (sender, e) => { PrintToListBox(new List<ListBox> { listBox }); };

            if (listBox.SelectedItem is EntranceData.EntranceRandoExit)
            {
                var exitObject = (listBox.SelectedItem as EntranceData.EntranceRandoExit);
                //Star Item
                string StarFunction = exitObject.Starred ? "UnStar Location" : "Star Location";
                ToolStripItem StarContextItem = contextMenuStrip.Items.Add(StarFunction);
                StarContextItem.Click += (sender, e) => { exitObject.Starred = !exitObject.Starred; PrintToListBox(new List<ListBox> { listBox }); };
                //ShowLogic
                ToolStripItem ShowLogicFunction = contextMenuStrip.Items.Add("Show Logic");
                ShowLogicFunction.Click += (sender, e) =>
                {
                    ShowLogic showLogic = new ShowLogic(CurrentTrackerInstance.EntrancePool.GetLogicNameFromExit(exitObject), CurrentTrackerInstance);
                    showLogic.Show();
                };
                //DevData
                ToolStripItem ShowDevData = contextMenuStrip.Items.Add("Show Dev Data");
                ShowDevData.Click += (sender, e) =>
                {
                    Debug.WriteLine(JsonConvert.SerializeObject(exitObject, Testing._NewtonsoftJsonSerializerOptions));
                };
            }
            else if (listBox.SelectedItem is LocationData.LocationObject)
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
                    ShowLogic showLogic = new ShowLogic(itemObject.ID, CurrentTrackerInstance);
                    showLogic.Show();
                };
                //DevData
                ToolStripItem ShowDevData = contextMenuStrip.Items.Add("Show Dev Data");
                ShowDevData.Click += (sender, e) =>
                {
                    Debug.WriteLine(JsonConvert.SerializeObject(itemObject, Testing._NewtonsoftJsonSerializerOptions));
                    Debug.WriteLine(JsonConvert.SerializeObject(itemObject.GetDictEntry(CurrentTrackerInstance), Testing._NewtonsoftJsonSerializerOptions));
                };

                if (listBox == LBValidLocations)
                {
                    //Check Item
                    ToolStripItem CheckContextItem = contextMenuStrip.Items.Add("Check Location");
                    CheckContextItem.Click += (sender, e) => { HandleItemSelect(new List<object> { listBox.SelectedItem }, MiscData.CheckState.Checked, LB: listBox); };
                    //Mark\UnMark Item
                    string MarkFunction = itemObject.CheckState == MiscData.CheckState.Marked ? "UnMark Location" : "Mark Location";
                    ToolStripItem MarkContextItem = contextMenuStrip.Items.Add(MarkFunction);
                    MarkContextItem.Click += (sender, e) => { HandleItemSelect(new List<object> { listBox.SelectedItem }, MiscData.CheckState.Marked, LB: listBox); };
                    //Set Item Price
                    string SetPriceText = itemObject.CheckPrice > -1 ? "Clear Price" : "Set Price";
                    ToolStripItem SetPrice = contextMenuStrip.Items.Add(SetPriceText);
                    SetPrice.Click += (sender, e) =>
                    {
                        if (itemObject.CheckPrice > -1) { itemObject.CheckPrice = -1; }
                        else { SetCheckPrice(itemObject); }
                    };
                }
                else if (listBox == LBCheckedLocations)
                {
                    //UnCheck Item
                    ToolStripItem CheckContextItem = contextMenuStrip.Items.Add("UnCheck Location");
                    CheckContextItem.Click += (sender, e) => { HandleItemSelect(new List<object> { listBox.SelectedItem }, MiscData.CheckState.Unchecked, LB: listBox); };
                    //Uncheck and Mark Item
                    ToolStripItem MarkContextItem = contextMenuStrip.Items.Add("UnCheck and Mark Location");
                    MarkContextItem.Click += (sender, e) => { HandleItemSelect(new List<object> { listBox.SelectedItem }, MiscData.CheckState.Marked, LB: listBox); };
                }
            }

            if (contextMenuStrip.Items.Count > 0)
            {
                contextMenuStrip.Show(Cursor.Position);
            }
        }

        //ListboxObject Handeling

        private void HandleItemSelect(IEnumerable<object> Items, MiscData.CheckState checkState, bool EnforceMarkAction = false, object LB = null)
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
            Debug.WriteLine("Checking Item===========================================================");
            Stopwatch TotalTime = new Stopwatch();
            Stopwatch FunctionTime = new Stopwatch();
            Utility.TimeCodeExecution(TotalTime);
            Utility.TimeCodeExecution(FunctionTime);

            bool ChangesMade = false;
            string CurrentState = Utf8Json.JsonSerializer.ToJsonString(CurrentTrackerInstance);
            Utility.TimeCodeExecution(FunctionTime, "Saving Current State", 1);

            //Search for valid Object types in the list of selected Objects and sort them into lists
            IEnumerable<LocationData.LocationObject> locationObjects = Items.Where(x => x is LocationData.LocationObject).Select(x => x as LocationData.LocationObject);
            IEnumerable<EntranceData.EntranceRandoExit> ExitObjects = Items.Where(x => x is EntranceData.EntranceRandoExit).Select(x => x as EntranceData.EntranceRandoExit);
            IEnumerable<OptionData.TrackerOption> OptionObjects = Items.Where(x => x is OptionData.TrackerOption).Select(x => x as OptionData.TrackerOption);
            Utility.TimeCodeExecution(FunctionTime, "Sorting Selected Items", 1);

            //If we are performing an uncheck action there should be no unchecked locations in the list and even if there are nothing will be done to them anyway
            //This check is neccessary for the "UnMark Only" action and also provides a bit more efficiency.
            IEnumerable<LocationData.LocationObject> UncheckedlocationObjects = (checkState == MiscData.CheckState.Unchecked) ?
                new List<LocationData.LocationObject>() :
                locationObjects.Where(x => x.CheckState == MiscData.CheckState.Unchecked);
            IEnumerable<EntranceData.EntranceRandoExit> UncheckedExitObjects = (checkState == MiscData.CheckState.Unchecked) ?
                new List<EntranceData.EntranceRandoExit>() :
                ExitObjects.Where(x => x.CheckState == MiscData.CheckState.Unchecked);
            Utility.TimeCodeExecution(FunctionTime, "Getting Unchecked Entries", 1);

            //For any Locations with no randomized item, check if an item can be automatically assigned.
            foreach (LocationData.LocationObject LocationObject in UncheckedlocationObjects)
            {
                LocationObject.Randomizeditem.Item = LocationObject.GetItemAtCheck(CurrentTrackerInstance);
            }
            foreach (EntranceData.EntranceRandoExit ExitObject in UncheckedExitObjects)
            {
                ExitObject.DestinationExit = ExitObject.GetDestinationAtExit(CurrentTrackerInstance);
            }
            Utility.TimeCodeExecution(FunctionTime, "Checking for Randmomized item data", 1);

            //Get Entries that need a value manually assigned and pass them to the "CheckItemForm" to be assigned.
            IEnumerable<object> ManualChecks = UncheckedlocationObjects.Where(x => x.Randomizeditem.Item == null); //Locations with no item
            ManualChecks = ManualChecks.Concat(UncheckedExitObjects.Where(x => x.DestinationExit == null)); //Exits With No Destination
            ManualChecks = ManualChecks.Concat(OptionObjects.Where(x => !x.IsToggleOption())); //Non Toggle Options
            if (ManualChecks.Any())
            {
                TotalTime.Stop();
                CheckItemForm checkItemForm = new CheckItemForm(ManualChecks, CurrentTrackerInstance);
                checkItemForm.ShowDialog();
                ChangesMade = true;
                TotalTime.Start();
            }
            Utility.TimeCodeExecution(FunctionTime, "Manual Check Form", 1);

            //Options======================================
            foreach (var i in OptionObjects.Where(x => x.IsToggleOption())) { i.ToggleOption(); ChangesMade = true; }
            Utility.TimeCodeExecution(FunctionTime, "Toggling Options", 1);
            //Items======================================
            foreach (LocationData.LocationObject LocationObject in locationObjects)
            {
                //When we mark a location, the action is always sent as Marked, but if the location is already marked we should instead Unchecked it unless EnforceMarkAction is true.
                var Action = (checkState == MiscData.CheckState.Marked && LocationObject.CheckState == MiscData.CheckState.Marked) && !EnforceMarkAction ? MiscData.CheckState.Unchecked : checkState;
                if (LocationObject.ToggleChecked(Action, CurrentTrackerInstance)) { ChangesMade = true; }
            }
            Utility.TimeCodeExecution(FunctionTime, "Checking Locations", 1);
            //Exits======================================
            foreach (EntranceData.EntranceRandoExit ExitObject in ExitObjects)
            {
                var Action = (checkState == MiscData.CheckState.Marked && ExitObject.CheckState == MiscData.CheckState.Marked) && !EnforceMarkAction ? MiscData.CheckState.Unchecked : checkState;
                if (ExitObject.ToggleExitChecked(Action, CurrentTrackerInstance)) { ChangesMade = true; }
            }
            Utility.TimeCodeExecution(FunctionTime, "Checking Entrances", 1);

            //Hints======================================
            List<HintData.HintObject> HintObjects = Items.Where(x => x is HintData.HintObject).Select(x => x as HintData.HintObject).ToList();
            List<LogicDictionaryData.TrackerVariable> VariableObjects = Items.Where(x => x is LogicDictionaryData.TrackerVariable).Select(x => x as LogicDictionaryData.TrackerVariable).ToList();

            var UncheckedHintObjects = HintObjects.Where(x => x.CheckState == MiscData.CheckState.Unchecked);
            foreach(var i in UncheckedHintObjects.Where(x => !string.IsNullOrWhiteSpace(x.SpoilerHintText))) { i.HintText = i.SpoilerHintText; }

            IEnumerable<object> UncheckedVariableObjects = UncheckedHintObjects.Where(x => string.IsNullOrWhiteSpace(x.HintText));
            UncheckedVariableObjects = UncheckedVariableObjects.Concat(VariableObjects.Where(x => x.Value is not bool));
            if (UncheckedVariableObjects.Any())
            {
                TotalTime.Stop();
                VariableInputWindow variableInputWindow = new VariableInputWindow(UncheckedVariableObjects, CurrentTrackerInstance);
                variableInputWindow.ShowDialog();
                ChangesMade = true;
                TotalTime.Start();
            }
            foreach(var i in VariableObjects.Where(x => x.Value is bool)) { i.Value = !i.Value; ChangesMade = true; }
            foreach (HintData.HintObject hintObject in HintObjects)
            {
                ChangesMade = true;
                var CheckAction = (checkState == MiscData.CheckState.Marked && hintObject.CheckState == MiscData.CheckState.Marked) && !EnforceMarkAction ? MiscData.CheckState.Unchecked : checkState;
                hintObject.CheckState = CheckAction;
                hintObject.HintText = CheckAction == MiscData.CheckState.Unchecked ? null : hintObject.HintText;
            }
            Utility.TimeCodeExecution(FunctionTime, "Setting Hints", 1);

            //Cleanup======================================

            if (ChangesMade)
            {
                SaveTrackerState(CurrentState);
                Utility.TimeCodeExecution(FunctionTime, "Committng Save State", 1);
                LogicCalculation.CalculateLogic(CurrentTrackerInstance);
                Utility.TimeCodeExecution(FunctionTime, "Calculating Logic", 1);
                if (checkState == MiscData.CheckState.Checked && CurrentTrackerInstance.StaticOptions.AutoCheckCoupleEntrances && !CurrentTrackerInstance.StaticOptions.DecoupleEntrances && LogicCalculation.CheckEntrancePair(CurrentTrackerInstance)) 
                { 
                    LogicCalculation.CalculateLogic(CurrentTrackerInstance);
                    Utility.TimeCodeExecution(FunctionTime, "Chcking Entrance Pairs", 1);
                }
            }
            UpdateUI();
            Utility.TimeCodeExecution(FunctionTime, "Updating UI", -1);
            Utility.TimeCodeExecution(TotalTime, "Total Check Action", -1);
        }

        private void CHKShowAll_CheckedChanged(object sender, EventArgs e)
        {
            PrintToListBox();
        }

        private void UpdateUndoList(string State)
        {
            RedoStringList.Clear();
            UndoStringList.Add(State);
            redoToolStripMenuItem.Enabled = RedoStringList.Any();
            undoToolStripMenuItem.Enabled = UndoStringList.Any();
        }

        //Other

        private void SaveTrackerState(string PreviousState = null)
        {
            if (PreviousState == null) { UpdateUndoList(Utf8Json.JsonSerializer.ToJsonString(CurrentTrackerInstance)); }
            else { UpdateUndoList(PreviousState); }
            UnsavedChanges = true;
        }

        public bool PromptSave()
        {
            if (CurrentTrackerInstance == null || !UnsavedChanges) { return true; }
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
            StaticOptionSelect staticOptionSelect = new StaticOptionSelect(CurrentTrackerInstance);
            staticOptionSelect.ShowDialog();
            UpdateUI();
        }

        private void BTNFindPath_Click(object sender, EventArgs e)
        {
            if (CMBStart.SelectedIndex < 0 || CMBEnd.SelectedIndex < 0) { return; }
            LBPathFinder.DataSource = new List<string> { "Finding path" };
            Pathfinder pathfinder = new Pathfinder();
            pathfinder.FindPath(CurrentTrackerInstance, (string)CMBStart.SelectedItem, (string)CMBEnd.SelectedItem, new List<string>(), new Dictionary<string, string>());
            if (pathfinder.FinalPath == null)
            {
                LBPathFinder.DataSource = new List<string> { "No Path Found" };
            }
            else
            {
                var PathList = pathfinder.FinalPath.Select(x => $"{x.Key} => {x.Value}");
                LBPathFinder.DataSource = PathList;
            }
        }
    }
}
