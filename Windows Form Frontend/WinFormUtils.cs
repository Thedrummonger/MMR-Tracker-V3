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
            else
            {
                return new MMR_Tracker_V3.TrackerObjects.MiscData.Divider { Display = DividerText };
            }

            int marks = 1;
            string Divider = "";
            while ((int)g.MeasureString(Divider, font).Width <= width)
            {
                string Section = "";
                for (var i = 0; i < marks; i++)
                {
                    Section += "=";
                }
                Divider = Section + DividerText + Section;
                marks++;
            }
            return new MMR_Tracker_V3.TrackerObjects.MiscData.Divider { Display = Divider };
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
