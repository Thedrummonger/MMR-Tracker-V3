
namespace Windows_Form_Frontend
{
    partial class ShowLogic
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShowLogic));
            this.LBReq = new System.Windows.Forms.ListBox();
            this.lbCond = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnGoBack = new System.Windows.Forms.Button();
            this.chkShowUnaltered = new System.Windows.Forms.CheckBox();
            this.ND1 = new System.Windows.Forms.CheckBox();
            this.NN1 = new System.Windows.Forms.CheckBox();
            this.NN2 = new System.Windows.Forms.CheckBox();
            this.ND2 = new System.Windows.Forms.CheckBox();
            this.NN3 = new System.Windows.Forms.CheckBox();
            this.ND3 = new System.Windows.Forms.CheckBox();
            this.SD3 = new System.Windows.Forms.CheckBox();
            this.SN3 = new System.Windows.Forms.CheckBox();
            this.SD2 = new System.Windows.Forms.CheckBox();
            this.SN2 = new System.Windows.Forms.CheckBox();
            this.SN1 = new System.Windows.Forms.CheckBox();
            this.SD1 = new System.Windows.Forms.CheckBox();
            this.chkShowTime = new System.Windows.Forms.CheckBox();
            this.BTNGotTo = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.tlbReq = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tlbCond = new System.Windows.Forms.TableLayoutPanel();
            this.tlbmaster = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.tlbReq.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tlbCond.SuspendLayout();
            this.tlbmaster.SuspendLayout();
            this.SuspendLayout();
            // 
            // LBReq
            // 
            this.LBReq.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tlbReq.SetColumnSpan(this.LBReq, 2);
            this.LBReq.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LBReq.FormattingEnabled = true;
            this.LBReq.HorizontalScrollbar = true;
            this.LBReq.IntegralHeight = false;
            this.LBReq.ItemHeight = 15;
            this.LBReq.Location = new System.Drawing.Point(3, 53);
            this.LBReq.Name = "LBReq";
            this.LBReq.Size = new System.Drawing.Size(193, 347);
            this.LBReq.TabIndex = 0;
            this.LBReq.DoubleClick += new System.EventHandler(this.lbReq_DoubleClick);
            // 
            // lbCond
            // 
            this.lbCond.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tlbCond.SetColumnSpan(this.lbCond, 3);
            this.lbCond.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbCond.FormattingEnabled = true;
            this.lbCond.HorizontalScrollbar = true;
            this.lbCond.IntegralHeight = false;
            this.lbCond.ItemHeight = 15;
            this.lbCond.Location = new System.Drawing.Point(3, 28);
            this.lbCond.Name = "lbCond";
            this.lbCond.Size = new System.Drawing.Size(296, 412);
            this.lbCond.TabIndex = 1;
            this.lbCond.DoubleClick += new System.EventHandler(this.LBCond_DoubleClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.tlbReq.SetColumnSpan(this.label1, 2);
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.ForeColor = System.Drawing.SystemColors.Control;
            this.label1.Location = new System.Drawing.Point(5, 5);
            this.label1.Margin = new System.Windows.Forms.Padding(5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(189, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Requirements";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.tlbCond.SetColumnSpan(this.label2, 2);
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.ForeColor = System.Drawing.SystemColors.Control;
            this.label2.Location = new System.Drawing.Point(5, 5);
            this.label2.Margin = new System.Windows.Forms.Padding(5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(212, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Conditionals";
            // 
            // btnGoBack
            // 
            this.btnGoBack.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnGoBack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnGoBack.Location = new System.Drawing.Point(145, 446);
            this.btnGoBack.Name = "btnGoBack";
            this.btnGoBack.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.tlbCond.SetRowSpan(this.btnGoBack, 2);
            this.btnGoBack.Size = new System.Drawing.Size(74, 34);
            this.btnGoBack.TabIndex = 8;
            this.btnGoBack.Text = "Go Back";
            this.btnGoBack.UseVisualStyleBackColor = false;
            this.btnGoBack.Click += new System.EventHandler(this.btnGoBack_Click);
            // 
            // chkShowUnaltered
            // 
            this.chkShowUnaltered.AutoSize = true;
            this.chkShowUnaltered.BackColor = System.Drawing.Color.Transparent;
            this.chkShowUnaltered.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowUnaltered.ForeColor = System.Drawing.SystemColors.Control;
            this.chkShowUnaltered.Location = new System.Drawing.Point(0, 443);
            this.chkShowUnaltered.Margin = new System.Windows.Forms.Padding(0);
            this.chkShowUnaltered.Name = "chkShowUnaltered";
            this.chkShowUnaltered.Size = new System.Drawing.Size(142, 20);
            this.chkShowUnaltered.TabIndex = 11;
            this.chkShowUnaltered.Text = "Show Unaltered Logic";
            this.chkShowUnaltered.UseVisualStyleBackColor = false;
            this.chkShowUnaltered.CheckedChanged += new System.EventHandler(this.miscChk_CheckedChanged);
            // 
            // ND1
            // 
            this.ND1.AutoSize = true;
            this.ND1.BackColor = System.Drawing.Color.Transparent;
            this.ND1.ForeColor = System.Drawing.SystemColors.Control;
            this.ND1.Location = new System.Drawing.Point(6, 16);
            this.ND1.Name = "ND1";
            this.ND1.Size = new System.Drawing.Size(40, 19);
            this.ND1.TabIndex = 12;
            this.ND1.Text = "D1";
            this.ND1.UseVisualStyleBackColor = false;
            this.ND1.CheckedChanged += new System.EventHandler(this.miscChk_CheckedChanged);
            // 
            // NN1
            // 
            this.NN1.AutoSize = true;
            this.NN1.BackColor = System.Drawing.Color.Transparent;
            this.NN1.ForeColor = System.Drawing.SystemColors.Control;
            this.NN1.Location = new System.Drawing.Point(46, 16);
            this.NN1.Name = "NN1";
            this.NN1.Size = new System.Drawing.Size(41, 19);
            this.NN1.TabIndex = 13;
            this.NN1.Text = "N1";
            this.NN1.UseVisualStyleBackColor = false;
            this.NN1.CheckedChanged += new System.EventHandler(this.miscChk_CheckedChanged);
            // 
            // NN2
            // 
            this.NN2.AutoSize = true;
            this.NN2.BackColor = System.Drawing.Color.Transparent;
            this.NN2.ForeColor = System.Drawing.SystemColors.Control;
            this.NN2.Location = new System.Drawing.Point(46, 34);
            this.NN2.Name = "NN2";
            this.NN2.Size = new System.Drawing.Size(41, 19);
            this.NN2.TabIndex = 15;
            this.NN2.Text = "N2";
            this.NN2.UseVisualStyleBackColor = false;
            this.NN2.CheckedChanged += new System.EventHandler(this.miscChk_CheckedChanged);
            // 
            // ND2
            // 
            this.ND2.AutoSize = true;
            this.ND2.BackColor = System.Drawing.Color.Transparent;
            this.ND2.ForeColor = System.Drawing.SystemColors.Control;
            this.ND2.Location = new System.Drawing.Point(6, 34);
            this.ND2.Name = "ND2";
            this.ND2.Size = new System.Drawing.Size(40, 19);
            this.ND2.TabIndex = 16;
            this.ND2.Text = "D2";
            this.ND2.UseVisualStyleBackColor = false;
            this.ND2.CheckedChanged += new System.EventHandler(this.miscChk_CheckedChanged);
            // 
            // NN3
            // 
            this.NN3.AutoSize = true;
            this.NN3.BackColor = System.Drawing.Color.Transparent;
            this.NN3.ForeColor = System.Drawing.SystemColors.Control;
            this.NN3.Location = new System.Drawing.Point(46, 51);
            this.NN3.Name = "NN3";
            this.NN3.Size = new System.Drawing.Size(41, 19);
            this.NN3.TabIndex = 19;
            this.NN3.Text = "N3";
            this.NN3.UseVisualStyleBackColor = false;
            this.NN3.CheckedChanged += new System.EventHandler(this.miscChk_CheckedChanged);
            // 
            // ND3
            // 
            this.ND3.AutoSize = true;
            this.ND3.BackColor = System.Drawing.Color.Transparent;
            this.ND3.ForeColor = System.Drawing.SystemColors.Control;
            this.ND3.Location = new System.Drawing.Point(6, 51);
            this.ND3.Name = "ND3";
            this.ND3.Size = new System.Drawing.Size(40, 19);
            this.ND3.TabIndex = 20;
            this.ND3.Text = "D3";
            this.ND3.UseVisualStyleBackColor = false;
            this.ND3.CheckedChanged += new System.EventHandler(this.miscChk_CheckedChanged);
            // 
            // SD3
            // 
            this.SD3.AutoSize = true;
            this.SD3.BackColor = System.Drawing.Color.Transparent;
            this.SD3.ForeColor = System.Drawing.SystemColors.Control;
            this.SD3.Location = new System.Drawing.Point(6, 51);
            this.SD3.Name = "SD3";
            this.SD3.Size = new System.Drawing.Size(40, 19);
            this.SD3.TabIndex = 26;
            this.SD3.Text = "D3";
            this.SD3.UseVisualStyleBackColor = false;
            this.SD3.CheckedChanged += new System.EventHandler(this.miscChk_CheckedChanged);
            // 
            // SN3
            // 
            this.SN3.AutoSize = true;
            this.SN3.BackColor = System.Drawing.Color.Transparent;
            this.SN3.ForeColor = System.Drawing.SystemColors.Control;
            this.SN3.Location = new System.Drawing.Point(46, 51);
            this.SN3.Name = "SN3";
            this.SN3.Size = new System.Drawing.Size(41, 19);
            this.SN3.TabIndex = 25;
            this.SN3.Text = "N3";
            this.SN3.UseVisualStyleBackColor = false;
            this.SN3.CheckedChanged += new System.EventHandler(this.miscChk_CheckedChanged);
            // 
            // SD2
            // 
            this.SD2.AutoSize = true;
            this.SD2.BackColor = System.Drawing.Color.Transparent;
            this.SD2.ForeColor = System.Drawing.SystemColors.Control;
            this.SD2.Location = new System.Drawing.Point(6, 34);
            this.SD2.Name = "SD2";
            this.SD2.Size = new System.Drawing.Size(40, 19);
            this.SD2.TabIndex = 24;
            this.SD2.Text = "D2";
            this.SD2.UseVisualStyleBackColor = false;
            this.SD2.CheckedChanged += new System.EventHandler(this.miscChk_CheckedChanged);
            // 
            // SN2
            // 
            this.SN2.AutoSize = true;
            this.SN2.BackColor = System.Drawing.Color.Transparent;
            this.SN2.ForeColor = System.Drawing.SystemColors.Control;
            this.SN2.Location = new System.Drawing.Point(46, 34);
            this.SN2.Name = "SN2";
            this.SN2.Size = new System.Drawing.Size(41, 19);
            this.SN2.TabIndex = 23;
            this.SN2.Text = "N2";
            this.SN2.UseVisualStyleBackColor = false;
            this.SN2.CheckedChanged += new System.EventHandler(this.miscChk_CheckedChanged);
            // 
            // SN1
            // 
            this.SN1.AutoSize = true;
            this.SN1.BackColor = System.Drawing.Color.Transparent;
            this.SN1.ForeColor = System.Drawing.SystemColors.Control;
            this.SN1.Location = new System.Drawing.Point(46, 16);
            this.SN1.Name = "SN1";
            this.SN1.Size = new System.Drawing.Size(41, 19);
            this.SN1.TabIndex = 22;
            this.SN1.Text = "N1";
            this.SN1.UseVisualStyleBackColor = false;
            this.SN1.CheckedChanged += new System.EventHandler(this.miscChk_CheckedChanged);
            // 
            // SD1
            // 
            this.SD1.AutoSize = true;
            this.SD1.BackColor = System.Drawing.Color.Transparent;
            this.SD1.ForeColor = System.Drawing.SystemColors.Control;
            this.SD1.Location = new System.Drawing.Point(6, 16);
            this.SD1.Name = "SD1";
            this.SD1.Size = new System.Drawing.Size(40, 19);
            this.SD1.TabIndex = 21;
            this.SD1.Text = "D1";
            this.SD1.UseVisualStyleBackColor = false;
            this.SD1.CheckedChanged += new System.EventHandler(this.miscChk_CheckedChanged);
            // 
            // chkShowTime
            // 
            this.chkShowTime.AutoSize = true;
            this.chkShowTime.BackColor = System.Drawing.Color.Transparent;
            this.chkShowTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkShowTime.ForeColor = System.Drawing.SystemColors.Control;
            this.chkShowTime.Location = new System.Drawing.Point(0, 463);
            this.chkShowTime.Margin = new System.Windows.Forms.Padding(0);
            this.chkShowTime.Name = "chkShowTime";
            this.chkShowTime.Size = new System.Drawing.Size(142, 20);
            this.chkShowTime.TabIndex = 29;
            this.chkShowTime.Text = "Show Time Logic";
            this.chkShowTime.UseVisualStyleBackColor = false;
            this.chkShowTime.CheckedChanged += new System.EventHandler(this.miscChk_CheckedChanged);
            // 
            // BTNGotTo
            // 
            this.BTNGotTo.BackColor = System.Drawing.SystemColors.ControlDark;
            this.BTNGotTo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BTNGotTo.Location = new System.Drawing.Point(225, 446);
            this.BTNGotTo.Name = "BTNGotTo";
            this.BTNGotTo.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.tlbCond.SetRowSpan(this.BTNGotTo, 2);
            this.BTNGotTo.Size = new System.Drawing.Size(74, 34);
            this.BTNGotTo.TabIndex = 30;
            this.BTNGotTo.Text = "Go To";
            this.BTNGotTo.UseVisualStyleBackColor = false;
            this.BTNGotTo.Click += new System.EventHandler(this.btnGoTo_Click);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tlbReq.SetColumnSpan(this.textBox1, 2);
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(3, 28);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(193, 23);
            this.textBox1.TabIndex = 31;
            this.textBox1.TextChanged += new System.EventHandler(this.lbReq_TextChanged);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.numericUpDown1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numericUpDown1.Location = new System.Drawing.Point(225, 3);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(74, 23);
            this.numericUpDown1.TabIndex = 32;
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // tlbReq
            // 
            this.tlbReq.ColumnCount = 2;
            this.tlbReq.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlbReq.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlbReq.Controls.Add(this.groupBox1, 0, 3);
            this.tlbReq.Controls.Add(this.groupBox2, 1, 3);
            this.tlbReq.Controls.Add(this.textBox1, 0, 1);
            this.tlbReq.Controls.Add(this.LBReq, 0, 2);
            this.tlbReq.Controls.Add(this.label1, 0, 0);
            this.tlbReq.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlbReq.Location = new System.Drawing.Point(3, 3);
            this.tlbReq.Name = "tlbReq";
            this.tlbReq.RowCount = 4;
            this.tlbReq.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlbReq.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlbReq.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlbReq.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tlbReq.Size = new System.Drawing.Size(199, 483);
            this.tlbReq.TabIndex = 33;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.NN3);
            this.groupBox1.Controls.Add(this.ND1);
            this.groupBox1.Controls.Add(this.NN1);
            this.groupBox1.Controls.Add(this.NN2);
            this.groupBox1.Controls.Add(this.ND2);
            this.groupBox1.Controls.Add(this.ND3);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.ForeColor = System.Drawing.SystemColors.Control;
            this.groupBox1.Location = new System.Drawing.Point(3, 406);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(93, 74);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Available On";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.SN3);
            this.groupBox2.Controls.Add(this.SD1);
            this.groupBox2.Controls.Add(this.SN1);
            this.groupBox2.Controls.Add(this.SN2);
            this.groupBox2.Controls.Add(this.SD2);
            this.groupBox2.Controls.Add(this.SD3);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.ForeColor = System.Drawing.SystemColors.Control;
            this.groupBox2.Location = new System.Drawing.Point(102, 406);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(94, 74);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Setup On";
            // 
            // tlbCond
            // 
            this.tlbCond.ColumnCount = 3;
            this.tlbCond.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlbCond.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tlbCond.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tlbCond.Controls.Add(this.btnGoBack, 1, 2);
            this.tlbCond.Controls.Add(this.BTNGotTo, 2, 2);
            this.tlbCond.Controls.Add(this.numericUpDown1, 2, 0);
            this.tlbCond.Controls.Add(this.chkShowUnaltered, 0, 2);
            this.tlbCond.Controls.Add(this.chkShowTime, 0, 3);
            this.tlbCond.Controls.Add(this.lbCond, 0, 1);
            this.tlbCond.Controls.Add(this.label2, 0, 0);
            this.tlbCond.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlbCond.Location = new System.Drawing.Point(208, 3);
            this.tlbCond.Name = "tlbCond";
            this.tlbCond.RowCount = 4;
            this.tlbCond.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlbCond.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlbCond.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlbCond.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlbCond.Size = new System.Drawing.Size(302, 483);
            this.tlbCond.TabIndex = 34;
            // 
            // tlbmaster
            // 
            this.tlbmaster.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.tlbmaster.ColumnCount = 2;
            this.tlbmaster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlbmaster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tlbmaster.Controls.Add(this.tlbReq, 0, 0);
            this.tlbmaster.Controls.Add(this.tlbCond, 1, 0);
            this.tlbmaster.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlbmaster.Location = new System.Drawing.Point(0, 0);
            this.tlbmaster.Name = "tlbmaster";
            this.tlbmaster.RowCount = 1;
            this.tlbmaster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlbmaster.Size = new System.Drawing.Size(513, 489);
            this.tlbmaster.TabIndex = 35;
            // 
            // ShowLogic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(513, 489);
            this.Controls.Add(this.tlbmaster);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ShowLogic";
            this.Text = "ShowLogic";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ShowLogic_FormClosed);
            this.Load += new System.EventHandler(this.ShowLogic_Load);
            this.ResizeEnd += new System.EventHandler(this.ShowLogic_ResizeEnd);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.tlbReq.ResumeLayout(false);
            this.tlbReq.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tlbCond.ResumeLayout(false);
            this.tlbCond.PerformLayout();
            this.tlbmaster.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox LBReq;
        private System.Windows.Forms.ListBox lbCond;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnGoBack;
        private System.Windows.Forms.CheckBox chkShowUnaltered;
        private System.Windows.Forms.CheckBox ND1;
        private System.Windows.Forms.CheckBox NN1;
        private System.Windows.Forms.CheckBox NN2;
        private System.Windows.Forms.CheckBox ND2;
        private System.Windows.Forms.CheckBox NN3;
        private System.Windows.Forms.CheckBox ND3;
        private System.Windows.Forms.CheckBox SD3;
        private System.Windows.Forms.CheckBox SN3;
        private System.Windows.Forms.CheckBox SD2;
        private System.Windows.Forms.CheckBox SN2;
        private System.Windows.Forms.CheckBox SN1;
        private System.Windows.Forms.CheckBox SD1;
        private System.Windows.Forms.CheckBox chkShowTime;
        private System.Windows.Forms.Button BTNGotTo;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.TableLayoutPanel tlbReq;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tlbCond;
        private System.Windows.Forms.TableLayoutPanel tlbmaster;
    }
}