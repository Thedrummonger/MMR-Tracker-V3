using MMR_Tracker_V3;
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
    public partial class MainInterface : Form
    {
        public static MMR_Tracker_V3.LogicObjects.TrackerInstance MainUITrackerInstance = null;
        public MainInterface()
        {
            InitializeComponent();
        }

        private void MainInterface_Load(object sender, EventArgs e)
        {

        }

        private void AlignUI()
        {
            var UpperLeftLBL = label1;
            var UpperRightLBL = label3;
            var LowerLeftLBL = label2;
            var LowerRightLBL = label4;
            var LowerRight2LBL = label5;
            var LowerRight3LBL = label6;
            var Menuhieght = menuStrip1.Height;
            var FormHeight = this.Height - 40 - Menuhieght;
            var FormWidth = this.Width - 18;
            var FormHalfHeight = FormHeight / 2;
            var FormHalfWidth = FormWidth / 2;

            var locX = 2;
            var locY = 2 + Menuhieght;

            if (MainUITrackerInstance == null)
            {
                SetObjectVisibility(false, false);
            }
            else if (MainUITrackerInstance.Options.EntranceRadnoEnabled)
            {
                SetObjectVisibility(true, true);
                UpperLeftLBL.Location = new Point(locX, locY + 2);
                BTNSetItem.Location = new Point(FormHalfWidth - BTNSetItem.Width, Menuhieght + 1);
                TXTLocSearch.Location = new Point(locX, locY + UpperLeftLBL.Height + 6);
                TXTLocSearch.Width = FormHalfWidth - 2;
                LBValidLocations.Location = new Point(locX, locY + UpperLeftLBL.Height + TXTLocSearch.Height + 8);
                LBValidLocations.Width = FormHalfWidth - 2;
                LBValidLocations.Height = FormHalfHeight - UpperLeftLBL.Height - TXTLocSearch.Height - 14;

                UpperRightLBL.Location = new Point(FormHalfWidth + locX, locY + 2);
                BTNSetEntrance.Location = new Point(FormWidth - BTNSetEntrance.Width, Menuhieght + 1);
                TXTEntSearch.Location = new Point(FormHalfWidth + locX, locY + UpperRightLBL.Height + 6);
                TXTEntSearch.Width = FormHalfWidth - 2;
                LBValidEntrances.Location = new Point(FormHalfWidth + locX, locY + UpperRightLBL.Height + TXTEntSearch.Height + 8);
                LBValidEntrances.Width = FormHalfWidth - 2;
                LBValidEntrances.Height = FormHalfHeight - UpperRightLBL.Height - TXTEntSearch.Height - 14;

                LowerLeftLBL.Location = new Point(locX, FormHalfHeight + locY - 2);
                CHKShowAll.Location = new Point(FormHalfWidth - CHKShowAll.Width, Menuhieght + FormHalfHeight - 2);
                TXTCheckedSearch.Location = new Point(locX, locY + UpperLeftLBL.Height + 2 + FormHalfHeight);
                TXTCheckedSearch.Width = FormHalfWidth - 2;
                LBCheckedLocations.Location = new Point(locX, locY + UpperLeftLBL.Height + TXTCheckedSearch.Height + 4 + FormHalfHeight);
                LBCheckedLocations.Width = FormHalfWidth - 2;
                LBCheckedLocations.Height = FormHalfHeight - UpperLeftLBL.Height - TXTCheckedSearch.Height - 8;

                LowerRightLBL.Location = new Point(FormHalfWidth + locX, FormHalfHeight + locY - 2);
                BTNFindPath.Location = new Point(FormWidth - BTNFindPath.Width, Menuhieght + FormHalfHeight - 3);
                LowerRight2LBL.Location = new Point(FormHalfWidth + locX, FormHalfHeight + locY + LowerRightLBL.Height + 6);
                lblSwapPathfinder.Location = new Point(LowerRight2LBL.Location.X + LowerRight2LBL.Width + 4, LowerRight2LBL.Location.Y - 3);
                LowerRight3LBL.Location = new Point(FormHalfWidth + locX, FormHalfHeight + locY + LowerRightLBL.Height + 7 + CMBStart.Height);
                CMBStart.Location = new Point(FormHalfWidth + locX + LowerRight3LBL.Width + 2, FormHalfHeight + locY + LowerRightLBL.Height + 2);
                CMBEnd.Location = new Point(FormHalfWidth + locX + LowerRight3LBL.Width + 2, FormHalfHeight + locY + LowerRightLBL.Height + CMBStart.Height + 5);
                CMBStart.Width = FormHalfWidth - LowerRight3LBL.Width - 4;
                CMBEnd.Width = FormHalfWidth - LowerRight3LBL.Width - 4;
                LBPathFinder.Location = new Point(locX + FormHalfWidth, FormHalfHeight + locY + 8 + LowerRightLBL.Height + CMBStart.Height + CMBEnd.Height);
                LBPathFinder.Width = FormHalfWidth - 2;
                LBPathFinder.Height = LBCheckedLocations.Height - CMBEnd.Height - 5;
            }
            else
            {
                SetObjectVisibility(true, false);
                if (MainUITrackerInstance.Options.WinformData.HorizontalLayout)
                {
                    UpperLeftLBL.Location = new Point(locX, locY + 2);
                    BTNSetItem.Location = new Point(FormHalfWidth - BTNSetItem.Width, Menuhieght + 1);
                    TXTLocSearch.Location = new Point(locX, locY + UpperLeftLBL.Height + 6);
                    TXTLocSearch.Width = FormHalfWidth - 2;
                    LBValidLocations.Location = new Point(locX, locY + UpperLeftLBL.Height + TXTLocSearch.Height + 8);
                    LBValidLocations.Width = FormHalfWidth - 2;
                    LBValidLocations.Height = FormHeight - UpperLeftLBL.Height - TXTLocSearch.Height - 14;

                    LowerLeftLBL.Location = new Point(FormHalfWidth + locX, locY + 2);
                    CHKShowAll.Location = new Point(FormWidth - CHKShowAll.Width, Menuhieght + 3);
                    TXTCheckedSearch.Location = new Point(FormHalfWidth + locX, locY + UpperRightLBL.Height + 6);
                    TXTCheckedSearch.Width = FormHalfWidth - 2;
                    LBCheckedLocations.Location = new Point(FormHalfWidth + locX, locY + UpperRightLBL.Height + TXTEntSearch.Height + 8);
                    LBCheckedLocations.Width = FormHalfWidth - 2;
                    LBCheckedLocations.Height = FormHeight - UpperRightLBL.Height - TXTEntSearch.Height - 14;
                }
                else
                {
                    UpperLeftLBL.Location = new Point(locX, locY + 2);
                    BTNSetItem.Location = new Point(FormWidth - BTNSetItem.Width, Menuhieght + 1);
                    TXTLocSearch.Location = new Point(locX, locY + UpperLeftLBL.Height + 6);
                    TXTLocSearch.Width = FormWidth - 2;
                    LBValidLocations.Location = new Point(locX, locY + UpperLeftLBL.Height + TXTLocSearch.Height + 8);
                    LBValidLocations.Width = FormWidth - 2;
                    LBValidLocations.Height = FormHalfHeight - UpperLeftLBL.Height - TXTLocSearch.Height - 14;

                    LowerLeftLBL.Location = new Point(locX, FormHalfHeight + locY - 2);
                    CHKShowAll.Location = new Point(FormWidth - CHKShowAll.Width, Menuhieght + FormHalfHeight - 2);
                    TXTCheckedSearch.Location = new Point(locX, locY + UpperLeftLBL.Height + 2 + FormHalfHeight);
                    TXTCheckedSearch.Width = FormWidth - 2;
                    LBCheckedLocations.Location = new Point(locX, locY + UpperLeftLBL.Height + TXTCheckedSearch.Height + 4 + FormHalfHeight);
                    LBCheckedLocations.Width = FormWidth - 2;
                    LBCheckedLocations.Height = FormHalfHeight - UpperLeftLBL.Height - TXTCheckedSearch.Height - 8;
                }
            }
            //PrintToListBox();
            this.Refresh();
        }
        public void SetObjectVisibility(bool item, bool location)
        {
            var UpperLeftLBL = label1;
            var UpperRightLBL = label3;
            var LowerLeftLBL = label2;
            var LowerRightLBL = label4;
            var LowerRight2LBL = label5;
            var LowerRight3LBL = label6;

            UpperLeftLBL.Visible = item;
            BTNSetItem.Visible = item;
            TXTLocSearch.Visible = item;
            LBValidLocations.Visible = item;
            LowerLeftLBL.Visible = item;
            CHKShowAll.Visible = item;
            TXTCheckedSearch.Visible = item;
            LBCheckedLocations.Visible = item;

            UpperRightLBL.Visible = location;
            BTNSetEntrance.Visible = location;
            TXTEntSearch.Visible = location;
            LBValidEntrances.Visible = location;
            LowerRightLBL.Visible = location;
            BTNFindPath.Visible = location;
            LowerRight2LBL.Visible = location;
            LowerRight3LBL.Visible = location;
            CMBStart.Visible = location;
            CMBEnd.Visible = location;
            LBPathFinder.Visible = location;
            lblSwapPathfinder.Visible = location;
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
