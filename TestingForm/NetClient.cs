using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows_Form_Frontend;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TestingForm
{
    public partial class NetClient : Form
    {
        public NetClient()
        {
            InitializeComponent();
        }

        bool ModeUpdating = false;

        private void TrackerDataHandeling_CheckedObjectsUpdate(List<object> arg1, MMR_Tracker_V3.InstanceData.TrackerInstance arg2)
        {
            Debug.WriteLine($"Sending Items:\n{MMR_Tracker_V3.Utility.ToFormattedJson(GetNetItemsToSend(arg2))}");
        }

        public static Dictionary<int, Dictionary<string, int>> GetNetItemsToSend(MMR_Tracker_V3.InstanceData.TrackerInstance instance)
        {
            Dictionary<int, Dictionary<string, int>> PlayersSentItem = new Dictionary<int, Dictionary<string, int>>();
            foreach (var Item in instance.ItemPool)
            {
                if (!Item.Value.AmountSentToPlayer.Any()) { continue; }

                foreach (var Player in Item.Value.AmountSentToPlayer)
                {
                    if (Player.Value < 1) { continue; }
                    if (!PlayersSentItem.ContainsKey(Player.Key)) { PlayersSentItem[Player.Key] = new Dictionary<string, int>(); }
                    PlayersSentItem[Player.Key][Item.Key] = Player.Value;
                }
            }
            return PlayersSentItem;
        }

        private void NetClient_Load(object sender, EventArgs e)
        {
            ModeUpdating = true;
            MMR_Tracker_V3.TrackerDataHandeling.CheckedObjectsUpdate += TrackerDataHandeling_CheckedObjectsUpdate;
            Array EnumArray = Enum.GetValues(typeof(MiscData.OnlineMode));
            comboBox1.DataSource = EnumArray;

            comboBox1.SelectedIndex = 0;
            ModeUpdating = false;
        }

        private void NetClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show($"Closing this windows will disable the we socket, are you sure?", "Close Net Socket", MessageBoxButtons.YesNo);
            if (result != DialogResult.Yes) { e.Cancel = true; return; }
            MMR_Tracker_V3.TrackerDataHandeling.CheckedObjectsUpdate -= TrackerDataHandeling_CheckedObjectsUpdate;
            MainInterface.InstanceContainer.OnlineMode = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ModeUpdating) { return; }
            MainInterface.InstanceContainer.OnlineMode = (MiscData.OnlineMode)comboBox1.SelectedIndex;

            Debug.WriteLine(MainInterface.InstanceContainer.OnlineMode);
        }
    }
}
