using Microsoft.VisualBasic;
using MMR_Tracker_V3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;

namespace Windows_Form_Frontend
{
    public static class WinFormUtils
    {
        public class DropDownOptionTree
        {
            public DropDownOptionTree Parent { get; set; } = null;
            public ToolStripMenuItem MenuItem { get; set; }
            public string GroupID { get; set; } = "Root";
            public List<ToolStripMenuItem> MenuItems { get; set; } = new List<ToolStripMenuItem>();
            public Dictionary<string, DropDownOptionTree> SubGroups { get; set; } = new Dictionary<string, DropDownOptionTree>();
        }

        public static MMR_Tracker_V3.TrackerObjects.MiscData.Divider CreateDivider(object containerObject, string DividerText = "")
        {
            Font font;
            Graphics g;
            int width;
            if (containerObject is ListView LVcontainer)
            {
                font = LVcontainer.Font;
                width = LVcontainer.Width - (LVcontainer.CheckBoxes ? 45 : 0);
                g = LVcontainer.CreateGraphics();
            }
            else if (containerObject is ListBox LBcontainer)
            {
                font = LBcontainer.Font;
                width = LBcontainer.Width;
                g = LBcontainer.CreateGraphics();
            }
            else if (containerObject is ComboBox cmb)
            {
                font = cmb.Font;
                width = cmb.Width;
                g = cmb.CreateGraphics();
            }
            else
            {
                return new MMR_Tracker_V3.TrackerObjects.MiscData.Divider(DividerText);
            }

            string Divider = DividerText;
            while (true)
            {
                string newDivider = Divider;
                if (string.IsNullOrWhiteSpace(DividerText)) { newDivider += "="; }
                else { newDivider = $"={newDivider}="; }
                if ((int)g.MeasureString(newDivider, font).Width < width) { Divider = newDivider; }
                else { break; }
            }
            return new MMR_Tracker_V3.TrackerObjects.MiscData.Divider(Divider);
        }

        public static void PrintMessageToListBox(object containerObject, string Text = "")
        {
            Font font;
            Graphics g;
            int width;
            dynamic container;
            if (containerObject is ListView LVcontainer)
            {
                container = LVcontainer;
                font = LVcontainer.Font;
                width = LVcontainer.Width - (LVcontainer.CheckBoxes ? 45 : 0);
                g = LVcontainer.CreateGraphics();
            }
            else if (containerObject is ListBox LBcontainer)
            {
                container = LBcontainer;
                font = LBcontainer.Font;
                width = LBcontainer.Width;
                g = LBcontainer.CreateGraphics();
            }
            else { return; }

            container.Items.Clear();
            string CurrentMessage = "";
            foreach(var c in Text.Split(" "))
            {
                if (c == "\n")
                {
                    container.Items.Add(CurrentMessage.Trim());
                    CurrentMessage = "";
                }
                else if ((int)g.MeasureString($"{CurrentMessage} {c}", font).Width < width)
                {
                    CurrentMessage = $"{CurrentMessage} {c}";
                }
                else
                {
                    container.Items.Add(CurrentMessage.Trim());
                    CurrentMessage = c;
                }
            }
            container.Items.Add(CurrentMessage.Trim());
            container.Refresh();
        }

        public static Font GetFont(this OptionFile optionFile)
        {
            return GetFontFromString(optionFile.WinformData.FormFont);
        }

        public static Font GetFontFromString(string Font)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(Font));
            return string.IsNullOrWhiteSpace(Font) ? SystemFonts.DefaultFont : (Font)converter.ConvertFromString(Font);
        }
        public static string ConvertFontToString(Font Font)
        {
            Font ??= SystemFonts.DefaultFont;
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(Font));
            return converter.ConvertToString(Font);
        }

        public static string PromptforPastebinKey()
        {
            string input = Interaction.InputBox("A Pastebin Developer Key is required to paste to Pastebin.\n\n" +
                $"You will only need to enter this once, the key will then be stored in {References.Globalpaths.UserData}\n" +
                $"This key should be kept private and only be stored on your local machine.\n\n" +
                $"Visit this link for information on how to obtain your key https://pastebin.com/doc_api#1", "Enter Pastebin Key", "Default");
            if (!string.IsNullOrWhiteSpace(input)) { return input; }
            return null;
        }

        public static void AdjustComboBoxWidth(ComboBox C)
        {
            Graphics g = C.CreateGraphics();
            float longest = 0;
            foreach (var i in C.Items)
            {
                SizeF textLength = g.MeasureString(i.ToString(), C.Font);
                if (textLength.Width > longest)
                    longest = textLength.Width;
            }
            if (longest > 0)
                C.DropDownWidth = (int)longest;
        }

        public static bool ShouldShowEntranceListBoxes(InstanceContainer InstanceContainer)
        {
            if (InstanceContainer == null || InstanceContainer.Instance == null) { return false; }
            return InstanceContainer.Instance.StaticOptions.OptionFile.EntranceRandoFeatures && (InstanceContainer.Instance.EntrancePool.IsEntranceRando || InstanceContainer.Instance.EntrancePool.CheckForRandomEntrances());
        }
    }
}
