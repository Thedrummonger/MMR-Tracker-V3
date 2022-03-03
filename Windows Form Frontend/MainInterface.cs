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
    public partial class MainInterface : Form
    {
        public static MMR_Tracker_V3.LogicObjects.TrackerInstance MainUITrackerInstance = new LogicObjects.TrackerInstance();

        public static MainInterface CurrentProgram;

        private bool FormIsMaximized = false;

        public MainInterface()
        {
            InitializeComponent();
        }

        private void MainInterface_Load(object sender, EventArgs e)
        {
            AlignUI();
        }

        private void AlignUI()
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

            if (MainUITrackerInstance.LogicFile == null || MainUITrackerInstance.LogicFile.Version < 0)
            {
                Debug.WriteLine("InstanceNull");
                SetObjectVisibility(false, false);
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
            //PrintToListBox();
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
            AlignUI();
        }

        private void MainInterface_Resize(object sender, EventArgs e)
        {
            //Maximizing and unmaximizing does not trigger ResizeEnd which should be used normally since it doesn't constantly run while resizing.
            //so run this code only if the form becomes maximized or becomes un maximized.
            if (WindowState == FormWindowState.Maximized)
            {
                AlignUI();
                FormIsMaximized = true;
            }
            else if (FormIsMaximized)
            {
                AlignUI();
                FormIsMaximized = false;
            }
        }

        private void PrintToListBox(List<ListBox> ToUpdate = null)
        {
            if (ToUpdate == null) { ToUpdate = new List<ListBox> { LBCheckedLocations, LBValidEntrances, LBValidLocations }; }

            foreach (var i in ToUpdate)
            {
                i.Items.Clear();
                i.ItemHeight = Convert.ToInt32(WinFormUtils.GetFontFromString(MainUITrackerInstance.Options.WinformData.FormFont).Size * 1.8);
            }

            Dictionary<string, int> Groups = Utility.GetCategoriesFromFile(MainUITrackerInstance);

            var DataSets = TrackerDataHandeling.PopulateDataSets(MainUITrackerInstance);

            var AvailableLocations = DataSets.AvailableLocations
                .OrderBy(x => (Groups.ContainsKey(x.UIData.LocationArea.ToLower().Trim()) ? Groups[x.UIData.LocationArea.ToLower().Trim()] : DataSets.AvailableLocations.Count() + 1))
                .ThenBy(x => x.UIData.LocationArea)
                .ThenBy(x => x.UIData.DisplayName).ToList();
            var CheckedLocations = DataSets.CheckedLocations
                .OrderBy(x => (Groups.ContainsKey(x.UIData.LocationArea.ToLower().Trim()) ? Groups[x.UIData.LocationArea.ToLower().Trim()] : DataSets.CheckedLocations.Count() + 1))
                .ThenBy(x => x.UIData.LocationArea)
                .ThenBy(x => x.UIData.DisplayName).ToList();

            string CurrentLocation = "";
            foreach(var i in AvailableLocations)
            {
                if (!ToUpdate.Contains(LBValidLocations)) { break; }

                i.UIData.DisplayName = i.UIData.LocationName ?? i.LogicData.Id;
                if (i.TrackerData.CheckState == MiscData.CheckState.Marked)
                {
                    string RandomizedItemDisplay = i.TrackerData.RandomizedItem;
                    var RandomizedItem = MainUITrackerInstance.ItemPool.SearchPoolForMatchingItem(i.TrackerData.RandomizedItem);
                    if (RandomizedItem != null) { RandomizedItemDisplay = RandomizedItem.ItemName ?? RandomizedItem.Id; }
                    i.UIData.DisplayName += $": {RandomizedItemDisplay}";
                }
                if (!Utility.FilterSearch(i, TXTLocSearch.Text, i.UIData.DisplayName)) { continue; }

                if (CurrentLocation != i.UIData.LocationArea)
                {
                    if (LBValidLocations.Items.Count > 0) { LBValidLocations.Items.Add(WinFormUtils.CreateDivider(LBValidLocations)); }
                    LBValidLocations.Items.Add(i.UIData.LocationArea.ToUpper() + ":");
                    CurrentLocation = i.UIData.LocationArea;
                }
                LBValidLocations.Items.Add(i);
            }
            CurrentLocation = "";
            foreach (var i in CheckedLocations)
            {
                if (!ToUpdate.Contains(LBCheckedLocations)) { break; }

                string RandomizedItemDisplay = i.TrackerData.RandomizedItem;
                var RandomizedItem = MainUITrackerInstance.ItemPool.SearchPoolForMatchingItem(i.TrackerData.RandomizedItem);
                if (RandomizedItem != null) { RandomizedItemDisplay = RandomizedItem.ItemName ?? RandomizedItem.Id; }
                i.UIData.DisplayName = $"{RandomizedItemDisplay}: {i.UIData.LocationName ?? i.LogicData.Id}";

                if (!Utility.FilterSearch(i, TXTCheckedSearch.Text, i.UIData.DisplayName)) { continue; }

                if (CurrentLocation != i.UIData.LocationArea)
                {
                    if (LBCheckedLocations.Items.Count > 0) { LBCheckedLocations.Items.Add(WinFormUtils.CreateDivider(LBValidLocations)); }
                    LBCheckedLocations.Items.Add(i.UIData.LocationArea.ToUpper() + ":");
                    CurrentLocation = i.UIData.LocationArea;
                }
                LBCheckedLocations.Items.Add(i);
            }

            if (DataSets.LocalObtainedItems.Any())
                Debug.WriteLine($"Items Aquired Locally====================================");
            foreach (var i in DataSets.LocalObtainedItems)
            {
                Debug.WriteLine($"{i.ItemName} X{i.AmountAquiredLocally}");
            }
            if (DataSets.StartingItems.Any())
                Debug.WriteLine($"Items Aquired Locally====================================");
            foreach (var i in DataSets.StartingItems)
            {
                Debug.WriteLine($"{i.ItemName} X{i.AmountInStartingpool}");
            }
            if (DataSets.OnlineObtainedItems.Any())
                Debug.WriteLine($"Items Aquired Online====================================");
            foreach (var i in DataSets.OnlineObtainedItems)
            {
                Debug.WriteLine($"{i.ItemName}");
                foreach(var j in i.AmountAquiredOnline)
                {
                    Debug.WriteLine($"Player {j.Key}: X{j.Value}");
                }
            }

        }

        private void NewToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.ShowDialog();
            string Logic = File.ReadAllText(fileDialog.FileName);

            WinFormInstanceCreation.CreateWinFormInstance(Logic);
            LogicCalculation.CalculateLogic(MainUITrackerInstance);
            AlignUI();
            PrintToListBox();
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
                if (item.TrackerData.CheckState == MiscData.CheckState.Marked && !item.TrackerData.Available && item.UIData.Starred) 
                { F = new Font(F.FontFamily, F.Size, FontStyle.Bold | FontStyle.Strikeout); }
                else if (item.UIData.Starred) 
                { F = new Font(F.FontFamily, F.Size, FontStyle.Bold); }
                else if (item.TrackerData.CheckState == MiscData.CheckState.Marked && !item.TrackerData.Available) 
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
                if (!item.TrackerData.Available && item.UIData.Starred)
                { F = new Font(F.FontFamily, F.Size, FontStyle.Bold | FontStyle.Strikeout); }
                else if (item.UIData.Starred)
                { F = new Font(F.FontFamily, F.Size, FontStyle.Bold); }
                else if (!item.TrackerData.Available)
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
                this.ActiveControl = LB;
            }
        }

        private void HandleItemSelect(List<object> Items, MiscData.CheckState checkState)
        {
            List<LocationData.LocationObject> ManualChecks = new List<LocationData.LocationObject>();
            foreach (LocationData.LocationObject LocationObject in Items.Where(x => x is LocationData.LocationObject))
            {
                if (LocationObject.TrackerData.CheckState != MiscData.CheckState.Unchecked) { continue; }
                if (LocationObject.TrackerData.RandomizedItem == null)
                {
                    LocationObject.TrackerData.RandomizedItem = LocationObject.TrackerData.GetItemAtCheck();
                    if (LocationObject.TrackerData.RandomizedItem == null)
                    {
                        ManualChecks.Add(LocationObject);
                    }
                }
            }

            if (ManualChecks.Any())
            {
                CheckItemForm checkItemForm = new CheckItemForm(ManualChecks, MainUITrackerInstance);
                checkItemForm.ShowDialog();
            }

            foreach (LocationData.LocationObject LocationObject in Items.Where(x => x is LocationData.LocationObject))
            {
                var Action = (checkState == MiscData.CheckState.Marked && LocationObject.TrackerData.CheckState == MiscData.CheckState.Marked) ? MiscData.CheckState.Unchecked : checkState;
                LocationObject.TrackerData.ToggleChecked(Action, MainUITrackerInstance);
            }

            LogicCalculation.CalculateLogic(MainUITrackerInstance);
            PrintToListBox();

        }

        private void TXTLocSearch_TextChanged(object sender, EventArgs e)
        {
            List<ListBox> ToUpdate = new List<ListBox>();
            if (sender == TXTLocSearch) { ToUpdate.Add(LBValidLocations); }
            if (sender == TXTEntSearch) { ToUpdate.Add(LBValidEntrances); }
            if (sender == TXTCheckedSearch) { ToUpdate.Add(LBCheckedLocations); }
            PrintToListBox(ToUpdate);
        }
    }
}
