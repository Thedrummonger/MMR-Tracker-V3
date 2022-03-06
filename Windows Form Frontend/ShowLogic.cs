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
        private List<string> GoBackList = new List<string>();
        public ShowLogic(String id, LogicObjects.TrackerInstance _instance)
        {
            InitializeComponent();
            CurrentID = id;
            instance = _instance;
        }

        private void ShowLogic_Load(object sender, EventArgs e)
        {
            PrintData();
            PrintAllLocations();
            listBox3.Sorted = true;
            listBox4.Sorted = true;
        }

        private void PrintAllLocations()
        {
            listBox4.Items.Clear();
            foreach (var i in instance.LocationPool)
            {
                if (!i.ID.ToLower().Contains(textBox1.Text.ToLower())) { continue; }
                listBox4.Items.Add(i.ID);
            }
            foreach (var i in instance.MacroPool)
            {
                if (!i.ID.ToLower().Contains(textBox1.Text.ToLower())) { continue; }
                listBox4.Items.Add(i.ID);
            }
        }
        public static bool checkEquality<T>(T[] first, T[] second)
        {
            return Enumerable.SequenceEqual(first, second);
        }

        private void PrintData()
        {
            button1.Enabled = GoBackList.Any();
            this.Text = $"{CurrentID} Available: {GetAvailable(CurrentID)}";
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            var OriginalLogic = instance.GetOriginalLogic(CurrentID);
            var AlteredLogic = instance.GetLogic(CurrentID);

            bool ReqEqual = OriginalLogic.RequiredItems.SequenceEqual(AlteredLogic.RequiredItems);
            bool ConEqual = OriginalLogic.ConditionalItems.SelectMany(x => x).SequenceEqual(AlteredLogic.ConditionalItems.SelectMany(x => x));

            checkBox1.Visible = !ReqEqual || !ConEqual;

            var Logic = checkBox1.Checked ? OriginalLogic : AlteredLogic;
            foreach(var i in Logic.RequiredItems)
            {
                bool Literal = i.IsLiteralID(out string ID);
                LogicEntryType entryType = instance.GetLocationEntryType(ID, Literal);
                if (entryType == LogicEntryType.macro && !listBox3.Items.Contains(i))
                {
                    listBox3.Items.Add(i);
                }

                string Display = i;
                if (entryType == LogicEntryType.macro)
                {
                    Display += instance.GetMacroByID(ID).Aquired;
                }
                else if (entryType == LogicEntryType.item)
                {
                    Display += instance.GetItemByID(ID).Useable(1);
                }
                listBox1.Items.Add(GetDisplayName(i));
            }
            foreach (var cond in Logic.ConditionalItems)
            {
                listBox2.Items.Add(string.Join(", ", cond.Select(x => GetDisplayName(x)))); 
                foreach (var i in cond)
                {
                    bool Literal = i.IsLiteralID(out string ID);
                    LogicEntryType entryType = instance.GetLocationEntryType(ID, Literal);
                    if (entryType == LogicEntryType.macro && !listBox3.Items.Contains(i))
                    {
                        listBox3.Items.Add(i);
                    }
                }
            }
        }

        public bool GetAvailable(String i)
        {
            bool Literal = i.IsLiteralID(out string LogicItem);
            var type = instance.GetItemEntryType(LogicItem, Literal);
            if (type == LogicEntryType.item)
            {
                var ItemObject = instance.GetItemByID(LogicItem);
                if (!ItemObject.Useable(1)) { return false; }
            }
            else if (type == LogicEntryType.macro)
            {
                var MacroObject = instance.GetMacroByID(LogicItem);
                if (!MacroObject.Aquired) { return false; }
            }
            return true;
        }

        private string GetDisplayName(string i)
        {
            bool Literal = i.IsLiteralID(out string ID);
            LogicEntryType entryType = instance.GetItemEntryType(ID, Literal);
            string Display = i;
            if (entryType == LogicEntryType.macro)
            {
                Display += instance.GetMacroByID(ID).Aquired ? "*" : "";
            }
            else if (entryType == LogicEntryType.item)
            {
                Display += instance.GetItemByID(ID).Useable() ? "*" : "";
            }
            return Display;
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
