
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
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
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
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.HorizontalScrollbar = true;
            this.listBox1.ItemHeight = 15;
            this.listBox1.Location = new System.Drawing.Point(13, 57);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(178, 334);
            this.listBox1.TabIndex = 0;
            this.listBox1.DoubleClick += new System.EventHandler(this.listBox3_DoubleClick);
            // 
            // listBox2
            // 
            this.listBox2.FormattingEnabled = true;
            this.listBox2.HorizontalScrollbar = true;
            this.listBox2.ItemHeight = 15;
            this.listBox2.Location = new System.Drawing.Point(197, 27);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(305, 394);
            this.listBox2.TabIndex = 1;
            this.listBox2.DoubleClick += new System.EventHandler(this.listBox3_DoubleClick);
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
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(346, 427);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Go Back";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.BackColor = System.Drawing.Color.Transparent;
            this.checkBox1.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox1.Location = new System.Drawing.Point(197, 421);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(141, 19);
            this.checkBox1.TabIndex = 11;
            this.checkBox1.Text = "Show Unaltered Logic";
            this.checkBox1.UseVisualStyleBackColor = false;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // ND1
            // 
            this.ND1.AutoSize = true;
            this.ND1.BackColor = System.Drawing.Color.Transparent;
            this.ND1.Enabled = false;
            this.ND1.ForeColor = System.Drawing.SystemColors.Control;
            this.ND1.Location = new System.Drawing.Point(17, 405);
            this.ND1.Name = "ND1";
            this.ND1.Size = new System.Drawing.Size(40, 19);
            this.ND1.TabIndex = 12;
            this.ND1.Text = "D1";
            this.ND1.UseVisualStyleBackColor = false;
            // 
            // NN1
            // 
            this.NN1.AutoSize = true;
            this.NN1.BackColor = System.Drawing.Color.Transparent;
            this.NN1.Enabled = false;
            this.NN1.ForeColor = System.Drawing.SystemColors.Control;
            this.NN1.Location = new System.Drawing.Point(57, 405);
            this.NN1.Name = "NN1";
            this.NN1.Size = new System.Drawing.Size(41, 19);
            this.NN1.TabIndex = 13;
            this.NN1.Text = "N1";
            this.NN1.UseVisualStyleBackColor = false;
            // 
            // NN2
            // 
            this.NN2.AutoSize = true;
            this.NN2.BackColor = System.Drawing.Color.Transparent;
            this.NN2.Enabled = false;
            this.NN2.ForeColor = System.Drawing.SystemColors.Control;
            this.NN2.Location = new System.Drawing.Point(57, 421);
            this.NN2.Name = "NN2";
            this.NN2.Size = new System.Drawing.Size(41, 19);
            this.NN2.TabIndex = 15;
            this.NN2.Text = "N2";
            this.NN2.UseVisualStyleBackColor = false;
            // 
            // ND2
            // 
            this.ND2.AutoSize = true;
            this.ND2.BackColor = System.Drawing.Color.Transparent;
            this.ND2.Enabled = false;
            this.ND2.ForeColor = System.Drawing.SystemColors.Control;
            this.ND2.Location = new System.Drawing.Point(17, 421);
            this.ND2.Name = "ND2";
            this.ND2.Size = new System.Drawing.Size(40, 19);
            this.ND2.TabIndex = 16;
            this.ND2.Text = "D2";
            this.ND2.UseVisualStyleBackColor = false;
            // 
            // NN3
            // 
            this.NN3.AutoSize = true;
            this.NN3.BackColor = System.Drawing.Color.Transparent;
            this.NN3.Enabled = false;
            this.NN3.ForeColor = System.Drawing.SystemColors.Control;
            this.NN3.Location = new System.Drawing.Point(57, 438);
            this.NN3.Name = "NN3";
            this.NN3.Size = new System.Drawing.Size(41, 19);
            this.NN3.TabIndex = 19;
            this.NN3.Text = "N3";
            this.NN3.UseVisualStyleBackColor = false;
            // 
            // ND3
            // 
            this.ND3.AutoSize = true;
            this.ND3.BackColor = System.Drawing.Color.Transparent;
            this.ND3.Enabled = false;
            this.ND3.ForeColor = System.Drawing.SystemColors.Control;
            this.ND3.Location = new System.Drawing.Point(17, 438);
            this.ND3.Name = "ND3";
            this.ND3.Size = new System.Drawing.Size(40, 19);
            this.ND3.TabIndex = 20;
            this.ND3.Text = "D3";
            this.ND3.UseVisualStyleBackColor = false;
            // 
            // SD3
            // 
            this.SD3.AutoSize = true;
            this.SD3.BackColor = System.Drawing.Color.Transparent;
            this.SD3.Enabled = false;
            this.SD3.ForeColor = System.Drawing.SystemColors.Control;
            this.SD3.Location = new System.Drawing.Point(107, 438);
            this.SD3.Name = "SD3";
            this.SD3.Size = new System.Drawing.Size(40, 19);
            this.SD3.TabIndex = 26;
            this.SD3.Text = "D3";
            this.SD3.UseVisualStyleBackColor = false;
            // 
            // SN3
            // 
            this.SN3.AutoSize = true;
            this.SN3.BackColor = System.Drawing.Color.Transparent;
            this.SN3.Enabled = false;
            this.SN3.ForeColor = System.Drawing.SystemColors.Control;
            this.SN3.Location = new System.Drawing.Point(147, 438);
            this.SN3.Name = "SN3";
            this.SN3.Size = new System.Drawing.Size(41, 19);
            this.SN3.TabIndex = 25;
            this.SN3.Text = "N3";
            this.SN3.UseVisualStyleBackColor = false;
            // 
            // SD2
            // 
            this.SD2.AutoSize = true;
            this.SD2.BackColor = System.Drawing.Color.Transparent;
            this.SD2.Enabled = false;
            this.SD2.ForeColor = System.Drawing.SystemColors.Control;
            this.SD2.Location = new System.Drawing.Point(107, 421);
            this.SD2.Name = "SD2";
            this.SD2.Size = new System.Drawing.Size(40, 19);
            this.SD2.TabIndex = 24;
            this.SD2.Text = "D2";
            this.SD2.UseVisualStyleBackColor = false;
            // 
            // SN2
            // 
            this.SN2.AutoSize = true;
            this.SN2.BackColor = System.Drawing.Color.Transparent;
            this.SN2.Enabled = false;
            this.SN2.ForeColor = System.Drawing.SystemColors.Control;
            this.SN2.Location = new System.Drawing.Point(147, 421);
            this.SN2.Name = "SN2";
            this.SN2.Size = new System.Drawing.Size(41, 19);
            this.SN2.TabIndex = 23;
            this.SN2.Text = "N2";
            this.SN2.UseVisualStyleBackColor = false;
            // 
            // SN1
            // 
            this.SN1.AutoSize = true;
            this.SN1.BackColor = System.Drawing.Color.Transparent;
            this.SN1.Enabled = false;
            this.SN1.ForeColor = System.Drawing.SystemColors.Control;
            this.SN1.Location = new System.Drawing.Point(147, 405);
            this.SN1.Name = "SN1";
            this.SN1.Size = new System.Drawing.Size(41, 19);
            this.SN1.TabIndex = 22;
            this.SN1.Text = "N1";
            this.SN1.UseVisualStyleBackColor = false;
            // 
            // SD1
            // 
            this.SD1.AutoSize = true;
            this.SD1.BackColor = System.Drawing.Color.Transparent;
            this.SD1.Enabled = false;
            this.SD1.ForeColor = System.Drawing.SystemColors.Control;
            this.SD1.Location = new System.Drawing.Point(107, 405);
            this.SD1.Name = "SD1";
            this.SD1.Size = new System.Drawing.Size(40, 19);
            this.SD1.TabIndex = 21;
            this.SD1.Text = "D1";
            this.SD1.UseVisualStyleBackColor = false;
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
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.BackColor = System.Drawing.Color.Transparent;
            this.checkBox2.ForeColor = System.Drawing.SystemColors.Control;
            this.checkBox2.Location = new System.Drawing.Point(197, 438);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(116, 19);
            this.checkBox2.TabIndex = 29;
            this.checkBox2.Text = "Show Time Logic";
            this.checkBox2.UseVisualStyleBackColor = false;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(427, 427);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 30;
            this.button2.Text = "Go To";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(13, 27);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(178, 23);
            this.textBox1.TabIndex = 31;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // ShowLogic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(514, 461);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.checkBox2);
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
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listBox2);
            this.Controls.Add(this.listBox1);
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

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox checkBox1;
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
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox1;
    }
}