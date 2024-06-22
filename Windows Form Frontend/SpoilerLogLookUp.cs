using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TDMUtils;

namespace Windows_Form_Frontend
{
    public partial class SpoilerLogLookUp : Form
    {
        public MMR_Tracker_V3.TrackerObjects.InstanceData.TrackerInstance _instance;
        public Dictionary<string, PlaythroughGenerator.PlaythroughObject> SpoilerLookupPlaythrough = null;

        SeedCheckMode seedCheckMode = SeedCheckMode.view;
        List<MiscData.StandardListBoxItem> SeedCheckResults = new List<MiscData.StandardListBoxItem>();
        List<MiscData.StandardListBoxItem> SeedCheckRequiredItems = new List<MiscData.StandardListBoxItem>();
        List<MiscData.StandardListBoxItem> SeedCheckIgnoredLocations = new List<MiscData.StandardListBoxItem>();
        public enum SeedCheckMode
        {
            addReq,
            addIgnore,
            view
        }
        public SpoilerLogLookUp(MMR_Tracker_V3.TrackerObjects.InstanceData.TrackerInstance instance)
        {
            InitializeComponent();
            _instance = instance;
        }

        private void SpoilerLogLookUp_Load(object sender, EventArgs e)
        {
            PopulateWinConCMB();
            PopulateSpoilerLogList();
            listBox1_SelectedValueChanged(sender, e);
            SetDefaultWinCon();
            UpdateSeedChckUI();
            if (_instance.SpoilerLog?.Playthrough is not null)
            {
                SpoilerLookupPlaythrough = _instance.SpoilerLog.Playthrough;
            }
            lbRequiredItems.Font = _instance.StaticOptions.OptionFile.GetFont();
            LBIgnoredItems.Font = _instance.StaticOptions.OptionFile.GetFont();
            lbObtainable.Font = _instance.StaticOptions.OptionFile.GetFont();
            lbSpoilerLookupItems.Font = _instance.StaticOptions.OptionFile.GetFont();

        }

        private void SetDefaultWinCon()
        {
            var LogicItemData = _instance.GetLogicItemData(_instance.LogicDictionary.WinCondition);
            if (LogicItemData.Object is null) { return; }
            foreach (var i in cmbWinCon.Items)
            {
                if (i is MiscData.StandardListBoxItem LBI)
                {
                    if (LogicItemData.Object is ItemData.ItemObject && LBI.Tag is ItemData.ItemObject IO && IO.ID == LogicItemData.CleanID)
                    {
                        cmbWinCon.SelectedItem = i;
                        break;
                    }
                    else if (LogicItemData.Object is LocationData.LocationObject && LBI.Tag is LocationData.LocationObject LO && LO.ID == LogicItemData.CleanID)
                    {
                        cmbWinCon.SelectedItem = i;
                        break;
                    }
                    else if (LogicItemData.Object is MacroObject && LBI.Tag is MacroObject MO && MO.ID == LogicItemData.CleanID)
                    {
                        cmbWinCon.SelectedItem = i;
                        break;
                    }
                    else if (LogicItemData.Object is EntranceData.EntranceRandoArea && LBI.Tag is EntranceData.EntranceRandoArea AO && AO.ID == LogicItemData.CleanID)
                    {
                        cmbWinCon.SelectedItem = i;
                        break;
                    }
                }
            }

            if (LogicItemData.Object is ItemData.ItemObject ItemObject)
            {
                string displayName = ItemObject.GetDictEntry().Name ?? ItemObject.ID;
                SeedCheckRequiredItems.Add(new MiscData.StandardListBoxItem { Display = displayName, Tag = ItemObject.ID });
            }
            else if (LogicItemData.Object is MacroObject MacroObject)
            {
                string displayName = MacroObject.GetDictEntry().Name ?? MacroObject.ID;
                SeedCheckRequiredItems.Add(new MiscData.StandardListBoxItem { Display = displayName, Tag = MacroObject.ID });
            }
        }

        private Dictionary<string, int> GetDuplicateItemNames(IEnumerable<ItemData.ItemObject> Items)
        {
            Dictionary<string, int> DisplayCounts = new Dictionary<string, int>();
            foreach (var i in Items)
            {
                string Dis = i.GetDictEntry().GetName();
                if (!DisplayCounts.ContainsKey(Dis)) { DisplayCounts[Dis] = 0; }
                DisplayCounts[Dis]++;
            }
            return DisplayCounts;
        }

        #region PlaythroughGenerator
        private void PopulateWinConCMB()
        {
            cmbWinCon.Items.Clear();
            var Items = _instance.ItemPool.Values.Where(x => SearchStringParser.FilterSearch(_instance, x, txtWinConFilter.Text, x.GetDictEntry().GetName())).OrderBy(x => x.GetDictEntry().GetName());
            var Locations = _instance.LocationPool.Values.Where(x => SearchStringParser.FilterSearch(_instance, x, txtWinConFilter.Text, x.GetDictEntry().GetName())).OrderBy(x => x.GetDictEntry().GetName());
            var Macros = _instance.MacroPool.Values.Where(x => SearchStringParser.FilterSearch(_instance, x, txtWinConFilter.Text, x.GetDictEntry().Name ?? x.ID)).OrderBy(x => x.GetDictEntry().Name ?? x.ID);
            var Areas = _instance.AreaPool.Values.Where(x => SearchStringParser.FilterSearch(_instance, x, txtWinConFilter.Text, x.ID)).OrderBy(x => x.ID);

            int CMBWidth = cmbWinCon.DropDownWidth;
            var Graphic = cmbWinCon.CreateGraphics();

            if (Items.Any())
            {
                cmbWinCon.Items.Add(WinFormUtils.CreateDivider(cmbWinCon, "Items"));
                Dictionary<string, int> DisplayCounts = GetDuplicateItemNames(_instance.ItemPool.Values);
                foreach (var i in Items)
                {
                    string Dis = i.GetDictEntry().GetName();
                    if (DisplayCounts.ContainsKey(Dis) && DisplayCounts[Dis] > 1) { Dis = $"{Dis} [{i.ID}]"; }
                    cmbWinCon.Items.Add(new MiscData.StandardListBoxItem { Display = Dis, Tag = i });
                    if (Graphic.MeasureString(Dis, cmbWinCon.Font).Width > CMBWidth) { CMBWidth = (int)Graphic.MeasureString(Dis, cmbWinCon.Font).Width; }
                }
            }

            if (Locations.Any())
            {
                cmbWinCon.Items.Add(WinFormUtils.CreateDivider(cmbWinCon, "Locations"));
                foreach (var i in Locations)
                {
                    string Dis = i.GetDictEntry().GetName();
                    cmbWinCon.Items.Add(new MiscData.StandardListBoxItem { Display = Dis, Tag = i });
                    if (Graphic.MeasureString(Dis, cmbWinCon.Font).Width > CMBWidth) { CMBWidth = (int)Graphic.MeasureString(Dis, cmbWinCon.Font).Width; }
                }
            }

            if (Macros.Any())
            {
                cmbWinCon.Items.Add(WinFormUtils.CreateDivider(cmbWinCon, "Macros"));
                foreach (var i in Macros)
                {
                    string Dis = i.GetDictEntry().Name ?? i.ID;
                    cmbWinCon.Items.Add(new MiscData.StandardListBoxItem { Display = Dis, Tag = i });
                    if (Graphic.MeasureString(Dis, cmbWinCon.Font).Width > CMBWidth) { CMBWidth = (int)Graphic.MeasureString(Dis, cmbWinCon.Font).Width; }
                }
            }

            if (Areas.Any())
            {
                cmbWinCon.Items.Add(WinFormUtils.CreateDivider(cmbWinCon, "Areas"));
                foreach (var i in Areas)
                {
                    string Dis = i.ID;
                    cmbWinCon.Items.Add(new MiscData.StandardListBoxItem { Display = Dis, Tag = i });
                    if (Graphic.MeasureString(Dis, cmbWinCon.Font).Width > CMBWidth) { CMBWidth = (int)Graphic.MeasureString(Dis, cmbWinCon.Font).Width; }
                }
            }
            cmbWinCon.DropDownWidth = CMBWidth;
        }

        private void txtWinConFilter_TextChanged(object sender, EventArgs e)
        {
            PopulateWinConCMB();
        }

        private void btnGenPlaythrough_Click(object sender, EventArgs e)
        {
            PlaythroughGenerator playthroughGenerator = new PlaythroughGenerator(_instance);
            playthroughGenerator.GeneratePlaythrough();
            var SelectedItem = cmbWinCon.SelectedItem as MiscData.StandardListBoxItem;

            bool FullPlaythrough = !chkOnlyImportant.Checked || SelectedItem?.Tag == null;

            if (!FullPlaythrough)
            {
                bool sucess = playthroughGenerator.FilterImportantPlaythrough(SelectedItem.Tag);
                if (!sucess) { MessageBox.Show($"Error {SelectedItem.Tag} Could not be reached this seed"); return; }
            }

            var Result = playthroughGenerator.Playthrough.Where(x => x.Value.Important || (FullPlaythrough && x.Value.CheckType != MiscData.CheckableLocationTypes.macro)).ToDictionary(x => x.Key, x => x.Value);

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "Playthrough";
            dlg.Filter = "Text Files | *.txt";
            dlg.DefaultExt = "txt";
            dlg.ShowDialog();
            if (string.IsNullOrWhiteSpace(dlg.FileName)) { return; }

            File.WriteAllLines(dlg.FileName, playthroughGenerator.CreateReadablePlaythrough(Result));

        }
        #endregion PlaythroughGenerator

        #region SpoilerLogLookup
        private void PopulateSpoilerLogList()
        {
            lbSpoilerLookupItems.Items.Clear();
            var SpoilerChecks = _instance.LocationPool.Values.Where(x => x.GetItemAtCheck() != null);
            List<MiscData.StandardListBoxItem> SpoilerList = new List<MiscData.StandardListBoxItem>();
            foreach (var check in SpoilerChecks)
            {
                MiscData.StandardListBoxItem LBI = new MiscData.StandardListBoxItem() { Tag = check };
                ItemData.ItemObject Item = _instance.GetItemByID(check.GetItemAtCheck());
                if (Item == null)
                {
                    var fakeItem = check.GetItemAtCheck();
                    LBI.Display = check.GetItemAtCheck();
                    if (SearchStringParser.FilterSearch(_instance, fakeItem, textBox2.Text, LBI.Display)) { SpoilerList.Add(LBI); }
                }
                else
                {
                    LBI.Display = Item.GetDictEntry().GetName();
                    if (SearchStringParser.FilterSearch(_instance, Item, textBox2.Text, LBI.Display)) { SpoilerList.Add(LBI); }
                }

            }

            Dictionary<string, List<LocationData.LocationObject>> CleanedEntries = new Dictionary<string, List<LocationData.LocationObject>>();
            foreach (var spoiler in SpoilerList)
            {
                if (!CleanedEntries.ContainsKey(spoiler.Display)) { CleanedEntries[spoiler.Display] = new List<LocationData.LocationObject>(); }
                CleanedEntries[spoiler.Display].Add(spoiler.Tag as LocationData.LocationObject);
            }
            CleanedEntries = CleanedEntries.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            if (CleanedEntries.Any() && chkShowMacros.Checked) { lbSpoilerLookupItems.Items.Add(WinFormUtils.CreateDivider(lbSpoilerLookupItems, "Locations")); }
            foreach (var i in CleanedEntries)
            {
                lbSpoilerLookupItems.Items.Add(new MiscData.StandardListBoxItem { Display = i.Key, Tag = i.Value });
            }

            List<MiscData.StandardListBoxItem> MacroList = new List<MiscData.StandardListBoxItem>();
            if (chkShowMacros.Checked)
            {
                foreach (var check in _instance.MacroPool.Values)
                {
                    MiscData.StandardListBoxItem LBI = new MiscData.StandardListBoxItem() { Tag = check, Display = check.GetDictEntry().Name ?? check.ID };
                    if (SearchStringParser.FilterSearch(_instance, check, textBox2.Text, LBI.Display)) { MacroList.Add(LBI); }
                }
                if (MacroList.Any()) { lbSpoilerLookupItems.Items.Add(WinFormUtils.CreateDivider(lbSpoilerLookupItems, "Macros")); }
                foreach (var i in MacroList.OrderBy(x => x.Display)) { lbSpoilerLookupItems.Items.Add(i); }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            PopulateSpoilerLogList();
            listBox1_SelectedValueChanged(sender, e);
        }

        private void btnArea_Click(object sender, EventArgs e)
        {
            if (lbSpoilerLookupItems.SelectedItem is MiscData.StandardListBoxItem SLI)
            {
                if (SLI.Tag is LocationData.LocationObject LO)
                {
                    MessageBox.Show($"{SLI.Display} Can be found in\n\n-{LO.GetDictEntry().Area}");
                }
                else if (SLI.Tag is List<LocationData.LocationObject> LLO)
                {
                    if (LLO.Count == 1)
                    {
                        MessageBox.Show($"{SLI.Display} Can be found in\n\n-{LLO.First().GetDictEntry().Area}");
                    }
                    else
                    {
                        MessageBox.Show($"{SLI.Display} Can be found in these areas:\n\n-{string.Join("\n-", LLO.Select(x => x.GetDictEntry().Area).Distinct().OrderBy(x => x))}");
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (lbSpoilerLookupItems.SelectedItem is MiscData.StandardListBoxItem SLI)
            {
                if (SpoilerLookupPlaythrough is null)
                {
                    int currInd = lbSpoilerLookupItems.SelectedIndex;
                    int TopInd = lbSpoilerLookupItems.TopIndex;
                    WinFormUtils.PrintMessageToListBox(lbSpoilerLookupItems, "Generating Playthrough Data... \n \n This could take a while but will only happen once.");
                    var TempPlaythrough = new PlaythroughGenerator(_instance);
                    TempPlaythrough.GeneratePlaythrough();
                    SpoilerLookupPlaythrough = TempPlaythrough.Playthrough;
                    PopulateSpoilerLogList();
                    lbSpoilerLookupItems.TopIndex = TopInd;
                    lbSpoilerLookupItems.SelectedIndex = currInd;
                }

                if (SLI.Tag is LocationData.LocationObject LO)
                {
                    if (SpoilerLookupPlaythrough.ContainsKey(LO.ID)) { MessageBox.Show($"{SLI.Display} Can be obtained sphere {SpoilerLookupPlaythrough[LO.ID].sphere}"); }
                    else { MessageBox.Show($"{SLI.Display} Can not be obtained with known items"); }
                }
                else if (SLI.Tag is List<LocationData.LocationObject> LLO)
                {
                    if (LLO.Count == 1)
                    {
                        if (SpoilerLookupPlaythrough.ContainsKey(LLO.First().ID)) { MessageBox.Show($"{SLI.Display} Can be obtained sphere {SpoilerLookupPlaythrough[LLO.First().ID].sphere}"); }
                        else { MessageBox.Show($"{SLI.Display} Can not be obtained with known items"); }
                    }
                    else
                    {
                        List<int> SpheresObtainable = new List<int>();
                        foreach (var i in LLO.Where(x => SpoilerLookupPlaythrough.ContainsKey(x.ID)))
                        {
                            SpheresObtainable.Add(SpoilerLookupPlaythrough[i.ID].sphere);
                        }
                        if (!SpheresObtainable.Any()) { MessageBox.Show($"{SLI.Display} Can not be obtained with known items"); }
                        else { MessageBox.Show($"{SLI.Display} Can be obtained sphere {SpheresObtainable.Min()}"); }
                    }
                }
                else if (SLI.Tag is MacroObject MO)
                {
                    if (SpoilerLookupPlaythrough.ContainsKey(MO.ID)) { MessageBox.Show($"{SLI.Display} Can be obtained sphere {SpoilerLookupPlaythrough[MO.ID].sphere}"); }
                    else if (MO.isTrick() && !MO.TrickEnabled) { MessageBox.Show($"{SLI.Display} Is a trick and is not enabled"); }
                    else { MessageBox.Show($"{SLI.Display} Can not be obtained with known items"); }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (lbSpoilerLookupItems.SelectedItem is MiscData.StandardListBoxItem SLI)
            {
                if (SLI.Tag is LocationData.LocationObject LO)
                {
                    MessageBox.Show($"{SLI.Display} Can be found at\n\n-{LO.GetDictEntry().GetName()}");
                }
                else if (SLI.Tag is List<LocationData.LocationObject> LLO)
                {
                    if (LLO.Count == 1)
                    {
                        MessageBox.Show($"{SLI.Display} Can be found at\n\n-{LLO.First().GetDictEntry().GetName()}");
                    }
                    else
                    {
                        MessageBox.Show($"{SLI.Display} Can be found at these locations:\n\n-{string.Join("\n-", LLO.Select(x => x.GetDictEntry().GetName()).Distinct().OrderBy(x => x))}");
                    }
                }
            }
        }

        private void listBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (lbSpoilerLookupItems.SelectedItem is MiscData.StandardListBoxItem SLI)
            {
                bool EnableLocation = SLI.Tag is LocationData.LocationObject || SLI.Tag is List<LocationData.LocationObject>;
                bool EnableSphere = SLI.Tag is MacroObject || SLI.Tag is List<MacroObject>;
                btnArea.Enabled = EnableLocation;
                btnLocation.Enabled = EnableLocation;
                btnSphere.Enabled = EnableLocation || EnableSphere;
            }
            else
            {
                btnArea.Enabled = false;
                btnLocation.Enabled = false;
                btnSphere.Enabled = false;
            }
        }
        #endregion SpoilerLogLookup

        #region SeedChecker
        private void UpdateSeedChckUI()
        {
            btnAddReq.Text = seedCheckMode == SeedCheckMode.addReq ? "✓" : "Add";
            btnAddIgnored.Text = seedCheckMode == SeedCheckMode.addIgnore ? "✓" : "Add";
            btnCheckSeed.Text = seedCheckMode == SeedCheckMode.view ? "Check Seed" : "Select";
            labelSeedCheckResults.Text = seedCheckMode == SeedCheckMode.addReq ? "Items" : (seedCheckMode == SeedCheckMode.addIgnore ? "Locaitons" : "Results");

            if (seedCheckMode == SeedCheckMode.addReq)
            {
                lbRequiredItems.Items.Clear();
                LBIgnoredItems.Items.Clear();
                lbObtainable.Items.Clear();
                SeedCheckRequiredItems.ForEach(x => lbRequiredItems.Items.Add(x));
                btnAddReq.Visible = true;
                btnAddIgnored.Visible = false;
                LabelSeedCheckItemsNeeded.Text = "Items Needed";
                LabelSeedCheckChecksIgnored.Text = "Adding Needed Items";
                WinFormUtils.PrintMessageToListBox(LBIgnoredItems, "Select Items Needed for seed Completion \n ----->");
                Dictionary<string, int> DisplayCounts = GetDuplicateItemNames(_instance.ItemPool.Values);
                foreach (var i in _instance.ItemPool.Values.OrderBy(x => x.GetDictEntry().GetName()))
                {
                    string displayName = i.GetDictEntry().GetName();
                    if (DisplayCounts.ContainsKey(displayName) && DisplayCounts[displayName] > 1) { displayName = $"{displayName} [{i.ID}]"; }
                    if (SearchStringParser.FilterSearch(_instance, i, txtSeedCheckFilter.Text, displayName))
                        lbObtainable.Items.Add(new MiscData.StandardListBoxItem { Display = displayName, Tag = i.ID });
                }
                foreach (var i in _instance.MacroPool.Values.OrderBy(x => x.GetDictEntry().Name ?? x.ID))
                {
                    string displayName = i.GetDictEntry().Name ?? i.ID;
                    if (SearchStringParser.FilterSearch(_instance, i, txtSeedCheckFilter.Text, displayName))
                        lbObtainable.Items.Add(new MiscData.StandardListBoxItem { Display = displayName, Tag = i.ID });
                }
            }
            if (seedCheckMode == SeedCheckMode.addIgnore)
            {
                lbRequiredItems.Items.Clear();
                LBIgnoredItems.Items.Clear();
                lbObtainable.Items.Clear();
                SeedCheckIgnoredLocations.ForEach(x => LBIgnoredItems.Items.Add(x));
                btnAddReq.Visible = false;
                btnAddIgnored.Visible = true;
                LabelSeedCheckItemsNeeded.Text = "Adding Ignored checks";
                LabelSeedCheckChecksIgnored.Text = "Ignored checks";
                WinFormUtils.PrintMessageToListBox(lbRequiredItems, "Select checks that should be ignored when checking seed \n ----->");
                foreach (var i in _instance.LocationPool.Values.OrderBy(x => x.GetDictEntry().GetName()))
                {
                    string displayName = i.GetDictEntry().GetName();
                    if (SearchStringParser.FilterSearch(_instance, i, txtSeedCheckFilter.Text, displayName))
                        lbObtainable.Items.Add(new MiscData.StandardListBoxItem { Display = displayName, Tag = i.ID });
                }
            }
            if (seedCheckMode == SeedCheckMode.view)
            {
                btnAddReq.Visible = true;
                btnAddIgnored.Visible = true;
                LabelSeedCheckItemsNeeded.Text = "Items Needed";
                LabelSeedCheckChecksIgnored.Text = "Ignored checks";
                lbRequiredItems.Items.Clear();
                LBIgnoredItems.Items.Clear();
                lbObtainable.Items.Clear();
                SeedCheckRequiredItems.ForEach(x => lbRequiredItems.Items.Add(x));
                SeedCheckIgnoredLocations.ForEach(x => LBIgnoredItems.Items.Add(x));
                var ObtainableItems = SeedCheckResults.Where(x => x.Tag is bool obt && obt && SearchStringParser.FilterSearch(_instance, x.Display, txtSeedCheckFilter.Text, x.Display));
                var UnObtainableItems = SeedCheckResults.Where(x => x.Tag is bool obt && !obt && SearchStringParser.FilterSearch(_instance, x.Display, txtSeedCheckFilter.Text, x.Display));

                if (chkShowObtainable.Checked)
                {
                    if (ObtainableItems.Any()) { lbObtainable.Items.Add(WinFormUtils.CreateDivider(lbObtainable, "Obtainable")); }
                    foreach (var i in ObtainableItems) { lbObtainable.Items.Add(i); }
                }
                if (chkShowUnObtainable.Checked)
                {
                    if (UnObtainableItems.Any()) { lbObtainable.Items.Add(WinFormUtils.CreateDivider(lbObtainable, "UnObtainable")); }
                    foreach (var i in UnObtainableItems) { lbObtainable.Items.Add(i); }
                }
            }
        }

        private void btnAddReq_Click(object sender, EventArgs e)
        {
            if (seedCheckMode != SeedCheckMode.addReq) { seedCheckMode = SeedCheckMode.addReq; }
            else if (seedCheckMode == SeedCheckMode.addReq) { seedCheckMode = SeedCheckMode.view; }
            txtSeedCheckFilter.Text = "";

            UpdateSeedChckUI();
        }

        private void btnAddIgnored_Click(object sender, EventArgs e)
        {
            if (seedCheckMode != SeedCheckMode.addIgnore) { seedCheckMode = SeedCheckMode.addIgnore; }
            else if (seedCheckMode == SeedCheckMode.addIgnore) { seedCheckMode = SeedCheckMode.view; }
            txtSeedCheckFilter.Text = "";

            UpdateSeedChckUI();
        }

        private void txtSeedCheckFilter_TextChanged(object sender, EventArgs e)
        {
            UpdateSeedChckUI();
        }

        private void btnCheckSeed_Click(object sender, EventArgs e)
        {
            if (seedCheckMode == SeedCheckMode.view)
            {
                if (sender == lbObtainable) { return; }
                CheckSeed();
                UpdateSeedChckUI();
                return;
            }

            foreach (var item in lbObtainable.SelectedItems)
            {
                if (item is MiscData.StandardListBoxItem SLI)
                {
                    if (seedCheckMode == SeedCheckMode.addReq)
                    {
                        SeedCheckRequiredItems.Add(SLI);
                    }
                    else if (seedCheckMode == SeedCheckMode.addIgnore)
                    {
                        SeedCheckIgnoredLocations.Add(SLI);
                    }
                }
            }
            UpdateSeedChckUI();
        }

        private void CheckSeed()
        {
            lbObtainable.Items.Clear();
            lbObtainable.Items.Add("Generating Playthrough");
            lbObtainable.Items.Add("This may take a while");
            lbObtainable.Update();
            var IgnoredChecks = SeedCheckIgnoredLocations.Select(x => x.Tag).Select(x => x as string).ToList();
            var requiredItems = SeedCheckRequiredItems.Select(x => x.Tag).Select(x => x as string).ToList();
            if (!SeedCheckRequiredItems.Any())
            {
                requiredItems = _instance.ItemPool.Values.Select(x => x.ID).ToList();
            }
            Debug.WriteLine($"Genning Seed Check Playthrough");
            PlaythroughGenerator SeedCheckPlaytrhough = new PlaythroughGenerator(_instance, IgnoredChecks);
            var Yest = _instance.AreaPool;
            var test = SeedCheckPlaytrhough.container.Instance.AreaPool;
            SeedCheckPlaytrhough.GeneratePlaythrough();
            SeedCheckResults.Clear();
            foreach (var i in requiredItems)
            {
                bool ItemObtainable = false;
                var data = _instance.GetLogicItemData(i);
                string Dis = i;
                if (data.Type == MiscData.LogicItemTypes.macro)
                {
                    var MacroObject = data.Object as MacroObject;
                    ItemObtainable = SeedCheckPlaytrhough.Playthrough.Any(x =>
                        x.Value.CheckType == MiscData.CheckableLocationTypes.macro && x.Key == data.CleanID);
                    Dis = MacroObject?.GetDictEntry()?.Name ?? i;
                }
                else if (data.Type == MiscData.LogicItemTypes.item)
                {
                    var ItemObject = data.Object as ItemData.ItemObject;
                    ItemObtainable = SeedCheckPlaytrhough.Playthrough.Any(x =>
                        x.Value.CheckType == MiscData.CheckableLocationTypes.location && 
                        ((LocationData.LocationObject)x.Value.GetCheckableLocation(SeedCheckPlaytrhough.container.Instance)).Randomizeditem.Item == data.CleanID) ||
                        ItemObject.AmountInStartingpool > 0;
                    Dis = ItemObject?.GetDictEntry()?.GetName() ?? i;
                }
                SeedCheckResults.Add(new MiscData.StandardListBoxItem { Display = Dis, Tag = ItemObtainable });
            }
        }

        private void lbRequiredItems_DoubleClick(object sender, EventArgs e)
        {
            if (seedCheckMode == SeedCheckMode.addIgnore) { return; }
            if (lbRequiredItems.SelectedItem is MiscData.StandardListBoxItem SLI)
            {
                SeedCheckRequiredItems.Remove(SLI);
            }
            UpdateSeedChckUI();
        }

        private void LBIgnoredItems_DoubleClick(object sender, EventArgs e)
        {
            if (seedCheckMode == SeedCheckMode.addReq) { return; }
            if (LBIgnoredItems.SelectedItem is MiscData.StandardListBoxItem SLI)
            {
                SeedCheckIgnoredLocations.Remove(SLI);
            }
            UpdateSeedChckUI();
        }
        #endregion SeedChecker

        private void GenerateHint(object sender, EventArgs e)
        {
            if (SpoilerLookupPlaythrough is null)
            {
                int currInd = lbSpoilerLookupItems.SelectedIndex;
                int TopInd = lbSpoilerLookupItems.TopIndex;
                var TempPlaythrough = new PlaythroughGenerator(_instance);
                TempPlaythrough.GeneratePlaythrough();
                SpoilerLookupPlaythrough = TempPlaythrough.Playthrough;
                PopulateSpoilerLogList();
                lbSpoilerLookupItems.TopIndex = TopInd;
                lbSpoilerLookupItems.SelectedIndex = currInd;
            }
            var Source = sender as ToolStripMenuItem;

            var PlaythroughLocations = SpoilerLookupPlaythrough.Where(x => x.Value.CheckType == MiscData.CheckableLocationTypes.location);
            var ImportantLocations = PlaythroughLocations.Where(x => x.Value.Important);
            var AllLocationObj = PlaythroughLocations.Select(x => _instance.LocationPool[x.Key]).ToArray();
            var ImportantLocationsObj = ImportantLocations.Select(x => _instance.LocationPool[x.Key]).ToArray();
            var UnImportantLocations = PlaythroughLocations.Where(x => !x.Value.Important);
            var UnImportantLocationsObj = UnImportantLocations.Select(x => _instance.LocationPool[x.Key]).ToArray();

            if (Source == wayOfTheHeroToolStripMenuItem)
            {
                if (!ImportantLocations.Any()) { MessageBox.Show("Hint could not be Generated!"); return; }
                var WOTHLocation = ImportantLocations.PickRandom();
                var LocationOBJ = _instance.LocationPool[WOTHLocation.Key];
                MessageBox.Show($"{LocationOBJ.GetDictEntry().Area} is Way of the Hero");
            }
            else if (Source == foolishToolStripMenuItem)
            {
                var Areas = _instance.LocationPool.Select(x => x.Value.GetDictEntry().Area).Distinct().ToArray();
                var ImportantAreas = ImportantLocationsObj.Select(x => x.GetDictEntry().Area).Distinct().ToArray();
                var UnimportantAreas = Areas.Where(x => !ImportantAreas.Contains(x));
                if (!UnimportantAreas.Any()) { MessageBox.Show("Hint could not be Generated!"); return; }
                var FoolishArea = UnimportantAreas.PickRandom();
                MessageBox.Show($"{FoolishArea} did not have any items used in the trackers playthough");
            }
            else if (Source == randomLocationToolStripMenuItem)
            {
                var RandomizedLocations = AllLocationObj.Where(x => x.CheckState == MiscData.CheckState.Unchecked && x.IsRandomized() && !string.IsNullOrWhiteSpace(x.Randomizeditem.SpoilerLogGivenItem));
                if (!RandomizedLocations.Any()) { MessageBox.Show("Hint could not be Generated!"); return; }
                var randomLocation = RandomizedLocations.PickRandom();
                string Item = _instance.ItemPool.ContainsKey(randomLocation.Randomizeditem.SpoilerLogGivenItem) ?
                    _instance.ItemPool[randomLocation.Randomizeditem.SpoilerLogGivenItem].GetDictEntry().GetName() :
                    randomLocation.Randomizeditem.SpoilerLogGivenItem;
                MessageBox.Show($"{randomLocation.GetDictEntry().GetName()} Contained {Item}");
            }
            else if (Source == itemAreaToolStripMenuItem)
            {
                var RandomizedLocations = AllLocationObj.Where(x => x.CheckState == MiscData.CheckState.Unchecked && x.IsRandomized() && !string.IsNullOrWhiteSpace(x.Randomizeditem.SpoilerLogGivenItem));
                if (!RandomizedLocations.Any()) { MessageBox.Show("Hint could not be Generated!"); return; }
                var randomLocation = RandomizedLocations.PickRandom();
                string Item = _instance.ItemPool.ContainsKey(randomLocation.Randomizeditem.SpoilerLogGivenItem) ?
                    _instance.ItemPool[randomLocation.Randomizeditem.SpoilerLogGivenItem].GetDictEntry().GetName() :
                    randomLocation.Randomizeditem.SpoilerLogGivenItem;
                MessageBox.Show($"{Item} Can be found at {randomLocation.GetDictEntry().Area}");
            }
            else if (Source == playthroughLocationToolStripMenuItem)
            {
                var RandomizedLocations = ImportantLocationsObj.Where(x => x.CheckState == MiscData.CheckState.Unchecked && x.IsRandomized() && !string.IsNullOrWhiteSpace(x.Randomizeditem.SpoilerLogGivenItem));
                if (!RandomizedLocations.Any()) { MessageBox.Show("Hint could not be Generated!"); return; }
                var randomLocation = RandomizedLocations.PickRandom();
                string Item = _instance.ItemPool.ContainsKey(randomLocation.Randomizeditem.SpoilerLogGivenItem) ?
                    _instance.ItemPool[randomLocation.Randomizeditem.SpoilerLogGivenItem].GetDictEntry().GetName() :
                    randomLocation.Randomizeditem.SpoilerLogGivenItem;
                MessageBox.Show($"{randomLocation.GetDictEntry().GetName()} Contained {Item}");
            }
            else if (Source == playtrhoughItemAreaToolStripMenuItem)
            {
                var RandomizedLocations = ImportantLocationsObj.Where(x => x.CheckState == MiscData.CheckState.Unchecked && x.IsRandomized() && !string.IsNullOrWhiteSpace(x.Randomizeditem.SpoilerLogGivenItem));
                if (!RandomizedLocations.Any()) { MessageBox.Show("Hint could not be Generated!"); return; }
                var randomLocation = RandomizedLocations.PickRandom();
                string Item = _instance.ItemPool.ContainsKey(randomLocation.Randomizeditem.SpoilerLogGivenItem) ?
                    _instance.ItemPool[randomLocation.Randomizeditem.SpoilerLogGivenItem].GetDictEntry().GetName() :
                    randomLocation.Randomizeditem.SpoilerLogGivenItem;
                MessageBox.Show($"{Item} Can be found at {randomLocation.GetDictEntry().Area}");
            }


        }

        private void regenerateGlobalPlaythroughToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_instance.SpoilerLog is null) { MessageBox.Show("Not spoiler log has been imported. A spoiler log must be imported to generate a playthrough."); return; }
            _instance.SpoilerLog.GetStaticPlaythrough(_instance.GetParentContainer());
        }

        private void regenerateLocalSpoilerLogToolsPlaythroughToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int currInd = lbSpoilerLookupItems.SelectedIndex;
            int TopInd = lbSpoilerLookupItems.TopIndex;
            var TempPlaythrough = new PlaythroughGenerator(_instance);
            TempPlaythrough.GeneratePlaythrough();
            SpoilerLookupPlaythrough = TempPlaythrough.Playthrough;
            PopulateSpoilerLogList();
            lbSpoilerLookupItems.TopIndex = TopInd;
            lbSpoilerLookupItems.SelectedIndex = currInd;
        }

        private void tableLayoutPanel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel4_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
