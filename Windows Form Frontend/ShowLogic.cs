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
        public string state = "show";
        public ShowLogic(string id, LogicObjects.TrackerInstance _instance)
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
        }

        private void ShowLogic_ResizeEnd(object sender, EventArgs e)
        {

        }

        private void PrintAllLocations()
        {
            listBox1.Items.Clear();
            foreach (var i in instance.LocationPool.Values)
            {
                if (!i.ID.ToLower().Contains(textBox1.Text.ToLower())) { continue; }
                listBox1.Items.Add(i.ID);
            }
            foreach (var i in instance.EntrancePool.AreaList.SelectMany(x => x.Value.LoadingZoneExits))
            {
                var ID = instance.GetLogicNameFromExit(i.Value);
                if (!ID.ToLower().Contains(textBox1.Text.ToLower())) { continue; }
                listBox1.Items.Add(ID);
            }
            foreach (var i in instance.EntrancePool.AreaList.SelectMany(x => x.Value.MacroExits))
            {
                var ID = instance.GetLogicNameFromExit(i.Value);
                if (!ID.ToLower().Contains(textBox1.Text.ToLower())) { continue; }
                listBox1.Items.Add(ID);
            }
            foreach (var i in instance.MacroPool.Values)
            {
                if (!i.ID.ToLower().Contains(textBox1.Text.ToLower())) { continue; }
                listBox1.Items.Add(i.ID);
            }
        }

        private void PrintData()
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            if (state == "show")
            {
                label1.Text = "Requirements";
                label2.Text = "Conditionals";
                button2.Text = "Go To";
                button1.Visible = true;

                button1.Enabled = GoBackList.Any();
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
                UpdateTimeCheckboxes(OriginalLogic);

                foreach (var i in Logic.RequiredItems)
                {
                    listBox1.Items.Add(GetDisplayName(i));
                }
                foreach (var cond in Logic.ConditionalItems)
                {
                    listBox2.Items.Add(string.Join(" | ", cond.Select(x => GetDisplayName(x))));
                }

            }
            else
            {
                label1.Text = "All Logic Items";
                label2.Text = "Logic Items used in this entry";
                button2.Text = "Cancel";
                button1.Visible = false;

                PrintSelectedLogicItemsToGotoList();
                PrintAllLocations();
            }

        }

        public void PrintSelectedLogicItemsToGotoList()
        {
            var OriginalLogic = instance.GetLogic(CurrentID, false);
            var AlteredLogic = instance.GetLogic(CurrentID);
            var Logic = checkBox1.Checked ? OriginalLogic : AlteredLogic;

            Dictionary<string, LogicEntryType> GotoList = new Dictionary<string, LogicEntryType>();

            foreach (var i in Logic.RequiredItems)
            {
                bool ReqItemIsLitteral = i.IsLiteralID(out string ReqLogicItem);
                GotoList[i] = instance.GetItemEntryType(ReqLogicItem, ReqItemIsLitteral, out _);
            }
            foreach (var cond in Logic.ConditionalItems)
            {
                foreach (var i in cond)
                {
                    bool ReqItemIsLitteral = i.IsLiteralID(out string ReqLogicItem);
                    GotoList[i] = instance.GetItemEntryType(ReqLogicItem, ReqItemIsLitteral, out _);
                }
            }
            AddToGotoList(GotoList);
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

        private void AddToGotoList(Dictionary<string, LogicEntryType> GotoList)
        {
            GotoList = GotoList.OrderBy(x => x.Value).ThenBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            LogicEntryType CurrentType = LogicEntryType.location;
            foreach(var i in GotoList)
            {
                var Checks = GetChecksContainingSelectedID(i.Key, out _, out string CleanedID);
                if (!Checks.Any()) { continue; }
                if (CurrentType != i.Value) 
                {
                    listBox2.Items.Add(WinFormUtils.CreateDivider(listBox2, $"{i.Value}"));
                    CurrentType = i.Value; 
                }
                foreach(var c in Checks)
                {
                    var LitEntry = new StandardListBoxItem();
                    switch (CurrentType)
                    {
                        case LogicEntryType.Area:
                            LitEntry.tag = c;
                            LitEntry.Display = $"{CleanedID}: {c}";
                            break;
                        case LogicEntryType.item:
                            LitEntry.tag = c;
                            LitEntry.Display = $"{instance.GetItemByID(CleanedID)?.GetDictEntry(instance)?.GetItemName(instance)??CleanedID}: {c}";
                            break;
                        case LogicEntryType.macro:
                        default:
                            LitEntry.tag = c;
                            LitEntry.Display = CleanedID;
                            break;
                    }
                    listBox2.Items.Add(LitEntry);
                }
            }
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
            if (state == "go")
            {
                if (listBox2.SelectedItem is StandardListBoxItem LBI)
                {
                    state = "show";
                    GoBackList.Add(CurrentID);
                    CurrentID = LBI.tag.ToString();
                    PrintData();
                }
                else if (listBox1.SelectedItem is string LBS)
                {
                    state = "show";
                    GoBackList.Add(CurrentID);
                    CurrentID = LBS;
                    PrintData();
                }
            }
            else
            {
                button2_Click(sender, e);
            }
        }

        private List<string> GetChecksContainingSelectedID(string ID, out LogicEntryType Type, out string OutCleanedID)
        {
            LogicCalculation.MultipleItemEntry(instance, ID, out string CleanedID, out int Amount);
            bool ItemIsLitteral = CleanedID.IsLiteralID(out CleanedID);
            Type = instance.GetItemEntryType(CleanedID, ItemIsLitteral, out _);
            OutCleanedID = CleanedID;
            switch (Type)
            {
                case LogicEntryType.Area:
                    var ValidLoadingZoneExits = instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values.Where(x => x.DestinationExit is not null  && x.DestinationExit.region == CleanedID));
                    var ValidMacroExits = instance.EntrancePool.AreaList.Values.SelectMany(x => x.MacroExits.Values.Where(x => x.DestinationExit is not null  && x.DestinationExit.region == CleanedID));
                    var ValidExits = ValidLoadingZoneExits.Concat(ValidMacroExits);
                    return ValidExits.Select(x => instance.GetLogicNameFromExit(x)).ToList();
                case LogicEntryType.item:
                    var ValidLocations = instance.LocationPool.Values.Where(x => x.Randomizeditem.Item is not null && x.Randomizeditem.Item == CleanedID);
                    return ValidLocations.Select(x => x.ID).ToList();
                case LogicEntryType.macro:
                    return new List<string> { CleanedID };
                default:
                    return new List<string>();
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
            PrintData();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            PrintData();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (state == "show") { state = "go"; }
            else { state = "show"; }
            PrintData();
        }
    }
}
