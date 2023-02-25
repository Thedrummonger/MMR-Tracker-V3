
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
            this.button3 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.chkCompressSave = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudFontSize)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(35, 15);
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
            this.label2.Location = new System.Drawing.Point(12, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 18);
            this.label2.TabIndex = 1;
            this.label2.Text = "?";
            this.toolTip1.SetToolTip(this.label2, "Should the tracker check for updates and notify you when a new one is available?");
            // 
            // chkUpdates
            // 
            this.chkUpdates.AutoSize = true;
            this.chkUpdates.Location = new System.Drawing.Point(197, 16);
            this.chkUpdates.Name = "chkUpdates";
            this.chkUpdates.Size = new System.Drawing.Size(15, 14);
            this.chkUpdates.TabIndex = 2;
            this.chkUpdates.UseVisualStyleBackColor = true;
            this.chkUpdates.CheckStateChanged += new System.EventHandler(this.chkUpdates_CheckStateChanged);
            // 
            // chkHorizontal
            // 
            this.chkHorizontal.AutoSize = true;
            this.chkHorizontal.Location = new System.Drawing.Point(197, 36);
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
            this.label3.Location = new System.Drawing.Point(12, 33);
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
            this.label4.Location = new System.Drawing.Point(35, 35);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(110, 16);
            this.label4.TabIndex = 3;
            this.label4.Text = "Horizontal Layout";
            // 
            // chkTooltips
            // 
            this.chkTooltips.AutoSize = true;
            this.chkTooltips.Location = new System.Drawing.Point(197, 56);
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
            this.label5.Location = new System.Drawing.Point(12, 53);
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
            this.label6.Location = new System.Drawing.Point(35, 55);
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
            this.label7.Location = new System.Drawing.Point(12, 104);
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
            this.label8.Location = new System.Drawing.Point(35, 96);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(62, 16);
            this.label8.TabIndex = 9;
            this.label8.Text = "Font Size";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label10.Location = new System.Drawing.Point(35, 116);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(66, 16);
            this.label10.TabIndex = 12;
            this.label10.Text = "Font Style";
            // 
            // nudFontSize
            // 
            this.nudFontSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.nudFontSize.Location = new System.Drawing.Point(167, 96);
            this.nudFontSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudFontSize.Name = "nudFontSize";
            this.nudFontSize.Size = new System.Drawing.Size(45, 20);
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
            this.cmbFontStyle.Location = new System.Drawing.Point(112, 115);
            this.cmbFontStyle.Name = "cmbFontStyle";
            this.cmbFontStyle.Size = new System.Drawing.Size(100, 21);
            this.cmbFontStyle.TabIndex = 15;
            this.cmbFontStyle.SelectedValueChanged += new System.EventHandler(this.cmbFontStyle_SelectedValueChanged);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.button1.Location = new System.Drawing.Point(12, 142);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 22);
            this.button1.TabIndex = 16;
            this.button1.Text = "Apply";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.button2.Location = new System.Drawing.Point(112, 142);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(100, 22);
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
            this.label11.Location = new System.Drawing.Point(12, 73);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(17, 18);
            this.label11.TabIndex = 22;
            this.label11.Text = "?";
            this.toolTip1.SetToolTip(this.label11, "Should the tracker Compress it\'s save files to take less disk space?\r\nCompressed " +
        "save files are unreadable and cannot be edited manually.");
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.button3.Location = new System.Drawing.Point(112, 96);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(55, 19);
            this.button3.TabIndex = 18;
            this.button3.Text = "Default";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(112, 171);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 24);
            this.textBox1.TabIndex = 19;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label9.Location = new System.Drawing.Point(12, 176);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(92, 16);
            this.label9.TabIndex = 20;
            this.label9.Text = "Font Example:";
            // 
            // chkCompressSave
            // 
            this.chkCompressSave.AutoSize = true;
            this.chkCompressSave.Location = new System.Drawing.Point(197, 76);
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
            this.label12.Location = new System.Drawing.Point(35, 75);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(136, 16);
            this.label12.TabIndex = 21;
            this.label12.Text = "Compress Save Files";
            // 
            // StaticOptionSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(221, 208);
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
    }
}