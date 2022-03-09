
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
            this.SuspendLayout();
            // 
            // lvLocationList
            // 
            this.lvLocationList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderEntry,
            this.columnHeaderVanilla,
            this.columnHeaderRandomizedState});
            this.lvLocationList.HideSelection = false;
            this.lvLocationList.Location = new System.Drawing.Point(12, 81);
            this.lvLocationList.Name = "lvLocationList";
            this.lvLocationList.Size = new System.Drawing.Size(455, 445);
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
            this.btnSetRandomized.Location = new System.Drawing.Point(246, 3);
            this.btnSetRandomized.Name = "btnSetRandomized";
            this.btnSetRandomized.Size = new System.Drawing.Size(110, 23);
            this.btnSetRandomized.TabIndex = 1;
            this.btnSetRandomized.Text = "Randomize";
            this.btnSetRandomized.UseVisualStyleBackColor = true;
            this.btnSetRandomized.Click += new System.EventHandler(this.btnSetManual_Click);
            // 
            // btnSetUnRandomized
            // 
            this.btnSetUnRandomized.Location = new System.Drawing.Point(247, 27);
            this.btnSetUnRandomized.Name = "btnSetUnRandomized";
            this.btnSetUnRandomized.Size = new System.Drawing.Size(110, 23);
            this.btnSetUnRandomized.TabIndex = 2;
            this.btnSetUnRandomized.Text = "Unrandomize";
            this.btnSetUnRandomized.UseVisualStyleBackColor = true;
            this.btnSetUnRandomized.Click += new System.EventHandler(this.btnSetManual_Click);
            // 
            // btnSetManual
            // 
            this.btnSetManual.Location = new System.Drawing.Point(357, 27);
            this.btnSetManual.Name = "btnSetManual";
            this.btnSetManual.Size = new System.Drawing.Size(110, 23);
            this.btnSetManual.TabIndex = 4;
            this.btnSetManual.Text = "UnRand Manual";
            this.btnSetManual.UseVisualStyleBackColor = true;
            this.btnSetManual.Click += new System.EventHandler(this.btnSetManual_Click);
            // 
            // btnSetJunk
            // 
            this.btnSetJunk.Location = new System.Drawing.Point(357, 3);
            this.btnSetJunk.Name = "btnSetJunk";
            this.btnSetJunk.Size = new System.Drawing.Size(110, 23);
            this.btnSetJunk.TabIndex = 3;
            this.btnSetJunk.Text = "Force Junk";
            this.btnSetJunk.UseVisualStyleBackColor = true;
            this.btnSetJunk.Click += new System.EventHandler(this.btnSetManual_Click);
            // 
            // btnAddStartingItem
            // 
            this.btnAddStartingItem.Location = new System.Drawing.Point(610, 2);
            this.btnAddStartingItem.Name = "btnAddStartingItem";
            this.btnAddStartingItem.Size = new System.Drawing.Size(90, 23);
            this.btnAddStartingItem.TabIndex = 7;
            this.btnAddStartingItem.Text = "Add Selected";
            this.btnAddStartingItem.UseVisualStyleBackColor = true;
            this.btnAddStartingItem.Click += new System.EventHandler(this.btnAddStartingItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(13, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 15);
            this.label1.TabIndex = 8;
            this.label1.Text = "Location Settings";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(473, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(131, 15);
            this.label3.TabIndex = 10;
            this.label3.Text = "Available Starting Items";
            // 
            // lvTricks
            // 
            this.lvTricks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Tricks});
            this.lvTricks.HideSelection = false;
            this.lvTricks.Location = new System.Drawing.Point(706, 56);
            this.lvTricks.Name = "lvTricks";
            this.lvTricks.Size = new System.Drawing.Size(225, 353);
            this.lvTricks.TabIndex = 12;
            this.lvTricks.UseCompatibleStateImageBehavior = false;
            this.lvTricks.View = System.Windows.Forms.View.Details;
            this.lvTricks.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lvTricks_ItemChecked);
            // 
            // Tricks
            // 
            this.Tricks.Width = 203;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(706, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 15);
            this.label4.TabIndex = 13;
            this.label4.Text = "Trick Settings";
            // 
            // txtSearchAvailableStarting
            // 
            this.txtSearchAvailableStarting.Location = new System.Drawing.Point(474, 27);
            this.txtSearchAvailableStarting.Name = "txtSearchAvailableStarting";
            this.txtSearchAvailableStarting.Size = new System.Drawing.Size(226, 23);
            this.txtSearchAvailableStarting.TabIndex = 15;
            this.txtSearchAvailableStarting.TextChanged += new System.EventHandler(this.txtSearchAvailableStarting_TextChanged);
            // 
            // TxtLocationSearch
            // 
            this.TxtLocationSearch.Location = new System.Drawing.Point(12, 27);
            this.TxtLocationSearch.Name = "TxtLocationSearch";
            this.TxtLocationSearch.Size = new System.Drawing.Size(228, 23);
            this.TxtLocationSearch.TabIndex = 16;
            this.TxtLocationSearch.TextChanged += new System.EventHandler(this.TxtLocationSearch_TextChanged);
            // 
            // txtTrickSearch
            // 
            this.txtTrickSearch.Location = new System.Drawing.Point(706, 27);
            this.txtTrickSearch.Name = "txtTrickSearch";
            this.txtTrickSearch.Size = new System.Drawing.Size(225, 23);
            this.txtTrickSearch.TabIndex = 17;
            this.txtTrickSearch.TextChanged += new System.EventHandler(this.txtTrickSearch_TextChanged);
            // 
            // txtLocString
            // 
            this.txtLocString.Location = new System.Drawing.Point(706, 430);
            this.txtLocString.Name = "txtLocString";
            this.txtLocString.Size = new System.Drawing.Size(112, 23);
            this.txtLocString.TabIndex = 19;
            // 
            // cmbLocationType
            // 
            this.cmbLocationType.FormattingEnabled = true;
            this.cmbLocationType.Location = new System.Drawing.Point(136, 3);
            this.cmbLocationType.Name = "cmbLocationType";
            this.cmbLocationType.Size = new System.Drawing.Size(104, 23);
            this.cmbLocationType.TabIndex = 20;
            this.cmbLocationType.SelectedValueChanged += new System.EventHandler(this.cmbLocationType_SelectedValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(706, 412);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 15);
            this.label5.TabIndex = 21;
            this.label5.Text = "Location String";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(821, 412);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(87, 15);
            this.label6.TabIndex = 23;
            this.label6.Text = "Entrance String";
            // 
            // txtEntString
            // 
            this.txtEntString.Location = new System.Drawing.Point(821, 430);
            this.txtEntString.Name = "txtEntString";
            this.txtEntString.Size = new System.Drawing.Size(110, 23);
            this.txtEntString.TabIndex = 22;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(821, 456);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(114, 15);
            this.label7.TabIndex = 27;
            this.label7.Text = "Junk Location String";
            // 
            // txtjunkString
            // 
            this.txtjunkString.Location = new System.Drawing.Point(821, 474);
            this.txtjunkString.Name = "txtjunkString";
            this.txtjunkString.Size = new System.Drawing.Size(110, 23);
            this.txtjunkString.TabIndex = 26;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.Color.Transparent;
            this.label8.ForeColor = System.Drawing.Color.White;
            this.label8.Location = new System.Drawing.Point(706, 456);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(109, 15);
            this.label8.TabIndex = 25;
            this.label8.Text = "Starting Item String";
            // 
            // txtStartString
            // 
            this.txtStartString.Location = new System.Drawing.Point(706, 474);
            this.txtStartString.Name = "txtStartString";
            this.txtStartString.Size = new System.Drawing.Size(112, 23);
            this.txtStartString.TabIndex = 24;
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(708, 503);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(110, 23);
            this.button8.TabIndex = 30;
            this.button8.Text = "Apply Strings";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // chkShowRand
            // 
            this.chkShowRand.AutoSize = true;
            this.chkShowRand.BackColor = System.Drawing.Color.Transparent;
            this.chkShowRand.Checked = true;
            this.chkShowRand.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowRand.ForeColor = System.Drawing.Color.White;
            this.chkShowRand.Location = new System.Drawing.Point(13, 57);
            this.chkShowRand.Name = "chkShowRand";
            this.chkShowRand.Size = new System.Drawing.Size(121, 19);
            this.chkShowRand.TabIndex = 31;
            this.chkShowRand.Text = "Show randomized";
            this.chkShowRand.UseVisualStyleBackColor = false;
            this.chkShowRand.CheckedChanged += new System.EventHandler(this.TxtLocationSearch_TextChanged);
            // 
            // chkShowJunk
            // 
            this.chkShowJunk.AutoSize = true;
            this.chkShowJunk.BackColor = System.Drawing.Color.Transparent;
            this.chkShowJunk.Checked = true;
            this.chkShowJunk.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowJunk.ForeColor = System.Drawing.Color.White;
            this.chkShowJunk.Location = new System.Drawing.Point(140, 57);
            this.chkShowJunk.Name = "chkShowJunk";
            this.chkShowJunk.Size = new System.Drawing.Size(82, 19);
            this.chkShowJunk.TabIndex = 32;
            this.chkShowJunk.Text = "Show Junk";
            this.chkShowJunk.UseVisualStyleBackColor = false;
            this.chkShowJunk.CheckedChanged += new System.EventHandler(this.TxtLocationSearch_TextChanged);
            // 
            // chkShowManual
            // 
            this.chkShowManual.AutoSize = true;
            this.chkShowManual.BackColor = System.Drawing.Color.Transparent;
            this.chkShowManual.Checked = true;
            this.chkShowManual.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowManual.ForeColor = System.Drawing.Color.White;
            this.chkShowManual.Location = new System.Drawing.Point(369, 56);
            this.chkShowManual.Name = "chkShowManual";
            this.chkShowManual.Size = new System.Drawing.Size(98, 19);
            this.chkShowManual.TabIndex = 34;
            this.chkShowManual.Text = "Show Manual";
            this.chkShowManual.UseVisualStyleBackColor = false;
            this.chkShowManual.CheckedChanged += new System.EventHandler(this.TxtLocationSearch_TextChanged);
            // 
            // chkShowUnrand
            // 
            this.chkShowUnrand.AutoSize = true;
            this.chkShowUnrand.BackColor = System.Drawing.Color.Transparent;
            this.chkShowUnrand.Checked = true;
            this.chkShowUnrand.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowUnrand.ForeColor = System.Drawing.Color.White;
            this.chkShowUnrand.Location = new System.Drawing.Point(228, 57);
            this.chkShowUnrand.Name = "chkShowUnrand";
            this.chkShowUnrand.Size = new System.Drawing.Size(135, 19);
            this.chkShowUnrand.TabIndex = 33;
            this.chkShowUnrand.Text = "Show unrandomized";
            this.chkShowUnrand.UseVisualStyleBackColor = false;
            this.chkShowUnrand.CheckedChanged += new System.EventHandler(this.TxtLocationSearch_TextChanged);
            // 
            // lbAvailableStarting
            // 
            this.lbAvailableStarting.FormattingEnabled = true;
            this.lbAvailableStarting.ItemHeight = 15;
            this.lbAvailableStarting.Location = new System.Drawing.Point(473, 57);
            this.lbAvailableStarting.Name = "lbAvailableStarting";
            this.lbAvailableStarting.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbAvailableStarting.Size = new System.Drawing.Size(227, 214);
            this.lbAvailableStarting.TabIndex = 5;
            this.lbAvailableStarting.DoubleClick += new System.EventHandler(this.btnAddStartingItem_Click);
            // 
            // lbCurrentStarting
            // 
            this.lbCurrentStarting.FormattingEnabled = true;
            this.lbCurrentStarting.ItemHeight = 15;
            this.lbCurrentStarting.Location = new System.Drawing.Point(473, 327);
            this.lbCurrentStarting.Name = "lbCurrentStarting";
            this.lbCurrentStarting.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbCurrentStarting.Size = new System.Drawing.Size(227, 199);
            this.lbCurrentStarting.TabIndex = 6;
            this.lbCurrentStarting.DoubleClick += new System.EventHandler(this.btnRemoveStartingItem_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(474, 281);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(123, 15);
            this.label2.TabIndex = 9;
            this.label2.Text = "Current Starting Items";
            // 
            // btnRemoveStartingItem
            // 
            this.btnRemoveStartingItem.Location = new System.Drawing.Point(594, 277);
            this.btnRemoveStartingItem.Name = "btnRemoveStartingItem";
            this.btnRemoveStartingItem.Size = new System.Drawing.Size(106, 23);
            this.btnRemoveStartingItem.TabIndex = 11;
            this.btnRemoveStartingItem.Text = "Remove Selected";
            this.btnRemoveStartingItem.UseVisualStyleBackColor = true;
            this.btnRemoveStartingItem.Click += new System.EventHandler(this.btnRemoveStartingItem_Click);
            // 
            // txtSearchCurrentStarting
            // 
            this.txtSearchCurrentStarting.Location = new System.Drawing.Point(474, 301);
            this.txtSearchCurrentStarting.Name = "txtSearchCurrentStarting";
            this.txtSearchCurrentStarting.Size = new System.Drawing.Size(226, 23);
            this.txtSearchCurrentStarting.TabIndex = 18;
            this.txtSearchCurrentStarting.TextChanged += new System.EventHandler(this.txtSearchCurrentStarting_TextChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(821, 503);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(110, 23);
            this.button1.TabIndex = 35;
            this.button1.Text = "Load Setting File";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // RandomizedStateEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(939, 530);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.chkShowManual);
            this.Controls.Add(this.chkShowUnrand);
            this.Controls.Add(this.chkShowJunk);
            this.Controls.Add(this.chkShowRand);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtjunkString);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtStartString);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtEntString);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cmbLocationType);
            this.Controls.Add(this.txtLocString);
            this.Controls.Add(this.txtSearchCurrentStarting);
            this.Controls.Add(this.txtTrickSearch);
            this.Controls.Add(this.TxtLocationSearch);
            this.Controls.Add(this.txtSearchAvailableStarting);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lvTricks);
            this.Controls.Add(this.btnRemoveStartingItem);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnAddStartingItem);
            this.Controls.Add(this.lbCurrentStarting);
            this.Controls.Add(this.lbAvailableStarting);
            this.Controls.Add(this.btnSetManual);
            this.Controls.Add(this.btnSetJunk);
            this.Controls.Add(this.btnSetUnRandomized);
            this.Controls.Add(this.btnSetRandomized);
            this.Controls.Add(this.lvLocationList);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RandomizedStateEditor";
            this.Text = "RandomizedStateEditor";
            this.Load += new System.EventHandler(this.RandomizedStateEditor_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}