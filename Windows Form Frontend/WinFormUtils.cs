using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Windows_Form_Frontend
{
    class WinFormUtils
    {
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
                return new MMR_Tracker_V3.TrackerObjects.MiscData.Divider { Display = DividerText };
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
            return new MMR_Tracker_V3.TrackerObjects.MiscData.Divider { Display = Divider };
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

        public static Font GetFontFromString(string Font)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(Font));
            if (string.IsNullOrWhiteSpace(Font))
            {
                return System.Drawing.SystemFonts.DefaultFont;
            }
            return (Font)converter.ConvertFromString(Font);
        }
        public static string ConvertFontToString(Font Font)
        {
            if (Font == null)
            {
                Font = System.Drawing.SystemFonts.DefaultFont;
            }

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(Font));
            return converter.ConvertToString(Font);
        }
    }
}
