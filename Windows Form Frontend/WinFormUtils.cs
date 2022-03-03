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
        public static string CreateDivider(ListBox container, string DividerText = "")
        {
            Font font = container.Font;
            int width = container.Width;
            Graphics g = container.CreateGraphics();

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
            return Divider;
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
