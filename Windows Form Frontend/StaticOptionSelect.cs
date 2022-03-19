using MMR_Tracker_V3;
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
    public partial class StaticOptionSelect : Form
    {
        private LogicObjects.OptionFile TempOptionFile;
        private LogicObjects.TrackerInstance _Instance;
        public StaticOptionSelect(LogicObjects.TrackerInstance Instance)
        {
            InitializeComponent();
            _Instance = Instance;
            TempOptionFile = GenericCopier<LogicObjects.OptionFile>.DeepCopy(_Instance.StaticOptions.OptionFile);
        } 
        private bool ValuesUpdating = false;

        private void StaticOptionSelect_Load(object sender, EventArgs e)
        {
            ValuesUpdating = true;
            chkHorizontal.Checked = TempOptionFile.WinformData.HorizontalLayout;
            chkTooltips.Checked = TempOptionFile.WinformData.ShowEntryNameTooltip;
            chkUpdates.Checked = TempOptionFile.CheckForUpdate;
            int counter = 0;
            var CurrentFont = WinFormUtils.GetFontFromString(TempOptionFile.WinformData.FormFont);
            foreach (FontFamily font in System.Drawing.FontFamily.Families)
            {
                cmbFontStyle.Items.Add(font.Name);
                if (font.Name == CurrentFont.FontFamily.Name) { cmbFontStyle.SelectedIndex = counter; }
                counter++;
            }
            nudFontSize.Value = (decimal)CurrentFont.Size;
            textBox1.Text = "Example";
            textBox1.Font = CurrentFont;
            ValuesUpdating = false;
        }

        private void chkUpdates_CheckStateChanged(object sender, EventArgs e)
        {
            if (ValuesUpdating) { return; }
            TempOptionFile.WinformData.HorizontalLayout = chkHorizontal.Checked;
            TempOptionFile.WinformData.ShowEntryNameTooltip = chkTooltips.Checked;
            TempOptionFile.CheckForUpdate = chkUpdates.Checked;
        }

        private void nudFontSize_ValueChanged(object sender, EventArgs e)
        {
            if (ValuesUpdating) { return; }
            UpdateFont();
        }

        private void cmbFontStyle_SelectedValueChanged(object sender, EventArgs e)
        {
            if (ValuesUpdating) { return; }
            UpdateFont();
        }

        private void UpdateFont()
        {
            TempOptionFile.WinformData.FormFont = WinFormUtils.ConvertFontToString(new Font(familyName: cmbFontStyle.SelectedItem.ToString(), (float)nudFontSize.Value, FontStyle.Regular));
            textBox1.Font = new Font(familyName: cmbFontStyle.SelectedItem.ToString(), (float)nudFontSize.Value, FontStyle.Regular);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _Instance.StaticOptions.OptionFile = TempOptionFile;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            File.WriteAllText(References.WindowsPaths.OptionFile, JsonConvert.SerializeObject(TempOptionFile, Testing._NewtonsoftJsonSerializerOptions));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            TempOptionFile.WinformData.FormFont = WinFormUtils.ConvertFontToString( System.Drawing.SystemFonts.DefaultFont);
        }
    }
}
