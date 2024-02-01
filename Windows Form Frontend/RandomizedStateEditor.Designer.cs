
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
            lvLocationList = new System.Windows.Forms.ListView();
            columnHeaderEntry = new System.Windows.Forms.ColumnHeader();
            columnHeaderVanilla = new System.Windows.Forms.ColumnHeader();
            columnHeaderRandomizedState = new System.Windows.Forms.ColumnHeader();
            btnSetRandomized = new System.Windows.Forms.Button();
            btnSetUnRandomized = new System.Windows.Forms.Button();
            btnSetManual = new System.Windows.Forms.Button();
            btnSetJunk = new System.Windows.Forms.Button();
            btnAddStartingItem = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            lvTricks = new System.Windows.Forms.ListView();
            Tricks = new System.Windows.Forms.ColumnHeader();
            label4 = new System.Windows.Forms.Label();
            txtSearchAvailableStarting = new System.Windows.Forms.TextBox();
            TxtLocationSearch = new System.Windows.Forms.TextBox();
            txtTrickSearch = new System.Windows.Forms.TextBox();
            txtLocString = new System.Windows.Forms.TextBox();
            cmbLocationType = new System.Windows.Forms.ComboBox();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            txtEntString = new System.Windows.Forms.TextBox();
            label7 = new System.Windows.Forms.Label();
            txtjunkString = new System.Windows.Forms.TextBox();
            label8 = new System.Windows.Forms.Label();
            txtStartString = new System.Windows.Forms.TextBox();
            button8 = new System.Windows.Forms.Button();
            chkShowRand = new System.Windows.Forms.CheckBox();
            chkShowJunk = new System.Windows.Forms.CheckBox();
            chkShowManual = new System.Windows.Forms.CheckBox();
            chkShowUnrand = new System.Windows.Forms.CheckBox();
            lbAvailableStarting = new System.Windows.Forms.ListBox();
            lbCurrentStarting = new System.Windows.Forms.ListBox();
            label2 = new System.Windows.Forms.Label();
            btnRemoveStartingItem = new System.Windows.Forms.Button();
            txtSearchCurrentStarting = new System.Windows.Forms.TextBox();
            button1 = new System.Windows.Forms.Button();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            SuspendLayout();
            // 
            // lvLocationList
            // 
            lvLocationList.BackColor = System.Drawing.SystemColors.ControlDark;
            lvLocationList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderEntry, columnHeaderVanilla, columnHeaderRandomizedState });
            tableLayoutPanel1.SetColumnSpan(lvLocationList, 4);
            lvLocationList.Dock = System.Windows.Forms.DockStyle.Fill;
            lvLocationList.Location = new System.Drawing.Point(3, 78);
            lvLocationList.Name = "lvLocationList";
            lvLocationList.Size = new System.Drawing.Size(439, 388);
            lvLocationList.TabIndex = 0;
            lvLocationList.UseCompatibleStateImageBehavior = false;
            lvLocationList.View = System.Windows.Forms.View.Details;
            lvLocationList.ItemChecked += lvLocationList_ItemChecked;
            // 
            // columnHeaderEntry
            // 
            columnHeaderEntry.Text = "Entry";
            columnHeaderEntry.Width = 233;
            // 
            // columnHeaderVanilla
            // 
            columnHeaderVanilla.Text = "Vanilla Item";
            columnHeaderVanilla.Width = 150;
            // 
            // columnHeaderRandomizedState
            // 
            columnHeaderRandomizedState.Text = "State";
            columnHeaderRandomizedState.Width = 50;
            // 
            // btnSetRandomized
            // 
            btnSetRandomized.BackColor = System.Drawing.SystemColors.ControlDark;
            btnSetRandomized.Dock = System.Windows.Forms.DockStyle.Fill;
            btnSetRandomized.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSetRandomized.Location = new System.Drawing.Point(222, 0);
            btnSetRandomized.Margin = new System.Windows.Forms.Padding(0);
            btnSetRandomized.Name = "btnSetRandomized";
            btnSetRandomized.Size = new System.Drawing.Size(111, 25);
            btnSetRandomized.TabIndex = 1;
            btnSetRandomized.Text = "Randomize";
            btnSetRandomized.UseVisualStyleBackColor = false;
            btnSetRandomized.Click += ChangeRandomizationState;
            // 
            // btnSetUnRandomized
            // 
            btnSetUnRandomized.BackColor = System.Drawing.SystemColors.ControlDark;
            btnSetUnRandomized.Dock = System.Windows.Forms.DockStyle.Fill;
            btnSetUnRandomized.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSetUnRandomized.Location = new System.Drawing.Point(222, 25);
            btnSetUnRandomized.Margin = new System.Windows.Forms.Padding(0);
            btnSetUnRandomized.Name = "btnSetUnRandomized";
            btnSetUnRandomized.Size = new System.Drawing.Size(111, 25);
            btnSetUnRandomized.TabIndex = 2;
            btnSetUnRandomized.Text = "Unrandomize";
            btnSetUnRandomized.UseVisualStyleBackColor = false;
            btnSetUnRandomized.Click += ChangeRandomizationState;
            // 
            // btnSetManual
            // 
            btnSetManual.BackColor = System.Drawing.SystemColors.ControlDark;
            btnSetManual.Dock = System.Windows.Forms.DockStyle.Fill;
            btnSetManual.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSetManual.Location = new System.Drawing.Point(333, 25);
            btnSetManual.Margin = new System.Windows.Forms.Padding(0);
            btnSetManual.Name = "btnSetManual";
            btnSetManual.Size = new System.Drawing.Size(112, 25);
            btnSetManual.TabIndex = 4;
            btnSetManual.Text = "UnRand Manual";
            btnSetManual.UseVisualStyleBackColor = false;
            btnSetManual.Click += ChangeRandomizationState;
            // 
            // btnSetJunk
            // 
            btnSetJunk.BackColor = System.Drawing.SystemColors.ControlDark;
            btnSetJunk.Dock = System.Windows.Forms.DockStyle.Fill;
            btnSetJunk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSetJunk.Location = new System.Drawing.Point(333, 0);
            btnSetJunk.Margin = new System.Windows.Forms.Padding(0);
            btnSetJunk.Name = "btnSetJunk";
            btnSetJunk.Size = new System.Drawing.Size(112, 25);
            btnSetJunk.TabIndex = 3;
            btnSetJunk.Text = "Force Junk";
            btnSetJunk.UseVisualStyleBackColor = false;
            btnSetJunk.Click += ChangeRandomizationState;
            // 
            // btnAddStartingItem
            // 
            btnAddStartingItem.BackColor = System.Drawing.SystemColors.ControlDark;
            btnAddStartingItem.Dock = System.Windows.Forms.DockStyle.Fill;
            btnAddStartingItem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnAddStartingItem.Location = new System.Drawing.Point(159, 0);
            btnAddStartingItem.Margin = new System.Windows.Forms.Padding(0);
            btnAddStartingItem.Name = "btnAddStartingItem";
            btnAddStartingItem.Size = new System.Drawing.Size(60, 25);
            btnAddStartingItem.TabIndex = 7;
            btnAddStartingItem.Text = "Add";
            btnAddStartingItem.UseVisualStyleBackColor = false;
            btnAddStartingItem.Click += btnAddStartingItem_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = System.Drawing.Color.Transparent;
            label1.Dock = System.Windows.Forms.DockStyle.Fill;
            label1.ForeColor = System.Drawing.Color.White;
            label1.Location = new System.Drawing.Point(5, 5);
            label1.Margin = new System.Windows.Forms.Padding(5);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(101, 15);
            label1.TabIndex = 8;
            label1.Text = "Location Settings";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = System.Drawing.Color.Transparent;
            label3.ForeColor = System.Drawing.Color.White;
            label3.Location = new System.Drawing.Point(5, 5);
            label3.Margin = new System.Windows.Forms.Padding(5);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(131, 15);
            label3.TabIndex = 10;
            label3.Text = "Available Starting Items";
            // 
            // lvTricks
            // 
            lvTricks.BackColor = System.Drawing.SystemColors.ControlDark;
            lvTricks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { Tricks });
            tableLayoutPanel3.SetColumnSpan(lvTricks, 2);
            lvTricks.Dock = System.Windows.Forms.DockStyle.Fill;
            lvTricks.Location = new System.Drawing.Point(3, 53);
            lvTricks.Name = "lvTricks";
            lvTricks.Size = new System.Drawing.Size(214, 288);
            lvTricks.TabIndex = 12;
            lvTricks.UseCompatibleStateImageBehavior = false;
            lvTricks.View = System.Windows.Forms.View.Details;
            lvTricks.ItemChecked += lvTricks_ItemChecked;
            // 
            // Tricks
            // 
            Tricks.Text = "Trick List";
            Tricks.Width = 203;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.BackColor = System.Drawing.Color.Transparent;
            tableLayoutPanel3.SetColumnSpan(label4, 2);
            label4.Dock = System.Windows.Forms.DockStyle.Fill;
            label4.ForeColor = System.Drawing.Color.White;
            label4.Location = new System.Drawing.Point(5, 5);
            label4.Margin = new System.Windows.Forms.Padding(5);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(210, 15);
            label4.TabIndex = 13;
            label4.Text = "Trick Settings";
            // 
            // txtSearchAvailableStarting
            // 
            txtSearchAvailableStarting.BackColor = System.Drawing.SystemColors.ControlDark;
            tableLayoutPanel2.SetColumnSpan(txtSearchAvailableStarting, 2);
            txtSearchAvailableStarting.Dock = System.Windows.Forms.DockStyle.Fill;
            txtSearchAvailableStarting.Location = new System.Drawing.Point(3, 28);
            txtSearchAvailableStarting.Name = "txtSearchAvailableStarting";
            txtSearchAvailableStarting.Size = new System.Drawing.Size(213, 23);
            txtSearchAvailableStarting.TabIndex = 15;
            txtSearchAvailableStarting.TextChanged += txtSearchAvailableStarting_TextChanged;
            // 
            // TxtLocationSearch
            // 
            TxtLocationSearch.BackColor = System.Drawing.SystemColors.ControlDark;
            tableLayoutPanel1.SetColumnSpan(TxtLocationSearch, 2);
            TxtLocationSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            TxtLocationSearch.Location = new System.Drawing.Point(2, 26);
            TxtLocationSearch.Margin = new System.Windows.Forms.Padding(2, 1, 0, 0);
            TxtLocationSearch.Name = "TxtLocationSearch";
            TxtLocationSearch.Size = new System.Drawing.Size(220, 23);
            TxtLocationSearch.TabIndex = 16;
            TxtLocationSearch.TextChanged += TxtLocationSearch_TextChanged;
            // 
            // txtTrickSearch
            // 
            txtTrickSearch.BackColor = System.Drawing.SystemColors.ControlDark;
            tableLayoutPanel3.SetColumnSpan(txtTrickSearch, 2);
            txtTrickSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            txtTrickSearch.Location = new System.Drawing.Point(3, 28);
            txtTrickSearch.Name = "txtTrickSearch";
            txtTrickSearch.Size = new System.Drawing.Size(214, 23);
            txtTrickSearch.TabIndex = 17;
            txtTrickSearch.TextChanged += txtTrickSearch_TextChanged;
            // 
            // txtLocString
            // 
            txtLocString.BackColor = System.Drawing.SystemColors.ControlDark;
            txtLocString.Dock = System.Windows.Forms.DockStyle.Fill;
            txtLocString.Location = new System.Drawing.Point(0, 369);
            txtLocString.Margin = new System.Windows.Forms.Padding(0);
            txtLocString.Name = "txtLocString";
            txtLocString.Size = new System.Drawing.Size(110, 23);
            txtLocString.TabIndex = 19;
            // 
            // cmbLocationType
            // 
            cmbLocationType.BackColor = System.Drawing.SystemColors.ControlDark;
            cmbLocationType.Dock = System.Windows.Forms.DockStyle.Fill;
            cmbLocationType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            cmbLocationType.FormattingEnabled = true;
            cmbLocationType.Location = new System.Drawing.Point(111, 1);
            cmbLocationType.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            cmbLocationType.Name = "cmbLocationType";
            cmbLocationType.Size = new System.Drawing.Size(111, 23);
            cmbLocationType.TabIndex = 20;
            cmbLocationType.SelectedValueChanged += cmbLocationType_SelectedValueChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.BackColor = System.Drawing.Color.Transparent;
            label5.Dock = System.Windows.Forms.DockStyle.Fill;
            label5.ForeColor = System.Drawing.Color.White;
            label5.Location = new System.Drawing.Point(5, 349);
            label5.Margin = new System.Windows.Forms.Padding(5);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(100, 15);
            label5.TabIndex = 21;
            label5.Text = "Location String";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.BackColor = System.Drawing.Color.Transparent;
            label6.Dock = System.Windows.Forms.DockStyle.Fill;
            label6.ForeColor = System.Drawing.Color.White;
            label6.Location = new System.Drawing.Point(115, 349);
            label6.Margin = new System.Windows.Forms.Padding(5);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(100, 15);
            label6.TabIndex = 23;
            label6.Text = "Entrance String";
            // 
            // txtEntString
            // 
            txtEntString.BackColor = System.Drawing.SystemColors.ControlDark;
            txtEntString.Dock = System.Windows.Forms.DockStyle.Fill;
            txtEntString.Location = new System.Drawing.Point(110, 369);
            txtEntString.Margin = new System.Windows.Forms.Padding(0);
            txtEntString.Name = "txtEntString";
            txtEntString.Size = new System.Drawing.Size(110, 23);
            txtEntString.TabIndex = 22;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.BackColor = System.Drawing.Color.Transparent;
            label7.Dock = System.Windows.Forms.DockStyle.Fill;
            label7.ForeColor = System.Drawing.Color.White;
            label7.Location = new System.Drawing.Point(115, 399);
            label7.Margin = new System.Windows.Forms.Padding(5);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(100, 15);
            label7.TabIndex = 27;
            label7.Text = "Junk Location String";
            // 
            // txtjunkString
            // 
            txtjunkString.BackColor = System.Drawing.SystemColors.ControlDark;
            txtjunkString.Dock = System.Windows.Forms.DockStyle.Fill;
            txtjunkString.Location = new System.Drawing.Point(110, 419);
            txtjunkString.Margin = new System.Windows.Forms.Padding(0);
            txtjunkString.Name = "txtjunkString";
            txtjunkString.Size = new System.Drawing.Size(110, 23);
            txtjunkString.TabIndex = 26;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.BackColor = System.Drawing.Color.Transparent;
            label8.Dock = System.Windows.Forms.DockStyle.Fill;
            label8.ForeColor = System.Drawing.Color.White;
            label8.Location = new System.Drawing.Point(5, 399);
            label8.Margin = new System.Windows.Forms.Padding(5);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(100, 15);
            label8.TabIndex = 25;
            label8.Text = "Starting Item String";
            // 
            // txtStartString
            // 
            txtStartString.BackColor = System.Drawing.SystemColors.ControlDark;
            txtStartString.Dock = System.Windows.Forms.DockStyle.Fill;
            txtStartString.Location = new System.Drawing.Point(0, 419);
            txtStartString.Margin = new System.Windows.Forms.Padding(0);
            txtStartString.Name = "txtStartString";
            txtStartString.Size = new System.Drawing.Size(110, 23);
            txtStartString.TabIndex = 24;
            // 
            // button8
            // 
            button8.BackColor = System.Drawing.SystemColors.ControlDark;
            button8.Dock = System.Windows.Forms.DockStyle.Fill;
            button8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            button8.Location = new System.Drawing.Point(0, 444);
            button8.Margin = new System.Windows.Forms.Padding(0);
            button8.Name = "button8";
            button8.Size = new System.Drawing.Size(110, 25);
            button8.TabIndex = 30;
            button8.Text = "Apply Strings";
            button8.UseVisualStyleBackColor = false;
            button8.Click += btnApplySettingStrings_Click;
            // 
            // chkShowRand
            // 
            chkShowRand.AutoSize = true;
            chkShowRand.BackColor = System.Drawing.Color.Transparent;
            chkShowRand.Checked = true;
            chkShowRand.CheckState = System.Windows.Forms.CheckState.Checked;
            chkShowRand.Dock = System.Windows.Forms.DockStyle.Fill;
            chkShowRand.ForeColor = System.Drawing.Color.White;
            chkShowRand.Location = new System.Drawing.Point(3, 53);
            chkShowRand.Name = "chkShowRand";
            chkShowRand.Size = new System.Drawing.Size(105, 19);
            chkShowRand.TabIndex = 31;
            chkShowRand.Text = "Randomized";
            chkShowRand.UseVisualStyleBackColor = false;
            chkShowRand.CheckedChanged += TxtLocationSearch_TextChanged;
            // 
            // chkShowJunk
            // 
            chkShowJunk.AutoSize = true;
            chkShowJunk.BackColor = System.Drawing.Color.Transparent;
            chkShowJunk.Checked = true;
            chkShowJunk.CheckState = System.Windows.Forms.CheckState.Checked;
            chkShowJunk.Dock = System.Windows.Forms.DockStyle.Fill;
            chkShowJunk.ForeColor = System.Drawing.Color.White;
            chkShowJunk.Location = new System.Drawing.Point(114, 53);
            chkShowJunk.Name = "chkShowJunk";
            chkShowJunk.Size = new System.Drawing.Size(105, 19);
            chkShowJunk.TabIndex = 32;
            chkShowJunk.Text = "Junk";
            chkShowJunk.UseVisualStyleBackColor = false;
            chkShowJunk.CheckedChanged += TxtLocationSearch_TextChanged;
            // 
            // chkShowManual
            // 
            chkShowManual.AutoSize = true;
            chkShowManual.BackColor = System.Drawing.Color.Transparent;
            chkShowManual.Checked = true;
            chkShowManual.CheckState = System.Windows.Forms.CheckState.Checked;
            chkShowManual.Dock = System.Windows.Forms.DockStyle.Fill;
            chkShowManual.ForeColor = System.Drawing.Color.White;
            chkShowManual.Location = new System.Drawing.Point(336, 53);
            chkShowManual.Name = "chkShowManual";
            chkShowManual.Size = new System.Drawing.Size(106, 19);
            chkShowManual.TabIndex = 34;
            chkShowManual.Text = "Manual";
            chkShowManual.UseVisualStyleBackColor = false;
            chkShowManual.CheckedChanged += TxtLocationSearch_TextChanged;
            // 
            // chkShowUnrand
            // 
            chkShowUnrand.AutoSize = true;
            chkShowUnrand.BackColor = System.Drawing.Color.Transparent;
            chkShowUnrand.Checked = true;
            chkShowUnrand.CheckState = System.Windows.Forms.CheckState.Checked;
            chkShowUnrand.Dock = System.Windows.Forms.DockStyle.Fill;
            chkShowUnrand.ForeColor = System.Drawing.Color.White;
            chkShowUnrand.Location = new System.Drawing.Point(225, 53);
            chkShowUnrand.Name = "chkShowUnrand";
            chkShowUnrand.Size = new System.Drawing.Size(105, 19);
            chkShowUnrand.TabIndex = 33;
            chkShowUnrand.Text = "Unrandomized";
            chkShowUnrand.UseVisualStyleBackColor = false;
            chkShowUnrand.CheckedChanged += TxtLocationSearch_TextChanged;
            // 
            // lbAvailableStarting
            // 
            lbAvailableStarting.BackColor = System.Drawing.SystemColors.ControlDark;
            tableLayoutPanel2.SetColumnSpan(lbAvailableStarting, 2);
            lbAvailableStarting.Dock = System.Windows.Forms.DockStyle.Fill;
            lbAvailableStarting.FormattingEnabled = true;
            lbAvailableStarting.ItemHeight = 15;
            lbAvailableStarting.Location = new System.Drawing.Point(3, 53);
            lbAvailableStarting.Name = "lbAvailableStarting";
            lbAvailableStarting.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            lbAvailableStarting.Size = new System.Drawing.Size(213, 178);
            lbAvailableStarting.TabIndex = 5;
            lbAvailableStarting.DoubleClick += btnAddStartingItem_Click;
            // 
            // lbCurrentStarting
            // 
            lbCurrentStarting.BackColor = System.Drawing.SystemColors.ControlDark;
            tableLayoutPanel2.SetColumnSpan(lbCurrentStarting, 2);
            lbCurrentStarting.Dock = System.Windows.Forms.DockStyle.Fill;
            lbCurrentStarting.FormattingEnabled = true;
            lbCurrentStarting.ItemHeight = 15;
            lbCurrentStarting.Location = new System.Drawing.Point(3, 287);
            lbCurrentStarting.Name = "lbCurrentStarting";
            lbCurrentStarting.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            lbCurrentStarting.Size = new System.Drawing.Size(213, 179);
            lbCurrentStarting.TabIndex = 6;
            lbCurrentStarting.DoubleClick += btnRemoveStartingItem_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = System.Drawing.Color.Transparent;
            label2.ForeColor = System.Drawing.Color.White;
            label2.Location = new System.Drawing.Point(5, 239);
            label2.Margin = new System.Windows.Forms.Padding(5);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(123, 15);
            label2.TabIndex = 9;
            label2.Text = "Current Starting Items";
            // 
            // btnRemoveStartingItem
            // 
            btnRemoveStartingItem.BackColor = System.Drawing.SystemColors.ControlDark;
            btnRemoveStartingItem.Dock = System.Windows.Forms.DockStyle.Fill;
            btnRemoveStartingItem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnRemoveStartingItem.Location = new System.Drawing.Point(159, 234);
            btnRemoveStartingItem.Margin = new System.Windows.Forms.Padding(0);
            btnRemoveStartingItem.Name = "btnRemoveStartingItem";
            btnRemoveStartingItem.Size = new System.Drawing.Size(60, 25);
            btnRemoveStartingItem.TabIndex = 11;
            btnRemoveStartingItem.Text = "Remove";
            btnRemoveStartingItem.UseVisualStyleBackColor = false;
            btnRemoveStartingItem.Click += btnRemoveStartingItem_Click;
            // 
            // txtSearchCurrentStarting
            // 
            txtSearchCurrentStarting.BackColor = System.Drawing.SystemColors.ControlDark;
            tableLayoutPanel2.SetColumnSpan(txtSearchCurrentStarting, 2);
            txtSearchCurrentStarting.Dock = System.Windows.Forms.DockStyle.Fill;
            txtSearchCurrentStarting.Location = new System.Drawing.Point(3, 262);
            txtSearchCurrentStarting.Name = "txtSearchCurrentStarting";
            txtSearchCurrentStarting.Size = new System.Drawing.Size(213, 23);
            txtSearchCurrentStarting.TabIndex = 18;
            txtSearchCurrentStarting.TextChanged += txtSearchCurrentStarting_TextChanged;
            // 
            // button1
            // 
            button1.BackColor = System.Drawing.SystemColors.ControlDark;
            button1.Dock = System.Windows.Forms.DockStyle.Fill;
            button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            button1.Location = new System.Drawing.Point(110, 444);
            button1.Margin = new System.Windows.Forms.Padding(0);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(110, 25);
            button1.TabIndex = 35;
            button1.Text = "Load Setting File";
            button1.UseVisualStyleBackColor = false;
            button1.Click += btnLoadSettingFile_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 4;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(cmbLocationType, 1, 0);
            tableLayoutPanel1.Controls.Add(chkShowManual, 3, 2);
            tableLayoutPanel1.Controls.Add(btnSetRandomized, 2, 0);
            tableLayoutPanel1.Controls.Add(chkShowUnrand, 2, 2);
            tableLayoutPanel1.Controls.Add(btnSetJunk, 3, 0);
            tableLayoutPanel1.Controls.Add(chkShowJunk, 1, 2);
            tableLayoutPanel1.Controls.Add(btnSetUnRandomized, 2, 1);
            tableLayoutPanel1.Controls.Add(chkShowRand, 0, 2);
            tableLayoutPanel1.Controls.Add(btnSetManual, 3, 1);
            tableLayoutPanel1.Controls.Add(TxtLocationSearch, 0, 1);
            tableLayoutPanel1.Controls.Add(lvLocationList, 0, 3);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new System.Drawing.Size(445, 469);
            tableLayoutPanel1.TabIndex = 36;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            tableLayoutPanel2.Controls.Add(btnAddStartingItem, 1, 0);
            tableLayoutPanel2.Controls.Add(btnRemoveStartingItem, 1, 3);
            tableLayoutPanel2.Controls.Add(label3, 0, 0);
            tableLayoutPanel2.Controls.Add(label2, 0, 3);
            tableLayoutPanel2.Controls.Add(txtSearchAvailableStarting, 0, 1);
            tableLayoutPanel2.Controls.Add(txtSearchCurrentStarting, 0, 4);
            tableLayoutPanel2.Controls.Add(lbAvailableStarting, 0, 2);
            tableLayoutPanel2.Controls.Add(lbCurrentStarting, 0, 5);
            tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel2.Location = new System.Drawing.Point(454, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 6;
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel2.Size = new System.Drawing.Size(219, 469);
            tableLayoutPanel2.TabIndex = 37;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel3.Controls.Add(label4, 0, 0);
            tableLayoutPanel3.Controls.Add(txtTrickSearch, 0, 1);
            tableLayoutPanel3.Controls.Add(label5, 0, 3);
            tableLayoutPanel3.Controls.Add(lvTricks, 0, 2);
            tableLayoutPanel3.Controls.Add(button1, 1, 7);
            tableLayoutPanel3.Controls.Add(label6, 1, 3);
            tableLayoutPanel3.Controls.Add(button8, 0, 7);
            tableLayoutPanel3.Controls.Add(label8, 0, 5);
            tableLayoutPanel3.Controls.Add(txtjunkString, 1, 6);
            tableLayoutPanel3.Controls.Add(label7, 1, 5);
            tableLayoutPanel3.Controls.Add(txtStartString, 0, 6);
            tableLayoutPanel3.Controls.Add(txtLocString, 0, 4);
            tableLayoutPanel3.Controls.Add(txtEntString, 1, 4);
            tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel3.Location = new System.Drawing.Point(679, 3);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 8;
            tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel3.Size = new System.Drawing.Size(220, 469);
            tableLayoutPanel3.TabIndex = 38;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.BackColor = System.Drawing.Color.FromArgb(0, 0, 64);
            tableLayoutPanel4.ColumnCount = 3;
            tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel4.Controls.Add(tableLayoutPanel3, 2, 0);
            tableLayoutPanel4.Controls.Add(tableLayoutPanel1, 0, 0);
            tableLayoutPanel4.Controls.Add(tableLayoutPanel2, 1, 0);
            tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 1;
            tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tableLayoutPanel4.Size = new System.Drawing.Size(902, 475);
            tableLayoutPanel4.TabIndex = 39;
            // 
            // RandomizedStateEditor
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            ClientSize = new System.Drawing.Size(902, 475);
            Controls.Add(tableLayoutPanel4);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "RandomizedStateEditor";
            Text = "Item Pool Editor";
            Load += RandomizedStateEditor_Load;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            tableLayoutPanel4.ResumeLayout(false);
            ResumeLayout(false);
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