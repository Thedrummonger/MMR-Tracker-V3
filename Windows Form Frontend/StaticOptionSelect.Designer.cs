
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StaticOptionSelect));
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            panel1 = new System.Windows.Forms.Panel();
            button1 = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            richTextBox1 = new System.Windows.Forms.RichTextBox();
            button3 = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // toolTip1
            // 
            toolTip1.AutomaticDelay = 50000;
            toolTip1.AutoPopDelay = 500000;
            toolTip1.InitialDelay = 100;
            toolTip1.ReshowDelay = 100;
            // 
            // panel1
            // 
            panel1.Location = new System.Drawing.Point(12, 12);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(370, 321);
            panel1.TabIndex = 46;
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(12, 339);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(208, 30);
            button1.TabIndex = 47;
            button1.Text = "Apply To Current Instance";
            button1.UseVisualStyleBackColor = true;
            button1.Click += Button_Apply_To_Current;
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(226, 339);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(156, 30);
            button2.TabIndex = 48;
            button2.Text = "Set As Default";
            button2.UseVisualStyleBackColor = true;
            button2.Click += Button_Set_Default;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 372);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(103, 18);
            label1.TabIndex = 49;
            label1.Text = "Font Example:";
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new System.Drawing.Point(121, 372);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new System.Drawing.Size(261, 56);
            richTextBox1.TabIndex = 50;
            richTextBox1.Text = "";
            // 
            // button3
            // 
            button3.Location = new System.Drawing.Point(12, 394);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(103, 34);
            button3.TabIndex = 51;
            button3.Text = "Reset Font";
            button3.UseVisualStyleBackColor = true;
            button3.Click += Button_ResetFont;
            // 
            // StaticOptionSelect
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(398, 440);
            Controls.Add(button3);
            Controls.Add(richTextBox1);
            Controls.Add(label1);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(panel1);
            Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "StaticOptionSelect";
            Text = "Options";
            Load += StaticOptionSelect_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button3;
    }
}