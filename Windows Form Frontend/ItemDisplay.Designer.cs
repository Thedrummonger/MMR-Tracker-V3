namespace Windows_Form_Frontend
{
    partial class ItemDisplay
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ItemDisplay));
            this.SuspendLayout();
            // 
            // ItemDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(233, 181);
            this.Name = "ItemDisplay";
            this.Text = "ItemDisplay";
            this.Load += new System.EventHandler(this.ItemDisplay_Load);
            this.Shown += new System.EventHandler(this.ItemDisplay_Shown);
            this.ResizeBegin += new System.EventHandler(this.ItemDisplay_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.ItemDisplay_ResizeEnd);
            this.ResumeLayout(false);

        }

        #endregion
    }
}