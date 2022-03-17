
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
            this.SavetoolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveAsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.presetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OptionstoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RandomizerOptionsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.logicOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miscOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logicEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importSpoilerLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spoilerLogToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.devToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewAsUserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CodeTestingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblSwapPathfinder
            // 
            this.lblSwapPathfinder.AutoSize = true;
            this.lblSwapPathfinder.BackColor = System.Drawing.Color.Transparent;
            this.lblSwapPathfinder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblSwapPathfinder.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lblSwapPathfinder.Location = new System.Drawing.Point(123, 270);
            this.lblSwapPathfinder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSwapPathfinder.Name = "lblSwapPathfinder";
            this.lblSwapPathfinder.Size = new System.Drawing.Size(23, 16);
            this.lblSwapPathfinder.TabIndex = 42;
            this.lblSwapPathfinder.Text = "↑↓";
            // 
            // BTNFindPath
            // 
            this.BTNFindPath.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.BTNFindPath.Location = new System.Drawing.Point(13, 262);
            this.BTNFindPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTNFindPath.Name = "BTNFindPath";
            this.BTNFindPath.Size = new System.Drawing.Size(93, 23);
            this.BTNFindPath.TabIndex = 41;
            this.BTNFindPath.Text = "Find Path";
            this.BTNFindPath.UseVisualStyleBackColor = false;
            // 
            // LBPathFinder
            // 
            this.LBPathFinder.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.LBPathFinder.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.LBPathFinder.FormattingEnabled = true;
            this.LBPathFinder.IntegralHeight = false;
            this.LBPathFinder.Location = new System.Drawing.Point(123, 408);
            this.LBPathFinder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LBPathFinder.Name = "LBPathFinder";
            this.LBPathFinder.Size = new System.Drawing.Size(87, 109);
            this.LBPathFinder.TabIndex = 40;
            // 
            // LBCheckedLocations
            // 
            this.LBCheckedLocations.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.LBCheckedLocations.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.LBCheckedLocations.FormattingEnabled = true;
            this.LBCheckedLocations.IntegralHeight = false;
            this.LBCheckedLocations.Location = new System.Drawing.Point(13, 408);
            this.LBCheckedLocations.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LBCheckedLocations.Name = "LBCheckedLocations";
            this.LBCheckedLocations.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LBCheckedLocations.Size = new System.Drawing.Size(87, 109);
            this.LBCheckedLocations.TabIndex = 39;
            this.LBCheckedLocations.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.LBCheckedLocations_DrawItem);
            this.LBCheckedLocations.DoubleClick += new System.EventHandler(this.LBValidEntrances_DoubleClick);
            this.LBCheckedLocations.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LB_KeyDown);
            this.LBCheckedLocations.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.preventKeyShortcuts);
            this.LBCheckedLocations.MouseMove += new System.Windows.Forms.MouseEventHandler(this.UpdateToolTip);
            this.LBCheckedLocations.MouseUp += new System.Windows.Forms.MouseEventHandler(this.LBValidLocations_MouseUp);
            // 
            // LBValidEntrances
            // 
            this.LBValidEntrances.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.LBValidEntrances.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.LBValidEntrances.FormattingEnabled = true;
            this.LBValidEntrances.IntegralHeight = false;
            this.LBValidEntrances.Location = new System.Drawing.Point(123, 292);
            this.LBValidEntrances.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LBValidEntrances.Name = "LBValidEntrances";
            this.LBValidEntrances.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LBValidEntrances.Size = new System.Drawing.Size(87, 109);
            this.LBValidEntrances.TabIndex = 38;
            this.LBValidEntrances.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.LBValidEntrances_DrawItem);
            this.LBValidEntrances.DoubleClick += new System.EventHandler(this.LBValidEntrances_DoubleClick);
            this.LBValidEntrances.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LB_KeyDown);
            this.LBValidEntrances.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.preventKeyShortcuts);
            this.LBValidEntrances.MouseMove += new System.Windows.Forms.MouseEventHandler(this.UpdateToolTip);
            this.LBValidEntrances.MouseUp += new System.Windows.Forms.MouseEventHandler(this.LBValidLocations_MouseUp);
            // 
            // LBValidLocations
            // 
            this.LBValidLocations.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.LBValidLocations.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.LBValidLocations.FormattingEnabled = true;
            this.LBValidLocations.IntegralHeight = false;
            this.LBValidLocations.Location = new System.Drawing.Point(13, 292);
            this.LBValidLocations.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LBValidLocations.Name = "LBValidLocations";
            this.LBValidLocations.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LBValidLocations.Size = new System.Drawing.Size(87, 109);
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
            this.CMBEnd.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CMBEnd.ForeColor = System.Drawing.SystemColors.InfoText;
            this.CMBEnd.FormattingEnabled = true;
            this.CMBEnd.Location = new System.Drawing.Point(13, 164);
            this.CMBEnd.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CMBEnd.Name = "CMBEnd";
            this.CMBEnd.Size = new System.Drawing.Size(116, 23);
            this.CMBEnd.TabIndex = 36;
            // 
            // CMBStart
            // 
            this.CMBStart.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CMBStart.ForeColor = System.Drawing.SystemColors.InfoText;
            this.CMBStart.FormattingEnabled = true;
            this.CMBStart.Location = new System.Drawing.Point(13, 132);
            this.CMBStart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CMBStart.Name = "CMBStart";
            this.CMBStart.Size = new System.Drawing.Size(116, 23);
            this.CMBStart.TabIndex = 35;
            // 
            // CHKShowAll
            // 
            this.CHKShowAll.AutoSize = true;
            this.CHKShowAll.BackColor = System.Drawing.Color.Transparent;
            this.CHKShowAll.ForeColor = System.Drawing.SystemColors.Control;
            this.CHKShowAll.Location = new System.Drawing.Point(140, 228);
            this.CHKShowAll.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CHKShowAll.Name = "CHKShowAll";
            this.CHKShowAll.Size = new System.Drawing.Size(72, 19);
            this.CHKShowAll.TabIndex = 34;
            this.CHKShowAll.Text = "Show All";
            this.CHKShowAll.UseVisualStyleBackColor = false;
            this.CHKShowAll.CheckedChanged += new System.EventHandler(this.CHKShowAll_CheckedChanged);
            // 
            // BTNSetEntrance
            // 
            this.BTNSetEntrance.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.BTNSetEntrance.Location = new System.Drawing.Point(13, 228);
            this.BTNSetEntrance.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTNSetEntrance.Name = "BTNSetEntrance";
            this.BTNSetEntrance.Size = new System.Drawing.Size(93, 23);
            this.BTNSetEntrance.TabIndex = 33;
            this.BTNSetEntrance.Text = "Mark Entrance";
            this.BTNSetEntrance.UseVisualStyleBackColor = false;
            this.BTNSetEntrance.Click += new System.EventHandler(this.BTNSetItem_Click);
            this.BTNSetEntrance.MouseUp += new System.Windows.Forms.MouseEventHandler(this.BTNSetEntrance_MouseUp);
            // 
            // BTNSetItem
            // 
            this.BTNSetItem.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.BTNSetItem.Location = new System.Drawing.Point(13, 195);
            this.BTNSetItem.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTNSetItem.Name = "BTNSetItem";
            this.BTNSetItem.Size = new System.Drawing.Size(93, 23);
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
            this.label6.ForeColor = System.Drawing.SystemColors.Control;
            this.label6.Location = new System.Drawing.Point(137, 189);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 15);
            this.label6.TabIndex = 31;
            this.label6.Text = "Destination";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.ForeColor = System.Drawing.SystemColors.Control;
            this.label5.Location = new System.Drawing.Point(137, 162);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(31, 15);
            this.label5.TabIndex = 30;
            this.label5.Text = "Start";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.ForeColor = System.Drawing.SystemColors.Control;
            this.label4.Location = new System.Drawing.Point(137, 132);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 15);
            this.label4.TabIndex = 29;
            this.label4.Text = "Path Finder";
            // 
            // lblAvailableEntrances
            // 
            this.lblAvailableEntrances.AutoSize = true;
            this.lblAvailableEntrances.BackColor = System.Drawing.Color.Transparent;
            this.lblAvailableEntrances.ForeColor = System.Drawing.SystemColors.Control;
            this.lblAvailableEntrances.Location = new System.Drawing.Point(137, 102);
            this.lblAvailableEntrances.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAvailableEntrances.Name = "lblAvailableEntrances";
            this.lblAvailableEntrances.Size = new System.Drawing.Size(109, 15);
            this.lblAvailableEntrances.TabIndex = 28;
            this.lblAvailableEntrances.Text = "Available Entrances";
            // 
            // lblCheckedLocation
            // 
            this.lblCheckedLocation.AutoSize = true;
            this.lblCheckedLocation.BackColor = System.Drawing.Color.Transparent;
            this.lblCheckedLocation.ForeColor = System.Drawing.SystemColors.Control;
            this.lblCheckedLocation.Location = new System.Drawing.Point(137, 72);
            this.lblCheckedLocation.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCheckedLocation.Name = "lblCheckedLocation";
            this.lblCheckedLocation.Size = new System.Drawing.Size(104, 15);
            this.lblCheckedLocation.TabIndex = 27;
            this.lblCheckedLocation.Text = "Checked locations";
            // 
            // lblAvailableLocation
            // 
            this.lblAvailableLocation.AutoSize = true;
            this.lblAvailableLocation.BackColor = System.Drawing.Color.Transparent;
            this.lblAvailableLocation.ForeColor = System.Drawing.SystemColors.Control;
            this.lblAvailableLocation.Location = new System.Drawing.Point(137, 42);
            this.lblAvailableLocation.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAvailableLocation.Name = "lblAvailableLocation";
            this.lblAvailableLocation.Size = new System.Drawing.Size(109, 15);
            this.lblAvailableLocation.TabIndex = 26;
            this.lblAvailableLocation.Text = "Available Locations";
            // 
            // TXTCheckedSearch
            // 
            this.TXTCheckedSearch.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.TXTCheckedSearch.Location = new System.Drawing.Point(13, 99);
            this.TXTCheckedSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TXTCheckedSearch.Name = "TXTCheckedSearch";
            this.TXTCheckedSearch.Size = new System.Drawing.Size(116, 23);
            this.TXTCheckedSearch.TabIndex = 25;
            this.TXTCheckedSearch.TextChanged += new System.EventHandler(this.TXTLocSearch_TextChanged);
            this.TXTCheckedSearch.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TXTLocSearch_MouseUp);
            // 
            // TXTEntSearch
            // 
            this.TXTEntSearch.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.TXTEntSearch.Location = new System.Drawing.Point(13, 69);
            this.TXTEntSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TXTEntSearch.Name = "TXTEntSearch";
            this.TXTEntSearch.Size = new System.Drawing.Size(116, 23);
            this.TXTEntSearch.TabIndex = 24;
            this.TXTEntSearch.TextChanged += new System.EventHandler(this.TXTLocSearch_TextChanged);
            this.TXTEntSearch.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TXTLocSearch_MouseUp);
            // 
            // TXTLocSearch
            // 
            this.TXTLocSearch.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.TXTLocSearch.Location = new System.Drawing.Point(13, 39);
            this.TXTLocSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TXTLocSearch.Name = "TXTLocSearch";
            this.TXTLocSearch.Size = new System.Drawing.Size(116, 23);
            this.TXTLocSearch.TabIndex = 23;
            this.TXTLocSearch.TextChanged += new System.EventHandler(this.TXTLocSearch_TextChanged);
            this.TXTLocSearch.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TXTLocSearch_MouseUp);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolMenuStrip,
            this.OptionstoolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.devToolsToolStripMenuItem,
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.refreshToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(390, 24);
            this.menuStrip1.TabIndex = 43;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolMenuStrip
            // 
            this.fileToolMenuStrip.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewToolStripMenuItem1,
            this.LoadToolStripMenuItem,
            this.SavetoolStripMenuItem1,
            this.SaveAsToolStripMenuItem1,
            this.presetsToolStripMenuItem});
            this.fileToolMenuStrip.Name = "fileToolMenuStrip";
            this.fileToolMenuStrip.Size = new System.Drawing.Size(37, 20);
            this.fileToolMenuStrip.Text = "File";
            // 
            // NewToolStripMenuItem1
            // 
            this.NewToolStripMenuItem1.Name = "NewToolStripMenuItem1";
            this.NewToolStripMenuItem1.Size = new System.Drawing.Size(114, 22);
            this.NewToolStripMenuItem1.Text = "New";
            this.NewToolStripMenuItem1.Click += new System.EventHandler(this.NewToolStripMenuItem1_Click);
            // 
            // LoadToolStripMenuItem
            // 
            this.LoadToolStripMenuItem.Name = "LoadToolStripMenuItem";
            this.LoadToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.LoadToolStripMenuItem.Text = "Load";
            this.LoadToolStripMenuItem.Click += new System.EventHandler(this.LoadToolStripMenuItem_Click);
            // 
            // SavetoolStripMenuItem1
            // 
            this.SavetoolStripMenuItem1.Name = "SavetoolStripMenuItem1";
            this.SavetoolStripMenuItem1.Size = new System.Drawing.Size(114, 22);
            this.SavetoolStripMenuItem1.Text = "Save";
            this.SavetoolStripMenuItem1.Click += new System.EventHandler(this.SavetoolStripMenuItem1_Click);
            // 
            // SaveAsToolStripMenuItem1
            // 
            this.SaveAsToolStripMenuItem1.Name = "SaveAsToolStripMenuItem1";
            this.SaveAsToolStripMenuItem1.Size = new System.Drawing.Size(114, 22);
            this.SaveAsToolStripMenuItem1.Text = "Save As";
            this.SaveAsToolStripMenuItem1.Click += new System.EventHandler(this.SavetoolStripMenuItem1_Click);
            // 
            // presetsToolStripMenuItem
            // 
            this.presetsToolStripMenuItem.Name = "presetsToolStripMenuItem";
            this.presetsToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.presetsToolStripMenuItem.Text = "Presets";
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
            this.RandomizerOptionsToolStripMenuItem1.Size = new System.Drawing.Size(179, 22);
            this.RandomizerOptionsToolStripMenuItem1.Text = "RandomizerOptions";
            // 
            // logicOptionsToolStripMenuItem
            // 
            this.logicOptionsToolStripMenuItem.Name = "logicOptionsToolStripMenuItem";
            this.logicOptionsToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.logicOptionsToolStripMenuItem.Text = "Logic Options";
            this.logicOptionsToolStripMenuItem.Click += new System.EventHandler(this.logicOptionsToolStripMenuItem_Click);
            // 
            // miscOptionsToolStripMenuItem
            // 
            this.miscOptionsToolStripMenuItem.Name = "miscOptionsToolStripMenuItem";
            this.miscOptionsToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.miscOptionsToolStripMenuItem.Text = "Misc Options";
            this.miscOptionsToolStripMenuItem.Click += new System.EventHandler(this.miscOptionsToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.logicEditorToolStripMenuItem,
            this.importSpoilerLogToolStripMenuItem,
            this.spoilerLogToolsToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // logicEditorToolStripMenuItem
            // 
            this.logicEditorToolStripMenuItem.Name = "logicEditorToolStripMenuItem";
            this.logicEditorToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.logicEditorToolStripMenuItem.Text = "Logic Editor";
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
            // 
            // devToolsToolStripMenuItem
            // 
            this.devToolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewAsUserToolStripMenuItem,
            this.CodeTestingToolStripMenuItem});
            this.devToolsToolStripMenuItem.Name = "devToolsToolStripMenuItem";
            this.devToolsToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            this.devToolsToolStripMenuItem.Text = "DevTools";
            // 
            // viewAsUserToolStripMenuItem
            // 
            this.viewAsUserToolStripMenuItem.Name = "viewAsUserToolStripMenuItem";
            this.viewAsUserToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.viewAsUserToolStripMenuItem.Text = "View as User";
            // 
            // CodeTestingToolStripMenuItem
            // 
            this.CodeTestingToolStripMenuItem.Name = "CodeTestingToolStripMenuItem";
            this.CodeTestingToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.CodeTestingToolStripMenuItem.Text = "Code Testing";
            this.CodeTestingToolStripMenuItem.Click += new System.EventHandler(this.CodeTestingToolStripMenuItem_Click);
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
            // MainInterface
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(390, 591);
            this.Controls.Add(this.lblSwapPathfinder);
            this.Controls.Add(this.BTNFindPath);
            this.Controls.Add(this.LBPathFinder);
            this.Controls.Add(this.LBCheckedLocations);
            this.Controls.Add(this.LBValidEntrances);
            this.Controls.Add(this.LBValidLocations);
            this.Controls.Add(this.CMBEnd);
            this.Controls.Add(this.CMBStart);
            this.Controls.Add(this.CHKShowAll);
            this.Controls.Add(this.BTNSetEntrance);
            this.Controls.Add(this.BTNSetItem);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblAvailableEntrances);
            this.Controls.Add(this.lblCheckedLocation);
            this.Controls.Add(this.lblAvailableLocation);
            this.Controls.Add(this.TXTCheckedSearch);
            this.Controls.Add(this.TXTEntSearch);
            this.Controls.Add(this.TXTLocSearch);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainInterface";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainInterface_FormClosing);
            this.Load += new System.EventHandler(this.MainInterface_Load);
            this.ResizeEnd += new System.EventHandler(this.MainInterface_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainInterface_KeyDown);
            this.Resize += new System.EventHandler(this.MainInterface_Resize);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
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
        private System.Windows.Forms.ToolStripMenuItem NewToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem LoadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SavetoolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem SaveAsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem OptionstoolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem RandomizerOptionsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem logicOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem miscOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logicEditorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importSpoilerLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spoilerLogToolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem devToolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewAsUserToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CodeTestingToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem presetsToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}

