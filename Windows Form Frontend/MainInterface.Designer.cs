
namespace Windows_Form_Frontend
{
    partial class MainInterface
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainInterface));
            lblSwapPathfinder = new System.Windows.Forms.Label();
            BTNFindPath = new System.Windows.Forms.Button();
            LBPathFinder = new System.Windows.Forms.ListBox();
            LBCheckedLocations = new System.Windows.Forms.ListBox();
            LBValidEntrances = new System.Windows.Forms.ListBox();
            LBValidLocations = new System.Windows.Forms.ListBox();
            CMBEnd = new System.Windows.Forms.ComboBox();
            CMBStart = new System.Windows.Forms.ComboBox();
            CHKShowAll = new System.Windows.Forms.CheckBox();
            BTNSetEntrance = new System.Windows.Forms.Button();
            BTNSetItem = new System.Windows.Forms.Button();
            label6 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            lblAvailableEntrances = new System.Windows.Forms.Label();
            lblCheckedLocation = new System.Windows.Forms.Label();
            lblAvailableLocation = new System.Windows.Forms.Label();
            TXTCheckedSearch = new System.Windows.Forms.TextBox();
            TXTEntSearch = new System.Windows.Forms.TextBox();
            TXTLocSearch = new System.Windows.Forms.TextBox();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            fileToolMenuStrip = new System.Windows.Forms.ToolStripMenuItem();
            NewToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            LoadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            SavetoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            SaveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            OptionstoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            RandomizerOptionsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            logicOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            miscOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importSpoilerLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            spoilerLogToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            PathFinderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            netClientToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            locationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            entrancesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            checkedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            tlpMaster = new System.Windows.Forms.TableLayoutPanel();
            tlpLocations = new System.Windows.Forms.TableLayoutPanel();
            tlpEntrances = new System.Windows.Forms.TableLayoutPanel();
            tlpChecked = new System.Windows.Forms.TableLayoutPanel();
            tlpPathFinder = new System.Windows.Forms.TableLayoutPanel();
            menuStrip1.SuspendLayout();
            tlpMaster.SuspendLayout();
            tlpLocations.SuspendLayout();
            tlpEntrances.SuspendLayout();
            tlpChecked.SuspendLayout();
            tlpPathFinder.SuspendLayout();
            SuspendLayout();
            // 
            // lblSwapPathfinder
            // 
            lblSwapPathfinder.AutoSize = true;
            lblSwapPathfinder.BackColor = System.Drawing.Color.Transparent;
            lblSwapPathfinder.Dock = System.Windows.Forms.DockStyle.Fill;
            lblSwapPathfinder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            lblSwapPathfinder.ForeColor = System.Drawing.SystemColors.ControlDark;
            lblSwapPathfinder.Location = new System.Drawing.Point(50, 29);
            lblSwapPathfinder.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            lblSwapPathfinder.Name = "lblSwapPathfinder";
            lblSwapPathfinder.Size = new System.Drawing.Size(25, 17);
            lblSwapPathfinder.TabIndex = 42;
            lblSwapPathfinder.Text = "↑↓";
            lblSwapPathfinder.Click += lblSwapPathfinder_Click;
            // 
            // BTNFindPath
            // 
            BTNFindPath.BackColor = System.Drawing.SystemColors.ButtonShadow;
            BTNFindPath.Dock = System.Windows.Forms.DockStyle.Right;
            BTNFindPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            BTNFindPath.Location = new System.Drawing.Point(153, 1);
            BTNFindPath.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
            BTNFindPath.Name = "BTNFindPath";
            BTNFindPath.Size = new System.Drawing.Size(93, 24);
            BTNFindPath.TabIndex = 41;
            BTNFindPath.Text = "Find Path";
            BTNFindPath.UseVisualStyleBackColor = false;
            BTNFindPath.Click += BTNFindPath_Click;
            // 
            // LBPathFinder
            // 
            LBPathFinder.BackColor = System.Drawing.SystemColors.ControlDark;
            tlpPathFinder.SetColumnSpan(LBPathFinder, 3);
            LBPathFinder.Dock = System.Windows.Forms.DockStyle.Fill;
            LBPathFinder.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            LBPathFinder.FormattingEnabled = true;
            LBPathFinder.HorizontalScrollbar = true;
            LBPathFinder.IntegralHeight = false;
            LBPathFinder.Location = new System.Drawing.Point(4, 78);
            LBPathFinder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            LBPathFinder.Name = "LBPathFinder";
            LBPathFinder.Size = new System.Drawing.Size(241, 180);
            LBPathFinder.TabIndex = 40;
            LBPathFinder.DrawItem += LBPathFinder_DrawItem;
            LBPathFinder.DoubleClick += LBPathFinder_DoubleClick;
            LBPathFinder.MouseMove += UpdateToolTip;
            // 
            // LBCheckedLocations
            // 
            LBCheckedLocations.BackColor = System.Drawing.SystemColors.ControlDark;
            tlpChecked.SetColumnSpan(LBCheckedLocations, 2);
            LBCheckedLocations.Dock = System.Windows.Forms.DockStyle.Fill;
            LBCheckedLocations.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            LBCheckedLocations.FormattingEnabled = true;
            LBCheckedLocations.IntegralHeight = false;
            LBCheckedLocations.Location = new System.Drawing.Point(4, 53);
            LBCheckedLocations.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            LBCheckedLocations.Name = "LBCheckedLocations";
            LBCheckedLocations.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            LBCheckedLocations.Size = new System.Drawing.Size(241, 205);
            LBCheckedLocations.TabIndex = 39;
            LBCheckedLocations.DrawItem += LBValidLocations_DrawItem;
            LBCheckedLocations.DoubleClick += LBValidEntrances_DoubleClick;
            LBCheckedLocations.KeyDown += LB_KeyDown;
            LBCheckedLocations.KeyPress += preventKeyShortcuts;
            LBCheckedLocations.MouseMove += UpdateToolTip;
            LBCheckedLocations.MouseUp += LBValidLocations_MouseUp;
            // 
            // LBValidEntrances
            // 
            LBValidEntrances.BackColor = System.Drawing.SystemColors.ControlDark;
            tlpEntrances.SetColumnSpan(LBValidEntrances, 2);
            LBValidEntrances.Dock = System.Windows.Forms.DockStyle.Fill;
            LBValidEntrances.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            LBValidEntrances.FormattingEnabled = true;
            LBValidEntrances.IntegralHeight = false;
            LBValidEntrances.Location = new System.Drawing.Point(4, 53);
            LBValidEntrances.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            LBValidEntrances.Name = "LBValidEntrances";
            LBValidEntrances.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            LBValidEntrances.Size = new System.Drawing.Size(241, 205);
            LBValidEntrances.TabIndex = 38;
            LBValidEntrances.DrawItem += LBValidLocations_DrawItem;
            LBValidEntrances.DoubleClick += LBValidEntrances_DoubleClick;
            LBValidEntrances.KeyDown += LB_KeyDown;
            LBValidEntrances.KeyPress += preventKeyShortcuts;
            LBValidEntrances.MouseMove += UpdateToolTip;
            LBValidEntrances.MouseUp += LBValidLocations_MouseUp;
            // 
            // LBValidLocations
            // 
            LBValidLocations.BackColor = System.Drawing.SystemColors.ControlDark;
            tlpLocations.SetColumnSpan(LBValidLocations, 2);
            LBValidLocations.Dock = System.Windows.Forms.DockStyle.Fill;
            LBValidLocations.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            LBValidLocations.FormattingEnabled = true;
            LBValidLocations.IntegralHeight = false;
            LBValidLocations.Location = new System.Drawing.Point(4, 53);
            LBValidLocations.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            LBValidLocations.Name = "LBValidLocations";
            LBValidLocations.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            LBValidLocations.Size = new System.Drawing.Size(241, 205);
            LBValidLocations.TabIndex = 37;
            LBValidLocations.DrawItem += LBValidLocations_DrawItem;
            LBValidLocations.DoubleClick += LBValidEntrances_DoubleClick;
            LBValidLocations.KeyDown += LB_KeyDown;
            LBValidLocations.KeyPress += preventKeyShortcuts;
            LBValidLocations.MouseMove += UpdateToolTip;
            LBValidLocations.MouseUp += LBValidLocations_MouseUp;
            // 
            // CMBEnd
            // 
            CMBEnd.BackColor = System.Drawing.SystemColors.ControlDark;
            CMBEnd.Dock = System.Windows.Forms.DockStyle.Fill;
            CMBEnd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            CMBEnd.ForeColor = System.Drawing.SystemColors.InfoText;
            CMBEnd.FormattingEnabled = true;
            CMBEnd.Location = new System.Drawing.Point(84, 53);
            CMBEnd.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            CMBEnd.Name = "CMBEnd";
            CMBEnd.Size = new System.Drawing.Size(161, 23);
            CMBEnd.TabIndex = 36;
            CMBEnd.DropDown += PathfinderCMB_DropDown;
            // 
            // CMBStart
            // 
            CMBStart.BackColor = System.Drawing.SystemColors.ControlDark;
            CMBStart.Dock = System.Windows.Forms.DockStyle.Fill;
            CMBStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            CMBStart.ForeColor = System.Drawing.SystemColors.InfoText;
            CMBStart.FormattingEnabled = true;
            CMBStart.Location = new System.Drawing.Point(84, 28);
            CMBStart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            CMBStart.Name = "CMBStart";
            CMBStart.Size = new System.Drawing.Size(161, 23);
            CMBStart.TabIndex = 35;
            CMBStart.DropDown += PathfinderCMB_DropDown;
            // 
            // CHKShowAll
            // 
            CHKShowAll.AutoSize = true;
            CHKShowAll.BackColor = System.Drawing.Color.Transparent;
            CHKShowAll.Dock = System.Windows.Forms.DockStyle.Right;
            CHKShowAll.Font = new System.Drawing.Font("Segoe UI", 8F);
            CHKShowAll.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            CHKShowAll.Location = new System.Drawing.Point(174, 3);
            CHKShowAll.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            CHKShowAll.Name = "CHKShowAll";
            CHKShowAll.Size = new System.Drawing.Size(71, 19);
            CHKShowAll.TabIndex = 34;
            CHKShowAll.Text = "Show All";
            CHKShowAll.UseVisualStyleBackColor = false;
            CHKShowAll.CheckedChanged += CHKShowAll_CheckedChanged;
            // 
            // BTNSetEntrance
            // 
            BTNSetEntrance.BackColor = System.Drawing.SystemColors.ButtonShadow;
            BTNSetEntrance.Dock = System.Windows.Forms.DockStyle.Fill;
            BTNSetEntrance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            BTNSetEntrance.Location = new System.Drawing.Point(149, 1);
            BTNSetEntrance.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
            BTNSetEntrance.Name = "BTNSetEntrance";
            BTNSetEntrance.Size = new System.Drawing.Size(97, 24);
            BTNSetEntrance.TabIndex = 33;
            BTNSetEntrance.Text = "Mark Entrance";
            BTNSetEntrance.UseVisualStyleBackColor = false;
            BTNSetEntrance.Click += BTNSetItem_Click;
            BTNSetEntrance.MouseUp += BTNSetEntrance_MouseUp;
            // 
            // BTNSetItem
            // 
            BTNSetItem.BackColor = System.Drawing.SystemColors.ButtonShadow;
            BTNSetItem.Dock = System.Windows.Forms.DockStyle.Fill;
            BTNSetItem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            BTNSetItem.Location = new System.Drawing.Point(149, 1);
            BTNSetItem.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
            BTNSetItem.Name = "BTNSetItem";
            BTNSetItem.Size = new System.Drawing.Size(97, 24);
            BTNSetItem.TabIndex = 32;
            BTNSetItem.Text = "Mark Item";
            BTNSetItem.UseVisualStyleBackColor = false;
            BTNSetItem.Click += BTNSetItem_Click;
            BTNSetItem.MouseUp += BTNSetEntrance_MouseUp;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.BackColor = System.Drawing.Color.Transparent;
            tlpPathFinder.SetColumnSpan(label6, 2);
            label6.Dock = System.Windows.Forms.DockStyle.Fill;
            label6.ForeColor = System.Drawing.SystemColors.ControlDark;
            label6.Location = new System.Drawing.Point(5, 55);
            label6.Margin = new System.Windows.Forms.Padding(5);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(70, 15);
            label6.TabIndex = 31;
            label6.Text = "Destination";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.BackColor = System.Drawing.Color.Transparent;
            label5.Dock = System.Windows.Forms.DockStyle.Left;
            label5.ForeColor = System.Drawing.SystemColors.ControlDark;
            label5.Location = new System.Drawing.Point(5, 30);
            label5.Margin = new System.Windows.Forms.Padding(5);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(31, 15);
            label5.TabIndex = 30;
            label5.Text = "Start";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.BackColor = System.Drawing.Color.Transparent;
            tlpPathFinder.SetColumnSpan(label4, 2);
            label4.Dock = System.Windows.Forms.DockStyle.Fill;
            label4.ForeColor = System.Drawing.SystemColors.ControlDark;
            label4.Location = new System.Drawing.Point(5, 5);
            label4.Margin = new System.Windows.Forms.Padding(5);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(70, 15);
            label4.TabIndex = 29;
            label4.Text = "Path Finder";
            // 
            // lblAvailableEntrances
            // 
            lblAvailableEntrances.AutoSize = true;
            lblAvailableEntrances.BackColor = System.Drawing.Color.Transparent;
            lblAvailableEntrances.Dock = System.Windows.Forms.DockStyle.Fill;
            lblAvailableEntrances.ForeColor = System.Drawing.SystemColors.ControlDark;
            lblAvailableEntrances.Location = new System.Drawing.Point(5, 5);
            lblAvailableEntrances.Margin = new System.Windows.Forms.Padding(5);
            lblAvailableEntrances.Name = "lblAvailableEntrances";
            lblAvailableEntrances.Size = new System.Drawing.Size(139, 15);
            lblAvailableEntrances.TabIndex = 28;
            lblAvailableEntrances.Text = "Available Entrances";
            // 
            // lblCheckedLocation
            // 
            lblCheckedLocation.AutoSize = true;
            lblCheckedLocation.BackColor = System.Drawing.Color.Transparent;
            lblCheckedLocation.Dock = System.Windows.Forms.DockStyle.Fill;
            lblCheckedLocation.ForeColor = System.Drawing.SystemColors.ControlDark;
            lblCheckedLocation.Location = new System.Drawing.Point(5, 5);
            lblCheckedLocation.Margin = new System.Windows.Forms.Padding(5);
            lblCheckedLocation.Name = "lblCheckedLocation";
            lblCheckedLocation.Size = new System.Drawing.Size(139, 15);
            lblCheckedLocation.TabIndex = 27;
            lblCheckedLocation.Text = "Checked locations";
            // 
            // lblAvailableLocation
            // 
            lblAvailableLocation.AutoSize = true;
            lblAvailableLocation.BackColor = System.Drawing.Color.Transparent;
            lblAvailableLocation.Dock = System.Windows.Forms.DockStyle.Fill;
            lblAvailableLocation.ForeColor = System.Drawing.SystemColors.ControlDark;
            lblAvailableLocation.Location = new System.Drawing.Point(5, 5);
            lblAvailableLocation.Margin = new System.Windows.Forms.Padding(5);
            lblAvailableLocation.Name = "lblAvailableLocation";
            lblAvailableLocation.Size = new System.Drawing.Size(139, 15);
            lblAvailableLocation.TabIndex = 26;
            lblAvailableLocation.Text = "Available Locations";
            // 
            // TXTCheckedSearch
            // 
            TXTCheckedSearch.BackColor = System.Drawing.SystemColors.ControlDark;
            tlpChecked.SetColumnSpan(TXTCheckedSearch, 2);
            TXTCheckedSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            TXTCheckedSearch.Location = new System.Drawing.Point(4, 28);
            TXTCheckedSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            TXTCheckedSearch.Name = "TXTCheckedSearch";
            TXTCheckedSearch.Size = new System.Drawing.Size(241, 23);
            TXTCheckedSearch.TabIndex = 25;
            TXTCheckedSearch.TextChanged += TXTLocSearch_TextChanged;
            TXTCheckedSearch.MouseUp += TXTLocSearch_MouseUp;
            // 
            // TXTEntSearch
            // 
            TXTEntSearch.BackColor = System.Drawing.SystemColors.ControlDark;
            tlpEntrances.SetColumnSpan(TXTEntSearch, 2);
            TXTEntSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            TXTEntSearch.Location = new System.Drawing.Point(4, 28);
            TXTEntSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            TXTEntSearch.Name = "TXTEntSearch";
            TXTEntSearch.Size = new System.Drawing.Size(241, 23);
            TXTEntSearch.TabIndex = 24;
            TXTEntSearch.TextChanged += TXTLocSearch_TextChanged;
            TXTEntSearch.MouseUp += TXTLocSearch_MouseUp;
            // 
            // TXTLocSearch
            // 
            TXTLocSearch.BackColor = System.Drawing.SystemColors.ControlDark;
            tlpLocations.SetColumnSpan(TXTLocSearch, 2);
            TXTLocSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            TXTLocSearch.Location = new System.Drawing.Point(4, 28);
            TXTLocSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            TXTLocSearch.Name = "TXTLocSearch";
            TXTLocSearch.Size = new System.Drawing.Size(241, 23);
            TXTLocSearch.TabIndex = 23;
            TXTLocSearch.TextChanged += TXTLocSearch_TextChanged;
            TXTLocSearch.MouseUp += TXTLocSearch_MouseUp;
            // 
            // menuStrip1
            // 
            menuStrip1.BackColor = System.Drawing.SystemColors.ControlDark;
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolMenuStrip, OptionstoolStripMenuItem, toolsToolStripMenuItem, viewToolStripMenuItem, undoToolStripMenuItem, redoToolStripMenuItem, refreshToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(498, 24);
            menuStrip1.TabIndex = 43;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolMenuStrip
            // 
            fileToolMenuStrip.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { NewToolStripMenuItem1, LoadToolStripMenuItem, SavetoolStripMenuItem, SaveAsToolStripMenuItem });
            fileToolMenuStrip.Name = "fileToolMenuStrip";
            fileToolMenuStrip.Size = new System.Drawing.Size(37, 20);
            fileToolMenuStrip.Text = "File";
            // 
            // NewToolStripMenuItem1
            // 
            NewToolStripMenuItem1.Name = "NewToolStripMenuItem1";
            NewToolStripMenuItem1.Size = new System.Drawing.Size(112, 22);
            NewToolStripMenuItem1.Text = "New";
            NewToolStripMenuItem1.Click += NewToolStripMenuItem1_Click;
            // 
            // LoadToolStripMenuItem
            // 
            LoadToolStripMenuItem.Name = "LoadToolStripMenuItem";
            LoadToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            LoadToolStripMenuItem.Text = "Load";
            LoadToolStripMenuItem.Click += LoadToolStripMenuItem_Click;
            // 
            // SavetoolStripMenuItem
            // 
            SavetoolStripMenuItem.Name = "SavetoolStripMenuItem";
            SavetoolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            SavetoolStripMenuItem.Text = "Save";
            SavetoolStripMenuItem.Click += SavetoolStripMenuItem1_Click;
            // 
            // SaveAsToolStripMenuItem
            // 
            SaveAsToolStripMenuItem.Name = "SaveAsToolStripMenuItem";
            SaveAsToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            SaveAsToolStripMenuItem.Text = "Save as";
            // 
            // OptionstoolStripMenuItem
            // 
            OptionstoolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { RandomizerOptionsToolStripMenuItem1, logicOptionsToolStripMenuItem, miscOptionsToolStripMenuItem });
            OptionstoolStripMenuItem.Name = "OptionstoolStripMenuItem";
            OptionstoolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            OptionstoolStripMenuItem.Text = "Options";
            // 
            // RandomizerOptionsToolStripMenuItem1
            // 
            RandomizerOptionsToolStripMenuItem1.Name = "RandomizerOptionsToolStripMenuItem1";
            RandomizerOptionsToolStripMenuItem1.Size = new System.Drawing.Size(170, 22);
            RandomizerOptionsToolStripMenuItem1.Text = "Logic Options";
            // 
            // logicOptionsToolStripMenuItem
            // 
            logicOptionsToolStripMenuItem.Name = "logicOptionsToolStripMenuItem";
            logicOptionsToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            logicOptionsToolStripMenuItem.Text = "Item Pool Options";
            logicOptionsToolStripMenuItem.Click += logicOptionsToolStripMenuItem_Click;
            // 
            // miscOptionsToolStripMenuItem
            // 
            miscOptionsToolStripMenuItem.Name = "miscOptionsToolStripMenuItem";
            miscOptionsToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            miscOptionsToolStripMenuItem.Text = "Tracker Options";
            miscOptionsToolStripMenuItem.Click += miscOptionsToolStripMenuItem_Click;
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { importSpoilerLogToolStripMenuItem, spoilerLogToolsToolStripMenuItem, PathFinderToolStripMenuItem, netClientToolStripMenuItem });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            toolsToolStripMenuItem.Text = "Tools";
            // 
            // importSpoilerLogToolStripMenuItem
            // 
            importSpoilerLogToolStripMenuItem.Name = "importSpoilerLogToolStripMenuItem";
            importSpoilerLogToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            importSpoilerLogToolStripMenuItem.Text = "Import Spoiler Log";
            importSpoilerLogToolStripMenuItem.Click += importSpoilerLogToolStripMenuItem_Click;
            // 
            // spoilerLogToolsToolStripMenuItem
            // 
            spoilerLogToolsToolStripMenuItem.Name = "spoilerLogToolsToolStripMenuItem";
            spoilerLogToolsToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            spoilerLogToolsToolStripMenuItem.Text = "Spoiler Log Tools";
            spoilerLogToolsToolStripMenuItem.Click += spoilerLogToolsToolStripMenuItem_Click;
            // 
            // PathFinderToolStripMenuItem
            // 
            PathFinderToolStripMenuItem.Name = "PathFinderToolStripMenuItem";
            PathFinderToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            PathFinderToolStripMenuItem.Text = "Path Finder";
            PathFinderToolStripMenuItem.Click += PathFinderToolStripMenuItem_Click;
            // 
            // netClientToolStripMenuItem
            // 
            netClientToolStripMenuItem.Name = "netClientToolStripMenuItem";
            netClientToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            netClientToolStripMenuItem.Text = "Net Client";
            netClientToolStripMenuItem.Click += netClientToolStripMenuItem_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { locationsToolStripMenuItem, entrancesToolStripMenuItem, checkedToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            viewToolStripMenuItem.Text = "View";
            viewToolStripMenuItem.Click += ViewToolStripMenuItem_Click;
            // 
            // locationsToolStripMenuItem
            // 
            locationsToolStripMenuItem.Name = "locationsToolStripMenuItem";
            locationsToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            locationsToolStripMenuItem.Text = "Locations";
            locationsToolStripMenuItem.Click += ViewToolStripMenuItem_Click;
            // 
            // entrancesToolStripMenuItem
            // 
            entrancesToolStripMenuItem.Name = "entrancesToolStripMenuItem";
            entrancesToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            entrancesToolStripMenuItem.Text = "Entrances";
            entrancesToolStripMenuItem.Click += ViewToolStripMenuItem_Click;
            // 
            // checkedToolStripMenuItem
            // 
            checkedToolStripMenuItem.Name = "checkedToolStripMenuItem";
            checkedToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            checkedToolStripMenuItem.Text = "Checked";
            checkedToolStripMenuItem.Click += ViewToolStripMenuItem_Click;
            // 
            // undoToolStripMenuItem
            // 
            undoToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("undoToolStripMenuItem.Image");
            undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            undoToolStripMenuItem.Size = new System.Drawing.Size(28, 20);
            undoToolStripMenuItem.Click += UndoRedo_Click;
            // 
            // redoToolStripMenuItem
            // 
            redoToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("redoToolStripMenuItem.Image");
            redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            redoToolStripMenuItem.Size = new System.Drawing.Size(28, 20);
            redoToolStripMenuItem.Click += UndoRedo_Click;
            // 
            // refreshToolStripMenuItem
            // 
            refreshToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("refreshToolStripMenuItem.Image");
            refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            refreshToolStripMenuItem.Size = new System.Drawing.Size(28, 20);
            refreshToolStripMenuItem.Click += UndoRedo_Click;
            // 
            // toolTip1
            // 
            toolTip1.AutoPopDelay = 50000;
            toolTip1.InitialDelay = 10;
            toolTip1.ReshowDelay = 100;
            // 
            // tlpMaster
            // 
            tlpMaster.BackColor = System.Drawing.Color.FromArgb(0, 0, 64);
            tlpMaster.ColumnCount = 2;
            tlpMaster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tlpMaster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tlpMaster.Controls.Add(tlpLocations, 0, 0);
            tlpMaster.Controls.Add(tlpEntrances, 1, 0);
            tlpMaster.Controls.Add(tlpChecked, 0, 1);
            tlpMaster.Controls.Add(tlpPathFinder, 1, 1);
            tlpMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpMaster.Location = new System.Drawing.Point(0, 24);
            tlpMaster.Name = "tlpMaster";
            tlpMaster.RowCount = 2;
            tlpMaster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tlpMaster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tlpMaster.Size = new System.Drawing.Size(498, 522);
            tlpMaster.TabIndex = 44;
            // 
            // tlpLocations
            // 
            tlpLocations.ColumnCount = 2;
            tlpLocations.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpLocations.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            tlpLocations.Controls.Add(lblAvailableLocation, 0, 0);
            tlpLocations.Controls.Add(BTNSetItem, 1, 0);
            tlpLocations.Controls.Add(TXTLocSearch, 0, 1);
            tlpLocations.Controls.Add(LBValidLocations, 0, 2);
            tlpLocations.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpLocations.Location = new System.Drawing.Point(0, 0);
            tlpLocations.Margin = new System.Windows.Forms.Padding(0);
            tlpLocations.Name = "tlpLocations";
            tlpLocations.RowCount = 3;
            tlpLocations.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tlpLocations.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tlpLocations.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpLocations.Size = new System.Drawing.Size(249, 261);
            tlpLocations.TabIndex = 0;
            // 
            // tlpEntrances
            // 
            tlpEntrances.ColumnCount = 2;
            tlpEntrances.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpEntrances.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            tlpEntrances.Controls.Add(BTNSetEntrance, 1, 0);
            tlpEntrances.Controls.Add(lblAvailableEntrances, 0, 0);
            tlpEntrances.Controls.Add(TXTEntSearch, 0, 1);
            tlpEntrances.Controls.Add(LBValidEntrances, 0, 2);
            tlpEntrances.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpEntrances.Location = new System.Drawing.Point(249, 0);
            tlpEntrances.Margin = new System.Windows.Forms.Padding(0);
            tlpEntrances.Name = "tlpEntrances";
            tlpEntrances.RowCount = 3;
            tlpEntrances.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tlpEntrances.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tlpEntrances.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpEntrances.Size = new System.Drawing.Size(249, 261);
            tlpEntrances.TabIndex = 1;
            // 
            // tlpChecked
            // 
            tlpChecked.ColumnCount = 2;
            tlpChecked.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpChecked.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            tlpChecked.Controls.Add(CHKShowAll, 1, 0);
            tlpChecked.Controls.Add(lblCheckedLocation, 0, 0);
            tlpChecked.Controls.Add(TXTCheckedSearch, 0, 1);
            tlpChecked.Controls.Add(LBCheckedLocations, 0, 2);
            tlpChecked.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpChecked.Location = new System.Drawing.Point(0, 261);
            tlpChecked.Margin = new System.Windows.Forms.Padding(0);
            tlpChecked.Name = "tlpChecked";
            tlpChecked.RowCount = 3;
            tlpChecked.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tlpChecked.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tlpChecked.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpChecked.Size = new System.Drawing.Size(249, 261);
            tlpChecked.TabIndex = 2;
            // 
            // tlpPathFinder
            // 
            tlpPathFinder.ColumnCount = 3;
            tlpPathFinder.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            tlpPathFinder.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            tlpPathFinder.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpPathFinder.Controls.Add(label4, 0, 0);
            tlpPathFinder.Controls.Add(CMBEnd, 2, 2);
            tlpPathFinder.Controls.Add(lblSwapPathfinder, 1, 1);
            tlpPathFinder.Controls.Add(CMBStart, 2, 1);
            tlpPathFinder.Controls.Add(BTNFindPath, 2, 0);
            tlpPathFinder.Controls.Add(label5, 0, 1);
            tlpPathFinder.Controls.Add(LBPathFinder, 0, 3);
            tlpPathFinder.Controls.Add(label6, 0, 2);
            tlpPathFinder.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpPathFinder.Location = new System.Drawing.Point(249, 261);
            tlpPathFinder.Margin = new System.Windows.Forms.Padding(0);
            tlpPathFinder.Name = "tlpPathFinder";
            tlpPathFinder.RowCount = 4;
            tlpPathFinder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tlpPathFinder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tlpPathFinder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tlpPathFinder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpPathFinder.Size = new System.Drawing.Size(249, 261);
            tlpPathFinder.TabIndex = 3;
            // 
            // MainInterface
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            ClientSize = new System.Drawing.Size(498, 546);
            Controls.Add(tlpMaster);
            Controls.Add(menuStrip1);
            DoubleBuffered = true;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MainMenuStrip = menuStrip1;
            Name = "MainInterface";
            Text = "Form1";
            FormClosing += MainInterface_FormClosing;
            Load += MainInterface_Load;
            KeyDown += MainInterface_KeyDown;
            Resize += MainInterface_Resize;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            tlpMaster.ResumeLayout(false);
            tlpLocations.ResumeLayout(false);
            tlpLocations.PerformLayout();
            tlpEntrances.ResumeLayout(false);
            tlpEntrances.PerformLayout();
            tlpChecked.ResumeLayout(false);
            tlpChecked.PerformLayout();
            tlpPathFinder.ResumeLayout(false);
            tlpPathFinder.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblSwapPathfinder;
        private System.Windows.Forms.Button BTNFindPath;
        public System.Windows.Forms.ListBox LBPathFinder;
        public System.Windows.Forms.ListBox LBCheckedLocations;
        public System.Windows.Forms.ListBox LBValidEntrances;
        public System.Windows.Forms.ListBox LBValidLocations;
        private System.Windows.Forms.ComboBox CMBStart;
        private System.Windows.Forms.CheckBox CHKShowAll;
        private System.Windows.Forms.Button BTNSetEntrance;
        private System.Windows.Forms.Button BTNSetItem;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblAvailableEntrances;
        private System.Windows.Forms.Label lblCheckedLocation;
        private System.Windows.Forms.Label lblAvailableLocation;
        public System.Windows.Forms.TextBox TXTCheckedSearch;
        public System.Windows.Forms.TextBox TXTEntSearch;
        public System.Windows.Forms.TextBox TXTLocSearch;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem LoadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SavetoolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem OptionstoolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem RandomizerOptionsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem logicOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem miscOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importSpoilerLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spoilerLogToolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem PathFinderToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem NewToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem SaveAsToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tlpLocations;
        private System.Windows.Forms.TableLayoutPanel tlpMaster;
        private System.Windows.Forms.TableLayoutPanel tlpChecked;
        private System.Windows.Forms.TableLayoutPanel tlpEntrances;
        private System.Windows.Forms.TableLayoutPanel tlpPathFinder;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem locationsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem entrancesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkedToolStripMenuItem;
        public System.Windows.Forms.ComboBox CMBEnd;
        private System.Windows.Forms.ToolStripMenuItem netClientToolStripMenuItem;
    }
}

