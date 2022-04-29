using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;

namespace Windows_Form_Frontend
{
    public partial class SpoilerLogLookUp : Form
    {
        public LogicObjects.TrackerInstance _instance;
        public PlaythroughGenerator SpoilerLookupPlaythrough = null;
        public SpoilerLogLookUp(LogicObjects.TrackerInstance instance)
        {
            InitializeComponent();
            _instance = instance;
        }

        private void SpoilerLogLookUp_Load(object sender, EventArgs e)
        {
            PopulateWinConCMB();
            PopulateSpoilerLogList();
            listBox1_SelectedValueChanged(sender, e);
        }

        private void PopulateWinConCMB()
        {
            cmbWinCon.Items.Clear();
            var Items = _instance.ItemPool.Values.Where(x => SearchStringParser.FilterSearch(_instance, x, txtWinConFilter.Text, x.GetDictEntry(_instance).GetItemName(_instance))).OrderBy(x => x.GetDictEntry(_instance).GetItemName(_instance));
            var Locations = _instance.LocationPool.Values.Where(x => SearchStringParser.FilterSearch(_instance, x, txtWinConFilter.Text, x.GetDictEntry(_instance).Name??x.ID)).OrderBy(x => x.GetDictEntry(_instance).Name??x.ID);
            var Macros = _instance.MacroPool.Values.Where(x => SearchStringParser.FilterSearch(_instance, x, txtWinConFilter.Text, x.GetDictEntry(_instance).Name??x.ID)).OrderBy(x => x.GetDictEntry(_instance).Name??x.ID);
            var Areas = _instance.EntrancePool.AreaList.Values.Where(x => SearchStringParser.FilterSearch(_instance, x, txtWinConFilter.Text, x.ID)).OrderBy(x => x.ID);

            if (Items.Any())
            {
                cmbWinCon.Items.Add(WinFormUtils.CreateDivider(cmbWinCon, "Items"));
                foreach (var i in Items)
                {
                    cmbWinCon.Items.Add(new MiscData.StandardListBoxItem { Display = i.GetDictEntry(_instance).GetItemName(_instance), tag = i });
                }
            }

            if (Locations.Any())
            {
                cmbWinCon.Items.Add(WinFormUtils.CreateDivider(cmbWinCon, "Locations"));
                foreach (var i in Locations)
                {
                    cmbWinCon.Items.Add(new MiscData.StandardListBoxItem { Display = i.GetDictEntry(_instance).Name??i.ID, tag = i });
                }
            }

            if (Macros.Any())
            {
                cmbWinCon.Items.Add(WinFormUtils.CreateDivider(cmbWinCon, "Macros"));
                foreach (var i in Macros)
                {
                    cmbWinCon.Items.Add(new MiscData.StandardListBoxItem { Display = i.GetDictEntry(_instance).Name??i.ID, tag = i });
                }
            }

            if (Areas.Any())
            {
                cmbWinCon.Items.Add(WinFormUtils.CreateDivider(cmbWinCon, "Areas"));
                foreach (var i in Areas)
                {
                    cmbWinCon.Items.Add(new MiscData.StandardListBoxItem { Display = i.ID, tag = i });
                }
            }
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

            bool FullPlaythrough = !chkOnlyImportant.Checked || SelectedItem?.tag == null;

            if (!FullPlaythrough) 
            {
                bool sucess = playthroughGenerator.FilterImportantPlaythrough(SelectedItem.tag); 
                if (!sucess) { MessageBox.Show($"Error {SelectedItem.tag} Could not be reached this seed"); return; }
            }

            var Result = playthroughGenerator.Playthrough.Where(x => x.Value.Important || (FullPlaythrough && x.Value.CheckType != MMR_Tracker_V3.TrackerObjects.MiscData.LogicEntryType.macro)).ToDictionary(x => x.Key, x => x.Value);

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "Playthrough";
            dlg.Filter = "Text Files | *.txt";
            dlg.DefaultExt = "txt";
            dlg.ShowDialog();
            if (string.IsNullOrWhiteSpace(dlg.FileName)) { return; }

            File.WriteAllText(dlg.FileName, JsonConvert.SerializeObject(Result, Testing._NewtonsoftJsonSerializerOptions));
            File.WriteAllText(Testing.CretaeTestingFile("WTF"), JsonConvert.SerializeObject(playthroughGenerator.FirstObtainedDict, Testing._NewtonsoftJsonSerializerOptions));
            File.WriteAllText(Testing.CretaeTestingFile("Unlock"), JsonConvert.SerializeObject(playthroughGenerator.PlaythroughUnlockData, Testing._NewtonsoftJsonSerializerOptions));
        }

        private void PopulateSpoilerLogList()
        {
            listBox1.Items.Clear();
            var SpoilerChecks = _instance.LocationPool.Values.Where(x => x.GetItemAtCheck(_instance) != null);
            List<MiscData.StandardListBoxItem> SpoilerList = new List<MiscData.StandardListBoxItem>();
            foreach (var check in SpoilerChecks) 
            {
                MiscData.StandardListBoxItem LBI = new MiscData.StandardListBoxItem() { tag = check };
                dynamic Item = _instance.GetItemByID(check.GetItemAtCheck(_instance));
                if (Item == null)
                {
                    Item = check.GetItemAtCheck(_instance);
                    LBI.Display = check.GetItemAtCheck(_instance);
                }
                else
                {
                    LBI.Display = Item.GetDictEntry(_instance).GetItemName(_instance);
                }
                if (SearchStringParser.FilterSearch(_instance, Item, textBox2.Text, LBI.Display)) { SpoilerList.Add(LBI); }
                
            }

            Dictionary<string, List<LocationData.LocationObject>> CleanedEntries = new Dictionary<string, List<LocationData.LocationObject>>();
            foreach(var spoiler in SpoilerList)
            {
                if (!CleanedEntries.ContainsKey(spoiler.Display)) { CleanedEntries[spoiler.Display] = new List<LocationData.LocationObject>(); }
                CleanedEntries[spoiler.Display].Add(spoiler.tag as LocationData.LocationObject);
            }
            CleanedEntries = CleanedEntries.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            if (CleanedEntries.Any() && chkShowMacros.Checked) { listBox1.Items.Add(WinFormUtils.CreateDivider(listBox1, "Locations")); }
            foreach(var i in CleanedEntries)
            {
                listBox1.Items.Add(new MiscData.StandardListBoxItem { Display = i.Key, tag = i.Value });
            }

            List<MiscData.StandardListBoxItem> MacroList = new List<MiscData.StandardListBoxItem>();
            if (chkShowMacros.Checked)
            {
                foreach (var check in _instance.MacroPool.Values)
                {
                    MiscData.StandardListBoxItem LBI = new MiscData.StandardListBoxItem() { tag = check, Display = check.GetDictEntry(_instance).Name??check.ID };
                    if (SearchStringParser.FilterSearch(_instance, check, textBox2.Text, LBI.Display)) { MacroList.Add(LBI); }
                }
                if (MacroList.Any()) { listBox1.Items.Add(WinFormUtils.CreateDivider(listBox1, "Macros")); }
                foreach(var i in MacroList.OrderBy(x => x.Display)) { listBox1.Items.Add(i); }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            PopulateSpoilerLogList();
            listBox1_SelectedValueChanged(sender, e);
        }

        private void btnArea_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is MiscData.StandardListBoxItem SLI)
            {
                if (SLI.tag is LocationData.LocationObject LO)
                {
                    MessageBox.Show($"{SLI.Display} Can be found in\n\n-{LO.GetDictEntry(_instance).Area}");
                }
                else if (SLI.tag is List<LocationData.LocationObject> LLO)
                {
                    if (LLO.Count == 1)
                    {
                        MessageBox.Show($"{SLI.Display} Can be found in\n\n-{LLO.First().GetDictEntry(_instance).Area}");
                    }
                    else
                    {
                        MessageBox.Show($"{SLI.Display} Can be found in these areas:\n\n-{string.Join("\n-", LLO.Select(x => x.GetDictEntry(_instance).Area).Distinct().OrderBy(x => x))}");
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is MiscData.StandardListBoxItem SLI)
            {
                if (SpoilerLookupPlaythrough is null)
                {
                    SpoilerLookupPlaythrough =  new PlaythroughGenerator(_instance);
                    SpoilerLookupPlaythrough.GeneratePlaythrough();
                }

                if (SLI.tag is LocationData.LocationObject LO)
                {
                    if (SpoilerLookupPlaythrough.Playthrough.ContainsKey(LO.ID)) { MessageBox.Show($"{SLI.Display} Can be obtained shpere {SpoilerLookupPlaythrough.Playthrough[LO.ID].sphere}"); }
                    else { MessageBox.Show($"{SLI.Display} Can not be obtained with known items");}
                }
                else if (SLI.tag is List<LocationData.LocationObject> LLO)
                {
                    if (LLO.Count == 1)
                    {
                        if (SpoilerLookupPlaythrough.Playthrough.ContainsKey(LLO.First().ID)) { MessageBox.Show($"{SLI.Display} Can be obtained shpere {SpoilerLookupPlaythrough.Playthrough[LLO.First().ID].sphere}"); }
                        else { MessageBox.Show($"{SLI.Display} Can not be obtained with known items"); }
                    }
                    else
                    {
                        List<int> SpheresObtainable = new List<int>();
                        foreach(var i in LLO.Where(x => SpoilerLookupPlaythrough.Playthrough.ContainsKey(x.ID)))
                        {
                            SpheresObtainable.Add(SpoilerLookupPlaythrough.Playthrough[i.ID].sphere);
                        }
                        if (!SpheresObtainable.Any()) { MessageBox.Show($"{SLI.Display} Can not be obtained with known items"); }
                        else { MessageBox.Show($"{SLI.Display} Can be obtained shpere {SpheresObtainable.Min()}");}
                    }
                }
                else if (SLI.tag is MacroObject MO)
                {
                    if (SpoilerLookupPlaythrough.Playthrough.ContainsKey(MO.ID)) { MessageBox.Show($"{SLI.Display} Can be obtained shpere {SpoilerLookupPlaythrough.Playthrough[MO.ID].sphere}"); }
                    else if (MO.isTrick(_instance) && !MO.TrickEnabled) { MessageBox.Show($"{SLI.Display} Is a trick and is not enabled"); }
                    else { MessageBox.Show($"{SLI.Display} Can not be obtained with known items"); }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is MiscData.StandardListBoxItem SLI)
            {
                if (SLI.tag is LocationData.LocationObject LO)
                {
                    MessageBox.Show($"{SLI.Display} Can be found at\n\n-{LO.GetDictEntry(_instance).Name??LO.ID}");
                }
                else if (SLI.tag is List<LocationData.LocationObject> LLO)
                {
                    if (LLO.Count == 1)
                    {
                        MessageBox.Show($"{SLI.Display} Can be found at\n\n-{LLO.First().GetDictEntry(_instance).Name??LLO.First().ID}");
                    }
                    else
                    {
                        MessageBox.Show($"{SLI.Display} Can be found at these locations:\n\n-{string.Join("\n-", LLO.Select(x => x.GetDictEntry(_instance).Name??x.ID).Distinct().OrderBy(x => x))}");
                    }
                }
            }
        }

        private void listBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is MiscData.StandardListBoxItem SLI)
            {
                bool EnableLocation = SLI.tag is LocationData.LocationObject || SLI.tag is List<LocationData.LocationObject>;
                bool EnableSphere = SLI.tag is MacroObject || SLI.tag is List<MacroObject>;
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
    }
}
