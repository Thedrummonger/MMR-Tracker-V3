
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
            LBReq = new System.Windows.Forms.ListBox();
            lbCond = new System.Windows.Forms.ListBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            btnGoBack = new System.Windows.Forms.Button();
            chkShowUnaltered = new System.Windows.Forms.CheckBox();
            ND1 = new System.Windows.Forms.CheckBox();
            NN1 = new System.Windows.Forms.CheckBox();
            NN2 = new System.Windows.Forms.CheckBox();
            ND2 = new System.Windows.Forms.CheckBox();
            NN3 = new System.Windows.Forms.CheckBox();
            ND3 = new System.Windows.Forms.CheckBox();
            SD3 = new System.Windows.Forms.CheckBox();
            SN3 = new System.Windows.Forms.CheckBox();
            SD2 = new System.Windows.Forms.CheckBox();
            SN2 = new System.Windows.Forms.CheckBox();
            SN1 = new System.Windows.Forms.CheckBox();
            SD1 = new System.Windows.Forms.CheckBox();
            chkShowTime = new System.Windows.Forms.CheckBox();
            BTNGotTo = new System.Windows.Forms.Button();
            textBox1 = new System.Windows.Forms.TextBox();
            numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            tlbReq = new System.Windows.Forms.TableLayoutPanel();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            tlbCond = new System.Windows.Forms.TableLayoutPanel();
            tlbmaster = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            tlbReq.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            tlbCond.SuspendLayout();
            tlbmaster.SuspendLayout();
            SuspendLayout();
            // 
            // LBReq
            // 
            LBReq.BackColor = System.Drawing.SystemColors.ControlDark;
            tlbReq.SetColumnSpan(LBReq, 2);
            LBReq.Dock = System.Windows.Forms.DockStyle.Fill;
            LBReq.FormattingEnabled = true;
            LBReq.HorizontalScrollbar = true;
            LBReq.IntegralHeight = false;
            LBReq.ItemHeight = 15;
            LBReq.Location = new System.Drawing.Point(3, 53);
            LBReq.Name = "LBReq";
            LBReq.Size = new System.Drawing.Size(193, 347);
            LBReq.TabIndex = 0;
            LBReq.DoubleClick += lbReq_DoubleClick;
            // 
            // lbCond
            // 
            lbCond.BackColor = System.Drawing.SystemColors.ControlDark;
            tlbCond.SetColumnSpan(lbCond, 3);
            lbCond.Dock = System.Windows.Forms.DockStyle.Fill;
            lbCond.FormattingEnabled = true;
            lbCond.HorizontalScrollbar = true;
            lbCond.IntegralHeight = false;
            lbCond.ItemHeight = 15;
            lbCond.Location = new System.Drawing.Point(3, 28);
            lbCond.Name = "lbCond";
            lbCond.Size = new System.Drawing.Size(296, 412);
            lbCond.TabIndex = 1;
            lbCond.DoubleClick += LBCond_DoubleClick;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = System.Drawing.Color.Transparent;
            tlbReq.SetColumnSpan(label1, 2);
            label1.Dock = System.Windows.Forms.DockStyle.Fill;
            label1.ForeColor = System.Drawing.SystemColors.Control;
            label1.Location = new System.Drawing.Point(5, 5);
            label1.Margin = new System.Windows.Forms.Padding(5);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(189, 15);
            label1.TabIndex = 3;
            label1.Text = "Requirements";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = System.Drawing.Color.Transparent;
            tlbCond.SetColumnSpan(label2, 2);
            label2.Dock = System.Windows.Forms.DockStyle.Fill;
            label2.ForeColor = System.Drawing.SystemColors.Control;
            label2.Location = new System.Drawing.Point(5, 5);
            label2.Margin = new System.Windows.Forms.Padding(5);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(212, 15);
            label2.TabIndex = 4;
            label2.Text = "Conditionals";
            // 
            // btnGoBack
            // 
            btnGoBack.BackColor = System.Drawing.SystemColors.ControlDark;
            btnGoBack.Dock = System.Windows.Forms.DockStyle.Fill;
            btnGoBack.Location = new System.Drawing.Point(145, 446);
            btnGoBack.Name = "btnGoBack";
            btnGoBack.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            tlbCond.SetRowSpan(btnGoBack, 2);
            btnGoBack.Size = new System.Drawing.Size(74, 34);
            btnGoBack.TabIndex = 8;
            btnGoBack.Text = "Go Back";
            btnGoBack.UseVisualStyleBackColor = false;
            btnGoBack.Click += btnGoBack_Click;
            // 
            // chkShowUnaltered
            // 
            chkShowUnaltered.AutoSize = true;
            chkShowUnaltered.BackColor = System.Drawing.Color.Transparent;
            chkShowUnaltered.Dock = System.Windows.Forms.DockStyle.Fill;
            chkShowUnaltered.ForeColor = System.Drawing.SystemColors.Control;
            chkShowUnaltered.Location = new System.Drawing.Point(0, 443);
            chkShowUnaltered.Margin = new System.Windows.Forms.Padding(0);
            chkShowUnaltered.Name = "chkShowUnaltered";
            chkShowUnaltered.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
            chkShowUnaltered.Size = new System.Drawing.Size(142, 20);
            chkShowUnaltered.TabIndex = 11;
            chkShowUnaltered.Text = "Show Unaltered Logic";
            chkShowUnaltered.UseVisualStyleBackColor = false;
            chkShowUnaltered.CheckedChanged += miscChk_CheckedChanged;
            // 
            // ND1
            // 
            ND1.AutoSize = true;
            ND1.BackColor = System.Drawing.Color.Transparent;
            ND1.ForeColor = System.Drawing.SystemColors.Control;
            ND1.Location = new System.Drawing.Point(6, 16);
            ND1.Name = "ND1";
            ND1.Size = new System.Drawing.Size(40, 19);
            ND1.TabIndex = 12;
            ND1.Text = "D1";
            ND1.UseVisualStyleBackColor = false;
            ND1.CheckedChanged += miscChk_CheckedChanged;
            // 
            // NN1
            // 
            NN1.AutoSize = true;
            NN1.BackColor = System.Drawing.Color.Transparent;
            NN1.ForeColor = System.Drawing.SystemColors.Control;
            NN1.Location = new System.Drawing.Point(46, 16);
            NN1.Name = "NN1";
            NN1.Size = new System.Drawing.Size(41, 19);
            NN1.TabIndex = 13;
            NN1.Text = "N1";
            NN1.UseVisualStyleBackColor = false;
            NN1.CheckedChanged += miscChk_CheckedChanged;
            // 
            // NN2
            // 
            NN2.AutoSize = true;
            NN2.BackColor = System.Drawing.Color.Transparent;
            NN2.ForeColor = System.Drawing.SystemColors.Control;
            NN2.Location = new System.Drawing.Point(46, 34);
            NN2.Name = "NN2";
            NN2.Size = new System.Drawing.Size(41, 19);
            NN2.TabIndex = 15;
            NN2.Text = "N2";
            NN2.UseVisualStyleBackColor = false;
            NN2.CheckedChanged += miscChk_CheckedChanged;
            // 
            // ND2
            // 
            ND2.AutoSize = true;
            ND2.BackColor = System.Drawing.Color.Transparent;
            ND2.ForeColor = System.Drawing.SystemColors.Control;
            ND2.Location = new System.Drawing.Point(6, 34);
            ND2.Name = "ND2";
            ND2.Size = new System.Drawing.Size(40, 19);
            ND2.TabIndex = 16;
            ND2.Text = "D2";
            ND2.UseVisualStyleBackColor = false;
            ND2.CheckedChanged += miscChk_CheckedChanged;
            // 
            // NN3
            // 
            NN3.AutoSize = true;
            NN3.BackColor = System.Drawing.Color.Transparent;
            NN3.ForeColor = System.Drawing.SystemColors.Control;
            NN3.Location = new System.Drawing.Point(46, 51);
            NN3.Name = "NN3";
            NN3.Size = new System.Drawing.Size(41, 19);
            NN3.TabIndex = 19;
            NN3.Text = "N3";
            NN3.UseVisualStyleBackColor = false;
            NN3.CheckedChanged += miscChk_CheckedChanged;
            // 
            // ND3
            // 
            ND3.AutoSize = true;
            ND3.BackColor = System.Drawing.Color.Transparent;
            ND3.ForeColor = System.Drawing.SystemColors.Control;
            ND3.Location = new System.Drawing.Point(6, 51);
            ND3.Name = "ND3";
            ND3.Size = new System.Drawing.Size(40, 19);
            ND3.TabIndex = 20;
            ND3.Text = "D3";
            ND3.UseVisualStyleBackColor = false;
            ND3.CheckedChanged += miscChk_CheckedChanged;
            // 
            // SD3
            // 
            SD3.AutoSize = true;
            SD3.BackColor = System.Drawing.Color.Transparent;
            SD3.ForeColor = System.Drawing.SystemColors.Control;
            SD3.Location = new System.Drawing.Point(6, 51);
            SD3.Name = "SD3";
            SD3.Size = new System.Drawing.Size(40, 19);
            SD3.TabIndex = 26;
            SD3.Text = "D3";
            SD3.UseVisualStyleBackColor = false;
            SD3.CheckedChanged += miscChk_CheckedChanged;
            // 
            // SN3
            // 
            SN3.AutoSize = true;
            SN3.BackColor = System.Drawing.Color.Transparent;
            SN3.ForeColor = System.Drawing.SystemColors.Control;
            SN3.Location = new System.Drawing.Point(46, 51);
            SN3.Name = "SN3";
            SN3.Size = new System.Drawing.Size(41, 19);
            SN3.TabIndex = 25;
            SN3.Text = "N3";
            SN3.UseVisualStyleBackColor = false;
            SN3.CheckedChanged += miscChk_CheckedChanged;
            // 
            // SD2
            // 
            SD2.AutoSize = true;
            SD2.BackColor = System.Drawing.Color.Transparent;
            SD2.ForeColor = System.Drawing.SystemColors.Control;
            SD2.Location = new System.Drawing.Point(6, 34);
            SD2.Name = "SD2";
            SD2.Size = new System.Drawing.Size(40, 19);
            SD2.TabIndex = 24;
            SD2.Text = "D2";
            SD2.UseVisualStyleBackColor = false;
            SD2.CheckedChanged += miscChk_CheckedChanged;
            // 
            // SN2
            // 
            SN2.AutoSize = true;
            SN2.BackColor = System.Drawing.Color.Transparent;
            SN2.ForeColor = System.Drawing.SystemColors.Control;
            SN2.Location = new System.Drawing.Point(46, 34);
            SN2.Name = "SN2";
            SN2.Size = new System.Drawing.Size(41, 19);
            SN2.TabIndex = 23;
            SN2.Text = "N2";
            SN2.UseVisualStyleBackColor = false;
            SN2.CheckedChanged += miscChk_CheckedChanged;
            // 
            // SN1
            // 
            SN1.AutoSize = true;
            SN1.BackColor = System.Drawing.Color.Transparent;
            SN1.ForeColor = System.Drawing.SystemColors.Control;
            SN1.Location = new System.Drawing.Point(46, 16);
            SN1.Name = "SN1";
            SN1.Size = new System.Drawing.Size(41, 19);
            SN1.TabIndex = 22;
            SN1.Text = "N1";
            SN1.UseVisualStyleBackColor = false;
            SN1.CheckedChanged += miscChk_CheckedChanged;
            // 
            // SD1
            // 
            SD1.AutoSize = true;
            SD1.BackColor = System.Drawing.Color.Transparent;
            SD1.ForeColor = System.Drawing.SystemColors.Control;
            SD1.Location = new System.Drawing.Point(6, 16);
            SD1.Name = "SD1";
            SD1.Size = new System.Drawing.Size(40, 19);
            SD1.TabIndex = 21;
            SD1.Text = "D1";
            SD1.UseVisualStyleBackColor = false;
            SD1.CheckedChanged += miscChk_CheckedChanged;
            // 
            // chkShowTime
            // 
            chkShowTime.AutoSize = true;
            chkShowTime.BackColor = System.Drawing.Color.Transparent;
            chkShowTime.Dock = System.Windows.Forms.DockStyle.Fill;
            chkShowTime.ForeColor = System.Drawing.SystemColors.Control;
            chkShowTime.Location = new System.Drawing.Point(0, 463);
            chkShowTime.Margin = new System.Windows.Forms.Padding(0);
            chkShowTime.Name = "chkShowTime";
            chkShowTime.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
            chkShowTime.Size = new System.Drawing.Size(142, 20);
            chkShowTime.TabIndex = 29;
            chkShowTime.Text = "Show Time Logic";
            chkShowTime.UseVisualStyleBackColor = false;
            chkShowTime.CheckedChanged += miscChk_CheckedChanged;
            // 
            // BTNGotTo
            // 
            BTNGotTo.BackColor = System.Drawing.SystemColors.ControlDark;
            BTNGotTo.Dock = System.Windows.Forms.DockStyle.Fill;
            BTNGotTo.Location = new System.Drawing.Point(225, 446);
            BTNGotTo.Name = "BTNGotTo";
            BTNGotTo.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            tlbCond.SetRowSpan(BTNGotTo, 2);
            BTNGotTo.Size = new System.Drawing.Size(74, 34);
            BTNGotTo.TabIndex = 30;
            BTNGotTo.Text = "Go To";
            BTNGotTo.UseVisualStyleBackColor = false;
            BTNGotTo.Click += btnGoTo_Click;
            // 
            // textBox1
            // 
            textBox1.BackColor = System.Drawing.SystemColors.ControlDark;
            tlbReq.SetColumnSpan(textBox1, 2);
            textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            textBox1.Location = new System.Drawing.Point(3, 28);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(193, 23);
            textBox1.TabIndex = 31;
            textBox1.TextChanged += lbReq_TextChanged;
            // 
            // numericUpDown1
            // 
            numericUpDown1.BackColor = System.Drawing.SystemColors.ControlDark;
            numericUpDown1.Dock = System.Windows.Forms.DockStyle.Fill;
            numericUpDown1.Location = new System.Drawing.Point(225, 3);
            numericUpDown1.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            numericUpDown1.Minimum = new decimal(new int[] { 1, 0, 0, int.MinValue });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new System.Drawing.Size(74, 23);
            numericUpDown1.TabIndex = 32;
            numericUpDown1.ValueChanged += numericUpDown1_ValueChanged;
            // 
            // tlbReq
            // 
            tlbReq.ColumnCount = 2;
            tlbReq.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tlbReq.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tlbReq.Controls.Add(groupBox1, 0, 3);
            tlbReq.Controls.Add(groupBox2, 1, 3);
            tlbReq.Controls.Add(textBox1, 0, 1);
            tlbReq.Controls.Add(LBReq, 0, 2);
            tlbReq.Controls.Add(label1, 0, 0);
            tlbReq.Dock = System.Windows.Forms.DockStyle.Fill;
            tlbReq.Location = new System.Drawing.Point(3, 3);
            tlbReq.Name = "tlbReq";
            tlbReq.RowCount = 4;
            tlbReq.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tlbReq.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tlbReq.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlbReq.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            tlbReq.Size = new System.Drawing.Size(199, 483);
            tlbReq.TabIndex = 33;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(NN3);
            groupBox1.Controls.Add(ND1);
            groupBox1.Controls.Add(NN1);
            groupBox1.Controls.Add(NN2);
            groupBox1.Controls.Add(ND2);
            groupBox1.Controls.Add(ND3);
            groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBox1.ForeColor = System.Drawing.SystemColors.Control;
            groupBox1.Location = new System.Drawing.Point(3, 406);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(93, 74);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Available On";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(SN3);
            groupBox2.Controls.Add(SD1);
            groupBox2.Controls.Add(SN1);
            groupBox2.Controls.Add(SN2);
            groupBox2.Controls.Add(SD2);
            groupBox2.Controls.Add(SD3);
            groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBox2.ForeColor = System.Drawing.SystemColors.Control;
            groupBox2.Location = new System.Drawing.Point(102, 406);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(94, 74);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Setup On";
            // 
            // tlbCond
            // 
            tlbCond.ColumnCount = 3;
            tlbCond.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlbCond.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            tlbCond.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            tlbCond.Controls.Add(btnGoBack, 1, 2);
            tlbCond.Controls.Add(BTNGotTo, 2, 2);
            tlbCond.Controls.Add(numericUpDown1, 2, 0);
            tlbCond.Controls.Add(chkShowUnaltered, 0, 2);
            tlbCond.Controls.Add(chkShowTime, 0, 3);
            tlbCond.Controls.Add(lbCond, 0, 1);
            tlbCond.Controls.Add(label2, 0, 0);
            tlbCond.Dock = System.Windows.Forms.DockStyle.Fill;
            tlbCond.Location = new System.Drawing.Point(208, 3);
            tlbCond.Name = "tlbCond";
            tlbCond.RowCount = 4;
            tlbCond.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tlbCond.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlbCond.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tlbCond.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tlbCond.Size = new System.Drawing.Size(302, 483);
            tlbCond.TabIndex = 34;
            // 
            // tlbmaster
            // 
            tlbmaster.BackColor = System.Drawing.Color.FromArgb(0, 0, 64);
            tlbmaster.ColumnCount = 2;
            tlbmaster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            tlbmaster.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            tlbmaster.Controls.Add(tlbReq, 0, 0);
            tlbmaster.Controls.Add(tlbCond, 1, 0);
            tlbmaster.Dock = System.Windows.Forms.DockStyle.Fill;
            tlbmaster.Location = new System.Drawing.Point(0, 0);
            tlbmaster.Name = "tlbmaster";
            tlbmaster.RowCount = 1;
            tlbmaster.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlbmaster.Size = new System.Drawing.Size(513, 489);
            tlbmaster.TabIndex = 35;
            // 
            // ShowLogic
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            ClientSize = new System.Drawing.Size(513, 489);
            Controls.Add(tlbmaster);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "ShowLogic";
            Text = "ShowLogic";
            FormClosed += ShowLogic_FormClosed;
            Load += ShowLogic_Load;
            ResizeEnd += ShowLogic_ResizeEnd;
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            tlbReq.ResumeLayout(false);
            tlbReq.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            tlbCond.ResumeLayout(false);
            tlbCond.PerformLayout();
            tlbmaster.ResumeLayout(false);
            ResumeLayout(false);
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