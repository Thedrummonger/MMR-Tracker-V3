namespace TestingForm
{
    partial class NetClient
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetClient));
            this.txtServerAddress = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.nudPort = new System.Windows.Forms.NumericUpDown();
            this.btnConnect = new System.Windows.Forms.Button();
            this.chkSendData = new System.Windows.Forms.CheckBox();
            this.chkRecieveData = new System.Windows.Forms.CheckBox();
            this.lbConsole = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtChatMessage = new System.Windows.Forms.TextBox();
            this.btnSendChat = new System.Windows.Forms.Button();
            this.nudPlayer = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.chkProcessData = new System.Windows.Forms.CheckBox();
            this.btnProcessData = new System.Windows.Forms.Button();
            this.chkShowPass = new System.Windows.Forms.CheckBox();
            this.chkAllowCheck = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nudPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPlayer)).BeginInit();
            this.SuspendLayout();
            // 
            // txtServerAddress
            // 
            this.txtServerAddress.Location = new System.Drawing.Point(11, 26);
            this.txtServerAddress.Name = "txtServerAddress";
            this.txtServerAddress.Size = new System.Drawing.Size(118, 23);
            this.txtServerAddress.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Server Address:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(136, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Port:";
            // 
            // nudPort
            // 
            this.nudPort.Location = new System.Drawing.Point(136, 27);
            this.nudPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudPort.Name = "nudPort";
            this.nudPort.Size = new System.Drawing.Size(72, 23);
            this.nudPort.TabIndex = 5;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(11, 99);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(107, 23);
            this.btnConnect.TabIndex = 6;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // chkSendData
            // 
            this.chkSendData.AutoSize = true;
            this.chkSendData.Checked = true;
            this.chkSendData.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSendData.Location = new System.Drawing.Point(11, 154);
            this.chkSendData.Name = "chkSendData";
            this.chkSendData.Size = new System.Drawing.Size(126, 19);
            this.chkSendData.TabIndex = 7;
            this.chkSendData.Text = "Send data to server";
            this.chkSendData.UseVisualStyleBackColor = true;
            this.chkSendData.CheckedChanged += new System.EventHandler(this.chkOption_CheckedChanged);
            // 
            // chkRecieveData
            // 
            this.chkRecieveData.AutoSize = true;
            this.chkRecieveData.Checked = true;
            this.chkRecieveData.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRecieveData.Location = new System.Drawing.Point(11, 129);
            this.chkRecieveData.Name = "chkRecieveData";
            this.chkRecieveData.Size = new System.Drawing.Size(155, 19);
            this.chkRecieveData.TabIndex = 8;
            this.chkRecieveData.Text = "Process data from server";
            this.chkRecieveData.UseVisualStyleBackColor = true;
            this.chkRecieveData.CheckedChanged += new System.EventHandler(this.chkOption_CheckedChanged);
            // 
            // lbConsole
            // 
            this.lbConsole.FormattingEnabled = true;
            this.lbConsole.HorizontalScrollbar = true;
            this.lbConsole.ItemHeight = 15;
            this.lbConsole.Location = new System.Drawing.Point(214, 27);
            this.lbConsole.Name = "lbConsole";
            this.lbConsole.Size = new System.Drawing.Size(200, 169);
            this.lbConsole.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(214, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 15);
            this.label4.TabIndex = 10;
            this.label4.Text = "Server Chat";
            // 
            // txtChatMessage
            // 
            this.txtChatMessage.Location = new System.Drawing.Point(214, 197);
            this.txtChatMessage.Name = "txtChatMessage";
            this.txtChatMessage.Size = new System.Drawing.Size(174, 23);
            this.txtChatMessage.TabIndex = 11;
            // 
            // btnSendChat
            // 
            this.btnSendChat.Image = ((System.Drawing.Image)(resources.GetObject("btnSendChat.Image")));
            this.btnSendChat.Location = new System.Drawing.Point(389, 196);
            this.btnSendChat.Name = "btnSendChat";
            this.btnSendChat.Size = new System.Drawing.Size(26, 25);
            this.btnSendChat.TabIndex = 12;
            this.btnSendChat.UseVisualStyleBackColor = true;
            this.btnSendChat.Click += new System.EventHandler(this.btnSendChat_Click);
            // 
            // nudPlayer
            // 
            this.nudPlayer.Location = new System.Drawing.Point(11, 71);
            this.nudPlayer.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudPlayer.Name = "nudPlayer";
            this.nudPlayer.Size = new System.Drawing.Size(49, 23);
            this.nudPlayer.TabIndex = 13;
            this.nudPlayer.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 53);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 15);
            this.label5.TabIndex = 14;
            this.label5.Text = "Player ID";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(66, 71);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(142, 23);
            this.txtPassword.TabIndex = 15;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(66, 53);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(57, 15);
            this.label6.TabIndex = 16;
            this.label6.Text = "Password";
            // 
            // chkProcessData
            // 
            this.chkProcessData.AutoSize = true;
            this.chkProcessData.Location = new System.Drawing.Point(11, 204);
            this.chkProcessData.Name = "chkProcessData";
            this.chkProcessData.Size = new System.Drawing.Size(169, 19);
            this.chkProcessData.TabIndex = 17;
            this.chkProcessData.Text = "Process data Automatically";
            this.chkProcessData.UseVisualStyleBackColor = true;
            this.chkProcessData.CheckedChanged += new System.EventHandler(this.chkOption_CheckedChanged);
            // 
            // btnProcessData
            // 
            this.btnProcessData.Location = new System.Drawing.Point(124, 100);
            this.btnProcessData.Name = "btnProcessData";
            this.btnProcessData.Size = new System.Drawing.Size(84, 23);
            this.btnProcessData.TabIndex = 18;
            this.btnProcessData.Text = "Process Data";
            this.btnProcessData.UseVisualStyleBackColor = true;
            this.btnProcessData.Click += new System.EventHandler(this.btnProcessData_Click);
            // 
            // chkShowPass
            // 
            this.chkShowPass.AutoSize = true;
            this.chkShowPass.Location = new System.Drawing.Point(153, 52);
            this.chkShowPass.Name = "chkShowPass";
            this.chkShowPass.Size = new System.Drawing.Size(55, 19);
            this.chkShowPass.TabIndex = 19;
            this.chkShowPass.Text = "Show";
            this.chkShowPass.UseVisualStyleBackColor = true;
            this.chkShowPass.CheckedChanged += new System.EventHandler(this.chkOption_CheckedChanged);
            // 
            // chkAllowCheck
            // 
            this.chkAllowCheck.AutoSize = true;
            this.chkAllowCheck.Location = new System.Drawing.Point(11, 179);
            this.chkAllowCheck.Name = "chkAllowCheck";
            this.chkAllowCheck.Size = new System.Drawing.Size(163, 19);
            this.chkAllowCheck.TabIndex = 20;
            this.chkAllowCheck.Text = "Allow Checking Locations";
            this.chkAllowCheck.UseVisualStyleBackColor = true;
            this.chkAllowCheck.CheckedChanged += new System.EventHandler(this.chkOption_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(196, 130);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(12, 15);
            this.label1.TabIndex = 21;
            this.label1.Text = "?";
            this.toolTip1.SetToolTip(this.label1, "When unchecked, any data sent by the server will be ignored.");
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label7.Location = new System.Drawing.Point(196, 155);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(12, 15);
            this.label7.TabIndex = 22;
            this.label7.Text = "?";
            this.toolTip1.SetToolTip(this.label7, "When unchecked, your client will not send any data to the server.");
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.label8.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label8.Location = new System.Drawing.Point(196, 180);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(12, 15);
            this.label8.TabIndex = 23;
            this.label8.Text = "?";
            this.toolTip1.SetToolTip(this.label8, "CO-OP ONLY: When unchecked, location checked by other users will only be marked o" +
        "n your client.");
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.label9.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label9.Location = new System.Drawing.Point(196, 205);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(12, 15);
            this.label9.TabIndex = 24;
            this.label9.Text = "?";
            this.toolTip1.SetToolTip(this.label9, "When Unchecked, data recieved from the server will not be processed until the \"Pr" +
        "ocess Data\" button is pressed.");
            // 
            // NetClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(423, 228);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkAllowCheck);
            this.Controls.Add(this.chkShowPass);
            this.Controls.Add(this.btnProcessData);
            this.Controls.Add(this.chkProcessData);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.nudPlayer);
            this.Controls.Add(this.btnSendChat);
            this.Controls.Add(this.txtChatMessage);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lbConsole);
            this.Controls.Add(this.chkRecieveData);
            this.Controls.Add(this.chkSendData);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.nudPort);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtServerAddress);
            this.KeyPreview = true;
            this.Name = "NetClient";
            this.Text = "NetClient";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NetClient_FormClosing);
            this.Load += new System.EventHandler(this.NetClient_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NetClient_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.nudPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPlayer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private TextBox txtServerAddress;
        private Label label2;
        private Label label3;
        private NumericUpDown nudPort;
        private Button btnConnect;
        private CheckBox chkSendData;
        private CheckBox chkRecieveData;
        private ListBox lbConsole;
        private Label label4;
        private TextBox txtChatMessage;
        private Button btnSendChat;
        private NumericUpDown nudPlayer;
        private Label label5;
        private TextBox txtPassword;
        private Label label6;
        private CheckBox chkProcessData;
        private Button btnProcessData;
        private CheckBox chkShowPass;
        private CheckBox chkAllowCheck;
        private Label label1;
        private Label label7;
        private Label label8;
        private Label label9;
        private ToolTip toolTip1;
    }
}