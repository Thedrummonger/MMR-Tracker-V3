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
    public partial class PopoutPathfinder : Form
    {
        LogicObjects.TrackerInstance _instance;
        Pathfinder pathfinder;
        public PopoutPathfinder(LogicObjects.TrackerInstance instance)
        {
            InitializeComponent();
            _instance = instance;
            pathfinder = new Pathfinder();
        }

        private void PopoutPathfinder_Load(object sender, EventArgs e)
        {
            var AccessableAreas = _instance.EntrancePool.AreaList.Values.Where(x => x.ExitsAcessibleFrom > 0).Select(x => x.ID);
            comboBox1.DataSource = AccessableAreas.ToList();
            comboBox2.DataSource = AccessableAreas.ToList();
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            var AccessableAreas = _instance.EntrancePool.AreaList.Values.Where(x => x.ExitsAcessibleFrom > 0).Select(x => x.ID);
            comboBox1.DataSource = AccessableAreas.ToList();
        }

        private void comboBox2_DropDown(object sender, EventArgs e)
        {
            var AccessableAreas = _instance.EntrancePool.AreaList.Values.Where(x => x.ExitsAcessibleFrom > 0).Select(x => x.ID);
            comboBox2.DataSource = AccessableAreas.ToList();
        }

        private void btnFindpath_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex < 0 || comboBox2.SelectedIndex < 0) { return; }
            listBox1.Font = WinFormUtils.GetFontFromString(_instance.StaticOptions.OptionFile.WinformData.FormFont);
            listBox1.ItemHeight = Convert.ToInt32(listBox1.Font.Size * 1.8);
            listBox1.DataSource = new List<string> { "Finding path" };
            pathfinder = new Pathfinder();
            pathfinder.FindPath(_instance, (string)comboBox1.SelectedItem, (string)comboBox2.SelectedItem, new List<string>(), new Dictionary<string, string>());
            pathfinder.FinalPath = pathfinder.FinalPath.OrderBy(x => x.Count).ToList();
            if (!pathfinder.FinalPath.Any())
            {
                listBox1.DataSource = new List<string> { "No Path Found" };
            }
            else
            {
                PrintPaths();
            }
        }

        private void PrintPaths()
        {
            List<object> Results = new List<object>();
            int index = 0;
            foreach (var i in pathfinder.FinalPath)
            {
                Results.Add(WinFormUtils.CreateDivider(listBox1));
                Results.Add(new Pathfinder.PathfinderPath { Display = $"Path {index + 1}: {i.Count} Stops", Index = index });
                var PathList = i.Select(x => new Pathfinder.PathfinderPath { Display = $"{x.Key} => {x.Value}", Index = index });
                Results = Results.Concat(PathList).ToList();
                index++;
            }
            listBox1.DataSource = Results;
        }

        private void LBPathFinder_DoubleClick(object sender, EventArgs e)
        {
            if (((ListBox)sender).SelectedItem is Pathfinder.PathfinderPath path)
            {
                if (path.Focused)
                {
                    PrintPaths();
                }
                else
                {
                    List<object> Results = new List<object>();
                    Results.Add(new Pathfinder.PathfinderPath { Display = $"Path {path.Index + 1}: {pathfinder.FinalPath[path.Index].Count} Stops", Index = path.Index });
                    Results = Results.Concat(pathfinder.FinalPath[path.Index].Select(x => new Pathfinder.PathfinderPath { Display = $"{x.Key} => {x.Value}", Index = path.Index, Focused = true })).ToList();
                    listBox1.DataSource = Results.ToList();
                }
            }
        }
    }
}
