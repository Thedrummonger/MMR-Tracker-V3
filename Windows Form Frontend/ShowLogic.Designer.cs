
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
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.chkShowTime = new System.Windows.Forms.CheckBox();
            this.BTNGotTo = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // LBReq
            // 
            this.LBReq.FormattingEnabled = true;
            this.LBReq.HorizontalScrollbar = true;
            this.LBReq.ItemHeight = 15;
            this.LBReq.Location = new System.Drawing.Point(13, 57);
            this.LBReq.Name = "LBReq";
            this.LBReq.Size = new System.Drawing.Size(178, 334);
            this.LBReq.TabIndex = 0;
            this.LBReq.DoubleClick += new System.EventHandler(this.lbReq_DoubleClick);
            // 
            // lbCond
            // 
            this.lbCond.FormattingEnabled = true;
            this.lbCond.HorizontalScrollbar = true;
            this.lbCond.ItemHeight = 15;
            this.lbCond.Location = new System.Drawing.Point(197, 27);
            this.lbCond.Name = "lbCond";
            this.lbCond.Size = new System.Drawing.Size(305, 394);
            this.lbCond.TabIndex = 1;
            this.lbCond.DoubleClick += new System.EventHandler(this.LBCond_DoubleClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.SystemColors.Control;
            this.label1.Location = new System.Drawing.Point(13, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Requirements";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.SystemColors.Control;
            this.label2.Location = new System.Drawing.Point(197, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Conditionals";
            // 
            // btnGoBack
            // 
            this.btnGoBack.Location = new System.Drawing.Point(346, 427);
            this.btnGoBack.Name = "btnGoBack";
            this.btnGoBack.Size = new System.Drawing.Size(75, 23);
            this.btnGoBack.TabIndex = 8;
            this.btnGoBack.Text = "Go Back";
            this.btnGoBack.UseVisualStyleBackColor = true;
            this.btnGoBack.Click += new System.EventHandler(this.btnGoBack_Click);
            // 
            // chkShowUnaltered
            // 
            this.chkShowUnaltered.AutoSize = true;
            this.chkShowUnaltered.BackColor = System.Drawing.Color.Transparent;
            this.chkShowUnaltered.ForeColor = System.Drawing.SystemColors.Control;
            this.chkShowUnaltered.Location = new System.Drawing.Point(197, 421);
            this.chkShowUnaltered.Name = "chkShowUnaltered";
            this.chkShowUnaltered.Size = new System.Drawing.Size(141, 19);
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
            this.ND1.Location = new System.Drawing.Point(17, 405);
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
            this.NN1.Location = new System.Drawing.Point(57, 405);
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
            this.NN2.Location = new System.Drawing.Point(57, 421);
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
            this.ND2.Location = new System.Drawing.Point(17, 421);
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
            this.NN3.Location = new System.Drawing.Point(57, 438);
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
            this.ND3.Location = new System.Drawing.Point(17, 438);
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
            this.SD3.Location = new System.Drawing.Point(107, 438);
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
            this.SN3.Location = new System.Drawing.Point(147, 438);
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
            this.SD2.Location = new System.Drawing.Point(107, 421);
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
            this.SN2.Location = new System.Drawing.Point(147, 421);
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
            this.SN1.Location = new System.Drawing.Point(147, 405);
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
            this.SD1.Location = new System.Drawing.Point(107, 405);
            this.SD1.Name = "SD1";
            this.SD1.Size = new System.Drawing.Size(40, 19);
            this.SD1.TabIndex = 21;
            this.SD1.Text = "D1";
            this.SD1.UseVisualStyleBackColor = false;
            this.SD1.CheckedChanged += new System.EventHandler(this.miscChk_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.ForeColor = System.Drawing.SystemColors.Control;
            this.label5.Location = new System.Drawing.Point(104, 392);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 15);
            this.label5.TabIndex = 27;
            this.label5.Text = "Setup On";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.ForeColor = System.Drawing.SystemColors.Control;
            this.label6.Location = new System.Drawing.Point(13, 392);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(74, 15);
            this.label6.TabIndex = 28;
            this.label6.Text = "Available On";
            // 
            // chkShowTime
            // 
            this.chkShowTime.AutoSize = true;
            this.chkShowTime.BackColor = System.Drawing.Color.Transparent;
            this.chkShowTime.ForeColor = System.Drawing.SystemColors.Control;
            this.chkShowTime.Location = new System.Drawing.Point(197, 438);
            this.chkShowTime.Name = "chkShowTime";
            this.chkShowTime.Size = new System.Drawing.Size(116, 19);
            this.chkShowTime.TabIndex = 29;
            this.chkShowTime.Text = "Show Time Logic";
            this.chkShowTime.UseVisualStyleBackColor = false;
            this.chkShowTime.CheckedChanged += new System.EventHandler(this.miscChk_CheckedChanged);
            // 
            // BTNGotTo
            // 
            this.BTNGotTo.Location = new System.Drawing.Point(427, 427);
            this.BTNGotTo.Name = "BTNGotTo";
            this.BTNGotTo.Size = new System.Drawing.Size(75, 23);
            this.BTNGotTo.TabIndex = 30;
            this.BTNGotTo.Text = "Go To";
            this.BTNGotTo.UseVisualStyleBackColor = true;
            this.BTNGotTo.Click += new System.EventHandler(this.btnGoTo_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(13, 27);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(178, 23);
            this.textBox1.TabIndex = 31;
            this.textBox1.TextChanged += new System.EventHandler(this.lbReq_TextChanged);
            // 
            // ShowLogic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(514, 461);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.BTNGotTo);
            this.Controls.Add(this.chkShowTime);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.SD3);
            this.Controls.Add(this.SN3);
            this.Controls.Add(this.SD2);
            this.Controls.Add(this.SN2);
            this.Controls.Add(this.SN1);
            this.Controls.Add(this.SD1);
            this.Controls.Add(this.ND3);
            this.Controls.Add(this.NN3);
            this.Controls.Add(this.ND2);
            this.Controls.Add(this.NN2);
            this.Controls.Add(this.NN1);
            this.Controls.Add(this.ND1);
            this.Controls.Add(this.chkShowUnaltered);
            this.Controls.Add(this.btnGoBack);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbCond);
            this.Controls.Add(this.LBReq);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(530, 500);
            this.MinimumSize = new System.Drawing.Size(530, 500);
            this.Name = "ShowLogic";
            this.Text = "ShowLogic";
            this.Load += new System.EventHandler(this.ShowLogic_Load);
            this.ResizeEnd += new System.EventHandler(this.ShowLogic_ResizeEnd);
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkShowTime;
        private System.Windows.Forms.Button BTNGotTo;
        private System.Windows.Forms.TextBox textBox1;
    }
}