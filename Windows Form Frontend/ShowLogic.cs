using MMR_Tracker_V3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace Windows_Form_Frontend
{
    public partial class ShowLogic : Form
    {
        public string CurrentID;
        private readonly LogicObjects.TrackerInstance instance;
        private readonly int ReqLBHeight;
        private int WindowWidth;
        private readonly List<string> GoBackList = new();
        public ShowLogic(String id, LogicObjects.TrackerInstance _instance)
        {
            InitializeComponent();
            CurrentID = id;
            instance = _instance;
            ReqLBHeight = listBox1.Height;
            WindowWidth = this.Width;
        }

        private void ShowLogic_Load(object sender, EventArgs e)
        {
            PrintData();
            PrintAllLocations();
            listBox3.Sorted = true;
            listBox4.Sorted = true;

            /*
            int LongestWidth = 0;
            Graphics graphics = listBox4.CreateGraphics();
            foreach (var i in listBox4.Items)
            {
                int ItemWidth = (int)graphics.MeasureString(i.ToString(), listBox4.Font).Width;
                if (ItemWidth > LongestWidth) { LongestWidth = ItemWidth; }
            }
            if (listBox4.Width < LongestWidth)
            {
                int diff = LongestWidth - listBox4.Width;
                listBox4.Width += diff;
                this.Width += diff;
                listBox3.Width += diff;
                textBox1.Width += diff;
                button1.Width += diff;
            }
            */

        }

        private void ShowLogic_ResizeEnd(object sender, EventArgs e)
        {
            var diff = this.Width - WindowWidth;
            listBox4.Width += diff;
            listBox3.Width += diff;
            textBox1.Width += diff;
            button1.Width += diff;
            WindowWidth = this.Width;

        }

        private void PrintAllLocations()
        {
            listBox4.Items.Clear();
            foreach (var i in instance.LocationPool.Values)
            {
                if (!i.ID.ToLower().Contains(textBox1.Text.ToLower())) { continue; }
                listBox4.Items.Add(i.ID);
            }
            foreach (var i in instance.EntrancePool.AreaList.SelectMany(x => x.Value.LoadingZoneExits))
            {
                var ID = instance.GetLogicNameFromExit(i.Value);
                if (!ID.ToLower().Contains(textBox1.Text.ToLower())) { continue; }
                listBox4.Items.Add(ID);
            }
            foreach (var i in instance.EntrancePool.AreaList.SelectMany(x => x.Value.MacroExits))
            {
                var ID = instance.GetLogicNameFromExit(i.Value);
                if (!ID.ToLower().Contains(textBox1.Text.ToLower())) { continue; }
                listBox4.Items.Add(ID);
            }
            foreach (var i in instance.MacroPool.Values)
            {
                if (!i.ID.ToLower().Contains(textBox1.Text.ToLower())) { continue; }
                listBox4.Items.Add(i.ID);
            }
        }

        private void PrintData()
        {
            button1.Enabled = GoBackList.Any();
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            var OriginalLogic = instance.GetLogic(CurrentID, false);
            var AlteredLogic = instance.GetLogic(CurrentID);

            bool ReqEqual = OriginalLogic.RequiredItems.SequenceEqual(AlteredLogic.RequiredItems);
            bool ConEqual = OriginalLogic.ConditionalItems.SelectMany(x => x).SequenceEqual(AlteredLogic.ConditionalItems.SelectMany(x => x));

            bool WasAltered = !ReqEqual || !ConEqual;
            checkBox1.Visible = WasAltered;

            bool Literal = CurrentID.IsLiteralID(out string LogicItem);
            var type = instance.GetLocationEntryType(LogicItem, Literal, out _);
            string Availablility = GetAvailable(AlteredLogic, type, LogicItem) ? "*" : "";
            string typeDisplay = type == LogicEntryType.macro && OriginalLogic.IsTrick ? "Trick" : type.ToString();
            this.Text = $"{typeDisplay}: {LogicItem}{Availablility}";

            var Logic = checkBox1.Checked ? OriginalLogic : AlteredLogic;
            foreach(var i in Logic.RequiredItems)
            {
                listBox1.Items.Add(GetDisplayName(i));
                AddToGotoList(i);
            }
            foreach (var cond in Logic.ConditionalItems)
            {
                listBox2.Items.Add(string.Join(" | ", cond.Select(x => GetDisplayName(x)))); 
                foreach (var i in cond) { AddToGotoList(i); }
            }
            UpdateTimeCheckboxes(OriginalLogic);
        }

        private void UpdateTimeCheckboxes(MMR_Tracker_V3.TrackerObjects.MMRData.JsonFormatLogicItem OriginalLogic)
        {
            List<CheckBox> TimeCheckBoxes = new() { ND1, NN1, ND2, NN2, ND3, NN3, SD1, SN1, SD2, SN2, SD3, SN3 };
            bool hasTimeLogic = OriginalLogic.TimeAvailable != TimeOfDay.None || OriginalLogic.TimeSetup != TimeOfDay.None;
            bool ShowTimeLogic = hasTimeLogic && checkBox2.Checked;
            checkBox2.Visible = hasTimeLogic;
            label5.Visible = ShowTimeLogic;
            label6.Visible = ShowTimeLogic;
            listBox1.Height = ShowTimeLogic ? ReqLBHeight : listBox2.Height;
            int Index = 0;
            foreach (var i in TimeCheckBoxes)
            {
                i.Visible = ShowTimeLogic;
                if (ShowTimeLogic)
                {
                    if (Index < 6) { i.Checked = ((((int)OriginalLogic.TimeAvailable >> Index) & 1) == 1); }
                    else { i.Checked = ((((int)OriginalLogic.TimeSetup >> Index - 6) & 1) == 1); }
                }
                Index++;
            }
        }

        private void AddToGotoList(string i)
        {
            bool Literal = i.IsLiteralID(out string ID);
            LogicEntryType entryType = instance.GetItemEntryType(ID, Literal, out _);
            if (entryType == LogicEntryType.macro && !listBox3.Items.Contains(i)) { listBox3.Items.Add(i); }
        }

        public bool GetAvailable(MMR_Tracker_V3.TrackerObjects.MMRData.JsonFormatLogicItem Logic, LogicEntryType type, string id)
        {
            bool AreaReached = type != LogicEntryType.Exit || LogicCalculation.AreaReached(instance.InstanceReference.EntranceLogicNameToEntryData[id].Area, instance);
            return LogicCalculation.RequirementsMet(Logic.RequiredItems, instance) && LogicCalculation.ConditionalsMet(Logic.ConditionalItems, instance) && AreaReached;
        }

        private string GetDisplayName(string i)
        {
            return i + (LogicCalculation.LogicEntryAquired(instance, i) ? "*": "");
        }

        private void listBox3_DoubleClick(object sender, EventArgs e)
        {
            if (listBox3.SelectedItem is string ID)
            {
                GoBackList.Add(CurrentID);
                CurrentID = ID;
                PrintData();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CurrentID = GoBackList[^1];
            GoBackList.RemoveAt(GoBackList.Count -1);
            PrintData();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            PrintAllLocations();
        }

        private void listBox4_DoubleClick(object sender, EventArgs e)
        {
            if (listBox4.SelectedItem is string ID)
            {
                GoBackList.Add(CurrentID);
                CurrentID = ID;
                PrintData();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            PrintData();
        }
    }
}
