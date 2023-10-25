using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestingForm
{
    public partial class NetClient : Form
    {
        public NetClient()
        {
            InitializeComponent();
            MMR_Tracker_V3.TrackerDataHandeling.CheckedObjectsUpdate +=TrackerDataHandeling_CheckedObjectsUpdate;
        }

        private void TrackerDataHandeling_CheckedObjectsUpdate(List<object> arg1, MMR_Tracker_V3.InstanceData.TrackerInstance arg2)
        {
            Dictionary<string, Tuple<int, int>> Sentitems = new Dictionary<string, Tuple<int, int>>();
            foreach(var i in arg2.ItemPool)
            {

            }
        }
    }
}
