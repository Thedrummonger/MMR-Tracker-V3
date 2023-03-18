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

namespace Windows_Form_Frontend
{
    public partial class VariableInputWindow : Form
    {
        LogicObjects.TrackerInstance _Instance;
        List<object> _InputItems;
        VarType varType;
        public VariableInputWindow(IEnumerable<object> InputItems, LogicObjects.TrackerInstance Instance)
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
            if (!_InputItems.Any())
            {
                this.Close();
                return;
            }
            if (_InputItems[0] is HintData.HintObject hintObject)
            {
                SetUIElements(true, $"Enter Hint at {hintObject.GetDictEntry(_Instance).Name}", "Set Hint", hintObject.GetDictEntry(_Instance).Name );
                varType = VarType.Gossip;
            }
            else if (_InputItems[0] is OptionData.TrackerVar IntVariableObject)
            {
                if (IntVariableObject.GetType() == MiscData.VariableEntryType.varint)
                {
                    SetUIElements(false, $"Set value for {IntVariableObject.Name??IntVariableObject.ID}", "Set Value", "Integer: " + (IntVariableObject.Name ?? IntVariableObject.ID));
                    numericUpDown1.Value = IntVariableObject.GetValue(_Instance);
                    varType = VarType.Number;
                }
                else if (IntVariableObject.GetType() == MiscData.VariableEntryType.varstring)
                {
                    SetUIElements(true, $"Set value for {IntVariableObject.Name ?? IntVariableObject.ID}", "Set Value", "String: " + (IntVariableObject.Name ?? IntVariableObject.ID));
                    textBox1.Text = IntVariableObject.GetValue(_Instance);
                    varType = VarType.String;
                }
                else if (IntVariableObject.GetType() == MiscData.VariableEntryType.varlist)
                {
                    SetUIElements(true, $"Enter values as a comman seperated List", $"Set Values", "List: " + (IntVariableObject.Name ?? IntVariableObject.ID));
                    textBox1.Text = string.Join(",", IntVariableObject.GetValue(_Instance));
                    varType = VarType.ListOf;
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            switch (varType)
            {
                case VarType.Gossip:
                    (_InputItems[0] as HintData.HintObject).HintText = textBox1.Text;
                    break;
                case VarType.Number:
                    (_InputItems[0] as OptionData.TrackerVar).Value = (int)numericUpDown1.Value;
                    break;
                case VarType.String:
                    (_InputItems[0] as OptionData.TrackerVar).Value = textBox1.Text;
                    break;
                case VarType.ListOf:
                    (_InputItems[0] as OptionData.TrackerVar).Value = textBox1.Text.Split(',').ToList();
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
