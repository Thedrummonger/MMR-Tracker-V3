
namespace Windows_Form_Frontend
{
    partial class MainInterface
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainInterface));
            this.lblSwapPathfinder = new System.Windows.Forms.Label();
            this.BTNFindPath = new System.Windows.Forms.Button();
            this.LBPathFinder = new System.Windows.Forms.ListBox();
            this.LBCheckedLocations = new System.Windows.Forms.ListBox();
            this.LBValidEntrances = new System.Windows.Forms.ListBox();
            this.LBValidLocations = new System.Windows.Forms.ListBox();
            this.CMBEnd = new System.Windows.Forms.ComboBox();
            this.CMBStart = new System.Windows.Forms.ComboBox();
            this.CHKShowAll = new System.Windows.Forms.CheckBox();
            this.BTNSetEntrance = new System.Windows.Forms.Button();
            this.BTNSetItem = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.TXTCheckedSearch = new System.Windows.Forms.TextBox();
            this.TXTEntSearch = new System.Windows.Forms.TextBox();
            this.TXTLocSearch = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolMenuStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblSwapPathfinder
            // 
            this.lblSwapPathfinder.AutoSize = true;
            this.lblSwapPathfinder.BackColor = System.Drawing.Color.Transparent;
            this.lblSwapPathfinder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblSwapPathfinder.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lblSwapPathfinder.Location = new System.Drawing.Point(123, 270);
            this.lblSwapPathfinder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSwapPathfinder.Name = "lblSwapPathfinder";
            this.lblSwapPathfinder.Size = new System.Drawing.Size(23, 16);
            this.lblSwapPathfinder.TabIndex = 42;
            this.lblSwapPathfinder.Text = "↑↓";
            // 
            // BTNFindPath
            // 
            this.BTNFindPath.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.BTNFindPath.Location = new System.Drawing.Point(13, 262);
            this.BTNFindPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTNFindPath.Name = "BTNFindPath";
            this.BTNFindPath.Size = new System.Drawing.Size(93, 23);
            this.BTNFindPath.TabIndex = 41;
            this.BTNFindPath.Text = "Find Path";
            this.BTNFindPath.UseVisualStyleBackColor = false;
            // 
            // LBPathFinder
            // 
            this.LBPathFinder.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.LBPathFinder.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.LBPathFinder.FormattingEnabled = true;
            this.LBPathFinder.IntegralHeight = false;
            this.LBPathFinder.Location = new System.Drawing.Point(123, 408);
            this.LBPathFinder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LBPathFinder.Name = "LBPathFinder";
            this.LBPathFinder.Size = new System.Drawing.Size(87, 109);
            this.LBPathFinder.TabIndex = 40;
            // 
            // LBCheckedLocations
            // 
            this.LBCheckedLocations.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.LBCheckedLocations.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.LBCheckedLocations.FormattingEnabled = true;
            this.LBCheckedLocations.IntegralHeight = false;
            this.LBCheckedLocations.Location = new System.Drawing.Point(13, 408);
            this.LBCheckedLocations.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LBCheckedLocations.Name = "LBCheckedLocations";
            this.LBCheckedLocations.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LBCheckedLocations.Size = new System.Drawing.Size(87, 109);
            this.LBCheckedLocations.TabIndex = 39;
            // 
            // LBValidEntrances
            // 
            this.LBValidEntrances.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.LBValidEntrances.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.LBValidEntrances.FormattingEnabled = true;
            this.LBValidEntrances.IntegralHeight = false;
            this.LBValidEntrances.Location = new System.Drawing.Point(123, 292);
            this.LBValidEntrances.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LBValidEntrances.Name = "LBValidEntrances";
            this.LBValidEntrances.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LBValidEntrances.Size = new System.Drawing.Size(87, 109);
            this.LBValidEntrances.TabIndex = 38;
            // 
            // LBValidLocations
            // 
            this.LBValidLocations.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.LBValidLocations.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.LBValidLocations.FormattingEnabled = true;
            this.LBValidLocations.IntegralHeight = false;
            this.LBValidLocations.Location = new System.Drawing.Point(13, 292);
            this.LBValidLocations.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LBValidLocations.Name = "LBValidLocations";
            this.LBValidLocations.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LBValidLocations.Size = new System.Drawing.Size(87, 109);
            this.LBValidLocations.TabIndex = 37;
            // 
            // CMBEnd
            // 
            this.CMBEnd.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CMBEnd.ForeColor = System.Drawing.SystemColors.InfoText;
            this.CMBEnd.FormattingEnabled = true;
            this.CMBEnd.Location = new System.Drawing.Point(13, 164);
            this.CMBEnd.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CMBEnd.Name = "CMBEnd";
            this.CMBEnd.Size = new System.Drawing.Size(116, 23);
            this.CMBEnd.TabIndex = 36;
            // 
            // CMBStart
            // 
            this.CMBStart.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CMBStart.ForeColor = System.Drawing.SystemColors.InfoText;
            this.CMBStart.FormattingEnabled = true;
            this.CMBStart.Location = new System.Drawing.Point(13, 132);
            this.CMBStart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CMBStart.Name = "CMBStart";
            this.CMBStart.Size = new System.Drawing.Size(116, 23);
            this.CMBStart.TabIndex = 35;
            // 
            // CHKShowAll
            // 
            this.CHKShowAll.AutoSize = true;
            this.CHKShowAll.BackColor = System.Drawing.Color.Transparent;
            this.CHKShowAll.ForeColor = System.Drawing.SystemColors.Control;
            this.CHKShowAll.Location = new System.Drawing.Point(140, 228);
            this.CHKShowAll.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CHKShowAll.Name = "CHKShowAll";
            this.CHKShowAll.Size = new System.Drawing.Size(72, 19);
            this.CHKShowAll.TabIndex = 34;
            this.CHKShowAll.Text = "Show All";
            this.CHKShowAll.UseVisualStyleBackColor = false;
            // 
            // BTNSetEntrance
            // 
            this.BTNSetEntrance.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.BTNSetEntrance.Location = new System.Drawing.Point(13, 228);
            this.BTNSetEntrance.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTNSetEntrance.Name = "BTNSetEntrance";
            this.BTNSetEntrance.Size = new System.Drawing.Size(93, 23);
            this.BTNSetEntrance.TabIndex = 33;
            this.BTNSetEntrance.Text = "Mark Entrance";
            this.BTNSetEntrance.UseVisualStyleBackColor = false;
            // 
            // BTNSetItem
            // 
            this.BTNSetItem.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.BTNSetItem.Location = new System.Drawing.Point(13, 195);
            this.BTNSetItem.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTNSetItem.Name = "BTNSetItem";
            this.BTNSetItem.Size = new System.Drawing.Size(93, 23);
            this.BTNSetItem.TabIndex = 32;
            this.BTNSetItem.Text = "Mark Item";
            this.BTNSetItem.UseVisualStyleBackColor = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.ForeColor = System.Drawing.SystemColors.Control;
            this.label6.Location = new System.Drawing.Point(137, 189);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 15);
            this.label6.TabIndex = 31;
            this.label6.Text = "Destination";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.ForeColor = System.Drawing.SystemColors.Control;
            this.label5.Location = new System.Drawing.Point(137, 162);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(31, 15);
            this.label5.TabIndex = 30;
            this.label5.Text = "Start";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.ForeColor = System.Drawing.SystemColors.Control;
            this.label4.Location = new System.Drawing.Point(137, 132);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 15);
            this.label4.TabIndex = 29;
            this.label4.Text = "Path Finder";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.SystemColors.Control;
            this.label3.Location = new System.Drawing.Point(137, 102);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(109, 15);
            this.label3.TabIndex = 28;
            this.label3.Text = "Available Entrances";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.SystemColors.Control;
            this.label2.Location = new System.Drawing.Point(137, 72);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(104, 15);
            this.label2.TabIndex = 27;
            this.label2.Text = "Checked locations";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.SystemColors.Control;
            this.label1.Location = new System.Drawing.Point(137, 42);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 15);
            this.label1.TabIndex = 26;
            this.label1.Text = "Available Locations";
            // 
            // TXTCheckedSearch
            // 
            this.TXTCheckedSearch.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.TXTCheckedSearch.Location = new System.Drawing.Point(13, 99);
            this.TXTCheckedSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TXTCheckedSearch.Name = "TXTCheckedSearch";
            this.TXTCheckedSearch.Size = new System.Drawing.Size(116, 23);
            this.TXTCheckedSearch.TabIndex = 25;
            // 
            // TXTEntSearch
            // 
            this.TXTEntSearch.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.TXTEntSearch.Location = new System.Drawing.Point(13, 69);
            this.TXTEntSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TXTEntSearch.Name = "TXTEntSearch";
            this.TXTEntSearch.Size = new System.Drawing.Size(116, 23);
            this.TXTEntSearch.TabIndex = 24;
            // 
            // TXTLocSearch
            // 
            this.TXTLocSearch.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.TXTLocSearch.Location = new System.Drawing.Point(13, 39);
            this.TXTLocSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TXTLocSearch.Name = "TXTLocSearch";
            this.TXTLocSearch.Size = new System.Drawing.Size(116, 23);
            this.TXTLocSearch.TabIndex = 23;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolMenuStrip});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(541, 24);
            this.menuStrip1.TabIndex = 43;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip1_ItemClicked);
            // 
            // fileToolMenuStrip
            // 
            this.fileToolMenuStrip.Name = "fileToolMenuStrip";
            this.fileToolMenuStrip.Size = new System.Drawing.Size(37, 20);
            this.fileToolMenuStrip.Text = "File";
            // 
            // MainInterface
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(541, 591);
            this.Controls.Add(this.lblSwapPathfinder);
            this.Controls.Add(this.BTNFindPath);
            this.Controls.Add(this.LBPathFinder);
            this.Controls.Add(this.LBCheckedLocations);
            this.Controls.Add(this.LBValidEntrances);
            this.Controls.Add(this.LBValidLocations);
            this.Controls.Add(this.CMBEnd);
            this.Controls.Add(this.CMBStart);
            this.Controls.Add(this.CHKShowAll);
            this.Controls.Add(this.BTNSetEntrance);
            this.Controls.Add(this.BTNSetItem);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TXTCheckedSearch);
            this.Controls.Add(this.TXTEntSearch);
            this.Controls.Add(this.TXTLocSearch);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainInterface";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MainInterface_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblSwapPathfinder;
        private System.Windows.Forms.Button BTNFindPath;
        public System.Windows.Forms.ListBox LBPathFinder;
        public System.Windows.Forms.ListBox LBCheckedLocations;
        public System.Windows.Forms.ListBox LBValidEntrances;
        public System.Windows.Forms.ListBox LBValidLocations;
        private System.Windows.Forms.ComboBox CMBEnd;
        private System.Windows.Forms.ComboBox CMBStart;
        private System.Windows.Forms.CheckBox CHKShowAll;
        private System.Windows.Forms.Button BTNSetEntrance;
        private System.Windows.Forms.Button BTNSetItem;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox TXTCheckedSearch;
        public System.Windows.Forms.TextBox TXTEntSearch;
        public System.Windows.Forms.TextBox TXTLocSearch;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolMenuStrip;
    }
}

