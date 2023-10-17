using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
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
    public partial class PopoutPathfinder : Form
    {
        InstanceData.TrackerInstance _instance;
        Pathfinder pathfinder;
        public PopoutPathfinder(InstanceData.TrackerInstance instance)
        {
            InitializeComponent();
            _instance = instance;
            pathfinder = new Pathfinder();
        }

        private void PopoutPathfinder_Load(object sender, EventArgs e)
        {
            var AccessableAreas = _instance.EntrancePool.AreaList.Values.Where(x => x.ExitsAcessibleFrom > 0 && (x.RandomizableExits(_instance).Any() || _instance.StaticOptions.OptionFile.ShowMacroExitsPathfinder)).Select(x => x.ID);
            comboBox1.DataSource = AccessableAreas.OrderBy(x => x).ToList();
            comboBox2.DataSource = AccessableAreas.OrderBy(x => x).ToList();
            listBox1.HorizontalScrollbar = true;
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            var AccessableAreas = _instance.EntrancePool.AreaList.Values.Where(x => x.ExitsAcessibleFrom > 0 && (x.RandomizableExits(_instance).Any() || _instance.StaticOptions.OptionFile.ShowMacroExitsPathfinder)).Select(x => x.ID);
            comboBox1.DataSource = AccessableAreas.OrderBy(x => x).ToList();
            WinFormUtils.AdjustComboBoxWidth(sender as System.Windows.Forms.ComboBox);
        }

        private void comboBox2_DropDown(object sender, EventArgs e)
        {
            var AccessableAreas = _instance.EntrancePool.AreaList.Values.Where(x => x.ExitsAcessibleFrom > 0 && (x.RandomizableExits(_instance).Any() || _instance.StaticOptions.OptionFile.ShowMacroExitsPathfinder)).Select(x => x.ID);
            comboBox2.DataSource = AccessableAreas.OrderBy(x => x).ToList();
            WinFormUtils.AdjustComboBoxWidth(sender as System.Windows.Forms.ComboBox);
        }

        private void btnFindpath_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex < 0 || comboBox2.SelectedIndex < 0 || comboBox1.SelectedItem.ToString() == comboBox2.SelectedItem.ToString()) { return; }
            listBox1.Font = WinFormUtils.GetFontFromString(_instance.StaticOptions.OptionFile.WinformData.FormFont);
            listBox1.ItemHeight = Convert.ToInt32(listBox1.Font.Size * 1.8);
            listBox1.DataSource = new List<string> { "Finding path" };
            pathfinder = new Pathfinder();
            pathfinder.FindPath(_instance, (string)comboBox1.SelectedItem, (string)comboBox2.SelectedItem);
            pathfinder.FinalPath = pathfinder.FinalPath.OrderBy(x => x.Count).ToList();
            PrintPaths(_instance, pathfinder, listBox1);
        }

        public static void PrintPaths(InstanceData.TrackerInstance instance, Pathfinder pathfinder, ListBox PathFinderLB, int FocusInd = -1)
        {
            if (!pathfinder.FinalPath.Any())
            {
                List<string> Error = new List<string>();
                Error.Add("No Paths Found");
                if (instance.StaticOptions.OptionFile.ShowRedundantPathfinder && pathfinder.Overloaded)
                {
                    Error.Add(string.Empty);
                    Error.Add("Maximum path");
                    Error.Add("cap was reached,");
                    Error.Add("Not all paths");
                    Error.Add("were checked.");
                    Error.Add("Try Disabling");
                    Error.Add("\"Show Redundant");
                    Error.Add("Paths Pathfinder\"");
                    Error.Add("in options ->");
                    Error.Add("tracker options");
                }
                PathFinderLB.DataSource = Error;
                return;
            }
            var inst = instance;
            List<object> Results = new List<object>();
            int index = 0;
            HashSet<string> PrintedPaths = new HashSet<string>();
            foreach (var i in pathfinder.FinalPath)
            {
                List<Pathfinder.PathfinderPath> Stops = new List<Pathfinder.PathfinderPath>();
                foreach (var stop in i)
                {
                    Stops.Add(new Pathfinder.PathfinderPath { Display = stop.Value == "" ? stop.Key : $"{stop.Key} => {stop.Value}", Index = index, Focused = FocusInd > -1 });
                }

                string PathHash = string.Join("|", Stops.Select(x => x.Display ));
                if (PrintedPaths.Contains(PathHash)) { continue; }
                PrintedPaths.Add(PathHash);

                if (FocusInd > -1 && index != FocusInd) { index++; continue; }
                Results.Add(WinFormUtils.CreateDivider(PathFinderLB));
                Results.Add(new Pathfinder.PathfinderPath { Display = $"Path {index+1}: {Stops.Count - 1} Stops", Index = index });
                Results.AddRange(Stops);
                index++;
            }
            PathFinderLB.DataSource = Results;
        }

        private void LBPathFinder_DoubleClick(object sender, EventArgs e)
        {
            if (((ListBox)sender).SelectedItem is Pathfinder.PathfinderPath path)
            {
                PrintPaths(_instance, pathfinder, listBox1, path.Focused ? -1 : path.Index);
            }
        }
    }
}
