using MathNet.Numerics;
using MMR_Tracker_V3;
using MMR_Tracker_V3.NetCode;
using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace Windows_Form_Frontend
{
    public partial class CheckItemForm : Form
    {
        List<object> _CheckList;
        public List<ManualCheckObjectResult> _Result = new List<ManualCheckObjectResult>();
        InstanceContainer _Container;
        bool _RestrictItems;
        public CheckItemForm(IEnumerable<object> ManualChecks, InstanceContainer Container, bool RestrictItems = true)
        {
            InitializeComponent();
            _CheckList = ManualChecks.ToList();
            _Container = Container;
            _RestrictItems = RestrictItems;
        }

        private void CheckItemForm_Load(object sender, EventArgs e)
        {
            WriteNextItem();
        }

        private void CheckItemForm_Shown(object sender, EventArgs e)
        {
            textBox1.Text = "";
            numericUpDown1.Value = _Container.netConnection.PlayerID;
            textBox1.Focus();
        }

        private void FormatUIItems(bool Multiworld, bool Button, string ButtonText)
        {
            listBox1.Font = _Container.Instance.StaticOptions.OptionFile.GetFont();
            numericUpDown1.Visible = Multiworld;
            label2.Visible = Multiworld;
            tableLayoutPanel1.ColumnStyles[2] = Multiworld ? new ColumnStyle(SizeType.Absolute, 55F) : new ColumnStyle(SizeType.Absolute, 0F);
            button1.Visible = Button;
            button1.Text = ButtonText;
        }

        private void WriteNextItem()
        {
            if (!_CheckList.Any())
            {
                this.Close();
                return;
            }
            textBox1.Text = "";
            textBox1.Focus();
            PrintItems();
        }

        private void PrintItems()
        {
            if (_CheckList[0] is LocationData.LocationObject LocationObject)
            {
                writeItemObjectsAtLocation(LocationObject);
            }
            else if (_CheckList[0] is OptionData.ChoiceOption OptionObject)
            {
                WriteTrackerOption(OptionObject);
            }
            else if (_CheckList[0] is EntranceData.EntranceRandoExit ExitObject)
            {
                WriteTrackerExits(ExitObject);
            }
            else if (_CheckList[0] is null)
            {
                WriteBasicItemSelect();
            }
            else
            {
                _CheckList.RemoveAt(0);
                WriteNextItem();
            }
        }

        private void WriteTrackerExits(EntranceData.EntranceRandoExit exitObject)
        {
            FormatUIItems(false, false, "");
            this.Text = $"Select Destination of Exit {exitObject.GetParentArea().ID} -> {exitObject.ExitID}";
            List<EntranceData.EntranceRandoDestination> EnteredItems = _Container.Instance.GetAllLoadingZoneDestinations(textBox1.Text);
            listBox1.DataSource = EnteredItems;
        }

        private void WriteTrackerOption(OptionData.ChoiceOption Option)
        {
            FormatUIItems(false, false, "");
            var EnteredItems = new List<OptionData.OptionValue>();
            this.Text = "Select Value for Option " + Option.Name ?? Option.ID;
            foreach (var i in Option.ValueList)
            {
                EnteredItems.Add(i.Value);
            }
            listBox1.DataSource = EnteredItems;
        }

        private void writeItemObjectsAtLocation(LocationData.LocationObject Location)
        {
            FormatUIItems(_Container.netConnection.OnlineMode == NetData.OnlineMode.Multiworld, true, "Set Junk");
            this.Text = "Select Item at " + Location.GetDictEntry().GetName();
            bool FoLocalPlayer = (numericUpDown1.Value < 0 || numericUpDown1.Value == _Container.netConnection.PlayerID);
            List<ItemData.ItemObject> EnteredItems = _Container.Instance.GetValidItemsForLocation(Location, textBox1.Text, (!FoLocalPlayer || !_RestrictItems));
            listBox1.DataSource = EnteredItems;
        }

        private void WriteBasicItemSelect()
        {
            FormatUIItems(false, false, "");
            this.Text = "Select Item";
            List<ItemData.ItemObject> EnteredItems = [];
            List<string> Names = [];
            foreach (var i in _Container.Instance.ItemPool.Values)
            {
                if (string.IsNullOrWhiteSpace(i.GetDictEntry().GetName())) { continue; }
                i.DisplayName = i.GetDictEntry().GetName();
                if (!SearchStringParser.FilterSearch(_Container.Instance, i, textBox1.Text, i.DisplayName)) { continue; }
                if (!EnteredItems.Contains(i) && !Names.Contains(i.ToString()))
                {
                    Names.Add(i.ToString());
                    EnteredItems.Add(i);
                }
            }
            listBox1.DataSource = EnteredItems;
        }

        private void ApplySelection(bool ButtonClick = false)
        {
            var Result = new ManualCheckObjectResult();
            if ((listBox1.SelectedIndex < 0 || listBox1.SelectedItem is MiscData.Areaheader || listBox1.SelectedItem is MiscData.Divider) && !ButtonClick) { return; }
            if (_CheckList[0] is LocationData.LocationObject LocationObject)
            {
                if (ButtonClick)
                {
                    _Result.Add(Result.SetItemLocation(LocationObject, "JUNK"));
                }
                else
                {
                    int OwningPlayer = (int)numericUpDown1.Value;
                    if (_Container.netConnection.PlayerID > -1 && _Container.netConnection.PlayerID == OwningPlayer) { OwningPlayer = -1; }
                    _Result.Add(Result.SetItemLocation(LocationObject, ((ItemData.ItemObject)listBox1.SelectedItem).ID, OwningPlayer));
                }
            }
            else if (_CheckList[0] is OptionData.ChoiceOption OptionObject)
            {
                OptionData.OptionValue SelectedValue = listBox1.SelectedItem as OptionData.OptionValue;
                _Result.Add(Result.SetChoiceOption(OptionObject, SelectedValue.ID));
            }
            else if (_CheckList[0] is EntranceData.EntranceRandoExit ExitObject)
            {
                _Result.Add(Result.SetExitDestination(ExitObject, (EntranceData.EntranceRandoDestination)listBox1.SelectedItem));
            }
            else if (_CheckList[0] is null)
            {
                _Result.Add(Result.SetItemLocation(null, ((ItemData.ItemObject)listBox1.SelectedItem).ID, -1));
            }
            _CheckList.RemoveAt(0);
            WriteNextItem();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            PrintItems();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            ApplySelection();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ApplySelection(true);
        }

        private void textBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                textBox1.Text = "";
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            PrintItems();
        }
    }
}
