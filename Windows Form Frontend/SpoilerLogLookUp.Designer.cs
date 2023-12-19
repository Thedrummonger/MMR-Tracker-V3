namespace Windows_Form_Frontend
{
    partial class SpoilerLogLookUp
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpoilerLogLookUp));
            this.lbRequiredItems = new System.Windows.Forms.ListBox();
            this.LBIgnoredItems = new System.Windows.Forms.ListBox();
            this.lbObtainable = new System.Windows.Forms.ListBox();
            this.btnAddReq = new System.Windows.Forms.Button();
            this.btnAddIgnored = new System.Windows.Forms.Button();
            this.btnCheckSeed = new System.Windows.Forms.Button();
            this.LabelSeedCheckChecksIgnored = new System.Windows.Forms.Label();
            this.LabelSeedCheckItemsNeeded = new System.Windows.Forms.Label();
            this.labelSeedCheckResults = new System.Windows.Forms.Label();
            this.chkShowObtainable = new System.Windows.Forms.CheckBox();
            this.chkShowUnObtainable = new System.Windows.Forms.CheckBox();
            this.txtSeedCheckFilter = new System.Windows.Forms.TextBox();
            this.gbSeedChecker = new System.Windows.Forms.GroupBox();
            this.tlpSeedCheckerMaster = new System.Windows.Forms.TableLayoutPanel();
            this.tlpSeedCheckerRight = new System.Windows.Forms.TableLayoutPanel();
            this.tlpSeedCheckerLeft = new System.Windows.Forms.TableLayoutPanel();
            this.btnGenPlaythrough = new System.Windows.Forms.Button();
            this.chkOnlyImportant = new System.Windows.Forms.CheckBox();
            this.cmbWinCon = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtWinConFilter = new System.Windows.Forms.TextBox();
            this.gbPlaythroughGen = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.gbSpoilerLogLookup = new System.Windows.Forms.GroupBox();
            this.tlpSpoilerLogLookup = new System.Windows.Forms.TableLayoutPanel();
            this.lbSpoilerLookupItems = new System.Windows.Forms.ListBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.chkShowMacros = new System.Windows.Forms.CheckBox();
            this.btnArea = new System.Windows.Forms.Button();
            this.btnSphere = new System.Windows.Forms.Button();
            this.btnLocation = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.regenerateGlobalPlaythroughToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.regenerateLocalSpoilerLogToolsPlaythroughToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateHintToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wayOfTheHeroToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.foolishToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.randomLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.itemAreaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playthroughLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playtrhoughItemAreaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tlpSpoilerLogToolsMaster = new System.Windows.Forms.TableLayoutPanel();
            this.gbSeedChecker.SuspendLayout();
            this.tlpSeedCheckerMaster.SuspendLayout();
            this.tlpSeedCheckerRight.SuspendLayout();
            this.tlpSeedCheckerLeft.SuspendLayout();
            this.gbPlaythroughGen.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.gbSpoilerLogLookup.SuspendLayout();
            this.tlpSpoilerLogLookup.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.tlpSpoilerLogToolsMaster.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbRequiredItems
            // 
            this.lbRequiredItems.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tlpSeedCheckerLeft.SetColumnSpan(this.lbRequiredItems, 2);
            this.lbRequiredItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbRequiredItems.FormattingEnabled = true;
            this.lbRequiredItems.HorizontalScrollbar = true;
            this.lbRequiredItems.IntegralHeight = false;
            this.lbRequiredItems.ItemHeight = 15;
            this.lbRequiredItems.Location = new System.Drawing.Point(3, 28);
            this.lbRequiredItems.Name = "lbRequiredItems";
            this.lbRequiredItems.Size = new System.Drawing.Size(154, 52);
            this.lbRequiredItems.TabIndex = 0;
            this.lbRequiredItems.DoubleClick += new System.EventHandler(this.lbRequiredItems_DoubleClick);
            // 
            // LBIgnoredItems
            // 
            this.LBIgnoredItems.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tlpSeedCheckerLeft.SetColumnSpan(this.LBIgnoredItems, 2);
            this.LBIgnoredItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LBIgnoredItems.FormattingEnabled = true;
            this.LBIgnoredItems.HorizontalScrollbar = true;
            this.LBIgnoredItems.IntegralHeight = false;
            this.LBIgnoredItems.ItemHeight = 15;
            this.LBIgnoredItems.Location = new System.Drawing.Point(3, 111);
            this.LBIgnoredItems.Name = "LBIgnoredItems";
            this.LBIgnoredItems.Size = new System.Drawing.Size(154, 52);
            this.LBIgnoredItems.TabIndex = 1;
            this.LBIgnoredItems.DoubleClick += new System.EventHandler(this.LBIgnoredItems_DoubleClick);
            // 
            // lbObtainable
            // 
            this.lbObtainable.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tlpSeedCheckerRight.SetColumnSpan(this.lbObtainable, 2);
            this.lbObtainable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbObtainable.FormattingEnabled = true;
            this.lbObtainable.HorizontalScrollbar = true;
            this.lbObtainable.ItemHeight = 15;
            this.lbObtainable.Location = new System.Drawing.Point(3, 53);
            this.lbObtainable.Name = "lbObtainable";
            this.lbObtainable.Size = new System.Drawing.Size(155, 110);
            this.lbObtainable.TabIndex = 2;
            this.lbObtainable.DoubleClick += new System.EventHandler(this.btnCheckSeed_Click);
            // 
            // btnAddReq
            // 
            this.btnAddReq.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnAddReq.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAddReq.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddReq.ForeColor = System.Drawing.Color.Black;
            this.btnAddReq.Location = new System.Drawing.Point(115, 2);
            this.btnAddReq.Margin = new System.Windows.Forms.Padding(0, 2, 2, 0);
            this.btnAddReq.Name = "btnAddReq";
            this.btnAddReq.Size = new System.Drawing.Size(43, 23);
            this.btnAddReq.TabIndex = 4;
            this.btnAddReq.Text = "Add";
            this.btnAddReq.UseVisualStyleBackColor = false;
            this.btnAddReq.Click += new System.EventHandler(this.btnAddReq_Click);
            // 
            // btnAddIgnored
            // 
            this.btnAddIgnored.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnAddIgnored.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAddIgnored.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddIgnored.ForeColor = System.Drawing.Color.Black;
            this.btnAddIgnored.Location = new System.Drawing.Point(115, 85);
            this.btnAddIgnored.Margin = new System.Windows.Forms.Padding(0, 2, 2, 0);
            this.btnAddIgnored.Name = "btnAddIgnored";
            this.btnAddIgnored.Size = new System.Drawing.Size(43, 23);
            this.btnAddIgnored.TabIndex = 5;
            this.btnAddIgnored.Text = "Add";
            this.btnAddIgnored.UseVisualStyleBackColor = false;
            this.btnAddIgnored.Click += new System.EventHandler(this.btnAddIgnored_Click);
            // 
            // btnCheckSeed
            // 
            this.btnCheckSeed.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnCheckSeed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCheckSeed.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCheckSeed.ForeColor = System.Drawing.Color.Black;
            this.btnCheckSeed.Location = new System.Drawing.Point(76, 2);
            this.btnCheckSeed.Margin = new System.Windows.Forms.Padding(0, 2, 2, 0);
            this.btnCheckSeed.Name = "btnCheckSeed";
            this.btnCheckSeed.Size = new System.Drawing.Size(83, 23);
            this.btnCheckSeed.TabIndex = 6;
            this.btnCheckSeed.Text = "Check Seed";
            this.btnCheckSeed.UseVisualStyleBackColor = false;
            this.btnCheckSeed.Click += new System.EventHandler(this.btnCheckSeed_Click);
            // 
            // LabelSeedCheckChecksIgnored
            // 
            this.LabelSeedCheckChecksIgnored.AutoSize = true;
            this.LabelSeedCheckChecksIgnored.BackColor = System.Drawing.Color.Transparent;
            this.LabelSeedCheckChecksIgnored.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelSeedCheckChecksIgnored.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.LabelSeedCheckChecksIgnored.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.LabelSeedCheckChecksIgnored.Location = new System.Drawing.Point(5, 88);
            this.LabelSeedCheckChecksIgnored.Margin = new System.Windows.Forms.Padding(5);
            this.LabelSeedCheckChecksIgnored.Name = "LabelSeedCheckChecksIgnored";
            this.LabelSeedCheckChecksIgnored.Size = new System.Drawing.Size(105, 15);
            this.LabelSeedCheckChecksIgnored.TabIndex = 7;
            this.LabelSeedCheckChecksIgnored.Text = "Ignored Checks";
            // 
            // LabelSeedCheckItemsNeeded
            // 
            this.LabelSeedCheckItemsNeeded.AutoSize = true;
            this.LabelSeedCheckItemsNeeded.BackColor = System.Drawing.Color.Transparent;
            this.LabelSeedCheckItemsNeeded.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelSeedCheckItemsNeeded.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.LabelSeedCheckItemsNeeded.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.LabelSeedCheckItemsNeeded.Location = new System.Drawing.Point(5, 5);
            this.LabelSeedCheckItemsNeeded.Margin = new System.Windows.Forms.Padding(5);
            this.LabelSeedCheckItemsNeeded.Name = "LabelSeedCheckItemsNeeded";
            this.LabelSeedCheckItemsNeeded.Size = new System.Drawing.Size(105, 15);
            this.LabelSeedCheckItemsNeeded.TabIndex = 8;
            this.LabelSeedCheckItemsNeeded.Text = "Items Needed";
            // 
            // labelSeedCheckResults
            // 
            this.labelSeedCheckResults.AutoSize = true;
            this.labelSeedCheckResults.BackColor = System.Drawing.Color.Transparent;
            this.labelSeedCheckResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelSeedCheckResults.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelSeedCheckResults.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.labelSeedCheckResults.Location = new System.Drawing.Point(5, 5);
            this.labelSeedCheckResults.Margin = new System.Windows.Forms.Padding(5);
            this.labelSeedCheckResults.Name = "labelSeedCheckResults";
            this.labelSeedCheckResults.Size = new System.Drawing.Size(66, 15);
            this.labelSeedCheckResults.TabIndex = 9;
            this.labelSeedCheckResults.Text = "Results";
            // 
            // chkShowObtainable
            // 
            this.chkShowObtainable.AutoSize = true;
            this.chkShowObtainable.BackColor = System.Drawing.Color.Transparent;
            this.chkShowObtainable.Checked = true;
            this.chkShowObtainable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tlpSeedCheckerLeft.SetColumnSpan(this.chkShowObtainable, 2);
            this.chkShowObtainable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowObtainable.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.chkShowObtainable.Location = new System.Drawing.Point(0, 166);
            this.chkShowObtainable.Margin = new System.Windows.Forms.Padding(0);
            this.chkShowObtainable.Name = "chkShowObtainable";
            this.chkShowObtainable.Size = new System.Drawing.Size(160, 20);
            this.chkShowObtainable.TabIndex = 10;
            this.chkShowObtainable.Text = "Show Obtainable";
            this.chkShowObtainable.UseVisualStyleBackColor = false;
            this.chkShowObtainable.CheckedChanged += new System.EventHandler(this.txtSeedCheckFilter_TextChanged);
            // 
            // chkShowUnObtainable
            // 
            this.chkShowUnObtainable.AutoSize = true;
            this.chkShowUnObtainable.BackColor = System.Drawing.Color.Transparent;
            this.chkShowUnObtainable.Checked = true;
            this.chkShowUnObtainable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tlpSeedCheckerRight.SetColumnSpan(this.chkShowUnObtainable, 2);
            this.chkShowUnObtainable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowUnObtainable.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.chkShowUnObtainable.Location = new System.Drawing.Point(0, 166);
            this.chkShowUnObtainable.Margin = new System.Windows.Forms.Padding(0);
            this.chkShowUnObtainable.Name = "chkShowUnObtainable";
            this.chkShowUnObtainable.Size = new System.Drawing.Size(161, 20);
            this.chkShowUnObtainable.TabIndex = 11;
            this.chkShowUnObtainable.Text = "Show UnObtainable";
            this.chkShowUnObtainable.UseVisualStyleBackColor = false;
            this.chkShowUnObtainable.CheckedChanged += new System.EventHandler(this.txtSeedCheckFilter_TextChanged);
            // 
            // txtSeedCheckFilter
            // 
            this.txtSeedCheckFilter.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tlpSeedCheckerRight.SetColumnSpan(this.txtSeedCheckFilter, 2);
            this.txtSeedCheckFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSeedCheckFilter.Location = new System.Drawing.Point(3, 28);
            this.txtSeedCheckFilter.Name = "txtSeedCheckFilter";
            this.txtSeedCheckFilter.Size = new System.Drawing.Size(155, 23);
            this.txtSeedCheckFilter.TabIndex = 13;
            this.txtSeedCheckFilter.TextChanged += new System.EventHandler(this.txtSeedCheckFilter_TextChanged);
            // 
            // gbSeedChecker
            // 
            this.gbSeedChecker.BackColor = System.Drawing.Color.Transparent;
            this.gbSeedChecker.Controls.Add(this.tlpSeedCheckerMaster);
            this.gbSeedChecker.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbSeedChecker.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.gbSeedChecker.Location = new System.Drawing.Point(3, 3);
            this.gbSeedChecker.Name = "gbSeedChecker";
            this.gbSeedChecker.Size = new System.Drawing.Size(327, 208);
            this.gbSeedChecker.TabIndex = 15;
            this.gbSeedChecker.TabStop = false;
            this.gbSeedChecker.Text = "Seed Checker";
            // 
            // tlpSeedCheckerMaster
            // 
            this.tlpSeedCheckerMaster.ColumnCount = 2;
            this.tlpSeedCheckerMaster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpSeedCheckerMaster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpSeedCheckerMaster.Controls.Add(this.tlpSeedCheckerRight, 1, 0);
            this.tlpSeedCheckerMaster.Controls.Add(this.tlpSeedCheckerLeft, 0, 0);
            this.tlpSeedCheckerMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSeedCheckerMaster.Location = new System.Drawing.Point(3, 19);
            this.tlpSeedCheckerMaster.Name = "tlpSeedCheckerMaster";
            this.tlpSeedCheckerMaster.RowCount = 1;
            this.tlpSeedCheckerMaster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSeedCheckerMaster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpSeedCheckerMaster.Size = new System.Drawing.Size(321, 186);
            this.tlpSeedCheckerMaster.TabIndex = 0;
            this.tlpSeedCheckerMaster.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel5_Paint);
            // 
            // tlpSeedCheckerRight
            // 
            this.tlpSeedCheckerRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.tlpSeedCheckerRight.ColumnCount = 2;
            this.tlpSeedCheckerRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSeedCheckerRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85F));
            this.tlpSeedCheckerRight.Controls.Add(this.chkShowUnObtainable, 0, 3);
            this.tlpSeedCheckerRight.Controls.Add(this.lbObtainable, 0, 2);
            this.tlpSeedCheckerRight.Controls.Add(this.txtSeedCheckFilter, 0, 1);
            this.tlpSeedCheckerRight.Controls.Add(this.labelSeedCheckResults, 0, 0);
            this.tlpSeedCheckerRight.Controls.Add(this.btnCheckSeed, 1, 0);
            this.tlpSeedCheckerRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSeedCheckerRight.Location = new System.Drawing.Point(160, 0);
            this.tlpSeedCheckerRight.Margin = new System.Windows.Forms.Padding(0);
            this.tlpSeedCheckerRight.Name = "tlpSeedCheckerRight";
            this.tlpSeedCheckerRight.RowCount = 4;
            this.tlpSeedCheckerRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpSeedCheckerRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpSeedCheckerRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSeedCheckerRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpSeedCheckerRight.Size = new System.Drawing.Size(161, 186);
            this.tlpSeedCheckerRight.TabIndex = 27;
            this.tlpSeedCheckerRight.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel4_Paint);
            // 
            // tlpSeedCheckerLeft
            // 
            this.tlpSeedCheckerLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.tlpSeedCheckerLeft.ColumnCount = 2;
            this.tlpSeedCheckerLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSeedCheckerLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tlpSeedCheckerLeft.Controls.Add(this.lbRequiredItems, 0, 1);
            this.tlpSeedCheckerLeft.Controls.Add(this.LabelSeedCheckItemsNeeded, 0, 0);
            this.tlpSeedCheckerLeft.Controls.Add(this.btnAddReq, 1, 0);
            this.tlpSeedCheckerLeft.Controls.Add(this.LBIgnoredItems, 0, 3);
            this.tlpSeedCheckerLeft.Controls.Add(this.btnAddIgnored, 1, 2);
            this.tlpSeedCheckerLeft.Controls.Add(this.LabelSeedCheckChecksIgnored, 0, 2);
            this.tlpSeedCheckerLeft.Controls.Add(this.chkShowObtainable, 0, 4);
            this.tlpSeedCheckerLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSeedCheckerLeft.Location = new System.Drawing.Point(0, 0);
            this.tlpSeedCheckerLeft.Margin = new System.Windows.Forms.Padding(0);
            this.tlpSeedCheckerLeft.Name = "tlpSeedCheckerLeft";
            this.tlpSeedCheckerLeft.RowCount = 5;
            this.tlpSeedCheckerLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpSeedCheckerLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpSeedCheckerLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpSeedCheckerLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpSeedCheckerLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpSeedCheckerLeft.Size = new System.Drawing.Size(160, 186);
            this.tlpSeedCheckerLeft.TabIndex = 26;
            // 
            // btnGenPlaythrough
            // 
            this.btnGenPlaythrough.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnGenPlaythrough.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnGenPlaythrough.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenPlaythrough.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnGenPlaythrough.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnGenPlaythrough.Location = new System.Drawing.Point(191, 25);
            this.btnGenPlaythrough.Margin = new System.Windows.Forms.Padding(0);
            this.btnGenPlaythrough.Name = "btnGenPlaythrough";
            this.btnGenPlaythrough.Size = new System.Drawing.Size(130, 25);
            this.btnGenPlaythrough.TabIndex = 16;
            this.btnGenPlaythrough.Text = "Generate Playthrough";
            this.btnGenPlaythrough.UseVisualStyleBackColor = false;
            this.btnGenPlaythrough.Click += new System.EventHandler(this.btnGenPlaythrough_Click);
            // 
            // chkOnlyImportant
            // 
            this.chkOnlyImportant.AutoSize = true;
            this.chkOnlyImportant.BackColor = System.Drawing.Color.Transparent;
            this.chkOnlyImportant.Checked = true;
            this.chkOnlyImportant.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOnlyImportant.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkOnlyImportant.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkOnlyImportant.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.chkOnlyImportant.Location = new System.Drawing.Point(194, 3);
            this.chkOnlyImportant.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.chkOnlyImportant.Name = "chkOnlyImportant";
            this.chkOnlyImportant.Size = new System.Drawing.Size(127, 19);
            this.chkOnlyImportant.TabIndex = 17;
            this.chkOnlyImportant.Text = "FIlter Important";
            this.chkOnlyImportant.UseVisualStyleBackColor = false;
            // 
            // cmbWinCon
            // 
            this.cmbWinCon.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tableLayoutPanel1.SetColumnSpan(this.cmbWinCon, 2);
            this.cmbWinCon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbWinCon.FormattingEnabled = true;
            this.cmbWinCon.Location = new System.Drawing.Point(0, 26);
            this.cmbWinCon.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.cmbWinCon.Name = "cmbWinCon";
            this.cmbWinCon.Size = new System.Drawing.Size(191, 23);
            this.cmbWinCon.TabIndex = 18;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label6.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label6.Location = new System.Drawing.Point(3, 5);
            this.label6.Margin = new System.Windows.Forms.Padding(3, 5, 2, 5);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(55, 15);
            this.label6.TabIndex = 19;
            this.label6.Text = "Win Con";
            // 
            // txtWinConFilter
            // 
            this.txtWinConFilter.BackColor = System.Drawing.SystemColors.ControlDark;
            this.txtWinConFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtWinConFilter.Location = new System.Drawing.Point(60, 2);
            this.txtWinConFilter.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.txtWinConFilter.Name = "txtWinConFilter";
            this.txtWinConFilter.PlaceholderText = "filter win con";
            this.txtWinConFilter.Size = new System.Drawing.Size(131, 23);
            this.txtWinConFilter.TabIndex = 20;
            this.txtWinConFilter.TextChanged += new System.EventHandler(this.txtWinConFilter_TextChanged);
            // 
            // gbPlaythroughGen
            // 
            this.gbPlaythroughGen.BackColor = System.Drawing.Color.Transparent;
            this.gbPlaythroughGen.Controls.Add(this.tableLayoutPanel1);
            this.gbPlaythroughGen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbPlaythroughGen.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.gbPlaythroughGen.Location = new System.Drawing.Point(3, 217);
            this.gbPlaythroughGen.Name = "gbPlaythroughGen";
            this.gbPlaythroughGen.Size = new System.Drawing.Size(327, 72);
            this.gbPlaythroughGen.TabIndex = 22;
            this.gbPlaythroughGen.TabStop = false;
            this.gbPlaythroughGen.Text = "Playthrough Generator";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tableLayoutPanel1.Controls.Add(this.btnGenPlaythrough, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.chkOnlyImportant, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtWinConFilter, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.cmbWinCon, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 19);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(321, 50);
            this.tableLayoutPanel1.TabIndex = 26;
            // 
            // gbSpoilerLogLookup
            // 
            this.gbSpoilerLogLookup.BackColor = System.Drawing.Color.Transparent;
            this.gbSpoilerLogLookup.Controls.Add(this.tlpSpoilerLogLookup);
            this.gbSpoilerLogLookup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbSpoilerLogLookup.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.gbSpoilerLogLookup.Location = new System.Drawing.Point(336, 3);
            this.gbSpoilerLogLookup.Name = "gbSpoilerLogLookup";
            this.tlpSpoilerLogToolsMaster.SetRowSpan(this.gbSpoilerLogLookup, 2);
            this.gbSpoilerLogLookup.Size = new System.Drawing.Size(138, 286);
            this.gbSpoilerLogLookup.TabIndex = 23;
            this.gbSpoilerLogLookup.TabStop = false;
            this.gbSpoilerLogLookup.Text = "Spoiler log lookup";
            // 
            // tlpSpoilerLogLookup
            // 
            this.tlpSpoilerLogLookup.ColumnCount = 2;
            this.tlpSpoilerLogLookup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpSpoilerLogLookup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpSpoilerLogLookup.Controls.Add(this.lbSpoilerLookupItems, 0, 3);
            this.tlpSpoilerLogLookup.Controls.Add(this.textBox2, 0, 2);
            this.tlpSpoilerLogLookup.Controls.Add(this.chkShowMacros, 1, 1);
            this.tlpSpoilerLogLookup.Controls.Add(this.btnArea, 0, 0);
            this.tlpSpoilerLogLookup.Controls.Add(this.btnSphere, 1, 0);
            this.tlpSpoilerLogLookup.Controls.Add(this.btnLocation, 0, 1);
            this.tlpSpoilerLogLookup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSpoilerLogLookup.Location = new System.Drawing.Point(3, 19);
            this.tlpSpoilerLogLookup.Name = "tlpSpoilerLogLookup";
            this.tlpSpoilerLogLookup.RowCount = 4;
            this.tlpSpoilerLogLookup.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tlpSpoilerLogLookup.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tlpSpoilerLogLookup.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpSpoilerLogLookup.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSpoilerLogLookup.Size = new System.Drawing.Size(132, 264);
            this.tlpSpoilerLogLookup.TabIndex = 25;
            // 
            // lbSpoilerLookupItems
            // 
            this.lbSpoilerLookupItems.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tlpSpoilerLogLookup.SetColumnSpan(this.lbSpoilerLookupItems, 2);
            this.lbSpoilerLookupItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbSpoilerLookupItems.FormattingEnabled = true;
            this.lbSpoilerLookupItems.HorizontalScrollbar = true;
            this.lbSpoilerLookupItems.ItemHeight = 15;
            this.lbSpoilerLookupItems.Location = new System.Drawing.Point(3, 84);
            this.lbSpoilerLookupItems.Name = "lbSpoilerLookupItems";
            this.lbSpoilerLookupItems.Size = new System.Drawing.Size(126, 177);
            this.lbSpoilerLookupItems.TabIndex = 0;
            this.lbSpoilerLookupItems.SelectedValueChanged += new System.EventHandler(this.listBox1_SelectedValueChanged);
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tlpSpoilerLogLookup.SetColumnSpan(this.textBox2, 2);
            this.textBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox2.Location = new System.Drawing.Point(3, 59);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(126, 23);
            this.textBox2.TabIndex = 4;
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // chkShowMacros
            // 
            this.chkShowMacros.AutoSize = true;
            this.chkShowMacros.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowMacros.Font = new System.Drawing.Font("Segoe UI", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkShowMacros.Location = new System.Drawing.Point(69, 28);
            this.chkShowMacros.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.chkShowMacros.Name = "chkShowMacros";
            this.chkShowMacros.Size = new System.Drawing.Size(63, 28);
            this.chkShowMacros.TabIndex = 5;
            this.chkShowMacros.Text = "Show Macros";
            this.chkShowMacros.UseVisualStyleBackColor = true;
            this.chkShowMacros.CheckedChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // btnArea
            // 
            this.btnArea.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnArea.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnArea.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnArea.Location = new System.Drawing.Point(2, 2);
            this.btnArea.Margin = new System.Windows.Forms.Padding(2, 2, 0, 0);
            this.btnArea.Name = "btnArea";
            this.btnArea.Size = new System.Drawing.Size(64, 26);
            this.btnArea.TabIndex = 1;
            this.btnArea.Text = "Area";
            this.btnArea.UseVisualStyleBackColor = false;
            this.btnArea.Click += new System.EventHandler(this.btnArea_Click);
            // 
            // btnSphere
            // 
            this.btnSphere.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnSphere.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSphere.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSphere.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnSphere.Location = new System.Drawing.Point(66, 2);
            this.btnSphere.Margin = new System.Windows.Forms.Padding(0, 2, 2, 0);
            this.btnSphere.Name = "btnSphere";
            this.btnSphere.Size = new System.Drawing.Size(64, 26);
            this.btnSphere.TabIndex = 2;
            this.btnSphere.Text = "Sphere";
            this.btnSphere.UseVisualStyleBackColor = false;
            this.btnSphere.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnLocation
            // 
            this.btnLocation.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnLocation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLocation.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLocation.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnLocation.Location = new System.Drawing.Point(2, 30);
            this.btnLocation.Margin = new System.Windows.Forms.Padding(2, 2, 0, 0);
            this.btnLocation.Name = "btnLocation";
            this.btnLocation.Size = new System.Drawing.Size(64, 26);
            this.btnLocation.TabIndex = 3;
            this.btnLocation.Text = "Location";
            this.btnLocation.UseVisualStyleBackColor = false;
            this.btnLocation.Click += new System.EventHandler(this.button3_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolsToolStripMenuItem,
            this.generateHintToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(477, 24);
            this.menuStrip1.TabIndex = 24;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.regenerateGlobalPlaythroughToolStripMenuItem,
            this.regenerateLocalSpoilerLogToolsPlaythroughToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // regenerateGlobalPlaythroughToolStripMenuItem
            // 
            this.regenerateGlobalPlaythroughToolStripMenuItem.Name = "regenerateGlobalPlaythroughToolStripMenuItem";
            this.regenerateGlobalPlaythroughToolStripMenuItem.Size = new System.Drawing.Size(324, 22);
            this.regenerateGlobalPlaythroughToolStripMenuItem.Text = "Regenerate Global Playthrough";
            this.regenerateGlobalPlaythroughToolStripMenuItem.Click += new System.EventHandler(this.regenerateGlobalPlaythroughToolStripMenuItem_Click);
            // 
            // regenerateLocalSpoilerLogToolsPlaythroughToolStripMenuItem
            // 
            this.regenerateLocalSpoilerLogToolsPlaythroughToolStripMenuItem.Name = "regenerateLocalSpoilerLogToolsPlaythroughToolStripMenuItem";
            this.regenerateLocalSpoilerLogToolsPlaythroughToolStripMenuItem.Size = new System.Drawing.Size(324, 22);
            this.regenerateLocalSpoilerLogToolsPlaythroughToolStripMenuItem.Text = "Regenerate Local Spoiler Log Tools Playthrough";
            this.regenerateLocalSpoilerLogToolsPlaythroughToolStripMenuItem.Click += new System.EventHandler(this.regenerateLocalSpoilerLogToolsPlaythroughToolStripMenuItem_Click);
            // 
            // generateHintToolStripMenuItem
            // 
            this.generateHintToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wayOfTheHeroToolStripMenuItem,
            this.foolishToolStripMenuItem,
            this.randomLocationToolStripMenuItem,
            this.itemAreaToolStripMenuItem,
            this.playthroughLocationToolStripMenuItem,
            this.playtrhoughItemAreaToolStripMenuItem});
            this.generateHintToolStripMenuItem.Name = "generateHintToolStripMenuItem";
            this.generateHintToolStripMenuItem.Size = new System.Drawing.Size(92, 20);
            this.generateHintToolStripMenuItem.Text = "Generate Hint";
            // 
            // wayOfTheHeroToolStripMenuItem
            // 
            this.wayOfTheHeroToolStripMenuItem.Name = "wayOfTheHeroToolStripMenuItem";
            this.wayOfTheHeroToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.wayOfTheHeroToolStripMenuItem.Text = "Way of the Hero";
            this.wayOfTheHeroToolStripMenuItem.Click += new System.EventHandler(this.GenerateHint);
            // 
            // foolishToolStripMenuItem
            // 
            this.foolishToolStripMenuItem.Name = "foolishToolStripMenuItem";
            this.foolishToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.foolishToolStripMenuItem.Text = "Foolish";
            this.foolishToolStripMenuItem.Click += new System.EventHandler(this.GenerateHint);
            // 
            // randomLocationToolStripMenuItem
            // 
            this.randomLocationToolStripMenuItem.Name = "randomLocationToolStripMenuItem";
            this.randomLocationToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.randomLocationToolStripMenuItem.Text = "Random Location";
            this.randomLocationToolStripMenuItem.Click += new System.EventHandler(this.GenerateHint);
            // 
            // itemAreaToolStripMenuItem
            // 
            this.itemAreaToolStripMenuItem.Name = "itemAreaToolStripMenuItem";
            this.itemAreaToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.itemAreaToolStripMenuItem.Text = "Random Item Area";
            this.itemAreaToolStripMenuItem.Click += new System.EventHandler(this.GenerateHint);
            // 
            // playthroughLocationToolStripMenuItem
            // 
            this.playthroughLocationToolStripMenuItem.Name = "playthroughLocationToolStripMenuItem";
            this.playthroughLocationToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.playthroughLocationToolStripMenuItem.Text = "Playthrough Location";
            this.playthroughLocationToolStripMenuItem.Click += new System.EventHandler(this.GenerateHint);
            // 
            // playtrhoughItemAreaToolStripMenuItem
            // 
            this.playtrhoughItemAreaToolStripMenuItem.Name = "playtrhoughItemAreaToolStripMenuItem";
            this.playtrhoughItemAreaToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.playtrhoughItemAreaToolStripMenuItem.Text = "Playtrhough Item area";
            this.playtrhoughItemAreaToolStripMenuItem.Click += new System.EventHandler(this.GenerateHint);
            // 
            // tlpSpoilerLogToolsMaster
            // 
            this.tlpSpoilerLogToolsMaster.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.tlpSpoilerLogToolsMaster.ColumnCount = 2;
            this.tlpSpoilerLogToolsMaster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tlpSpoilerLogToolsMaster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpSpoilerLogToolsMaster.Controls.Add(this.gbSpoilerLogLookup, 1, 0);
            this.tlpSpoilerLogToolsMaster.Controls.Add(this.gbPlaythroughGen, 0, 1);
            this.tlpSpoilerLogToolsMaster.Controls.Add(this.gbSeedChecker, 0, 0);
            this.tlpSpoilerLogToolsMaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSpoilerLogToolsMaster.Location = new System.Drawing.Point(0, 24);
            this.tlpSpoilerLogToolsMaster.Name = "tlpSpoilerLogToolsMaster";
            this.tlpSpoilerLogToolsMaster.RowCount = 2;
            this.tlpSpoilerLogToolsMaster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSpoilerLogToolsMaster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 78F));
            this.tlpSpoilerLogToolsMaster.Size = new System.Drawing.Size(477, 292);
            this.tlpSpoilerLogToolsMaster.TabIndex = 25;
            // 
            // SpoilerLogLookUp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(477, 316);
            this.Controls.Add(this.tlpSpoilerLogToolsMaster);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "SpoilerLogLookUp";
            this.Text = "SpoilerLogTools";
            this.Load += new System.EventHandler(this.SpoilerLogLookUp_Load);
            this.gbSeedChecker.ResumeLayout(false);
            this.tlpSeedCheckerMaster.ResumeLayout(false);
            this.tlpSeedCheckerRight.ResumeLayout(false);
            this.tlpSeedCheckerRight.PerformLayout();
            this.tlpSeedCheckerLeft.ResumeLayout(false);
            this.tlpSeedCheckerLeft.PerformLayout();
            this.gbPlaythroughGen.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.gbSpoilerLogLookup.ResumeLayout(false);
            this.tlpSpoilerLogLookup.ResumeLayout(false);
            this.tlpSpoilerLogLookup.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tlpSpoilerLogToolsMaster.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lbRequiredItems;
        private System.Windows.Forms.ListBox LBIgnoredItems;
        private System.Windows.Forms.ListBox lbObtainable;
        private System.Windows.Forms.Button btnAddReq;
        private System.Windows.Forms.Button btnAddIgnored;
        private System.Windows.Forms.Button btnCheckSeed;
        private System.Windows.Forms.Label LabelSeedCheckChecksIgnored;
        private System.Windows.Forms.Label LabelSeedCheckItemsNeeded;
        private System.Windows.Forms.Label labelSeedCheckResults;
        private System.Windows.Forms.CheckBox chkShowObtainable;
        private System.Windows.Forms.CheckBox chkShowUnObtainable;
        private System.Windows.Forms.TextBox txtSeedCheckFilter;
        private System.Windows.Forms.GroupBox gbSeedChecker;
        private System.Windows.Forms.Button btnGenPlaythrough;
        private System.Windows.Forms.CheckBox chkOnlyImportant;
        private System.Windows.Forms.ComboBox cmbWinCon;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtWinConFilter;
        private System.Windows.Forms.GroupBox gbPlaythroughGen;
        private System.Windows.Forms.GroupBox gbSpoilerLogLookup;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button btnLocation;
        private System.Windows.Forms.Button btnSphere;
        private System.Windows.Forms.Button btnArea;
        private System.Windows.Forms.ListBox lbSpoilerLookupItems;
        private System.Windows.Forms.CheckBox chkShowMacros;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem regenerateGlobalPlaythroughToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateHintToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wayOfTheHeroToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem foolishToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem randomLocationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem itemAreaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playthroughLocationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playtrhoughItemAreaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem regenerateLocalSpoilerLogToolsPlaythroughToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tlpSeedCheckerLeft;
        private System.Windows.Forms.TableLayoutPanel tlpSeedCheckerRight;
        private System.Windows.Forms.TableLayoutPanel tlpSeedCheckerMaster;
        private System.Windows.Forms.TableLayoutPanel tlpSpoilerLogLookup;
        private System.Windows.Forms.TableLayoutPanel tlpSpoilerLogToolsMaster;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}