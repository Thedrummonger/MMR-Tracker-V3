
namespace Windows_Form_Frontend
{
    partial class StaticOptionSelect
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StaticOptionSelect));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.chkUpdates = new System.Windows.Forms.CheckBox();
            this.chkHorizontal = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.chkTooltips = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.nudFontSize = new System.Windows.Forms.NumericUpDown();
            this.cmbFontStyle = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label11 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.chkCompressSave = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this.chkEntranceFeatures = new System.Windows.Forms.CheckBox();
            this.label14 = new System.Windows.Forms.Label();
            this.chkCheckCoupled = new System.Windows.Forms.CheckBox();
            this.label16 = new System.Windows.Forms.Label();
            this.chkUnrandExits = new System.Windows.Forms.CheckBox();
            this.label18 = new System.Windows.Forms.Label();
            this.chkRedundantPaths = new System.Windows.Forms.CheckBox();
            this.label20 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.nudMaxUndo = new System.Windows.Forms.NumericUpDown();
            this.chkSeperateUnavailableMarked = new System.Windows.Forms.CheckBox();
            this.label24 = new System.Windows.Forms.Label();
            this.chkShowUnavailableMarked = new System.Windows.Forms.CheckBox();
            this.label26 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudFontSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxUndo)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(35, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Check for updates";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label2.Location = new System.Drawing.Point(15, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 18);
            this.label2.TabIndex = 1;
            this.label2.Text = "?";
            this.toolTip1.SetToolTip(this.label2, "Should the tracker check for updates and notify you when a new one is available?");
            // 
            // chkUpdates
            // 
            this.chkUpdates.AutoSize = true;
            this.chkUpdates.Location = new System.Drawing.Point(257, 15);
            this.chkUpdates.Name = "chkUpdates";
            this.chkUpdates.Size = new System.Drawing.Size(15, 14);
            this.chkUpdates.TabIndex = 2;
            this.chkUpdates.UseVisualStyleBackColor = true;
            this.chkUpdates.CheckStateChanged += new System.EventHandler(this.chkUpdates_CheckStateChanged);
            // 
            // chkHorizontal
            // 
            this.chkHorizontal.AutoSize = true;
            this.chkHorizontal.Location = new System.Drawing.Point(257, 34);
            this.chkHorizontal.Name = "chkHorizontal";
            this.chkHorizontal.Size = new System.Drawing.Size(15, 14);
            this.chkHorizontal.TabIndex = 5;
            this.chkHorizontal.UseVisualStyleBackColor = true;
            this.chkHorizontal.CheckStateChanged += new System.EventHandler(this.chkUpdates_CheckStateChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label3.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label3.Location = new System.Drawing.Point(15, 31);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 18);
            this.label3.TabIndex = 4;
            this.label3.Text = "?";
            this.toolTip1.SetToolTip(this.label3, "Should the tracker display the Valid Location List and Checked Item List Side by " +
        "side instead of on top of each other? ");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label4.Location = new System.Drawing.Point(35, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(110, 16);
            this.label4.TabIndex = 3;
            this.label4.Text = "Horizontal Layout";
            // 
            // chkTooltips
            // 
            this.chkTooltips.AutoSize = true;
            this.chkTooltips.Location = new System.Drawing.Point(257, 192);
            this.chkTooltips.Name = "chkTooltips";
            this.chkTooltips.Size = new System.Drawing.Size(15, 14);
            this.chkTooltips.TabIndex = 8;
            this.chkTooltips.UseVisualStyleBackColor = true;
            this.chkTooltips.CheckStateChanged += new System.EventHandler(this.chkUpdates_CheckStateChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label5.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label5.Location = new System.Drawing.Point(15, 190);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 18);
            this.label5.TabIndex = 7;
            this.label5.Text = "?";
            this.toolTip1.SetToolTip(this.label5, "Should the tracker display tooltips that show the full text of an entry when you " +
        "mouse over it?");
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label6.Location = new System.Drawing.Point(35, 190);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(98, 16);
            this.label6.TabIndex = 6;
            this.label6.Text = "Show ToolTips";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label7.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label7.Location = new System.Drawing.Point(15, 246);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(17, 18);
            this.label7.TabIndex = 10;
            this.label7.Text = "?";
            this.toolTip1.SetToolTip(this.label7, "The font the tracker will try to display most list entries in.");
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label8.Location = new System.Drawing.Point(38, 238);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(62, 16);
            this.label8.TabIndex = 9;
            this.label8.Text = "Font Size";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label10.Location = new System.Drawing.Point(38, 258);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(66, 16);
            this.label10.TabIndex = 12;
            this.label10.Text = "Font Style";
            // 
            // nudFontSize
            // 
            this.nudFontSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.nudFontSize.Location = new System.Drawing.Point(221, 237);
            this.nudFontSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudFontSize.Name = "nudFontSize";
            this.nudFontSize.Size = new System.Drawing.Size(51, 20);
            this.nudFontSize.TabIndex = 14;
            this.nudFontSize.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.nudFontSize.ValueChanged += new System.EventHandler(this.nudFontSize_ValueChanged);
            // 
            // cmbFontStyle
            // 
            this.cmbFontStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cmbFontStyle.FormattingEnabled = true;
            this.cmbFontStyle.Location = new System.Drawing.Point(115, 257);
            this.cmbFontStyle.Name = "cmbFontStyle";
            this.cmbFontStyle.Size = new System.Drawing.Size(157, 21);
            this.cmbFontStyle.TabIndex = 15;
            this.cmbFontStyle.SelectedValueChanged += new System.EventHandler(this.cmbFontStyle_SelectedValueChanged);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.button1.Location = new System.Drawing.Point(15, 284);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(159, 22);
            this.button1.TabIndex = 16;
            this.button1.Text = "Apply to current instance";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.button2.Location = new System.Drawing.Point(180, 284);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(92, 22);
            this.button2.TabIndex = 17;
            this.button2.Text = "Set as Default";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label11.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label11.Location = new System.Drawing.Point(15, 210);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(17, 18);
            this.label11.TabIndex = 22;
            this.label11.Text = "?";
            this.toolTip1.SetToolTip(this.label11, "Should the tracker Compress it\'s save files to take less disk space?\r\nCompressed " +
        "save files are unreadable and cannot be edited manually.");
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label13.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label13.Location = new System.Drawing.Point(15, 110);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(17, 18);
            this.label13.TabIndex = 25;
            this.label13.Text = "?";
            this.toolTip1.SetToolTip(this.label13, resources.GetString("label13.ToolTip"));
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label15.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label15.Location = new System.Drawing.Point(15, 130);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(17, 18);
            this.label15.TabIndex = 28;
            this.label15.Text = "?";
            this.toolTip1.SetToolTip(this.label15, "When checking and entrance, should the coupled entrance be automatically checked." +
        "\r\n\r\nShould be disabled if playing with Decoupled entrances.");
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label17.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label17.Location = new System.Drawing.Point(15, 150);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(17, 18);
            this.label17.TabIndex = 31;
            this.label17.Text = "?";
            this.toolTip1.SetToolTip(this.label17, "Should the pathfinder show each connection when printing a path.\r\n\r\nIf disabled o" +
        "nly randomized exits will be listed.");
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label19.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label19.Location = new System.Drawing.Point(15, 170);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(17, 18);
            this.label19.TabIndex = 34;
            this.label19.Text = "?";
            this.toolTip1.SetToolTip(this.label19, resources.GetString("label19.ToolTip"));
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label21.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label21.Location = new System.Drawing.Point(15, 51);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(17, 18);
            this.label21.TabIndex = 37;
            this.label21.Text = "?";
            this.toolTip1.SetToolTip(this.label21, "The maximum anount of actions that will be undoable with the undo button.");
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label23.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label23.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label23.Location = new System.Drawing.Point(15, 91);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(17, 18);
            this.label23.TabIndex = 43;
            this.label23.Text = "?";
            this.toolTip1.SetToolTip(this.label23, "If the above option is true, should those locations be moved to the bottom of the" +
        " list box");
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label25.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label25.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label25.Location = new System.Drawing.Point(15, 71);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(17, 18);
            this.label25.TabIndex = 40;
            this.label25.Text = "?";
            this.toolTip1.SetToolTip(this.label25, "Should Locations that are not logically accessable be shown in the list box if th" +
        "ey are marked");
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.button3.Location = new System.Drawing.Point(115, 237);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(100, 20);
            this.button3.TabIndex = 18;
            this.button3.Text = "System Default";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(115, 313);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(157, 24);
            this.textBox1.TabIndex = 19;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label9.Location = new System.Drawing.Point(15, 318);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(92, 16);
            this.label9.TabIndex = 20;
            this.label9.Text = "Font Example:";
            // 
            // chkCompressSave
            // 
            this.chkCompressSave.AutoSize = true;
            this.chkCompressSave.Location = new System.Drawing.Point(257, 214);
            this.chkCompressSave.Name = "chkCompressSave";
            this.chkCompressSave.Size = new System.Drawing.Size(15, 14);
            this.chkCompressSave.TabIndex = 23;
            this.chkCompressSave.UseVisualStyleBackColor = true;
            this.chkCompressSave.CheckedChanged += new System.EventHandler(this.chkUpdates_CheckStateChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label12.Location = new System.Drawing.Point(35, 212);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(136, 16);
            this.label12.TabIndex = 21;
            this.label12.Text = "Compress Save Files";
            // 
            // chkEntranceFeatures
            // 
            this.chkEntranceFeatures.AutoSize = true;
            this.chkEntranceFeatures.Location = new System.Drawing.Point(257, 113);
            this.chkEntranceFeatures.Name = "chkEntranceFeatures";
            this.chkEntranceFeatures.Size = new System.Drawing.Size(15, 14);
            this.chkEntranceFeatures.TabIndex = 26;
            this.chkEntranceFeatures.UseVisualStyleBackColor = true;
            this.chkEntranceFeatures.CheckStateChanged += new System.EventHandler(this.chkUpdates_CheckStateChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label14.Location = new System.Drawing.Point(35, 110);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(191, 16);
            this.label14.TabIndex = 24;
            this.label14.Text = "Show entrances in seperate list";
            // 
            // chkCheckCoupled
            // 
            this.chkCheckCoupled.AutoSize = true;
            this.chkCheckCoupled.Location = new System.Drawing.Point(257, 132);
            this.chkCheckCoupled.Name = "chkCheckCoupled";
            this.chkCheckCoupled.Size = new System.Drawing.Size(15, 14);
            this.chkCheckCoupled.TabIndex = 29;
            this.chkCheckCoupled.UseVisualStyleBackColor = true;
            this.chkCheckCoupled.CheckStateChanged += new System.EventHandler(this.chkUpdates_CheckStateChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label16.Location = new System.Drawing.Point(35, 130);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(190, 16);
            this.label16.TabIndex = 27;
            this.label16.Text = "Auto check Coupled Entrances";
            // 
            // chkUnrandExits
            // 
            this.chkUnrandExits.AutoSize = true;
            this.chkUnrandExits.Location = new System.Drawing.Point(257, 152);
            this.chkUnrandExits.Name = "chkUnrandExits";
            this.chkUnrandExits.Size = new System.Drawing.Size(15, 14);
            this.chkUnrandExits.TabIndex = 32;
            this.chkUnrandExits.UseVisualStyleBackColor = true;
            this.chkUnrandExits.CheckStateChanged += new System.EventHandler(this.chkUpdates_CheckStateChanged);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label18.Location = new System.Drawing.Point(35, 150);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(220, 16);
            this.label18.TabIndex = 30;
            this.label18.Text = "Show unrandomized exits pathfinder";
            // 
            // chkRedundantPaths
            // 
            this.chkRedundantPaths.AutoSize = true;
            this.chkRedundantPaths.Location = new System.Drawing.Point(257, 172);
            this.chkRedundantPaths.Name = "chkRedundantPaths";
            this.chkRedundantPaths.Size = new System.Drawing.Size(15, 14);
            this.chkRedundantPaths.TabIndex = 35;
            this.chkRedundantPaths.UseVisualStyleBackColor = true;
            this.chkRedundantPaths.CheckStateChanged += new System.EventHandler(this.chkUpdates_CheckStateChanged);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label20.Location = new System.Drawing.Point(35, 170);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(209, 16);
            this.label20.TabIndex = 33;
            this.label20.Text = "Show Redundant Paths Pathfinder";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label22.Location = new System.Drawing.Point(35, 51);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(115, 16);
            this.label22.TabIndex = 36;
            this.label22.Text = "Max Undo Actions";
            // 
            // nudMaxUndo
            // 
            this.nudMaxUndo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.nudMaxUndo.Location = new System.Drawing.Point(221, 51);
            this.nudMaxUndo.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudMaxUndo.Name = "nudMaxUndo";
            this.nudMaxUndo.Size = new System.Drawing.Size(51, 20);
            this.nudMaxUndo.TabIndex = 38;
            this.nudMaxUndo.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudMaxUndo.ValueChanged += new System.EventHandler(this.chkUpdates_CheckStateChanged);
            // 
            // chkSeperateUnavailableMarked
            // 
            this.chkSeperateUnavailableMarked.AutoSize = true;
            this.chkSeperateUnavailableMarked.Location = new System.Drawing.Point(257, 93);
            this.chkSeperateUnavailableMarked.Name = "chkSeperateUnavailableMarked";
            this.chkSeperateUnavailableMarked.Size = new System.Drawing.Size(15, 14);
            this.chkSeperateUnavailableMarked.TabIndex = 44;
            this.chkSeperateUnavailableMarked.UseVisualStyleBackColor = true;
            this.chkSeperateUnavailableMarked.CheckStateChanged += new System.EventHandler(this.chkUpdates_CheckStateChanged);
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label24.Location = new System.Drawing.Point(35, 91);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(226, 15);
            this.label24.TabIndex = 42;
            this.label24.Text = "Seperate Unavailable Marked Locations";
            // 
            // chkShowUnavailableMarked
            // 
            this.chkShowUnavailableMarked.AutoSize = true;
            this.chkShowUnavailableMarked.Location = new System.Drawing.Point(257, 74);
            this.chkShowUnavailableMarked.Name = "chkShowUnavailableMarked";
            this.chkShowUnavailableMarked.Size = new System.Drawing.Size(15, 14);
            this.chkShowUnavailableMarked.TabIndex = 41;
            this.chkShowUnavailableMarked.UseVisualStyleBackColor = true;
            this.chkShowUnavailableMarked.CheckStateChanged += new System.EventHandler(this.chkUpdates_CheckStateChanged);
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label26.Location = new System.Drawing.Point(35, 71);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(207, 15);
            this.label26.TabIndex = 39;
            this.label26.Text = "Show Unavailable Marked Locations";
            // 
            // StaticOptionSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(286, 345);
            this.Controls.Add(this.chkSeperateUnavailableMarked);
            this.Controls.Add(this.label23);
            this.Controls.Add(this.label24);
            this.Controls.Add(this.chkShowUnavailableMarked);
            this.Controls.Add(this.label25);
            this.Controls.Add(this.label26);
            this.Controls.Add(this.nudMaxUndo);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.label22);
            this.Controls.Add(this.chkRedundantPaths);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.chkUnrandExits);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.chkCheckCoupled);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.chkEntranceFeatures);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.chkCompressSave);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cmbFontStyle);
            this.Controls.Add(this.nudFontSize);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.chkTooltips);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.chkHorizontal);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.chkUpdates);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "StaticOptionSelect";
            this.Text = "Options";
            this.Load += new System.EventHandler(this.StaticOptionSelect_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudFontSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxUndo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkUpdates;
        private System.Windows.Forms.CheckBox chkHorizontal;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkTooltips;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown nudFontSize;
        private System.Windows.Forms.ComboBox cmbFontStyle;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox chkCompressSave;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox chkEntranceFeatures;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.CheckBox chkCheckCoupled;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.CheckBox chkUnrandExits;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.CheckBox chkRedundantPaths;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.NumericUpDown nudMaxUndo;
        private System.Windows.Forms.CheckBox chkSeperateUnavailableMarked;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.CheckBox chkShowUnavailableMarked;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label label26;
    }
}