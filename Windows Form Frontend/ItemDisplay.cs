using MMR_Tracker_V3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Windows_Form_Frontend
{
    public partial class ItemDisplay : Form
    {
        private MainInterface _ParentForm;
        public ItemDisplay(MainInterface ParentForm)
        {
            _ParentForm = ParentForm;
            InitializeComponent();
        }

        public void UpdateData(ItemTracker.TrackerState newState)
        {
            Debug.WriteLine($"Data Recieved containing {newState.ItemValues.Count} items and {newState.MacroValues.Count} macros");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _ParentForm.Invoke(new MethodInvoker(delegate { _ParentForm.TXTCheckedSearch.Text = "Text from another Thread!"; }));
        }
    }
}
