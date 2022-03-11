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
            if (sender == undoToolStripMenuItem)
            {
                string CurrentState = Utf8Json.JsonSerializer.ToJsonString(CurrentTrackerInstance);
                CurrentTrackerInstance = LogicObjects.TrackerInstance.FromJson(UndoStringList[^1]);
                RedoStringList.Add(CurrentState);
                UndoStringList.RemoveAt(UndoStringList.Count - 1);
            }
            else if (sender == redoToolStripMenuItem)
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
                LogicCalculation.CalculateLogic(CurrentTrackerInstance);
                UpdateUI();
                UpdateDynamicUserOptions();
            }
        }

        //Menu Strip => Options

        private void ToggleRandomizerOption_Click(object sender, EventArgs e, OptionData.TrackerOption Option, Object Selection = null)
        {
            if (Selection == null)
            {
                Option.ToggleOption();
            }
            else
            {
                Option.CurrentValue = (string)Selection;
            }
            LogicCalculation.CalculateLogic(CurrentTrackerInstance);
            UpdateUI();
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
                    Filter = "MMR Text Spoiler Log|*.txt",
                    Title = "Load Text Spoiler Log"
                };
                openFileDialog.ShowDialog();
                if (openFileDialog.FileName != "" && File.Exists(openFileDialog.FileName))
                {
                    SaveTrackerState();
                    var ToUpdate = new List<ListBox> { LBCheckedLocations, LBValidEntrances, LBValidLocations };
                    foreach(var i in ToUpdate) { i.Items.Clear(); i.Items.Add("Importing Spoiler Log"); i.Items.Add("Please Wait..."); i.Items.Add("Reading Spoiler Log.."); i.Refresh(); }
                    CurrentTrackerInstance.SpoilerLog = SpoilerLogTools.ReadSpoilerLog(File.ReadAllLines(openFileDialog.FileName));
                    foreach (var i in ToUpdate) { i.Items.Add("Applying Settings..."); i.Refresh(); }
                    SpoilerLogTools.ApplyMMRandoSettings(CurrentTrackerInstance, CurrentTrackerInstance.SpoilerLog);
                    foreach (var i in ToUpdate) { i.Items.Add("Applying Spoiler Data..."); i.Refresh(); }
                    SpoilerLogTools.ApplyMMRandoSpoilerLog(CurrentTrackerInstance, CurrentTrackerInstance.SpoilerLog);
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
            Testing.CodeTesting(CurrentTrackerInstance);

        }

        //ListBoxes

        private void LBValidLocations_DrawItem(object sender, DrawItemEventArgs e)
        {
            var LB = sender as ListBox;
            if (e.Index < 0) { return; }
            e.DrawBackground();
            Font F = WinFormUtils.GetFontFromString(CurrentTrackerInstance.StaticOptions.WinformData.FormFont);
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
            Font F = WinFormUtils.GetFontFromString(CurrentTrackerInstance.StaticOptions.WinformData.FormFont);
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

                label3.Location = new Point(FormHalfWidth + locX, locY + 2);
                BTNSetEntrance.Location = new Point(FormWidth - BTNSetEntrance.Width, MenuHeight + 1);
                TXTEntSearch.Location = new Point(FormHalfWidth + locX, locY + label3.Height + 6);
                TXTEntSearch.Width = FormHalfWidth - 2;
                LBValidEntrances.Location = new Point(FormHalfWidth + locX, locY + label3.Height + TXTEntSearch.Height + 8);
                LBValidEntrances.Width = FormHalfWidth - 2;
                LBValidEntrances.Height = FormHalfHeight - label3.Height - TXTEntSearch.Height - 14;

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
                if (CurrentTrackerInstance.StaticOptions.WinformData.HorizontalLayout)
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
                    TXTCheckedSearch.Location = new Point(FormHalfWidth + locX, locY + label3.Height + 6);
                    TXTCheckedSearch.Width = FormHalfWidth - 2;
                    LBCheckedLocations.Location = new Point(FormHalfWidth + locX, locY + label3.Height + TXTEntSearch.Height + 8);
                    LBCheckedLocations.Width = FormHalfWidth - 2;
                    LBCheckedLocations.Height = FormHeight - label3.Height - TXTEntSearch.Height - 14;
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
            var UpperRightLBL = label3;
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
                i.Font = WinFormUtils.GetFontFromString(CurrentTrackerInstance.StaticOptions.WinformData.FormFont);
                i.ItemHeight = Convert.ToInt32(i.Font.Size * 1.8);
                i.BeginUpdate();
            }

            Dictionary<string, int> Groups = Utility.GetCategoriesFromFile(CurrentTrackerInstance);
            var DataSets = TrackerDataHandeling.PopulateDataSets(CurrentTrackerInstance);

            if (ToUpdate.Contains(LBValidLocations)) 
            {
                var Entries = TrackerDataHandeling.PrintToLocationList(Groups, DataSets, WinFormUtils.CreateDivider(LBValidLocations), CurrentTrackerInstance, TXTLocSearch.Text, CHKShowAll.Checked, out int x, out int y);
                lblAvailableLocation.Text = $"Available Locations: {y}" + (x != y ? $"/{x}" : "");
                foreach (var i in Entries) { LBValidLocations.Items.Add(i); }
                LBValidLocations.TopIndex = lbLocTop; 
            }
            if (ToUpdate.Contains(LBValidEntrances)) 
            {
                //PrintToEntranceList(Groups, DataSets);
                LBValidEntrances.TopIndex = lbEntTop; 
            }
            if (ToUpdate.Contains(LBCheckedLocations)) 
            {
                var Entries = TrackerDataHandeling.PrintToCheckedList(Groups, DataSets, WinFormUtils.CreateDivider(LBCheckedLocations), CurrentTrackerInstance, TXTCheckedSearch.Text, out int x, out int y);
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
            ToolStripMenuItem ListBoxDisplay = new ToolStripMenuItem();
            ListBoxDisplay.Text = "Display In List Box";
            ToolStripComboBox ListBoxDisplayOptions = new ToolStripComboBox();
            ListBoxDisplayOptions.Items.AddRange(new string[] { "None", "Available Locations", "Checked Locations" });
            ListBoxDisplay.Text = "Display In List Box";
            ListBoxDisplay.DropDownItems.Add(ListBoxDisplayOptions);
            RandomizerOptionsToolStripMenuItem1.DropDownItems.Add(ListBoxDisplay);
            RandomizerOptionsToolStripMenuItem1.DropDownItems.Add(new ToolStripSeparator());
            foreach (var i in CurrentTrackerInstance.UserOptions.Values)
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

        //Context Menus

        private void SetCheckPrice(LocationData.LocationObject itemObject)
        {
            StartPosition:
            string input = Interaction.InputBox("Set Check Price", $"{itemObject.GetDictEntry(CurrentTrackerInstance).Name}");
            if (!int.TryParse(input, out int Price)) { goto StartPosition; }
            itemObject.CheckPrice = Price;
            LogicCalculation.CalculateLogic(CurrentTrackerInstance);
            UpdateUI();
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
                    ShowLogic showLogic = new ShowLogic(itemObject.ID, CurrentTrackerInstance);
                    showLogic.Show();
                };

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
                    CheckContextItem.Click += (sender, e) => { HandleItemSelect(new List<object> { listBox.SelectedItem }, MiscData.CheckState.Checked); };
                    //Mark\UnMark Item
                    string MarkFunction = itemObject.CheckState == MiscData.CheckState.Marked ? "UnMark Location" : "Mark Location";
                    ToolStripItem MarkContextItem = contextMenuStrip.Items.Add(MarkFunction);
                    MarkContextItem.Click += (sender, e) => { HandleItemSelect(new List<object> { listBox.SelectedItem }, MiscData.CheckState.Marked); };
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

        //ListboxObject Handeling

        private void HandleItemSelect(List<object> Items, MiscData.CheckState checkState)
        {
            bool ChangesMade = false;
            string CurrentState = Utf8Json.JsonSerializer.ToJsonString(CurrentTrackerInstance);

            List<LocationData.LocationObject> locationObjects = Items.Where(x => x is LocationData.LocationObject).Select(x => x as LocationData.LocationObject).ToList();
            List<LocationData.LocationObject> UncheckedlocationObjects = locationObjects.Where(x => x.CheckState == MiscData.CheckState.Unchecked).ToList();

            foreach (LocationData.LocationObject LocationObject in UncheckedlocationObjects)
            {
                LocationObject.Randomizeditem.Item = LocationObject.GetItemAtCheck(CurrentTrackerInstance);
            }
            var ManualChecks = UncheckedlocationObjects.Where(x => x.Randomizeditem.Item == null);
            if (ManualChecks.Any())
            {
                CheckItemForm checkItemForm = new CheckItemForm(ManualChecks, CurrentTrackerInstance);
                checkItemForm.ShowDialog();
            }

            foreach (LocationData.LocationObject LocationObject in locationObjects)
            {
                var Action = (checkState == MiscData.CheckState.Marked && LocationObject.CheckState == MiscData.CheckState.Marked) ? MiscData.CheckState.Unchecked : checkState;
                if (LocationObject.ToggleChecked(Action, CurrentTrackerInstance)) { ChangesMade = true; }
            }

            //Hints======================================


            List<HintData.HintObject> HintObjects = Items.Where(x => x is HintData.HintObject).Select(x => x as HintData.HintObject).ToList();
            foreach (HintData.HintObject hintObject in HintObjects)
            {
                ChangesMade = true;
                if (hintObject.CheckState == MiscData.CheckState.Unchecked)
                {
                    if (hintObject.SpoilerHintText == null)
                    {
                        string input = Interaction.InputBox("Input Hint Text", hintObject.GetDictEntry(CurrentTrackerInstance).Name);
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

            List<OptionData.TrackerOption> OptionObjects = Items.Where(x => x is OptionData.TrackerOption).Select(x => x as OptionData.TrackerOption).ToList();

            foreach(var i in OptionObjects)
            {
                if (i.IsToggleOption())
                {
                    ToggleRandomizerOption_Click(null, null, i);
                }
            }

            if (ChangesMade)
            {
                SaveTrackerState(CurrentState);
            }

            LogicCalculation.CalculateLogic(CurrentTrackerInstance);

            UpdateUI();


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
    }
}
