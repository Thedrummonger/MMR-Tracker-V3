using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjects;
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
        private readonly ListBoxHeightData ReqLBHeightData;
        private bool Updating = false;
        private readonly List<CheckBox> TimeCheckBoxes;
        private readonly List<string> GoBackList = new();
        private List<object> CurrentGotoData;
        private LogicCalculation LogicCalculation;
        //0 = Show Logic; 1 = Goto Entry
        public int state = 0;
        public ShowLogic(string id, LogicObjects.TrackerInstance _instance)
        {
            InitializeComponent();
            CurrentID = id;
            instance = _instance;
            ReqLBHeightData = new ListBoxHeightData() { 
                Main = LBReq,
                TopPosFull = lbCond.Location.Y,
                TopPosCut = LBReq.Location.Y,
                BottomPosFull = ND3.Location.Y + ND3.Height,
                BottomPosCut = LBReq.Location.Y + LBReq.Height,
            };
            TimeCheckBoxes = new() { ND1, NN1, ND2, NN2, ND3, NN3, SD1, SN1, SD2, SN2, SD3, SN3 };
            LogicCalculation = new LogicCalculation(new InstanceContainer { Instance = instance });
        }

        public class ListBoxHeightData
        {
            public ListBox Main;

            public int TopPosFull;
            public int TopPosCut;
            public int BottomPosFull;
            public int BottomPosCut;
            public void ResizeLB(bool TopElement, bool BottomElement)
            {
                int StartingPos = TopElement ? TopPosCut : TopPosFull;
                int FinalPos = BottomElement ? BottomPosCut : BottomPosFull;
                Main.Location = new Point(Main.Location.X, StartingPos);
                Main.Height = FinalPos - StartingPos;
            }
        }

        private void UpdateUI()
        {
            Updating = true;
            if (state == 0)
            {
                var AlteredLogic = instance.GetLogic(CurrentID, true);
                var UnAlteredLogic = instance.GetLogic(CurrentID, false);
                bool Literal = CurrentID.IsLiteralID(out string LogicItem);
                var type = instance.GetLocationEntryType(LogicItem, Literal, out _);
                string Availablility = GetAvailable(AlteredLogic, type, LogicItem) ? "*" : "";
                string typeDisplay = type == LogicEntryType.macro && UnAlteredLogic.IsTrick ? "Trick" : type.ToString();
                bool hasTimeLogic = UnAlteredLogic.TimeAvailable != TimeOfDay.None || UnAlteredLogic.TimeSetup != TimeOfDay.None;
                bool ShowTimeLogic = hasTimeLogic && chkShowTime.Checked;
                //Title
                this.Text = $"{typeDisplay}: {LogicItem}{Availablility}";
                //Text Changes
                textBox1.Text = "";
                label1.Text = "Requirements";
                label2.Text = "Conditionals";
                BTNGotTo.Text = "Go to";
                //Hide/Show Items
                textBox1.Visible = false;
                chkShowTime.Visible = true;
                chkShowUnaltered.Visible = true;
                btnGoBack.Visible = true;
                TimeCheckBoxes.ForEach(x => x.Visible = ShowTimeLogic);
                label5.Visible = ShowTimeLogic;
                label6.Visible = ShowTimeLogic;
                BTNGotTo.Visible = true;
                //Enable Items
                textBox1.Enabled = false;
                btnGoBack.Enabled = GoBackList.Any();
                chkShowUnaltered.Enabled = !Utility.IsLogicEqual(AlteredLogic, UnAlteredLogic);
                chkShowTime.Enabled = hasTimeLogic;
                //Resize Items
                ReqLBHeightData.ResizeLB(false, ShowTimeLogic);
            }
            else if (state == 1)
            {
                //Title
                this.Text = $"Select Logic Entry";
                //Text Changes
                label1.Text = "All Logic Items";
                label2.Text = "Logic Items in Selected Entry";
                BTNGotTo.Text = "Cancel";
                //Hide/Show Items
                textBox1.Visible = true;
                TimeCheckBoxes.ForEach(x => x.Visible = false);
                chkShowTime.Visible = false;
                chkShowUnaltered.Visible = false;
                label5.Visible = false;
                label6.Visible = false;
                btnGoBack.Visible = false;
                BTNGotTo.Visible = CurrentID is not null;
                //Enable Items
                textBox1.Enabled = true;
                btnGoBack.Enabled = false;
                chkShowUnaltered.Enabled = false;
                chkShowTime.Enabled = false;
                //Resize Items
                ReqLBHeightData.ResizeLB(true, false);
            }
            Updating = false;
        }

        public bool GetAvailable(MMR_Tracker_V3.TrackerObjects.MMRData.JsonFormatLogicItem Logic, LogicEntryType type, string id)
        {
            string Area = type == LogicEntryType.Exit ? instance.InstanceReference.EntranceLogicNameToEntryData[id].Area : null;
            return LogicCalculation.CalculatReqAndCond(Logic, id, Area);
        }

        private void ShowLogic_Load(object sender, EventArgs e)
        {
            if (CurrentID is not null && instance.GetLogic(CurrentID) is not null)
            {
                state = 0;
                PrintLogicToLists();
            }
            else
            {
                state = 1;
                List<string> ItemIds = instance.ItemPool.Keys.ToList();
                List<string> MacroIds = instance.MacroPool.Keys.ToList();
                List<string> AreaIds = instance.EntrancePool.AreaList.Keys.ToList();
                PrintGotoData(CreatGotoList(ItemIds.Concat(MacroIds).Concat(AreaIds).ToList(), out _));
            }
            UpdateUI();
        }

        private void ShowLogic_ResizeEnd(object sender, EventArgs e)
        {

        }

        public List<string> GetAllLogicItemsByFromID(string ID)
        {
            var OriginalLogic = instance.GetLogic(ID, false);
            var AlteredLogic = instance.GetLogic(ID);
            var Logic = chkShowUnaltered.Checked ? OriginalLogic : AlteredLogic;

            List<string> LogicItems = new List<string>();

            foreach (var i in Logic.RequiredItems)
            {
                if (LogicItems.Contains(i)) { continue; }
                LogicItems.Add(i);
            }
            foreach (var cond in Logic.ConditionalItems)
            {
                foreach (var i in cond)
                {
                    if (LogicItems.Contains(i)) { continue; }
                    LogicItems.Add(i);
                }
            }
            return LogicItems;
        }

        private void PrintLogicToLists()
        {

            LBReq.Items.Clear();
            lbCond.Items.Clear();
            var Logic = instance.GetLogic(CurrentID, !chkShowUnaltered.Checked);
            bool Literal = CurrentID.IsLiteralID(out string LogicItem);
            var type = instance.GetLocationEntryType(LogicItem, Literal, out object LocationObject);
            UpdateTimeCheckboxes(Logic);

            foreach (var i in Logic.RequiredItems.Where(x => !bool.TryParse(x, out bool bout) || !bout))
            {
                StandardListBoxItem boxItem = new StandardListBoxItem() { Display = GetDisplayName(i), tag = i };
                LBReq.Items.Add(boxItem);
            }
            if (LocationObject is EntranceData.EntranceRandoExit EO && !Logic.RequiredItems.Contains(EO.ParentAreaID))
            {
                StandardListBoxItem boxItem = new StandardListBoxItem() { Display = GetDisplayName(EO.ParentAreaID), tag = EO.ParentAreaID };
                LBReq.Items.Add(boxItem);
            }
            foreach (var cond in Logic.ConditionalItems)
            {
                StandardListBoxItem boxItem = new StandardListBoxItem() { Display = string.Join(" | ", cond.Select(x => GetDisplayName(x))), tag = cond };
                lbCond.Items.Add(boxItem);
            }
        }

        private void PrintGotoData(List<object> GotoData, bool SearchGotoData = false)
        {
            LBReq.Items.Clear();
            foreach (var i in instance.LocationPool.Values)
            {
                if (!SearchStringParser.FilterSearch(instance, i, textBox1.Text, i.ID)) { continue; }
                LBReq.Items.Add(i.ID);
            }
            foreach (var i in instance.EntrancePool.AreaList.SelectMany(x => x.Value.LoadingZoneExits))
            {
                var ID = instance.GetLogicNameFromExit(i.Value);
                if (!SearchStringParser.FilterSearch(instance, i, textBox1.Text, ID)) { continue; }
                LBReq.Items.Add(ID);
            }
            foreach (var i in instance.EntrancePool.AreaList.SelectMany(x => x.Value.MacroExits))
            {
                var ID = instance.GetLogicNameFromExit(i.Value);
                if (!SearchStringParser.FilterSearch(instance, i, textBox1.Text, ID)) { continue; }
                LBReq.Items.Add(ID);
            }
            foreach (var i in instance.HintPool.Values)
            {
                if (!SearchStringParser.FilterSearch(instance, i, textBox1.Text, i.ID)) { continue; }
                LBReq.Items.Add(i.ID);
            }
            foreach (var i in instance.MacroPool.Values)
            {
                if (!SearchStringParser.FilterSearch(instance, i, textBox1.Text, i.ID)) { continue; }
                LBReq.Items.Add(i.ID);
            }
            if (GotoData != null)
            {
                lbCond.Items.Clear();
                foreach (var i in GotoData)
                {
                    lbCond.Items.Add(i);
                }
                CurrentGotoData = GotoData;
            }
            else if (CurrentGotoData != null && SearchGotoData)
            {
                lbCond.Items.Clear();
                foreach (var i in CurrentGotoData)
                {
                    if (i is StandardListBoxItem SLI && !SLI.Display.ToLower().Contains(textBox1.Text.ToLower())) { continue; }
                    lbCond.Items.Add(i);
                }
            }
        }

        private List<object> CreatGotoList(List<string> set, out List<object> DataEntriesOnly)
        {
            Dictionary<string, LogicEntryType> LogicItems = new Dictionary<string, LogicEntryType>();
            foreach (var i in set)
            {
                bool ReqItemIsLitteral = i.IsLiteralID(out string ReqLogicItem);
                LogicItems[i] = instance.GetItemEntryType(ReqLogicItem, ReqItemIsLitteral, out _);
            }
            var GotoData = CreateGotoDataFromList(LogicItems);
            DataEntriesOnly = GotoData.Where(x => x is StandardListBoxItem).ToList();
            return GotoData;
        }

        private List<object> CreateGotoDataFromList(Dictionary<string, LogicEntryType> GotoList)
        {
            GotoList = GotoList.OrderBy(x => x.Value).ThenBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            List<object> list = new List<object>();
            LogicEntryType CurrentType = LogicEntryType.location;
            foreach (var i in GotoList)
            {
                var Checks = GetChecksContainingSelectedID(i.Key, out _, out string CleanedID);
                if (!Checks.Any()) { continue; }
                if (CurrentType != i.Value)
                {
                    list.Add(WinFormUtils.CreateDivider(lbCond, $"{i.Value}"));
                    CurrentType = i.Value;
                }
                foreach (var c in Checks)
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
                            LitEntry.Display = $"{instance.GetItemByID(CleanedID)?.GetDictEntry(instance)?.GetName(instance)??CleanedID}: {c}";
                            break;
                        case LogicEntryType.macro:
                        default:
                            LitEntry.tag = c;
                            LitEntry.Display = CleanedID;
                            break;
                    }
                    list.Add(LitEntry);
                }
            }
            return list;
        }
        private void UpdateTimeCheckboxes(MMR_Tracker_V3.TrackerObjects.MMRData.JsonFormatLogicItem OriginalLogic)
        {
            Updating = true;
            int Index = 0;
            foreach (var i in TimeCheckBoxes)
            {
                if (Index < 6) { i.Checked = ((((int)OriginalLogic.TimeAvailable >> Index) & 1) == 1); }
                else { i.Checked = ((((int)OriginalLogic.TimeSetup >> Index - 6) & 1) == 1); }
                Index++;
            }
            Updating = false;
        }
        private List<string> GetChecksContainingSelectedID(string ID, out LogicEntryType Type, out string OutCleanedID)
        {
            instance.MultipleItemEntry(ID, out string CleanedID, out int Amount);
            bool ItemIsLitteral = CleanedID.IsLiteralID(out CleanedID);
            Type = instance.GetItemEntryType(CleanedID, ItemIsLitteral, out _);
            OutCleanedID = CleanedID;
            switch (Type)
            {
                case LogicEntryType.Area:
                    var ValidLoadingZoneExits = instance.EntrancePool.AreaList.Values.SelectMany(x => x.LoadingZoneExits.Values.Where(x => (x.DestinationExit is not null  && x.DestinationExit.region == CleanedID) || ((x.IsUnrandomized()) && x.GetVanillaDestination().region == CleanedID)));
                    var ValidMacroExits = instance.EntrancePool.AreaList.Values.SelectMany(x => x.MacroExits.Values.Where(x => (x.DestinationExit is not null  && x.DestinationExit.region == CleanedID) || x.GetVanillaDestination().region == CleanedID));
                    var ValidExits = ValidLoadingZoneExits.Concat(ValidMacroExits);
                    return ValidExits.Select(x => instance.GetLogicNameFromExit(x)).ToList();
                case LogicEntryType.item:
                    var ValidLocations = instance.LocationPool.Values.Where(x => x.Randomizeditem.Item is not null && x.Randomizeditem.Item == CleanedID && (x.CheckState != MiscData.CheckState.Unchecked || x.IsUnrandomized()));
                    return ValidLocations.Select(x => x.ID).ToList();
                case LogicEntryType.macro:
                    return new List<string> { CleanedID };
                default:
                    return new List<string>();
            }
        }
        private string GetDisplayName(string i)
        {
            return i + (LogicCalculation.LogicEntryAquired( i, new List<string>()) ? "*" : "");
        }

        private void btnGoTo_Click(object sender, EventArgs e)
        {
            Debug.WriteLine(state);
            if (state == 0) 
            { 
                var GotoList = CreatGotoList(GetAllLogicItemsByFromID(CurrentID), out List<object> DataEntriesOnly); 
                state = 1;
                PrintGotoData(GotoList);
            }
            else if (state == 1) 
            { 
                PrintLogicToLists(); 
                state = 0; 
            }
            UpdateUI();
        }

        private void btnGoBack_Click(object sender, EventArgs e)
        {
            CurrentID = GoBackList[^1];
            GoBackList.RemoveAt(GoBackList.Count -1);
            PrintLogicToLists();
            UpdateUI();
        }

        private void LBCond_DoubleClick(object sender, EventArgs e)
        {
            if (state == 0)
            {
                if (lbCond.SelectedItem is StandardListBoxItem SI && SI.tag is List<string> Cond)
                {
                    var GotoList = CreatGotoList(Cond, out List<object> DataEntriesOnly);
                    if (!DataEntriesOnly.Any()) { return; }
                    else if (DataEntriesOnly.Count == 1 && DataEntriesOnly.First() is StandardListBoxItem SLI && Cond.Count == 1)
                    {
                        GoBackList.Add(CurrentID);
                        CurrentID = SLI.tag.ToString();
                        PrintLogicToLists();
                    }
                    else
                    {
                        state = 1;
                        PrintGotoData(GotoList);
                    }
                }
            }
            else if (state == 1)
            {
                if (lbCond.SelectedItem is StandardListBoxItem SI && SI.tag is string str)
                {
                    GoBackList.Add(CurrentID);
                    CurrentID = str;
                    PrintLogicToLists(); 
                    state = 0;
                }
            }
            UpdateUI();
        }

        private void lbReq_DoubleClick(object sender, EventArgs e)
        {
            if (state == 0)
            {
                if (LBReq.SelectedItem is StandardListBoxItem SI && SI.tag is string req)
                {
                    var GotoList = CreatGotoList(new List<string> { req }, out List<object> DataEntriesOnly);
                    if (!DataEntriesOnly.Any()) { return; }
                    else if (DataEntriesOnly.Count == 1 && DataEntriesOnly.First() is StandardListBoxItem SLI)
                    {
                        GoBackList.Add(CurrentID);
                        CurrentID = SLI.tag.ToString();
                        PrintLogicToLists();
                    }
                    else
                    {
                        state = 1;
                        PrintGotoData(GotoList);
                    }
                }
            }
            else if (state == 1)
            {
                if (LBReq.SelectedItem is string str)
                {
                    if (CurrentID is not null) { GoBackList.Add(CurrentID); }
                    CurrentID = str;
                    PrintLogicToLists(); 
                    state = 0;
                }
            }
            UpdateUI();
        }

        private void lbReq_TextChanged(object sender, EventArgs e)
        {
            if (Updating) { return; }
            PrintGotoData(null);
            UpdateUI();
        }

        private void miscChk_CheckedChanged(object sender, EventArgs e)
        {
            if (Updating) { return; }
            UpdateUI();
            PrintLogicToLists();
        }
    }
}
