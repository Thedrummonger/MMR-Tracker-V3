﻿using MMR_Tracker_V3;
using MMR_Tracker_V3.Logic;
using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TDMUtils;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace Windows_Form_Frontend
{
    public partial class ShowLogic : Form
    {
        public enum FormState
        {
            showLogic,
            GoTo
        }

        public string CurrentID;
        private readonly ListBoxHeightData ReqLBHeightData;
        private readonly MMR_Tracker_V3.TrackerObjects.InstanceData.InstanceContainer IC;
        private bool Updating = false;
        private readonly List<CheckBox> TimeCheckBoxes;
        private readonly List<string> GoBackList = new();
        private List<object> CurrentGotoData;
        public FormState state = FormState.showLogic;
        public HashSet<string> AllLogicIDs;
        public ShowLogic(string id, MMR_Tracker_V3.TrackerObjects.InstanceData.InstanceContainer _instanceContainer)
        {
            InitializeComponent();
            CurrentID = id;
            IC = _instanceContainer;
            ReqLBHeightData = new ListBoxHeightData()
            {
                Main = LBReq,
                TopPosFull = lbCond.Location.Y,
                TopPosCut = LBReq.Location.Y,
                BottomPosFull = ND3.Location.Y + ND3.Height,
                BottomPosCut = LBReq.Location.Y + LBReq.Height,
            };
            TimeCheckBoxes = [ND1, NN1, ND2, NN2, ND3, NN3, SD1, SN1, SD2, SN2, SD3, SN3];
            LocationChecker.CheckStateChanged += TrackerDataHandeling_CheckedObjectsUpdate;
            LocationChecker.UserOptionUpdated += TrackerDataHandeling_UserOptionUpdated;
        }

        private void ShowLogic_FormClosed(object sender, FormClosedEventArgs e)
        {
            LocationChecker.CheckStateChanged -= TrackerDataHandeling_CheckedObjectsUpdate;
            LocationChecker.UserOptionUpdated -= TrackerDataHandeling_UserOptionUpdated;
        }

        private void TrackerDataHandeling_UserOptionUpdated(List<object> list, MMR_Tracker_V3.TrackerObjects.InstanceData.TrackerInstance instance)
        {
            TrackerDataUpdated();
        }

        private void TrackerDataHandeling_CheckedObjectsUpdate(List<object> arg1, MMR_Tracker_V3.TrackerObjects.InstanceData.TrackerInstance arg2)
        {
            TrackerDataUpdated();
        }

        private void TrackerDataUpdated()
        {
            if (this.IsDisposed)
            {
                Debug.WriteLine("Show logic form was still listening for events after being disposed");
                return;
            }
            if (state != FormState.showLogic) { return; }
            PrintLogicToLists();
            UpdateUI();
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
            LBReq.Font = IC.Instance.StaticOptions.OptionFile.GetFont();
            lbCond.Font = IC.Instance.StaticOptions.OptionFile.GetFont();
            if (state == FormState.showLogic)
            {
                var AlteredLogic = IC.Instance.GetLogic(CurrentID, true);
                var UnAlteredLogic = IC.Instance.GetLogic(CurrentID, false);
                bool Literal = CurrentID.IsLiteralID(out string LogicItem);
                var LogicItemObject = IC.Instance.GetCheckableLocationByID(LogicItem, Literal);
                string Availablility = LogicItemObject.Available ? "*" : "";
                string typeDisplay = GetObjectTypeString(LogicItemObject);
                bool hasTimeLogic = UnAlteredLogic.TimeAvailable != TimeOfDay.None || UnAlteredLogic.TimeSetup != TimeOfDay.None;
                bool ShowTimeLogic = hasTimeLogic && chkShowTime.Checked;
                //Title
                string EntryName = IC.Instance.GetDynamicObjName((object)LogicItemObject) ?? LogicItem;
                if (EntryName != LogicItem) { EntryName = $"{EntryName} ({LogicItem})"; }
                this.Text = $"{typeDisplay}: {EntryName}{Availablility}";
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
                groupBox1.Visible = ShowTimeLogic;
                groupBox2.Visible = ShowTimeLogic;
                BTNGotTo.Visible = true;
                //Enable Items
                textBox1.Enabled = false;
                btnGoBack.Enabled = GoBackList.Count != 0;
                chkShowUnaltered.Enabled = !AlteredLogic.HasIdenticalLogic(UnAlteredLogic);
                chkShowTime.Enabled = hasTimeLogic;
                //Resize Items
                tlbReq.RowStyles[3] = ShowTimeLogic ? new RowStyle(SizeType.Absolute, 80F) : new RowStyle(SizeType.Absolute, 0F);
                tlbReq.RowStyles[1] = new RowStyle(SizeType.Absolute, 0F);
            }
            else if (state == FormState.GoTo)
            {
                //Title
                this.Text = $"Select Logic Entry";
                //Text Changes
                label1.Text = "All Logic Items";
                label2.Text = "Logic Items in Selected Entry";
                BTNGotTo.Text = "Cancel";
                //Hide/Show Items
                textBox1.Visible = true;
                groupBox1.Visible = false;
                groupBox2.Visible = false;
                chkShowTime.Visible = false;
                chkShowUnaltered.Visible = false;
                btnGoBack.Visible = false;
                BTNGotTo.Visible = CurrentID is not null;
                //Enable Items
                textBox1.Enabled = true;
                btnGoBack.Enabled = false;
                chkShowUnaltered.Enabled = false;
                chkShowTime.Enabled = false;
                tlbReq.RowStyles[3] = new RowStyle(SizeType.Absolute, 0F);
                //Resize Items
                tlbReq.RowStyles[1] = new RowStyle(SizeType.Absolute, 25F);
            }
            Updating = false;
        }

        public string GetObjectTypeString(CheckableLocation CO)
        {
            return CO.LocationType().ToString().ConvertToCamelCase();
        }

        public bool GetAvailable(MMRData.JsonFormatLogicItem Logic, CheckableLocation co)
        {
            string Area = co is EntranceData.EntranceRandoExit EO ? EO.GetParentArea().ID : null;
            return IC.logicCalculation.CalculatReqAndCond(Logic, co.ID, Area);
        }

        private void ShowLogic_Load(object sender, EventArgs e)
        {
            AllLogicIDs = GetAllLogicIDs();
            if (CurrentID is not null && IC.Instance.GetLogic(CurrentID) is not null)
            {
                state = FormState.showLogic;
                PrintLogicToLists();
            }
            else
            {
                state = FormState.GoTo;
                List<string> ItemIds = IC.Instance.ItemPool.Keys.ToList();
                List<string> MacroIds = IC.Instance.MacroPool.Keys.ToList();
                List<string> AreaIds = IC.Instance.AreaPool.Keys.ToList();
                PrintGotoData(CreatGotoList(ItemIds.Concat(MacroIds).Concat(AreaIds).ToList(), out _));
            }
            UpdateUI();
        }

        private void ShowLogic_ResizeEnd(object sender, EventArgs e)
        {

        }

        public List<string> GetAllLogicItemsByFromID(string ID)
        {
            var OriginalLogic = IC.Instance.GetLogic(ID, false);
            var AlteredLogic = IC.Instance.GetLogic(ID);
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
            Updating = true;
            LBReq.Items.Clear();
            lbCond.Items.Clear();
            var Logic = IC.Instance.GetLogic(CurrentID, !chkShowUnaltered.Checked);
            bool Literal = CurrentID.IsLiteralID(out string LogicItem);
            var LocationObject = IC.Instance.GetCheckableLocationByID(LogicItem, Literal);
            UpdateTimeCheckboxes(Logic);

            numericUpDown1.Value = AllLogicIDs.ToList().IndexOf(CurrentID);

            foreach (var i in Logic.RequiredItems.Where(x => !bool.TryParse(x, out bool bout) || !bout))
            {
                StandardListBoxItem boxItem = new StandardListBoxItem() { Display = GetDisplayName(i), Tag = i };
                LBReq.Items.Add(boxItem);
            }
            if (LocationObject is EntranceData.EntranceRandoExit EO && !Logic.RequiredItems.Contains(EO.GetParentArea().ID))
            {
                StandardListBoxItem boxItem = new StandardListBoxItem() { Display = GetDisplayName(EO.GetParentArea().ID), Tag = EO.GetParentArea().ID };
                LBReq.Items.Add(boxItem);
            }
            foreach (var cond in Logic.ConditionalItems)
            {
                StandardListBoxItem boxItem = new StandardListBoxItem() { Display = string.Join(" | ", cond.Select(GetDisplayName)), Tag = cond };
                lbCond.Items.Add(boxItem);
            }
            Updating = false;
        }

        private HashSet<string> GetAllLogicIDs()
        {
            HashSet<string> LogicIds = new HashSet<string>();
            foreach (var i in IC.Instance.LogicFile.Logic)
            {
                LogicIds.Add(i.Id);
            }
            foreach (var i in IC.Instance.LogicDictionary.AdditionalLogic)
            {
                if (LogicIds.Contains(i.Id)) { continue; }
                LogicIds.Add(i.Id);
            }
            foreach (var i in IC.Instance.RuntimeLogic.Values)
            {
                if (LogicIds.Contains(i.Id)) { continue; }
                LogicIds.Add(i.Id);
            }
            return LogicIds;
        }

        private void PrintGotoData(List<object> GotoData, bool SearchGotoData = false)
        {
            LBReq.Items.Clear();

            foreach (var i in AllLogicIDs)
            {
                var entry = IC.Instance.GetCheckableLocationByID(i, false);
                if (!SearchStringParser.FilterSearch(IC.Instance, entry, textBox1.Text, i)) { continue; }
                LBReq.Items.Add(i);
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
            Dictionary<string, LogicItemTypes> LogicItems = new Dictionary<string, LogicItemTypes>();
            foreach (var i in set)
            {
                LogicItems[i] = IC.Instance.GetLogicItemData(i).Type;
            }
            var GotoData = CreateGotoDataFromList(LogicItems);
            DataEntriesOnly = GotoData.Where(x => x is StandardListBoxItem).ToList();
            return GotoData;
        }

        private List<object> CreateGotoDataFromList(Dictionary<string, LogicItemTypes> GotoList)
        {
            foreach (var i in AddItemsFromFunction(GotoList))
            {
                if (!GotoList.ContainsKey(i.Key)) { GotoList.Add(i.Key, i.Value); }
            }
            GotoList = GotoList.OrderBy(x => x.Value).ThenBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            List<object> list = new List<object>();
            LogicItemTypes CurrentType = LogicItemTypes.error;
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
                        case LogicItemTypes.Area:
                            LitEntry.Tag = c;
                            LitEntry.Display = $"{CleanedID}: {c}";
                            break;
                        case LogicItemTypes.item:
                            LitEntry.Tag = c;
                            LitEntry.Display = $"{IC.Instance.GetItemByID(CleanedID)?.GetDictEntry()?.GetName() ?? CleanedID}: {IC.Instance.GetLocationByID(c)?.GetDictEntry()?.GetName() ?? c}";
                            break;
                        case LogicItemTypes.macro:
                        default:
                            LitEntry.Tag = c;
                            LitEntry.Display = CleanedID;
                            break;
                    }
                    list.Add(LitEntry);
                }
            }
            CheckableLocationTypes? checkableLocationTypes = null;
            Dictionary<string, CheckableLocationTypes> LocationsGotoList = GetLocationsFromFunctions(GotoList).OrderBy(x => x.Value).ThenBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            foreach (var i in LocationsGotoList)
            {
                if (checkableLocationTypes is null || checkableLocationTypes != i.Value)
                {
                    list.Add(WinFormUtils.CreateDivider(lbCond, $"{i.Value}"));
                    checkableLocationTypes = i.Value;
                }
                var LitEntry = new StandardListBoxItem();
                switch (checkableLocationTypes)
                {
                    case CheckableLocationTypes.location:
                        LitEntry.Tag = i.Key;
                        LitEntry.Display = IC.Instance.GetLocationByID(i.Key)?.GetDictEntry()?.GetName() ?? i.Key;
                        break;
                    case CheckableLocationTypes.Exit:
                    default:
                        LitEntry.Tag = i.Key;
                        LitEntry.Display = i.Key;
                        break;
                }
                list.Add(LitEntry);
            }
            return list;
        }

        private Dictionary<string, CheckableLocationTypes> GetLocationsFromFunctions(Dictionary<string, LogicItemTypes> CurrentList)
        {
            Dictionary<string, CheckableLocationTypes> NewList = new Dictionary<string, CheckableLocationTypes>();
            foreach (var i in CurrentList.Where(x => x.Value == LogicItemTypes.function))
            {
                LogicFunctions.IsLogicFunction(i.Key, out string Func, out string Param);
                if (Func.In("contains", "check", "available"))
                {
                    var Data = Param.Split(',').Select(x => x.Trim()).ToArray();
                    if (IC.Instance.GetLogicEntryCollectionByID(Data[0]) is not null)
                    {
                        AddFromVariable(Data[0]);
                    }
                    else if (!CurrentList.ContainsKey(Data[0]))
                    {
                        if (IC.Instance.GetCheckableLocationByID(Data[0], false, out CheckableLocation CO)) { NewList[Data[0]] = CO.LocationType(); }
                    }
                }
            }
            return NewList;
            void AddFromVariable(string Entry)
            {
                IC.Instance.MultipleItemEntry(Entry, out string LogicItem, out int Amount);
                if (!IC.Instance.LogicEntryCollections.TryGetValue(LogicItem, out OptionData.LogicEntryCollection variable)) { return; }
                foreach (string varEntry in variable.GetValue(IC.Instance))
                {
                    if (IC.Instance.GetCheckableLocationByID(varEntry, false, out CheckableLocation CO)) { NewList[varEntry] = CO.LocationType(); }
                }
            }
        }

        private Dictionary<string, LogicItemTypes> AddItemsFromFunction(Dictionary<string, LogicItemTypes> CurrentList)
        {
            Dictionary<string, LogicItemTypes> NewList = new Dictionary<string, LogicItemTypes>();
            foreach (var i in CurrentList.Where(x => x.Value == LogicItemTypes.function))
            {
                LogicFunctions.IsLogicFunction(i.Key, out string Func, out string Param);
                if (Func == "contains")
                {
                    var Data = Param.Split(',').Select(x => x.Trim()).ToArray();
                    var ItemType = IC.Instance.GetLogicItemData(Data[1]).Type;
                    if (ItemType == LogicItemTypes.LogicEntryCollection)
                    {
                        AddFromVariable(Data[1]);
                    }
                    else if (!CurrentList.ContainsKey(Data[1]))
                    {
                        NewList[Data[1]] = ItemType;
                    }
                }
            }
            foreach (var i in CurrentList.Where(x => x.Value == LogicItemTypes.LogicEntryCollection))
            {
                AddFromVariable(i.Key);
            }
            return NewList;

            void AddFromVariable(string Entry)
            {
                IC.Instance.MultipleItemEntry(Entry, out string LogicItem, out int Amount);
                if (!IC.Instance.LogicEntryCollections.TryGetValue(LogicItem, out OptionData.LogicEntryCollection variable)) { return; }
                foreach (string varEntry in variable.GetValue(IC.Instance))
                {
                    var ItemType = IC.Instance.GetLogicItemData(varEntry).Type;
                    NewList[varEntry] = ItemType;
                }
            }
        }

        private void UpdateTimeCheckboxes(MMRData.JsonFormatLogicItem OriginalLogic)
        {
            int Index = 0;
            foreach (var i in TimeCheckBoxes)
            {
                if (Index < 6) { i.Checked = ((((int)OriginalLogic.TimeAvailable >> Index) & 1) == 1); }
                else { i.Checked = ((((int)OriginalLogic.TimeSetup >> Index - 6) & 1) == 1); }
                Index++;
            }
        }
        private List<string> GetChecksContainingSelectedID(string ID, out LogicItemTypes Type, out string OutCleanedID)
        {
            var LogicItem = IC.Instance.GetLogicItemData(ID);
            OutCleanedID = LogicItem.CleanID;
            Type = LogicItem.Type;
            switch (LogicItem.Type)
            {
                case LogicItemTypes.Area:
                    var ValidLoadingZoneExits = IC.Instance.GetAllRandomizableExits().Where(x =>
                        (x.DestinationExit is not null && x.DestinationExit.region == LogicItem.CleanID && x.CheckState != MiscData.CheckState.Unchecked) ||
                        (x.IsUnrandomized() && x.GetVanillaDestination().region == LogicItem.CleanID));
                    var ValidMacroExits = IC.Instance.GetMacroExits().Where(x =>
                        (x.DestinationExit is not null && x.DestinationExit.region == LogicItem.CleanID) ||
                        x.GetVanillaDestination().region == LogicItem.CleanID);
                    var ValidExits = ValidLoadingZoneExits.Concat(ValidMacroExits);
                    return ValidExits.Select(x => x.ID).ToList();
                case LogicItemTypes.item:
                    var ValidLocations = IC.Instance.LocationPool.Values.Where(x =>
                        (x.Randomizeditem.Item is not null && x.Randomizeditem.Item == LogicItem.CleanID && x.CheckState != MiscData.CheckState.Unchecked) ||
                        ((x.IsUnrandomized() || x.SingleValidItem is not null) && x.GetItemAtCheck() == LogicItem.CleanID));
                    return ValidLocations.Select(x => x.ID).ToList();
                case LogicItemTypes.macro:
                    return new List<string> { LogicItem.CleanID };
                default:
                    return new List<string>();
            }
        }
        private string GetDisplayName(string i)
        {
            var data = IC.Instance.GetLogicItemData(i);
            if (data.Type == LogicItemTypes.LogicEntryCollection) { Debug.WriteLine(((OptionData.LogicEntryCollection)data.Object).GetValue(IC.Instance).ToFormattedJson()); }
            return i + (data.IntOptionCount is null ? "" : $"[{data.IntOptionCount.Value}]") + (IC.logicCalculation.LogicEntryAquired(i) ? "*" : "");
        }

        private void btnGoTo_Click(object sender, EventArgs e)
        {
            Debug.WriteLine(state);
            if (state == FormState.showLogic)
            {
                var GotoList = CreatGotoList(GetAllLogicItemsByFromID(CurrentID), out List<object> DataEntriesOnly);
                state = FormState.GoTo;
                PrintGotoData(GotoList);
            }
            else if (state == FormState.GoTo)
            {
                PrintLogicToLists();
                state = FormState.showLogic;
            }
            UpdateUI();
        }

        private void btnGoBack_Click(object sender, EventArgs e)
        {
            CurrentID = GoBackList[^1];
            GoBackList.RemoveAt(GoBackList.Count - 1);
            PrintLogicToLists();
            UpdateUI();
        }

        private void LBCond_DoubleClick(object sender, EventArgs e)
        {
            if (state == FormState.showLogic)
            {
                if (lbCond.SelectedItem is StandardListBoxItem SI && SI.Tag is List<string> Cond)
                {
                    var GotoList = CreatGotoList(Cond, out List<object> DataEntriesOnly);
                    if (!DataEntriesOnly.Any()) { return; }
                    else if (DataEntriesOnly.Count == 1 && DataEntriesOnly.First() is StandardListBoxItem SLI && Cond.Count == 1)
                    {
                        GoBackList.Add(CurrentID);
                        CurrentID = SLI.Tag.ToString();
                        PrintLogicToLists();
                    }
                    else
                    {
                        state = FormState.GoTo;
                        PrintGotoData(GotoList);
                    }
                }
            }
            else if (state == FormState.GoTo)
            {
                if (lbCond.SelectedItem is StandardListBoxItem SI && SI.Tag is string str)
                {
                    GoBackList.Add(CurrentID);
                    CurrentID = str;
                    PrintLogicToLists();
                    state = FormState.showLogic;
                }
            }
            UpdateUI();
        }

        private void lbReq_DoubleClick(object sender, EventArgs e)
        {
            if (state == FormState.showLogic)
            {
                if (LBReq.SelectedItem is StandardListBoxItem SI && SI.Tag is string req)
                {
                    var GotoList = CreatGotoList(new List<string> { req }, out List<object> DataEntriesOnly);
                    if (!DataEntriesOnly.Any()) { return; }
                    else if (DataEntriesOnly.Count == 1 && DataEntriesOnly.First() is StandardListBoxItem SLI)
                    {
                        GoBackList.Add(CurrentID);
                        CurrentID = SLI.Tag.ToString();
                        PrintLogicToLists();
                    }
                    else
                    {
                        state = FormState.GoTo;
                        PrintGotoData(GotoList);
                    }
                }
            }
            else if (state == FormState.GoTo)
            {
                if (LBReq.SelectedItem is string str)
                {
                    if (CurrentID is not null) { GoBackList.Add(CurrentID); }
                    CurrentID = str;
                    PrintLogicToLists();
                    state = FormState.showLogic;
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

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (Updating) { return; }
            if (numericUpDown1.Value < 0) { numericUpDown1.Value = 0; }
            else if (numericUpDown1.Value >= AllLogicIDs.Count) { numericUpDown1.Value = AllLogicIDs.Count - 1; }
            CurrentID = AllLogicIDs.ToList()[(int)numericUpDown1.Value];
            PrintLogicToLists();
            UpdateUI();
        }
    }
}
