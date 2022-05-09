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
    public partial class BasicDisplay : Form
    {
        List<dynamic> _displayItems;
        public BasicDisplay(List<dynamic> Display)
        {
            InitializeComponent();
            _displayItems = Display;
        }

        private void BasicDisplay_Shown(object sender, EventArgs e)
        {
            listBox1.HorizontalScrollbar = true;
            listBox1.DataSource = _displayItems;
        }
    }
}
