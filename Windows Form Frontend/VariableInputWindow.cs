using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace Windows_Form_Frontend
{
    public partial class VariableInputWindow : Form
    {
        MiscData.InstanceContainer _Instance;
        List<object> _InputItems;
        public List<ManualCheckObjectResult> _Result = new List<ManualCheckObjectResult>();
        VarType varType;
        public VariableInputWindow(IEnumerable<object> InputItems, MiscData.InstanceContainer Instance)
        {
            InitializeComponent();
            _InputItems = InputItems.ToList();
            _Instance = Instance;
        }

        private void VariableInputWindow_Load(object sender, EventArgs e)
        {
            showNextItem();
        }

        private void showNextItem()
        {
            numericUpDown1.Maximum = decimal.MaxValue;
            numericUpDown1.Minimum = decimal.MinValue;
            if (!_InputItems.Any())
            {
                this.Close();
                return;
            }
            if (_InputItems[0] is HintData.HintObject hintObject)
            {
                SetUIElements(true, $"Enter Hint at {hintObject.GetDictEntry(_Instance.Instance).Name}", "Set Hint", hintObject.GetDictEntry(_Instance.Instance).Name );
                varType = VarType.Gossip;
            }
            else if (_InputItems[0] is OptionData.IntOption IntVariableObject)
            {
                SetUIElements(false, $"Set value for {IntVariableObject.Name??IntVariableObject.ID}", "Set Value", "Integer: " + (IntVariableObject.Name ?? IntVariableObject.ID));
                numericUpDown1.Value = IntVariableObject.Value;
                numericUpDown1.Maximum = IntVariableObject.Max;
                numericUpDown1.Minimum = IntVariableObject.Min;
                varType = VarType.Number;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            switch (varType)
            {
                case VarType.Gossip:
                    _Result.Add(new(_InputItems[0] as HintData.HintObject, textBox1.Text));
                    //(_InputItems[0] as HintData.HintObject).HintText = textBox1.Text;
                    break;
                case VarType.Number:
                    _Result.Add(new(_InputItems[0] as OptionData.IntOption, (int)numericUpDown1.Value));
                    //(_InputItems[0] as OptionData.IntOption).Value = (int)numericUpDown1.Value;
                    break;
            }
            _InputItems.RemoveAt(0);
            showNextItem();
        }

        public void SetUIElements(bool text, string Label, string buttonPrompt, string Title)
        {
            MinimizeBox = false;
            MaximizeBox = false;
            numericUpDown1.Visible = !text;
            textBox1.Visible = text;
            numericUpDown1.Location = new Point(label1.Location.X, numericUpDown1.Location.Y);
            textBox1.Location = new Point(label1.Location.X, numericUpDown1.Location.Y);
            numericUpDown1.Width = button1.Location.X - label1.Location.X - 10;
            textBox1.Width = button1.Location.X - label1.Location.X - 10;
            label1.Text = Label;
            button1.Text = buttonPrompt;
            this.Text = Title;
        }

        private enum VarType
        {
            Gossip,
            String,
            Number,
            ListOf,
            None
        }

    }
}
