
namespace Windows_Form_Frontend
{
    partial class RandomizedStateEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RandomizedStateEditor));
            this.lvLocationList = new System.Windows.Forms.ListView();
            this.columnHeaderEntry = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderVanilla = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderRandomizedState = new System.Windows.Forms.ColumnHeader();
            this.btnSetRandomized = new System.Windows.Forms.Button();
            this.btnSetUnRandomized = new System.Windows.Forms.Button();
            this.btnSetManual = new System.Windows.Forms.Button();
            this.btnSetJunk = new System.Windows.Forms.Button();
            this.btnAddStartingItem = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lvTricks = new System.Windows.Forms.ListView();
            this.Tricks = new System.Windows.Forms.ColumnHeader();
            this.label4 = new System.Windows.Forms.Label();
            this.txtSearchAvailableStarting = new System.Windows.Forms.TextBox();
            this.TxtLocationSearch = new System.Windows.Forms.TextBox();
            this.txtTrickSearch = new System.Windows.Forms.TextBox();
            this.txtLocString = new System.Windows.Forms.TextBox();
            this.cmbLocationType = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtEntString = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtjunkString = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtStartString = new System.Windows.Forms.TextBox();
            this.button8 = new System.Windows.Forms.Button();
            this.chkShowRand = new System.Windows.Forms.CheckBox();
            this.chkShowJunk = new System.Windows.Forms.CheckBox();
            this.chkShowManual = new System.Windows.Forms.CheckBox();
            this.chkShowUnrand = new System.Windows.Forms.CheckBox();
            this.lbAvailableStarting = new System.Windows.Forms.ListBox();
            this.lbCurrentStarting = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnRemoveStartingItem = new System.Windows.Forms.Button();
            this.txtSearchCurrentStarting = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvLocationList
            // 
            this.lvLocationList.BackColor = System.Drawing.SystemColors.ControlDark;
            this.lvLocationList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderEntry,
            this.columnHeaderVanilla,
            this.columnHeaderRandomizedState});
            this.tableLayoutPanel1.SetColumnSpan(this.lvLocationList, 4);
            this.lvLocationList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvLocationList.Location = new System.Drawing.Point(3, 78);
            this.lvLocationList.Name = "lvLocationList";
            this.lvLocationList.Size = new System.Drawing.Size(439, 388);
            this.lvLocationList.TabIndex = 0;
            this.lvLocationList.UseCompatibleStateImageBehavior = false;
            this.lvLocationList.View = System.Windows.Forms.View.Details;
            this.lvLocationList.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lvLocationList_ItemChecked);
            // 
            // columnHeaderEntry
            // 
            this.columnHeaderEntry.Text = "Entry";
            this.columnHeaderEntry.Width = 233;
            // 
            // columnHeaderVanilla
            // 
            this.columnHeaderVanilla.Text = "Vanilla Item";
            this.columnHeaderVanilla.Width = 150;
            // 
            // columnHeaderRandomizedState
            // 
            this.columnHeaderRandomizedState.Text = "State";
            this.columnHeaderRandomizedState.Width = 50;
            // 
            // btnSetRandomized
            // 
            this.btnSetRandomized.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnSetRandomized.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSetRandomized.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetRandomized.Location = new System.Drawing.Point(222, 0);
            this.btnSetRandomized.Margin = new System.Windows.Forms.Padding(0);
            this.btnSetRandomized.Name = "btnSetRandomized";
            this.btnSetRandomized.Size = new System.Drawing.Size(111, 25);
            this.btnSetRandomized.TabIndex = 1;
            this.btnSetRandomized.Text = "Randomize";
            this.btnSetRandomized.UseVisualStyleBackColor = false;
            this.btnSetRandomized.Click += new System.EventHandler(this.ChangeRandomizationState);
            // 
            // btnSetUnRandomized
            // 
            this.btnSetUnRandomized.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnSetUnRandomized.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSetUnRandomized.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetUnRandomized.Location = new System.Drawing.Point(222, 25);
            this.btnSetUnRandomized.Margin = new System.Windows.Forms.Padding(0);
            this.btnSetUnRandomized.Name = "btnSetUnRandomized";
            this.btnSetUnRandomized.Size = new System.Drawing.Size(111, 25);
            this.btnSetUnRandomized.TabIndex = 2;
            this.btnSetUnRandomized.Text = "Unrandomize";
            this.btnSetUnRandomized.UseVisualStyleBackColor = false;
            this.btnSetUnRandomized.Click += new System.EventHandler(this.ChangeRandomizationState);
            // 
            // btnSetManual
            // 
            this.btnSetManual.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnSetManual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSetManual.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetManual.Location = new System.Drawing.Point(333, 25);
            this.btnSetManual.Margin = new System.Windows.Forms.Padding(0);
            this.btnSetManual.Name = "btnSetManual";
            this.btnSetManual.Size = new System.Drawing.Size(112, 25);
            this.btnSetManual.TabIndex = 4;
            this.btnSetManual.Text = "UnRand Manual";
            this.btnSetManual.UseVisualStyleBackColor = false;
            this.btnSetManual.Click += new System.EventHandler(this.ChangeRandomizationState);
            // 
            // btnSetJunk
            // 
            this.btnSetJunk.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnSetJunk.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSetJunk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetJunk.Location = new System.Drawing.Point(333, 0);
            this.btnSetJunk.Margin = new System.Windows.Forms.Padding(0);
            this.btnSetJunk.Name = "btnSetJunk";
            this.btnSetJunk.Size = new System.Drawing.Size(112, 25);
            this.btnSetJunk.TabIndex = 3;
            this.btnSetJunk.Text = "Force Junk";
            this.btnSetJunk.UseVisualStyleBackColor = false;
            this.btnSetJunk.Click += new System.EventHandler(this.ChangeRandomizationState);
            // 
            // btnAddStartingItem
            // 
            this.btnAddStartingItem.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnAddStartingItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAddStartingItem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddStartingItem.Location = new System.Drawing.Point(159, 0);
            this.btnAddStartingItem.Margin = new System.Windows.Forms.Padding(0);
            this.btnAddStartingItem.Name = "btnAddStartingItem";
            this.btnAddStartingItem.Size = new System.Drawing.Size(60, 25);
            this.btnAddStartingItem.TabIndex = 7;
            this.btnAddStartingItem.Text = "Add";
            this.btnAddStartingItem.UseVisualStyleBackColor = false;
            this.btnAddStartingItem.Click += new System.EventHandler(this.btnAddStartingItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(5, 5);
            this.label1.Margin = new System.Windows.Forms.Padding(5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 15);
            this.label1.TabIndex = 8;
            this.label1.Text = "Location Settings";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(5, 5);
            this.label3.Margin = new System.Windows.Forms.Padding(5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(131, 15);
            this.label3.TabIndex = 10;
            this.label3.Text = "Available Starting Items";
            // 
            // lvTricks
            // 
            this.lvTricks.BackColor = System.Drawing.SystemColors.ControlDark;
            this.lvTricks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Tricks});
            this.tableLayoutPanel3.SetColumnSpan(this.lvTricks, 2);
            this.lvTricks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvTricks.Location = new System.Drawing.Point(3, 53);
            this.lvTricks.Name = "lvTricks";
            this.lvTricks.Size = new System.Drawing.Size(214, 288);
            this.lvTricks.TabIndex = 12;
            this.lvTricks.UseCompatibleStateImageBehavior = false;
            this.lvTricks.View = System.Windows.Forms.View.Details;
            this.lvTricks.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lvTricks_ItemChecked);
            // 
            // Tricks
            // 
            this.Tricks.Text = "Trick List";
            this.Tricks.Width = 203;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel3.SetColumnSpan(this.label4, 2);
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(5, 5);
            this.label4.Margin = new System.Windows.Forms.Padding(5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(210, 15);
            this.label4.TabIndex = 13;
            this.label4.Text = "Trick Settings";
            // 
            // txtSearchAvailableStarting
            // 
            this.txtSearchAvailableStarting.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tableLayoutPanel2.SetColumnSpan(this.txtSearchAvailableStarting, 2);
            this.txtSearchAvailableStarting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSearchAvailableStarting.Location = new System.Drawing.Point(3, 28);
            this.txtSearchAvailableStarting.Name = "txtSearchAvailableStarting";
            this.txtSearchAvailableStarting.Size = new System.Drawing.Size(213, 23);
            this.txtSearchAvailableStarting.TabIndex = 15;
            this.txtSearchAvailableStarting.TextChanged += new System.EventHandler(this.txtSearchAvailableStarting_TextChanged);
            // 
            // TxtLocationSearch
            // 
            this.TxtLocationSearch.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tableLayoutPanel1.SetColumnSpan(this.TxtLocationSearch, 2);
            this.TxtLocationSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TxtLocationSearch.Location = new System.Drawing.Point(2, 26);
            this.TxtLocationSearch.Margin = new System.Windows.Forms.Padding(2, 1, 0, 0);
            this.TxtLocationSearch.Name = "TxtLocationSearch";
            this.TxtLocationSearch.Size = new System.Drawing.Size(220, 23);
            this.TxtLocationSearch.TabIndex = 16;
            this.TxtLocationSearch.TextChanged += new System.EventHandler(this.TxtLocationSearch_TextChanged);
            // 
            // txtTrickSearch
            // 
            this.txtTrickSearch.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tableLayoutPanel3.SetColumnSpan(this.txtTrickSearch, 2);
            this.txtTrickSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTrickSearch.Location = new System.Drawing.Point(3, 28);
            this.txtTrickSearch.Name = "txtTrickSearch";
            this.txtTrickSearch.Size = new System.Drawing.Size(214, 23);
            this.txtTrickSearch.TabIndex = 17;
            this.txtTrickSearch.TextChanged += new System.EventHandler(this.txtTrickSearch_TextChanged);
            // 
            // txtLocString
            // 
            this.txtLocString.BackColor = System.Drawing.SystemColors.ControlDark;
            this.txtLocString.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLocString.Location = new System.Drawing.Point(0, 369);
            this.txtLocString.Margin = new System.Windows.Forms.Padding(0);
            this.txtLocString.Name = "txtLocString";
            this.txtLocString.Size = new System.Drawing.Size(110, 23);
            this.txtLocString.TabIndex = 19;
            // 
            // cmbLocationType
            // 
            this.cmbLocationType.BackColor = System.Drawing.SystemColors.ControlDark;
            this.cmbLocationType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbLocationType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbLocationType.FormattingEnabled = true;
            this.cmbLocationType.Location = new System.Drawing.Point(111, 1);
            this.cmbLocationType.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.cmbLocationType.Name = "cmbLocationType";
            this.cmbLocationType.Size = new System.Drawing.Size(111, 23);
            this.cmbLocationType.TabIndex = 20;
            this.cmbLocationType.SelectedValueChanged += new System.EventHandler(this.cmbLocationType_SelectedValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(5, 349);
            this.label5.Margin = new System.Windows.Forms.Padding(5);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 15);
            this.label5.TabIndex = 21;
            this.label5.Text = "Location String";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(115, 349);
            this.label6.Margin = new System.Windows.Forms.Padding(5);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(100, 15);
            this.label6.TabIndex = 23;
            this.label6.Text = "Entrance String";
            // 
            // txtEntString
            // 
            this.txtEntString.BackColor = System.Drawing.SystemColors.ControlDark;
            this.txtEntString.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEntString.Location = new System.Drawing.Point(110, 369);
            this.txtEntString.Margin = new System.Windows.Forms.Padding(0);
            this.txtEntString.Name = "txtEntString";
            this.txtEntString.Size = new System.Drawing.Size(110, 23);
            this.txtEntString.TabIndex = 22;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(115, 399);
            this.label7.Margin = new System.Windows.Forms.Padding(5);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(100, 15);
            this.label7.TabIndex = 27;
            this.label7.Text = "Junk Location String";
            // 
            // txtjunkString
            // 
            this.txtjunkString.BackColor = System.Drawing.SystemColors.ControlDark;
            this.txtjunkString.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtjunkString.Location = new System.Drawing.Point(110, 419);
            this.txtjunkString.Margin = new System.Windows.Forms.Padding(0);
            this.txtjunkString.Name = "txtjunkString";
            this.txtjunkString.Size = new System.Drawing.Size(110, 23);
            this.txtjunkString.TabIndex = 26;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.Color.Transparent;
            this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label8.ForeColor = System.Drawing.Color.White;
            this.label8.Location = new System.Drawing.Point(5, 399);
            this.label8.Margin = new System.Windows.Forms.Padding(5);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(100, 15);
            this.label8.TabIndex = 25;
            this.label8.Text = "Starting Item String";
            // 
            // txtStartString
            // 
            this.txtStartString.BackColor = System.Drawing.SystemColors.ControlDark;
            this.txtStartString.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtStartString.Location = new System.Drawing.Point(0, 419);
            this.txtStartString.Margin = new System.Windows.Forms.Padding(0);
            this.txtStartString.Name = "txtStartString";
            this.txtStartString.Size = new System.Drawing.Size(110, 23);
            this.txtStartString.TabIndex = 24;
            // 
            // button8
            // 
            this.button8.BackColor = System.Drawing.SystemColors.ControlDark;
            this.button8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button8.Location = new System.Drawing.Point(0, 444);
            this.button8.Margin = new System.Windows.Forms.Padding(0);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(110, 25);
            this.button8.TabIndex = 30;
            this.button8.Text = "Apply Strings";
            this.button8.UseVisualStyleBackColor = false;
            this.button8.Click += new System.EventHandler(this.btnApplySettingStrings_Click);
            // 
            // chkShowRand
            // 
            this.chkShowRand.AutoSize = true;
            this.chkShowRand.BackColor = System.Drawing.Color.Transparent;
            this.chkShowRand.Checked = true;
            this.chkShowRand.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowRand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowRand.ForeColor = System.Drawing.Color.White;
            this.chkShowRand.Location = new System.Drawing.Point(3, 53);
            this.chkShowRand.Name = "chkShowRand";
            this.chkShowRand.Size = new System.Drawing.Size(105, 19);
            this.chkShowRand.TabIndex = 31;
            this.chkShowRand.Text = "Randomized";
            this.chkShowRand.UseVisualStyleBackColor = false;
            this.chkShowRand.CheckedChanged += new System.EventHandler(this.TxtLocationSearch_TextChanged);
            // 
            // chkShowJunk
            // 
            this.chkShowJunk.AutoSize = true;
            this.chkShowJunk.BackColor = System.Drawing.Color.Transparent;
            this.chkShowJunk.Checked = true;
            this.chkShowJunk.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowJunk.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowJunk.ForeColor = System.Drawing.Color.White;
            this.chkShowJunk.Location = new System.Drawing.Point(114, 53);
            this.chkShowJunk.Name = "chkShowJunk";
            this.chkShowJunk.Size = new System.Drawing.Size(105, 19);
            this.chkShowJunk.TabIndex = 32;
            this.chkShowJunk.Text = "Junk";
            this.chkShowJunk.UseVisualStyleBackColor = false;
            this.chkShowJunk.CheckedChanged += new System.EventHandler(this.TxtLocationSearch_TextChanged);
            // 
            // chkShowManual
            // 
            this.chkShowManual.AutoSize = true;
            this.chkShowManual.BackColor = System.Drawing.Color.Transparent;
            this.chkShowManual.Checked = true;
            this.chkShowManual.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowManual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowManual.ForeColor = System.Drawing.Color.White;
            this.chkShowManual.Location = new System.Drawing.Point(336, 53);
            this.chkShowManual.Name = "chkShowManual";
            this.chkShowManual.Size = new System.Drawing.Size(106, 19);
            this.chkShowManual.TabIndex = 34;
            this.chkShowManual.Text = "Manual";
            this.chkShowManual.UseVisualStyleBackColor = false;
            this.chkShowManual.CheckedChanged += new System.EventHandler(this.TxtLocationSearch_TextChanged);
            // 
            // chkShowUnrand
            // 
            this.chkShowUnrand.AutoSize = true;
            this.chkShowUnrand.BackColor = System.Drawing.Color.Transparent;
            this.chkShowUnrand.Checked = true;
            this.chkShowUnrand.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowUnrand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowUnrand.ForeColor = System.Drawing.Color.White;
            this.chkShowUnrand.Location = new System.Drawing.Point(225, 53);
            this.chkShowUnrand.Name = "chkShowUnrand";
            this.chkShowUnrand.Size = new System.Drawing.Size(105, 19);
            this.chkShowUnrand.TabIndex = 33;
            this.chkShowUnrand.Text = "Unrandomized";
            this.chkShowUnrand.UseVisualStyleBackColor = false;
            this.chkShowUnrand.CheckedChanged += new System.EventHandler(this.TxtLocationSearch_TextChanged);
            // 
            // lbAvailableStarting
            // 
            this.lbAvailableStarting.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tableLayoutPanel2.SetColumnSpan(this.lbAvailableStarting, 2);
            this.lbAvailableStarting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbAvailableStarting.FormattingEnabled = true;
            this.lbAvailableStarting.ItemHeight = 15;
            this.lbAvailableStarting.Location = new System.Drawing.Point(3, 53);
            this.lbAvailableStarting.Name = "lbAvailableStarting";
            this.lbAvailableStarting.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbAvailableStarting.Size = new System.Drawing.Size(213, 178);
            this.lbAvailableStarting.TabIndex = 5;
            this.lbAvailableStarting.DoubleClick += new System.EventHandler(this.btnAddStartingItem_Click);
            // 
            // lbCurrentStarting
            // 
            this.lbCurrentStarting.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tableLayoutPanel2.SetColumnSpan(this.lbCurrentStarting, 2);
            this.lbCurrentStarting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbCurrentStarting.FormattingEnabled = true;
            this.lbCurrentStarting.ItemHeight = 15;
            this.lbCurrentStarting.Location = new System.Drawing.Point(3, 287);
            this.lbCurrentStarting.Name = "lbCurrentStarting";
            this.lbCurrentStarting.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbCurrentStarting.Size = new System.Drawing.Size(213, 179);
            this.lbCurrentStarting.TabIndex = 6;
            this.lbCurrentStarting.DoubleClick += new System.EventHandler(this.btnRemoveStartingItem_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(5, 239);
            this.label2.Margin = new System.Windows.Forms.Padding(5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(123, 15);
            this.label2.TabIndex = 9;
            this.label2.Text = "Current Starting Items";
            // 
            // btnRemoveStartingItem
            // 
            this.btnRemoveStartingItem.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnRemoveStartingItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRemoveStartingItem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveStartingItem.Location = new System.Drawing.Point(159, 234);
            this.btnRemoveStartingItem.Margin = new System.Windows.Forms.Padding(0);
            this.btnRemoveStartingItem.Name = "btnRemoveStartingItem";
            this.btnRemoveStartingItem.Size = new System.Drawing.Size(60, 25);
            this.btnRemoveStartingItem.TabIndex = 11;
            this.btnRemoveStartingItem.Text = "Remove";
            this.btnRemoveStartingItem.UseVisualStyleBackColor = false;
            this.btnRemoveStartingItem.Click += new System.EventHandler(this.btnRemoveStartingItem_Click);
            // 
            // txtSearchCurrentStarting
            // 
            this.txtSearchCurrentStarting.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tableLayoutPanel2.SetColumnSpan(this.txtSearchCurrentStarting, 2);
            this.txtSearchCurrentStarting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSearchCurrentStarting.Location = new System.Drawing.Point(3, 262);
            this.txtSearchCurrentStarting.Name = "txtSearchCurrentStarting";
            this.txtSearchCurrentStarting.Size = new System.Drawing.Size(213, 23);
            this.txtSearchCurrentStarting.TabIndex = 18;
            this.txtSearchCurrentStarting.TextChanged += new System.EventHandler(this.txtSearchCurrentStarting_TextChanged);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(110, 444);
            this.button1.Margin = new System.Windows.Forms.Padding(0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(110, 25);
            this.button1.TabIndex = 35;
            this.button1.Text = "Load Setting File";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.btnLoadSettingFile_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.cmbLocationType, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.chkShowManual, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnSetRandomized, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.chkShowUnrand, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnSetJunk, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.chkShowJunk, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnSetUnRandomized, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.chkShowRand, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnSetManual, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.TxtLocationSearch, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lvLocationList, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(445, 469);
            this.tableLayoutPanel1.TabIndex = 36;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel2.Controls.Add(this.btnAddStartingItem, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnRemoveStartingItem, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.txtSearchAvailableStarting, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.txtSearchCurrentStarting, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.lbAvailableStarting, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.lbCurrentStarting, 0, 5);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(454, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 6;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(219, 469);
            this.tableLayoutPanel2.TabIndex = 37;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.label4, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.txtTrickSearch, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.label5, 0, 3);
            this.tableLayoutPanel3.Controls.Add(this.lvTricks, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.button1, 1, 7);
            this.tableLayoutPanel3.Controls.Add(this.label6, 1, 3);
            this.tableLayoutPanel3.Controls.Add(this.button8, 0, 7);
            this.tableLayoutPanel3.Controls.Add(this.label8, 0, 5);
            this.tableLayoutPanel3.Controls.Add(this.txtjunkString, 1, 6);
            this.tableLayoutPanel3.Controls.Add(this.label7, 1, 5);
            this.tableLayoutPanel3.Controls.Add(this.txtStartString, 0, 6);
            this.tableLayoutPanel3.Controls.Add(this.txtLocString, 0, 4);
            this.tableLayoutPanel3.Controls.Add(this.txtEntString, 1, 4);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(679, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 8;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(220, 469);
            this.tableLayoutPanel3.TabIndex = 38;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.tableLayoutPanel4.ColumnCount = 3;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel3, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(902, 475);
            this.tableLayoutPanel4.TabIndex = 39;
            // 
            // RandomizedStateEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(902, 475);
            this.Controls.Add(this.tableLayoutPanel4);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RandomizedStateEditor";
            this.Text = "RandomizedStateEditor";
            this.Load += new System.EventHandler(this.RandomizedStateEditor_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvLocationList;
        private System.Windows.Forms.ColumnHeader columnHeaderEntry;
        private System.Windows.Forms.ColumnHeader columnHeaderRandomizedState;
        private System.Windows.Forms.Button btnSetRandomized;
        private System.Windows.Forms.Button btnSetUnRandomized;
        private System.Windows.Forms.Button btnSetManual;
        private System.Windows.Forms.Button btnSetJunk;
        private System.Windows.Forms.Button btnAddStartingItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListView lvTricks;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSearchAvailableStarting;
        private System.Windows.Forms.TextBox TxtLocationSearch;
        private System.Windows.Forms.TextBox txtTrickSearch;
        private System.Windows.Forms.TextBox txtLocString;
        private System.Windows.Forms.ComboBox cmbLocationType;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtEntString;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtjunkString;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtStartString;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.CheckBox chkShowRand;
        private System.Windows.Forms.CheckBox chkShowJunk;
        private System.Windows.Forms.CheckBox chkShowManual;
        private System.Windows.Forms.CheckBox chkShowUnrand;
        private System.Windows.Forms.ListBox lbAvailableStarting;
        private System.Windows.Forms.ListBox lbCurrentStarting;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnRemoveStartingItem;
        private System.Windows.Forms.TextBox txtSearchCurrentStarting;
        private System.Windows.Forms.ColumnHeader columnHeaderVanilla;
        private System.Windows.Forms.ColumnHeader Tricks;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
    }
}