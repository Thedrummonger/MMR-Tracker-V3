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
using Newtonsoft.Json;

namespace Windows_Form_Frontend
{
    public partial class SpoilerLogLookUp : Form
    {
        public LogicObjects.TrackerInstance _instance;
        public SpoilerLogLookUp(LogicObjects.TrackerInstance instance)
        {
            InitializeComponent();
            _instance = instance;
        }

        private void SpoilerLogLookUp_Load(object sender, EventArgs e)
        {
            PopulateWinConCMB();
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
                    cmbWinCon.Items.Add(new MMR_Tracker_V3.TrackerObjects.MiscData.StandardListBoxItem { Display = i.GetDictEntry(_instance).GetItemName(_instance), tag = i });
                }
            }

            if (Locations.Any())
            {
                cmbWinCon.Items.Add(WinFormUtils.CreateDivider(cmbWinCon, "Locations"));
                foreach (var i in Locations)
                {
                    cmbWinCon.Items.Add(new MMR_Tracker_V3.TrackerObjects.MiscData.StandardListBoxItem { Display = i.GetDictEntry(_instance).Name??i.ID, tag = i });
                }
            }

            if (Macros.Any())
            {
                cmbWinCon.Items.Add(WinFormUtils.CreateDivider(cmbWinCon, "Macros"));
                foreach (var i in Macros)
                {
                    cmbWinCon.Items.Add(new MMR_Tracker_V3.TrackerObjects.MiscData.StandardListBoxItem { Display = i.GetDictEntry(_instance).Name??i.ID, tag = i });
                }
            }

            if (Areas.Any())
            {
                cmbWinCon.Items.Add(WinFormUtils.CreateDivider(cmbWinCon, "Areas"));
                foreach (var i in Areas)
                {
                    cmbWinCon.Items.Add(new MMR_Tracker_V3.TrackerObjects.MiscData.StandardListBoxItem { Display = i.ID, tag = i });
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

            if (chkOnlyImportant.Checked) 
            { 
                bool sucess = playthroughGenerator.FilterImportantPlaythrough((cmbWinCon.SelectedItem as MMR_Tracker_V3.TrackerObjects.MiscData.StandardListBoxItem).tag); 
                if (!sucess) { MessageBox.Show($"Error"); }
            }

            var Result = playthroughGenerator.Playthrough.Where(x => x.Value.Important || !chkOnlyImportant.Checked).ToDictionary(x => x.Key, x => x.Value);

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
    }
}
