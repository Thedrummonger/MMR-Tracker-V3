﻿namespace Windows_Form_Frontend
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
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.chkShowObtainable = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnGenPlaythrough = new System.Windows.Forms.Button();
            this.chkOnlyImportant = new System.Windows.Forms.CheckBox();
            this.cmbWinCon = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtWinConFilter = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.btnArea = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbRequiredItems
            // 
            this.lbRequiredItems.FormattingEnabled = true;
            this.lbRequiredItems.ItemHeight = 15;
            this.lbRequiredItems.Location = new System.Drawing.Point(6, 48);
            this.lbRequiredItems.Name = "lbRequiredItems";
            this.lbRequiredItems.Size = new System.Drawing.Size(120, 64);
            this.lbRequiredItems.TabIndex = 0;
            // 
            // LBIgnoredItems
            // 
            this.LBIgnoredItems.FormattingEnabled = true;
            this.LBIgnoredItems.ItemHeight = 15;
            this.LBIgnoredItems.Location = new System.Drawing.Point(6, 138);
            this.LBIgnoredItems.Name = "LBIgnoredItems";
            this.LBIgnoredItems.Size = new System.Drawing.Size(120, 64);
            this.LBIgnoredItems.TabIndex = 1;
            // 
            // lbObtainable
            // 
            this.lbObtainable.FormattingEnabled = true;
            this.lbObtainable.ItemHeight = 15;
            this.lbObtainable.Location = new System.Drawing.Point(139, 76);
            this.lbObtainable.Name = "lbObtainable";
            this.lbObtainable.Size = new System.Drawing.Size(139, 184);
            this.lbObtainable.TabIndex = 2;
            // 
            // btnAddReq
            // 
            this.btnAddReq.ForeColor = System.Drawing.Color.Black;
            this.btnAddReq.Location = new System.Drawing.Point(87, 115);
            this.btnAddReq.Name = "btnAddReq";
            this.btnAddReq.Size = new System.Drawing.Size(39, 23);
            this.btnAddReq.TabIndex = 4;
            this.btnAddReq.Text = "Add";
            this.btnAddReq.UseVisualStyleBackColor = true;
            // 
            // btnAddIgnored
            // 
            this.btnAddIgnored.ForeColor = System.Drawing.Color.Black;
            this.btnAddIgnored.Location = new System.Drawing.Point(87, 24);
            this.btnAddIgnored.Name = "btnAddIgnored";
            this.btnAddIgnored.Size = new System.Drawing.Size(39, 23);
            this.btnAddIgnored.TabIndex = 5;
            this.btnAddIgnored.Text = "Add";
            this.btnAddIgnored.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button1.Location = new System.Drawing.Point(194, 22);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(84, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Check Seed";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label1.Location = new System.Drawing.Point(6, 118);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 17);
            this.label1.TabIndex = 7;
            this.label1.Text = "Ignored Checks";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label2.Location = new System.Drawing.Point(6, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 15);
            this.label2.TabIndex = 8;
            this.label2.Text = "Items Needed";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label3.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label3.Location = new System.Drawing.Point(139, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 17);
            this.label3.TabIndex = 9;
            this.label3.Text = "Results";
            // 
            // chkShowObtainable
            // 
            this.chkShowObtainable.AutoSize = true;
            this.chkShowObtainable.BackColor = System.Drawing.Color.Transparent;
            this.chkShowObtainable.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.chkShowObtainable.Location = new System.Drawing.Point(6, 240);
            this.chkShowObtainable.Name = "chkShowObtainable";
            this.chkShowObtainable.Size = new System.Drawing.Size(116, 19);
            this.chkShowObtainable.TabIndex = 10;
            this.chkShowObtainable.Text = "Show Obtainable";
            this.chkShowObtainable.UseVisualStyleBackColor = false;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.BackColor = System.Drawing.Color.Transparent;
            this.checkBox1.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.checkBox1.Location = new System.Drawing.Point(6, 215);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(131, 19);
            this.checkBox1.TabIndex = 11;
            this.checkBox1.Text = "Show UnObtainable";
            this.checkBox1.UseVisualStyleBackColor = false;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(139, 48);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(140, 23);
            this.textBox1.TabIndex = 13;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.lbRequiredItems);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Controls.Add(this.LBIgnoredItems);
            this.groupBox1.Controls.Add(this.lbObtainable);
            this.groupBox1.Controls.Add(this.checkBox1);
            this.groupBox1.Controls.Add(this.btnAddReq);
            this.groupBox1.Controls.Add(this.chkShowObtainable);
            this.groupBox1.Controls.Add(this.btnAddIgnored);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(285, 270);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Seed Checker";
            // 
            // btnGenPlaythrough
            // 
            this.btnGenPlaythrough.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnGenPlaythrough.Location = new System.Drawing.Point(170, 43);
            this.btnGenPlaythrough.Name = "btnGenPlaythrough";
            this.btnGenPlaythrough.Size = new System.Drawing.Size(108, 23);
            this.btnGenPlaythrough.TabIndex = 16;
            this.btnGenPlaythrough.Text = "Generate Playthrough";
            this.btnGenPlaythrough.UseVisualStyleBackColor = true;
            this.btnGenPlaythrough.Click += new System.EventHandler(this.btnGenPlaythrough_Click);
            // 
            // chkOnlyImportant
            // 
            this.chkOnlyImportant.AutoSize = true;
            this.chkOnlyImportant.BackColor = System.Drawing.Color.Transparent;
            this.chkOnlyImportant.Checked = true;
            this.chkOnlyImportant.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOnlyImportant.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.chkOnlyImportant.Location = new System.Drawing.Point(170, 21);
            this.chkOnlyImportant.Name = "chkOnlyImportant";
            this.chkOnlyImportant.Size = new System.Drawing.Size(108, 19);
            this.chkOnlyImportant.TabIndex = 17;
            this.chkOnlyImportant.Text = "FIlter Important";
            this.chkOnlyImportant.UseVisualStyleBackColor = false;
            // 
            // cmbWinCon
            // 
            this.cmbWinCon.FormattingEnabled = true;
            this.cmbWinCon.Location = new System.Drawing.Point(3, 44);
            this.cmbWinCon.Name = "cmbWinCon";
            this.cmbWinCon.Size = new System.Drawing.Size(161, 23);
            this.cmbWinCon.TabIndex = 18;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label6.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label6.Location = new System.Drawing.Point(6, 25);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 15);
            this.label6.TabIndex = 19;
            this.label6.Text = "Win Con";
            // 
            // txtWinConFilter
            // 
            this.txtWinConFilter.Location = new System.Drawing.Point(62, 22);
            this.txtWinConFilter.Name = "txtWinConFilter";
            this.txtWinConFilter.Size = new System.Drawing.Size(102, 23);
            this.txtWinConFilter.TabIndex = 20;
            this.txtWinConFilter.TextChanged += new System.EventHandler(this.txtWinConFilter_TextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.Transparent;
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.btnGenPlaythrough);
            this.groupBox2.Controls.Add(this.txtWinConFilter);
            this.groupBox2.Controls.Add(this.chkOnlyImportant);
            this.groupBox2.Controls.Add(this.cmbWinCon);
            this.groupBox2.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.groupBox2.Location = new System.Drawing.Point(12, 288);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(285, 77);
            this.groupBox2.TabIndex = 22;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Playthrough Generator";
            // 
            // groupBox3
            // 
            this.groupBox3.BackColor = System.Drawing.Color.Transparent;
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.textBox2);
            this.groupBox3.Controls.Add(this.button3);
            this.groupBox3.Controls.Add(this.button2);
            this.groupBox3.Controls.Add(this.btnArea);
            this.groupBox3.Controls.Add(this.listBox1);
            this.groupBox3.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.groupBox3.Location = new System.Drawing.Point(303, 13);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(154, 352);
            this.groupBox3.TabIndex = 23;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Spoiler log lookup";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.Color.Transparent;
            this.label8.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label8.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label8.Location = new System.Drawing.Point(6, 58);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(33, 15);
            this.label8.TabIndex = 22;
            this.label8.Text = "Filter";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(6, 79);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(142, 23);
            this.textBox2.TabIndex = 4;
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // button3
            // 
            this.button3.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.button3.Location = new System.Drawing.Point(78, 50);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(70, 23);
            this.button3.TabIndex = 3;
            this.button3.Text = "Location";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.button2.Location = new System.Drawing.Point(78, 21);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(70, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Sphere";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnArea
            // 
            this.btnArea.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnArea.Location = new System.Drawing.Point(6, 21);
            this.btnArea.Name = "btnArea";
            this.btnArea.Size = new System.Drawing.Size(70, 23);
            this.btnArea.TabIndex = 1;
            this.btnArea.Text = "Area";
            this.btnArea.UseVisualStyleBackColor = true;
            this.btnArea.Click += new System.EventHandler(this.btnArea_Click);
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 15;
            this.listBox1.Location = new System.Drawing.Point(6, 102);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(142, 244);
            this.listBox1.TabIndex = 0;
            // 
            // SpoilerLogLookUp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(472, 377);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "SpoilerLogLookUp";
            this.Text = "SpoilerLogTools";
            this.Load += new System.EventHandler(this.SpoilerLogLookUp_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbRequiredItems;
        private System.Windows.Forms.ListBox LBIgnoredItems;
        private System.Windows.Forms.ListBox lbObtainable;
        private System.Windows.Forms.Button btnAddReq;
        private System.Windows.Forms.Button btnAddIgnored;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkShowObtainable;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnGenPlaythrough;
        private System.Windows.Forms.CheckBox chkOnlyImportant;
        private System.Windows.Forms.ComboBox cmbWinCon;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtWinConFilter;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnArea;
        private System.Windows.Forms.ListBox listBox1;
    }
}