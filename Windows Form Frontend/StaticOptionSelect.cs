using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Windows_Form_Frontend
{
    public partial class StaticOptionSelect : Form
    {
        private TrackerSettings.OptionFile TempOptionFile;
        private InstanceData.TrackerInstance _Instance;
        public StaticOptionSelect(InstanceData.TrackerInstance Instance)
        {
            InitializeComponent();
            _Instance = Instance;
            TempOptionFile = _Instance.StaticOptions.OptionFile.Copy();
            PopulateOptions();
        }

        private class OptionLine
        {
            public string ToolTip;
            public string Description;
            public object Value;
            public object ValidValues;
            public Action<object> OnChange;
            public OptionLine(string _Desc, object _value, Action<object> _OnChange, string _ToolTip = null, object _ValidValues = null)
            {
                ToolTip = _ToolTip;
                Description = _Desc;
                Value = _value;
                ValidValues = _ValidValues;
                OnChange = _OnChange;
            }
        }

        private readonly MiscData.UILayout[] UILayoutsENUM = Utility.EnumAsArray<MiscData.UILayout>().ToArray();
        private readonly string[] UILayoutsSTRING = Utility.EnumAsStringArray<MiscData.UILayout>().ToArray();

        private List<OptionLine> OptionLines = new List<OptionLine>();
        private void PopulateOptions()
        {
            OptionLines.Clear();
            OptionLines.Add(new OptionLine("Check for Updates",
                TempOptionFile.CheckForUpdate, (val) => { TempOptionFile.ToggleUpdateCheck((bool)val); },
                "Should the tracker check for updates and notify you when a new one is available?"));
            OptionLines.Add(new OptionLine("UI Layout",
                UILayoutsSTRING[(int)TempOptionFile.WinformData.UILayout], (val) => { UpdateUILayout((string)val); },
                "How should the tracker arrange the list boxes.\n\n" +
                "L = Available Locations\nE = Available Entrances\nC = Checked Locations\nP = Pathfinder\n\n" +
                "With Entrances Enabled\nVertical:\nLE\nCP\n\nHorizontal\nLC\nEP\n\n" +
                "With Entrances Disabled\nVertical:\nL\nC\n\nHorizontal\nLC\n\n" +
                "Compact will only show one list box at a time.\nYou can select which list box is show in the \"View\" tab",
                UILayoutsSTRING));
            OptionLines.Add(new OptionLine("Max Undo Actions",
                TempOptionFile.MaxUndo, (val) => { TempOptionFile.SetMaxUndos((int)val); },
                "Max amount of undo states the tracker should store.\nThese can get quite large and eat up a lot of memory."));
            OptionLines.Add(new OptionLine("Show Unavailable Marked",
                TempOptionFile.ShowUnavailableMarkedLocations, (val) => { TempOptionFile.ToggleShowUnavailableMarked((bool)val); },
                "Should locations that are not logically available that have been marked manually or through hints be displayed in the available locations list?"));
            OptionLines.Add(new OptionLine("Separate Unavailable Marked",
                TempOptionFile.SeperateUnavailableMarkedLocations, (val) => { TempOptionFile.ToggleSeperateUnavailableMarked((bool)val); },
                "If the above option is true, should those locations be separated at the bottom of the list box?"));
            OptionLines.Add(new OptionLine("Show Entrance List",
                TempOptionFile.EntranceRandoFeatures, (val) => { TempOptionFile.ToggleEntranceFeatures((bool)val); },
                "Should an additional list box be added to show entrances?\nIf this is disable entrances will be shown in the valid locations list"));
            OptionLines.Add(new OptionLine("Couple Entrances",
                TempOptionFile.AutoCheckCoupleEntrances, (val) => { TempOptionFile.ToggleCheckCoupled((bool)val); },
                "When an entrance is checked, should the paired entrance be checked automatically?\nShould be disabled if playing with decoupled entrances"));
            OptionLines.Add(new OptionLine("Pathfinder Unrandomized Exits",
                TempOptionFile.ShowMacroExitsPathfinder, (val) => { TempOptionFile.TogglePathfinderMacros((bool)val); },
                "By default pathfinder will only show links between randomized exits, Should all links be shown instead?"));
            OptionLines.Add(new OptionLine("Pathfinder Redundant Paths",
                TempOptionFile.ShowRedundantPathfinder, (val) => { TempOptionFile.ToggleRedundantPaths((bool)val); },
                "Should longer paths be listed as an option in the pathfinder.\r\n\r\n" +
                "For example if the following path is available\r\n" +
                "A > B > C > D\r\n" +
                "The following path may also be shown as an option\r\n" +
                "A > X > Y > C > D\r\n\r\n" +
                "This can be useful if the path with less \"stops\" is actually longer to traverse in game.\r\n\r\n" +
                "WARNING: Enabling this option will exponentially increase the number of paths checked.\r\n" +
                "To prevent program instability, a cap is implemented which may prevent exceptionally long \r\n" +
                "paths from being found. if the only available path exceeds this cap, no path will be found."));
            OptionLines.Add(new OptionLine("ToolTips",
                TempOptionFile.WinformData.ShowEntryNameTooltip, (val) => { TempOptionFile.SetEntryTooltip((bool)val); },
                "Should the tracker display tooltips that show the full text of an entry when you mouse over it?"));
            OptionLines.Add(new OptionLine("Compressed Save File",
                TempOptionFile.CompressSave, (val) => { TempOptionFile.ToggleCompressSave((bool)val); },
                "Should the tracker compress it's save files to save space?\nThe save file will no longer be human readable and can't be edited manually."));
            OptionLines.Add(new OptionLine("Font Size",
                TempOptionFile.GetFont().Size, (val) => { TempOptionFile.SetFont(UpdateFont(null, (float)val)); },
                "The font size the tracker should use"));
            OptionLines.Add(new OptionLine("Font Family",
                TempOptionFile.GetFont().FontFamily.Name, (val) => { TempOptionFile.SetFont(UpdateFont((string)val, null)); },
                "The font family the tracker should use", FontFamily.Families.Select(x => x.Name).ToList()));
        }

        private void UpdateUILayout(string val)
        {
            TempOptionFile.SetUILayout(UILayoutsENUM[Array.IndexOf(UILayoutsSTRING, val)]);
        }

        private string UpdateFont(string FontFamily, float? FontSize)
        {
            Font CurrentFont = TempOptionFile.GetFont();
            FontFamily ??= CurrentFont.FontFamily.Name;
            FontSize ??= CurrentFont.Size;
            Font NewFont = new(FontFamily, (float)FontSize, FontStyle.Regular);

            richTextBox1.Font = NewFont;
            return WinFormUtils.ConvertFontToString(NewFont);
        }

        private Label GetToolTipLabel(string ToolTip)
        {
            Label label = new Label();
            label.AutoSize = true;
            label.BackColor = System.Drawing.SystemColors.ActiveBorder;
            label.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label.ForeColor = System.Drawing.SystemColors.ControlText;
            label.Location = new System.Drawing.Point(3, 180);
            label.Name = "label5";
            label.Size = new System.Drawing.Size(14, 18);
            label.TabIndex = 7;
            label.Text = "?";
            this.toolTip1.SetToolTip(label, ToolTip);
            return label;
        }
        static int ControlPadding = 3;

        private int XOffset(Control c, int width = -1)
        {
            if (width < 0) { width = c.Width; }
            return c.Location.X + width + ControlPadding;
        }

        private void PopulateListView()
        {
            var y = 9;
            var deltaY = 23;
            int MaxDescLength = 10;

            foreach (var line in OptionLines)
            {
                panel1.Controls.Clear();
                Label Test = new Label();
                Test.Text = line.Description;
                Test.AutoSize = true;
                Panel TestPanel = new Panel();
                TestPanel.Controls.Add(Test);
                if (MaxDescLength < Test.Width) { MaxDescLength = Test.Width; }
            }

            foreach (var item in OptionLines)
            {
                Label ToolTip = GetToolTipLabel(item.ToolTip);
                ToolTip.Location = new Point(ControlPadding, y);

                Label Description = new Label() { Text = item.Description, Location = new Point(XOffset(ToolTip), y), Width = MaxDescLength };

                if (item.Value is bool BoolVal)
                {
                    CheckBox toggle = new() { Location = Location = new Point(XOffset(Description), y), Checked = BoolVal };
                    toggle.CheckStateChanged += (sender, ee) => { item.OnChange(toggle.Checked); Utility.PrintObjectToConsole(TempOptionFile); };
                    panel1.Controls.Add(toggle);
                }
                else if (item.Value is float FloatVal)
                {
                    NumericUpDown nud = new NumericUpDown() { Location = new Point(XOffset(Description), y), Value = (decimal)FloatVal, Width = 40 };
                    nud.ValueChanged += (sender, e) => { item.OnChange((float)nud.Value); Utility.PrintObjectToConsole(TempOptionFile); };
                    panel1.Controls.Add(nud);
                }
                else if (item.Value is int intVal)
                {
                    NumericUpDown nud = new NumericUpDown() { Location = new Point(XOffset(Description), y), Value = intVal, Width = 40 };
                    nud.ValueChanged += (sender, e) => { item.OnChange((int)nud.Value); Utility.PrintObjectToConsole(TempOptionFile); };
                    panel1.Controls.Add(nud);
                }
                else if (item.Value is string StringValD && item.ValidValues is IEnumerable<string> ListVal)
                {
                    ComboBox comboBox = new ComboBox() { Location = new Point(XOffset(Description), y), Width = 100 };
                    int Ind = 0;
                    foreach (string i in ListVal)
                    {
                        comboBox.Items.Add(i);
                        if (i == StringValD) { comboBox.SelectedIndex = Ind; }
                        Ind++;
                    }
                    comboBox.SelectedValueChanged += (sender, e) => { item.OnChange(comboBox.SelectedItem); Utility.PrintObjectToConsole(TempOptionFile); };
                    panel1.Controls.Add(comboBox);
                }
                else
                {
                    var test = item.Value;
                    throw new Exception($"Could not handle value of type {test.GetType()}");
                    continue;
                }

                panel1.Controls.Add(ToolTip);
                panel1.Controls.Add(Description);

                y += deltaY;
            }
            richTextBox1.Font = TempOptionFile.GetFont();
            richTextBox1.Text = "Example 123";
        }

        private void StaticOptionSelect_Load(object sender, EventArgs e)
        {
            PopulateListView();
        }

        private void Button_Apply_To_Current(object sender, EventArgs e)
        {
            _Instance.StaticOptions.OptionFile = TempOptionFile;
            this.Close();
        }

        private void Button_Set_Default(object sender, EventArgs e)
        {
            TrackerSettings.WriteOptionFile(TempOptionFile);
        }

        private void Button_ResetFont(object sender, EventArgs e)
        {
            TempOptionFile.WinformData.FormFont = WinFormUtils.ConvertFontToString(SystemFonts.DefaultFont);
            PopulateOptions();
            PopulateListView();
        }
    }
}
