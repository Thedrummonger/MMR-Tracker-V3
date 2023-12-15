
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainInterface));
            this.lblSwapPathfinder = new System.Windows.Forms.Label();
            this.BTNFindPath = new System.Windows.Forms.Button();
            this.LBPathFinder = new System.Windows.Forms.ListBox();
            this.LBCheckedLocations = new System.Windows.Forms.ListBox();
            this.LBValidEntrances = new System.Windows.Forms.ListBox();
            this.LBValidLocations = new System.Windows.Forms.ListBox();
            this.CMBEnd = new System.Windows.Forms.ComboBox();
            this.CMBStart = new System.Windows.Forms.ComboBox();
            this.CHKShowAll = new System.Windows.Forms.CheckBox();
            this.BTNSetEntrance = new System.Windows.Forms.Button();
            this.BTNSetItem = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblAvailableEntrances = new System.Windows.Forms.Label();
            this.lblCheckedLocation = new System.Windows.Forms.Label();
            this.lblAvailableLocation = new System.Windows.Forms.Label();
            this.TXTCheckedSearch = new System.Windows.Forms.TextBox();
            this.TXTEntSearch = new System.Windows.Forms.TextBox();
            this.TXTLocSearch = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolMenuStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.NewToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.LoadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SavetoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OptionstoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RandomizerOptionsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.logicOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miscOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importSpoilerLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spoilerLogToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PathFinderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tlpMaster = new System.Windows.Forms.TableLayoutPanel();
            this.tlpLocations = new System.Windows.Forms.TableLayoutPanel();
            this.tlpEntrances = new System.Windows.Forms.TableLayoutPanel();
            this.tlpChecked = new System.Windows.Forms.TableLayoutPanel();
            this.tlpPathFinder = new System.Windows.Forms.TableLayoutPanel();
            this.menuStrip1.SuspendLayout();
            this.tlpMaster.SuspendLayout();
            this.tlpLocations.SuspendLayout();
            this.tlpEntrances.SuspendLayout();
            this.tlpChecked.SuspendLayout();
            this.tlpPathFinder.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblSwapPathfinder
            // 
            this.lblSwapPathfinder.AutoSize = true;
            this.lblSwapPathfinder.BackColor = System.Drawing.Color.Transparent;
            this.lblSwapPathfinder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSwapPathfinder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblSwapPathfinder.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lblSwapPathfinder.Location = new System.Drawing.Point(50, 29);
            this.lblSwapPathfinder.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.lblSwapPathfinder.Name = "lblSwapPathfinder";
            this.lblSwapPathfinder.Size = new System.Drawing.Size(25, 17);
            this.lblSwapPathfinder.TabIndex = 42;
            this.lblSwapPathfinder.Text = "↑↓";
            this.lblSwapPathfinder.Click += new System.EventHandler(this.lblSwapPathfinder_Click);
            // 
            // BTNFindPath
            // 
            this.BTNFindPath.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.BTNFindPath.Dock = System.Windows.Forms.DockStyle.Right;
            this.BTNFindPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTNFindPath.Location = new System.Drawing.Point(153, 1);
            this.BTNFindPath.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
            this.BTNFindPath.Name = "BTNFindPath";
            this.BTNFindPath.Size = new System.Drawing.Size(93, 24);
            this.BTNFindPath.TabIndex = 41;
            this.BTNFindPath.Text = "Find Path";
            this.BTNFindPath.UseVisualStyleBackColor = false;
            this.BTNFindPath.Click += new System.EventHandler(this.BTNFindPath_Click);
            // 
            // LBPathFinder
            // 
            this.LBPathFinder.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tlpPathFinder.SetColumnSpan(this.LBPathFinder, 3);
            this.LBPathFinder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LBPathFinder.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.LBPathFinder.FormattingEnabled = true;
            this.LBPathFinder.HorizontalScrollbar = true;
            this.LBPathFinder.IntegralHeight = false;
            this.LBPathFinder.Location = new System.Drawing.Point(4, 78);
            this.LBPathFinder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LBPathFinder.Name = "LBPathFinder";
            this.LBPathFinder.Size = new System.Drawing.Size(241, 180);
            this.LBPathFinder.TabIndex = 40;
            this.LBPathFinder.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.LBPathFinder_DrawItem);
            this.LBPathFinder.DoubleClick += new System.EventHandler(this.LBPathFinder_DoubleClick);
            this.LBPathFinder.MouseMove += new System.Windows.Forms.MouseEventHandler(this.UpdateToolTip);
            // 
            // LBCheckedLocations
            // 
            this.LBCheckedLocations.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tlpChecked.SetColumnSpan(this.LBCheckedLocations, 2);
            this.LBCheckedLocations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LBCheckedLocations.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.LBCheckedLocations.FormattingEnabled = true;
            this.LBCheckedLocations.IntegralHeight = false;
            this.LBCheckedLocations.Location = new System.Drawing.Point(4, 53);
            this.LBCheckedLocations.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LBCheckedLocations.Name = "LBCheckedLocations";
            this.LBCheckedLocations.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LBCheckedLocations.Size = new System.Drawing.Size(241, 205);
            this.LBCheckedLocations.TabIndex = 39;
            this.LBCheckedLocations.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.LBValidLocations_DrawItem);
            this.LBCheckedLocations.DoubleClick += new System.EventHandler(this.LBValidEntrances_DoubleClick);
            this.LBCheckedLocations.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LB_KeyDown);
            this.LBCheckedLocations.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.preventKeyShortcuts);
            this.LBCheckedLocations.MouseMove += new System.Windows.Forms.MouseEventHandler(this.UpdateToolTip);
            this.LBCheckedLocations.MouseUp += new System.Windows.Forms.MouseEventHandler(this.LBValidLocations_MouseUp);
            // 
            // LBValidEntrances
            // 
            this.LBValidEntrances.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tlpEntrances.SetColumnSpan(this.LBValidEntrances, 2);
            this.LBValidEntrances.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LBValidEntrances.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.LBValidEntrances.FormattingEnabled = true;
            this.LBValidEntrances.IntegralHeight = false;
            this.LBValidEntrances.Location = new System.Drawing.Point(4, 53);
            this.LBValidEntrances.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LBValidEntrances.Name = "LBValidEntrances";
            this.LBValidEntrances.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LBValidEntrances.Size = new System.Drawing.Size(241, 205);
            this.LBValidEntrances.TabIndex = 38;
            this.LBValidEntrances.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.LBValidLocations_DrawItem);
            this.LBValidEntrances.DoubleClick += new System.EventHandler(this.LBValidEntrances_DoubleClick);
            this.LBValidEntrances.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LB_KeyDown);
            this.LBValidEntrances.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.preventKeyShortcuts);
            this.LBValidEntrances.MouseMove += new System.Windows.Forms.MouseEventHandler(this.UpdateToolTip);
            this.LBValidEntrances.MouseUp += new System.Windows.Forms.MouseEventHandler(this.LBValidLocations_MouseUp);
            // 
            // LBValidLocations
            // 
            this.LBValidLocations.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tlpLocations.SetColumnSpan(this.LBValidLocations, 2);
            this.LBValidLocations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LBValidLocations.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.LBValidLocations.FormattingEnabled = true;
            this.LBValidLocations.IntegralHeight = false;
            this.LBValidLocations.Location = new System.Drawing.Point(4, 53);
            this.LBValidLocations.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LBValidLocations.Name = "LBValidLocations";
            this.LBValidLocations.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LBValidLocations.Size = new System.Drawing.Size(241, 205);
            this.LBValidLocations.TabIndex = 37;
            this.LBValidLocations.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.LBValidLocations_DrawItem);
            this.LBValidLocations.DoubleClick += new System.EventHandler(this.LBValidEntrances_DoubleClick);
            this.LBValidLocations.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LB_KeyDown);
            this.LBValidLocations.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.preventKeyShortcuts);
            this.LBValidLocations.MouseMove += new System.Windows.Forms.MouseEventHandler(this.UpdateToolTip);
            this.LBValidLocations.MouseUp += new System.Windows.Forms.MouseEventHandler(this.LBValidLocations_MouseUp);
            // 
            // CMBEnd
            // 
            this.CMBEnd.BackColor = System.Drawing.SystemColors.ControlDark;
            this.CMBEnd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CMBEnd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CMBEnd.ForeColor = System.Drawing.SystemColors.InfoText;
            this.CMBEnd.FormattingEnabled = true;
            this.CMBEnd.Location = new System.Drawing.Point(84, 53);
            this.CMBEnd.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CMBEnd.Name = "CMBEnd";
            this.CMBEnd.Size = new System.Drawing.Size(161, 23);
            this.CMBEnd.TabIndex = 36;
            this.CMBEnd.DropDown += new System.EventHandler(this.PathfinderCMB_DropDown);
            // 
            // CMBStart
            // 
            this.CMBStart.BackColor = System.Drawing.SystemColors.ControlDark;
            this.CMBStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CMBStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CMBStart.ForeColor = System.Drawing.SystemColors.InfoText;
            this.CMBStart.FormattingEnabled = true;
            this.CMBStart.Location = new System.Drawing.Point(84, 28);
            this.CMBStart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CMBStart.Name = "CMBStart";
            this.CMBStart.Size = new System.Drawing.Size(161, 23);
            this.CMBStart.TabIndex = 35;
            this.CMBStart.DropDown += new System.EventHandler(this.PathfinderCMB_DropDown);
            // 
            // CHKShowAll
            // 
            this.CHKShowAll.AutoSize = true;
            this.CHKShowAll.BackColor = System.Drawing.Color.Transparent;
            this.CHKShowAll.Dock = System.Windows.Forms.DockStyle.Right;
            this.CHKShowAll.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CHKShowAll.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.CHKShowAll.Location = new System.Drawing.Point(174, 3);
            this.CHKShowAll.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CHKShowAll.Name = "CHKShowAll";
            this.CHKShowAll.Size = new System.Drawing.Size(71, 19);
            this.CHKShowAll.TabIndex = 34;
            this.CHKShowAll.Text = "Show All";
            this.CHKShowAll.UseVisualStyleBackColor = false;
            this.CHKShowAll.CheckedChanged += new System.EventHandler(this.CHKShowAll_CheckedChanged);
            // 
            // BTNSetEntrance
            // 
            this.BTNSetEntrance.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.BTNSetEntrance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BTNSetEntrance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTNSetEntrance.Location = new System.Drawing.Point(149, 1);
            this.BTNSetEntrance.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
            this.BTNSetEntrance.Name = "BTNSetEntrance";
            this.BTNSetEntrance.Size = new System.Drawing.Size(97, 24);
            this.BTNSetEntrance.TabIndex = 33;
            this.BTNSetEntrance.Text = "Mark Entrance";
            this.BTNSetEntrance.UseVisualStyleBackColor = false;
            this.BTNSetEntrance.Click += new System.EventHandler(this.BTNSetItem_Click);
            this.BTNSetEntrance.MouseUp += new System.Windows.Forms.MouseEventHandler(this.BTNSetEntrance_MouseUp);
            // 
            // BTNSetItem
            // 
            this.BTNSetItem.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.BTNSetItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BTNSetItem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTNSetItem.Location = new System.Drawing.Point(149, 1);
            this.BTNSetItem.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
            this.BTNSetItem.Name = "BTNSetItem";
            this.BTNSetItem.Size = new System.Drawing.Size(97, 24);
            this.BTNSetItem.TabIndex = 32;
            this.BTNSetItem.Text = "Mark Item";
            this.BTNSetItem.UseVisualStyleBackColor = false;
            this.BTNSetItem.Click += new System.EventHandler(this.BTNSetItem_Click);
            this.BTNSetItem.MouseUp += new System.Windows.Forms.MouseEventHandler(this.BTNSetEntrance_MouseUp);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.tlpPathFinder.SetColumnSpan(this.label6, 2);
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.label6.Location = new System.Drawing.Point(5, 55);
            this.label6.Margin = new System.Windows.Forms.Padding(5);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(70, 15);
            this.label6.TabIndex = 31;
            this.label6.Text = "Destination";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Dock = System.Windows.Forms.DockStyle.Left;
            this.label5.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.label5.Location = new System.Drawing.Point(5, 30);
            this.label5.Margin = new System.Windows.Forms.Padding(5);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(31, 15);
            this.label5.TabIndex = 30;
            this.label5.Text = "Start";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.tlpPathFinder.SetColumnSpan(this.label4, 2);
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.label4.Location = new System.Drawing.Point(5, 5);
            this.label4.Margin = new System.Windows.Forms.Padding(5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 15);
            this.label4.TabIndex = 29;
            this.label4.Text = "Path Finder";
            // 
            // lblAvailableEntrances
            // 
            this.lblAvailableEntrances.AutoSize = true;
            this.lblAvailableEntrances.BackColor = System.Drawing.Color.Transparent;
            this.lblAvailableEntrances.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAvailableEntrances.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lblAvailableEntrances.Location = new System.Drawing.Point(5, 5);
            this.lblAvailableEntrances.Margin = new System.Windows.Forms.Padding(5);
            this.lblAvailableEntrances.Name = "lblAvailableEntrances";
            this.lblAvailableEntrances.Size = new System.Drawing.Size(139, 15);
            this.lblAvailableEntrances.TabIndex = 28;
            this.lblAvailableEntrances.Text = "Available Entrances";
            // 
            // lblCheckedLocation
            // 
            this.lblCheckedLocation.AutoSize = true;
            this.lblCheckedLocation.BackColor = System.Drawing.Color.Transparent;
            this.lblCheckedLocation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCheckedLocation.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lblCheckedLocation.Location = new System.Drawing.Point(5, 5);
            this.lblCheckedLocation.Margin = new System.Windows.Forms.Padding(5);
            this.lblCheckedLocation.Name = "lblCheckedLocation";
            this.lblCheckedLocation.Size = new System.Drawing.Size(139, 15);
            this.lblCheckedLocation.TabIndex = 27;
            this.lblCheckedLocation.Text = "Checked locations";
            // 
            // lblAvailableLocation
            // 
            this.lblAvailableLocation.AutoSize = true;
            this.lblAvailableLocation.BackColor = System.Drawing.Color.Transparent;
            this.lblAvailableLocation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAvailableLocation.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lblAvailableLocation.Location = new System.Drawing.Point(5, 5);
            this.lblAvailableLocation.Margin = new System.Windows.Forms.Padding(5);
            this.lblAvailableLocation.Name = "lblAvailableLocation";
            this.lblAvailableLocation.Size = new System.Drawing.Size(139, 15);
            this.lblAvailableLocation.TabIndex = 26;
            this.lblAvailableLocation.Text = "Available Locations";
            // 
            // TXTCheckedSearch
            // 
            this.TXTCheckedSearch.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tlpChecked.SetColumnSpan(this.TXTCheckedSearch, 2);
            this.TXTCheckedSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TXTCheckedSearch.Location = new System.Drawing.Point(4, 28);
            this.TXTCheckedSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TXTCheckedSearch.Name = "TXTCheckedSearch";
            this.TXTCheckedSearch.Size = new System.Drawing.Size(241, 23);
            this.TXTCheckedSearch.TabIndex = 25;
            this.TXTCheckedSearch.TextChanged += new System.EventHandler(this.TXTLocSearch_TextChanged);
            this.TXTCheckedSearch.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TXTLocSearch_MouseUp);
            // 
            // TXTEntSearch
            // 
            this.TXTEntSearch.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tlpEntrances.SetColumnSpan(this.TXTEntSearch, 2);
            this.TXTEntSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TXTEntSearch.Location = new System.Drawing.Point(4, 28);
            this.TXTEntSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TXTEntSearch.Name = "TXTEntSearch";
            this.TXTEntSearch.Size = new System.Drawing.Size(241, 23);
            this.TXTEntSearch.TabIndex = 24;
            this.TXTEntSearch.TextChanged += new System.EventHandler(this.TXTLocSearch_TextChanged);
            this.TXTEntSearch.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TXTLocSearch_MouseUp);
            // 
            // TXTLocSearch
            // 
            this.TXTLocSearch.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tlpLocations.SetColumnSpan(this.TXTLocSearch, 2);
            this.TXTLocSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TXTLocSearch.Location = new System.Drawing.Point(4, 28);
            this.TXTLocSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TXTLocSearch.Name = "TXTLocSearch";
            this.TXTLocSearch.Size = new System.Drawing.Size(241, 23);
            this.TXTLocSearch.TabIndex = 23;
            this.TXTLocSearch.TextChanged += new System.EventHandler(this.TXTLocSearch_TextChanged);
            this.TXTLocSearch.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TXTLocSearch_MouseUp);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolMenuStrip,
            this.OptionstoolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.refreshToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(498, 24);
            this.menuStrip1.TabIndex = 43;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolMenuStrip
            // 
            this.fileToolMenuStrip.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewToolStripMenuItem1,
            this.LoadToolStripMenuItem,
            this.SavetoolStripMenuItem,
            this.SaveAsToolStripMenuItem});
            this.fileToolMenuStrip.Name = "fileToolMenuStrip";
            this.fileToolMenuStrip.Size = new System.Drawing.Size(37, 20);
            this.fileToolMenuStrip.Text = "File";
            // 
            // NewToolStripMenuItem1
            // 
            this.NewToolStripMenuItem1.Name = "NewToolStripMenuItem1";
            this.NewToolStripMenuItem1.Size = new System.Drawing.Size(112, 22);
            this.NewToolStripMenuItem1.Text = "New";
            this.NewToolStripMenuItem1.Click += new System.EventHandler(this.NewToolStripMenuItem1_Click);
            // 
            // LoadToolStripMenuItem
            // 
            this.LoadToolStripMenuItem.Name = "LoadToolStripMenuItem";
            this.LoadToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.LoadToolStripMenuItem.Text = "Load";
            this.LoadToolStripMenuItem.Click += new System.EventHandler(this.LoadToolStripMenuItem_Click);
            // 
            // SavetoolStripMenuItem
            // 
            this.SavetoolStripMenuItem.Name = "SavetoolStripMenuItem";
            this.SavetoolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.SavetoolStripMenuItem.Text = "Save";
            this.SavetoolStripMenuItem.Click += new System.EventHandler(this.SavetoolStripMenuItem1_Click);
            // 
            // SaveAsToolStripMenuItem
            // 
            this.SaveAsToolStripMenuItem.Name = "SaveAsToolStripMenuItem";
            this.SaveAsToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.SaveAsToolStripMenuItem.Text = "Save as";
            // 
            // OptionstoolStripMenuItem
            // 
            this.OptionstoolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RandomizerOptionsToolStripMenuItem1,
            this.logicOptionsToolStripMenuItem,
            this.miscOptionsToolStripMenuItem});
            this.OptionstoolStripMenuItem.Name = "OptionstoolStripMenuItem";
            this.OptionstoolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.OptionstoolStripMenuItem.Text = "Options";
            // 
            // RandomizerOptionsToolStripMenuItem1
            // 
            this.RandomizerOptionsToolStripMenuItem1.Name = "RandomizerOptionsToolStripMenuItem1";
            this.RandomizerOptionsToolStripMenuItem1.Size = new System.Drawing.Size(170, 22);
            this.RandomizerOptionsToolStripMenuItem1.Text = "Logic Options";
            // 
            // logicOptionsToolStripMenuItem
            // 
            this.logicOptionsToolStripMenuItem.Name = "logicOptionsToolStripMenuItem";
            this.logicOptionsToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.logicOptionsToolStripMenuItem.Text = "Item Pool Options";
            this.logicOptionsToolStripMenuItem.Click += new System.EventHandler(this.logicOptionsToolStripMenuItem_Click);
            // 
            // miscOptionsToolStripMenuItem
            // 
            this.miscOptionsToolStripMenuItem.Name = "miscOptionsToolStripMenuItem";
            this.miscOptionsToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.miscOptionsToolStripMenuItem.Text = "Tracker Options";
            this.miscOptionsToolStripMenuItem.Click += new System.EventHandler(this.miscOptionsToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importSpoilerLogToolStripMenuItem,
            this.spoilerLogToolsToolStripMenuItem,
            this.PathFinderToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // importSpoilerLogToolStripMenuItem
            // 
            this.importSpoilerLogToolStripMenuItem.Name = "importSpoilerLogToolStripMenuItem";
            this.importSpoilerLogToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.importSpoilerLogToolStripMenuItem.Text = "Import Spoiler Log";
            this.importSpoilerLogToolStripMenuItem.Click += new System.EventHandler(this.importSpoilerLogToolStripMenuItem_Click);
            // 
            // spoilerLogToolsToolStripMenuItem
            // 
            this.spoilerLogToolsToolStripMenuItem.Name = "spoilerLogToolsToolStripMenuItem";
            this.spoilerLogToolsToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.spoilerLogToolsToolStripMenuItem.Text = "Spoiler Log Tools";
            this.spoilerLogToolsToolStripMenuItem.Click += new System.EventHandler(this.spoilerLogToolsToolStripMenuItem_Click);
            // 
            // PathFinderToolStripMenuItem
            // 
            this.PathFinderToolStripMenuItem.Name = "PathFinderToolStripMenuItem";
            this.PathFinderToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.PathFinderToolStripMenuItem.Text = "Path Finder";
            this.PathFinderToolStripMenuItem.Click += new System.EventHandler(this.PathFinderToolStripMenuItem_Click);
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("undoToolStripMenuItem.Image")));
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(28, 20);
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.UndoRedo_Click);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("redoToolStripMenuItem.Image")));
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(28, 20);
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.UndoRedo_Click);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("refreshToolStripMenuItem.Image")));
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(28, 20);
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.UndoRedo_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 50000;
            this.toolTip1.InitialDelay = 10;
            this.toolTip1.ReshowDelay = 100;
            // 
            // tlpMaster
            // 
            this.tlpMaster.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.tlpMaster.ColumnCount = 2;
            this.tlpMaster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMaster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMaster.Controls.Add(this.tlpLocations, 0, 0);
            this.tlpMaster.Controls.Add(this.tlpEntrances, 1, 0);
            this.tlpMaster.Controls.Add(this.tlpChecked, 0, 1);
            this.tlpMaster.Controls.Add(this.tlpPathFinder, 1, 1);
            this.tlpMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMaster.Location = new System.Drawing.Point(0, 24);
            this.tlpMaster.Name = "tlpMaster";
            this.tlpMaster.RowCount = 2;
            this.tlpMaster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMaster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMaster.Size = new System.Drawing.Size(498, 522);
            this.tlpMaster.TabIndex = 44;
            // 
            // tlpLocations
            // 
            this.tlpLocations.ColumnCount = 2;
            this.tlpLocations.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLocations.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpLocations.Controls.Add(this.lblAvailableLocation, 0, 0);
            this.tlpLocations.Controls.Add(this.BTNSetItem, 1, 0);
            this.tlpLocations.Controls.Add(this.TXTLocSearch, 0, 1);
            this.tlpLocations.Controls.Add(this.LBValidLocations, 0, 2);
            this.tlpLocations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpLocations.Location = new System.Drawing.Point(0, 0);
            this.tlpLocations.Margin = new System.Windows.Forms.Padding(0);
            this.tlpLocations.Name = "tlpLocations";
            this.tlpLocations.RowCount = 3;
            this.tlpLocations.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpLocations.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpLocations.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLocations.Size = new System.Drawing.Size(249, 261);
            this.tlpLocations.TabIndex = 0;
            // 
            // tlpEntrances
            // 
            this.tlpEntrances.ColumnCount = 2;
            this.tlpEntrances.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpEntrances.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpEntrances.Controls.Add(this.BTNSetEntrance, 1, 0);
            this.tlpEntrances.Controls.Add(this.lblAvailableEntrances, 0, 0);
            this.tlpEntrances.Controls.Add(this.TXTEntSearch, 0, 1);
            this.tlpEntrances.Controls.Add(this.LBValidEntrances, 0, 2);
            this.tlpEntrances.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpEntrances.Location = new System.Drawing.Point(249, 0);
            this.tlpEntrances.Margin = new System.Windows.Forms.Padding(0);
            this.tlpEntrances.Name = "tlpEntrances";
            this.tlpEntrances.RowCount = 3;
            this.tlpEntrances.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpEntrances.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpEntrances.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpEntrances.Size = new System.Drawing.Size(249, 261);
            this.tlpEntrances.TabIndex = 1;
            // 
            // tlpChecked
            // 
            this.tlpChecked.ColumnCount = 2;
            this.tlpChecked.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpChecked.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpChecked.Controls.Add(this.CHKShowAll, 1, 0);
            this.tlpChecked.Controls.Add(this.lblCheckedLocation, 0, 0);
            this.tlpChecked.Controls.Add(this.TXTCheckedSearch, 0, 1);
            this.tlpChecked.Controls.Add(this.LBCheckedLocations, 0, 2);
            this.tlpChecked.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpChecked.Location = new System.Drawing.Point(0, 261);
            this.tlpChecked.Margin = new System.Windows.Forms.Padding(0);
            this.tlpChecked.Name = "tlpChecked";
            this.tlpChecked.RowCount = 3;
            this.tlpChecked.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpChecked.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpChecked.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpChecked.Size = new System.Drawing.Size(249, 261);
            this.tlpChecked.TabIndex = 2;
            // 
            // tlpPathFinder
            // 
            this.tlpPathFinder.ColumnCount = 3;
            this.tlpPathFinder.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tlpPathFinder.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tlpPathFinder.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPathFinder.Controls.Add(this.label4, 0, 0);
            this.tlpPathFinder.Controls.Add(this.CMBEnd, 2, 2);
            this.tlpPathFinder.Controls.Add(this.lblSwapPathfinder, 1, 1);
            this.tlpPathFinder.Controls.Add(this.CMBStart, 2, 1);
            this.tlpPathFinder.Controls.Add(this.BTNFindPath, 2, 0);
            this.tlpPathFinder.Controls.Add(this.label5, 0, 1);
            this.tlpPathFinder.Controls.Add(this.LBPathFinder, 0, 3);
            this.tlpPathFinder.Controls.Add(this.label6, 0, 2);
            this.tlpPathFinder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPathFinder.Location = new System.Drawing.Point(249, 261);
            this.tlpPathFinder.Margin = new System.Windows.Forms.Padding(0);
            this.tlpPathFinder.Name = "tlpPathFinder";
            this.tlpPathFinder.RowCount = 4;
            this.tlpPathFinder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpPathFinder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpPathFinder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpPathFinder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPathFinder.Size = new System.Drawing.Size(249, 261);
            this.tlpPathFinder.TabIndex = 3;
            // 
            // MainInterface
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(498, 546);
            this.Controls.Add(this.tlpMaster);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainInterface";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainInterface_FormClosing);
            this.Load += new System.EventHandler(this.MainInterface_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainInterface_KeyDown);
            this.Resize += new System.EventHandler(this.MainInterface_Resize);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tlpMaster.ResumeLayout(false);
            this.tlpLocations.ResumeLayout(false);
            this.tlpLocations.PerformLayout();
            this.tlpEntrances.ResumeLayout(false);
            this.tlpEntrances.PerformLayout();
            this.tlpChecked.ResumeLayout(false);
            this.tlpChecked.PerformLayout();
            this.tlpPathFinder.ResumeLayout(false);
            this.tlpPathFinder.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblSwapPathfinder;
        private System.Windows.Forms.Button BTNFindPath;
        public System.Windows.Forms.ListBox LBPathFinder;
        public System.Windows.Forms.ListBox LBCheckedLocations;
        public System.Windows.Forms.ListBox LBValidEntrances;
        public System.Windows.Forms.ListBox LBValidLocations;
        private System.Windows.Forms.ComboBox CMBEnd;
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
    }
}

