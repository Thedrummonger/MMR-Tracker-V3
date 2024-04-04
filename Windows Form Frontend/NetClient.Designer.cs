using FParsec;
using System.Windows.Forms;

namespace Windows_Form_Frontend
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetClient));
            txtServerAddress = new TextBox();
            label2 = new Label();
            label3 = new Label();
            nudPort = new NumericUpDown();
            btnConnect = new Button();
            chkSendData = new CheckBox();
            chkRecieveData = new CheckBox();
            lbConsole = new ListBox();
            label4 = new Label();
            txtChatMessage = new TextBox();
            btnSendChat = new Button();
            nudPlayer = new NumericUpDown();
            label5 = new Label();
            txtPassword = new TextBox();
            label6 = new Label();
            chkProcessData = new CheckBox();
            btnProcessData = new Button();
            chkShowPass = new CheckBox();
            chkAllowCheck = new CheckBox();
            label1 = new Label();
            label7 = new Label();
            label8 = new Label();
            label9 = new Label();
            toolTip1 = new ToolTip(components);
            txtGameName = new TextBox();
            txtSlotID = new TextBox();
            label10 = new Label();
            label11 = new Label();
            cmbGameType = new ComboBox();
            label12 = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            panel1 = new Panel();
            ((System.ComponentModel.ISupportInitialize)nudPort).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudPlayer).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // txtServerAddress
            // 
            txtServerAddress.Location = new System.Drawing.Point(3, 17);
            txtServerAddress.Name = "txtServerAddress";
            txtServerAddress.Size = new System.Drawing.Size(118, 23);
            txtServerAddress.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(3, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(87, 15);
            label2.TabIndex = 3;
            label2.Text = "Server Address:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(128, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(32, 15);
            label3.TabIndex = 4;
            label3.Text = "Port:";
            // 
            // nudPort
            // 
            nudPort.Location = new System.Drawing.Point(128, 18);
            nudPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            nudPort.Name = "nudPort";
            nudPort.Size = new System.Drawing.Size(72, 23);
            nudPort.TabIndex = 5;
            // 
            // btnConnect
            // 
            btnConnect.Location = new System.Drawing.Point(4, 156);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new System.Drawing.Size(107, 23);
            btnConnect.TabIndex = 6;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // chkSendData
            // 
            chkSendData.AutoSize = true;
            chkSendData.Checked = true;
            chkSendData.CheckState = CheckState.Checked;
            chkSendData.Location = new System.Drawing.Point(4, 210);
            chkSendData.Name = "chkSendData";
            chkSendData.Size = new System.Drawing.Size(126, 19);
            chkSendData.TabIndex = 7;
            chkSendData.Text = "Send data to server";
            chkSendData.UseVisualStyleBackColor = true;
            chkSendData.CheckedChanged += chkOption_CheckedChanged;
            // 
            // chkRecieveData
            // 
            chkRecieveData.AutoSize = true;
            chkRecieveData.Checked = true;
            chkRecieveData.CheckState = CheckState.Checked;
            chkRecieveData.Location = new System.Drawing.Point(4, 185);
            chkRecieveData.Name = "chkRecieveData";
            chkRecieveData.Size = new System.Drawing.Size(155, 19);
            chkRecieveData.TabIndex = 8;
            chkRecieveData.Text = "Process data from server";
            chkRecieveData.UseVisualStyleBackColor = true;
            chkRecieveData.CheckedChanged += chkOption_CheckedChanged;
            // 
            // lbConsole
            // 
            tableLayoutPanel1.SetColumnSpan(lbConsole, 2);
            lbConsole.Dock = DockStyle.Fill;
            lbConsole.FormattingEnabled = true;
            lbConsole.HorizontalScrollbar = true;
            lbConsole.ItemHeight = 15;
            lbConsole.Location = new System.Drawing.Point(218, 18);
            lbConsole.Name = "lbConsole";
            lbConsole.Size = new System.Drawing.Size(250, 239);
            lbConsole.TabIndex = 9;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(218, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(67, 15);
            label4.TabIndex = 10;
            label4.Text = "Server Chat";
            // 
            // txtChatMessage
            // 
            txtChatMessage.Dock = DockStyle.Fill;
            txtChatMessage.Location = new System.Drawing.Point(218, 263);
            txtChatMessage.Name = "txtChatMessage";
            txtChatMessage.Size = new System.Drawing.Size(220, 23);
            txtChatMessage.TabIndex = 11;
            // 
            // btnSendChat
            // 
            btnSendChat.Dock = DockStyle.Fill;
            btnSendChat.Image = (System.Drawing.Image)resources.GetObject("btnSendChat.Image");
            btnSendChat.Location = new System.Drawing.Point(444, 263);
            btnSendChat.Name = "btnSendChat";
            btnSendChat.Size = new System.Drawing.Size(24, 24);
            btnSendChat.TabIndex = 12;
            btnSendChat.UseVisualStyleBackColor = true;
            btnSendChat.Click += btnSendChat_Click;
            // 
            // nudPlayer
            // 
            nudPlayer.Location = new System.Drawing.Point(3, 87);
            nudPlayer.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudPlayer.Name = "nudPlayer";
            nudPlayer.Size = new System.Drawing.Size(49, 23);
            nudPlayer.TabIndex = 13;
            nudPlayer.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(3, 69);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(53, 15);
            label5.TabIndex = 14;
            label5.Text = "Player ID";
            // 
            // txtPassword
            // 
            txtPassword.Location = new System.Drawing.Point(58, 87);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new System.Drawing.Size(142, 23);
            txtPassword.TabIndex = 15;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(58, 69);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(57, 15);
            label6.TabIndex = 16;
            label6.Text = "Password";
            // 
            // chkProcessData
            // 
            chkProcessData.AutoSize = true;
            chkProcessData.Location = new System.Drawing.Point(4, 260);
            chkProcessData.Name = "chkProcessData";
            chkProcessData.Size = new System.Drawing.Size(169, 19);
            chkProcessData.TabIndex = 17;
            chkProcessData.Text = "Process data Automatically";
            chkProcessData.UseVisualStyleBackColor = true;
            chkProcessData.CheckedChanged += chkOption_CheckedChanged;
            // 
            // btnProcessData
            // 
            btnProcessData.Location = new System.Drawing.Point(117, 157);
            btnProcessData.Name = "btnProcessData";
            btnProcessData.Size = new System.Drawing.Size(84, 23);
            btnProcessData.TabIndex = 18;
            btnProcessData.Text = "Process Data";
            btnProcessData.UseVisualStyleBackColor = true;
            btnProcessData.Click += btnProcessData_Click;
            // 
            // chkShowPass
            // 
            chkShowPass.AutoSize = true;
            chkShowPass.Location = new System.Drawing.Point(145, 68);
            chkShowPass.Name = "chkShowPass";
            chkShowPass.Size = new System.Drawing.Size(55, 19);
            chkShowPass.TabIndex = 19;
            chkShowPass.Text = "Show";
            chkShowPass.UseVisualStyleBackColor = true;
            chkShowPass.CheckedChanged += chkOption_CheckedChanged;
            // 
            // chkAllowCheck
            // 
            chkAllowCheck.AutoSize = true;
            chkAllowCheck.Location = new System.Drawing.Point(4, 235);
            chkAllowCheck.Name = "chkAllowCheck";
            chkAllowCheck.Size = new System.Drawing.Size(163, 19);
            chkAllowCheck.TabIndex = 20;
            chkAllowCheck.Text = "Allow Checking Locations";
            chkAllowCheck.UseVisualStyleBackColor = true;
            chkAllowCheck.CheckedChanged += chkOption_CheckedChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = System.Drawing.SystemColors.AppWorkspace;
            label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            label1.Location = new System.Drawing.Point(189, 186);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(12, 15);
            label1.TabIndex = 21;
            label1.Text = "?";
            toolTip1.SetToolTip(label1, "When unchecked, any data sent by the server will be ignored.");
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.BackColor = System.Drawing.SystemColors.AppWorkspace;
            label7.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            label7.Location = new System.Drawing.Point(189, 211);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(12, 15);
            label7.TabIndex = 22;
            label7.Text = "?";
            toolTip1.SetToolTip(label7, "When unchecked, your client will not send any data to the server.");
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.BackColor = System.Drawing.SystemColors.AppWorkspace;
            label8.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            label8.Location = new System.Drawing.Point(189, 236);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(12, 15);
            label8.TabIndex = 23;
            label8.Text = "?";
            toolTip1.SetToolTip(label8, "CO-OP ONLY: When unchecked, location checked by other users will only be marked on your client.");
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.BackColor = System.Drawing.SystemColors.AppWorkspace;
            label9.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            label9.Location = new System.Drawing.Point(189, 261);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(12, 15);
            label9.TabIndex = 24;
            label9.Text = "?";
            toolTip1.SetToolTip(label9, "When Unchecked, data recieved from the server will not be processed until the \"Process Data\" button is pressed.");
            // 
            // txtGameName
            // 
            txtGameName.Location = new System.Drawing.Point(100, 128);
            txtGameName.Name = "txtGameName";
            txtGameName.Size = new System.Drawing.Size(101, 23);
            txtGameName.TabIndex = 25;
            // 
            // txtSlotID
            // 
            txtSlotID.Location = new System.Drawing.Point(4, 127);
            txtSlotID.Name = "txtSlotID";
            txtSlotID.Size = new System.Drawing.Size(90, 23);
            txtSlotID.TabIndex = 26;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(100, 113);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(73, 15);
            label10.TabIndex = 27;
            label10.Text = "Game Name";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(4, 113);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(41, 15);
            label11.TabIndex = 28;
            label11.Text = "Slot ID";
            // 
            // cmbGameType
            // 
            cmbGameType.FormattingEnabled = true;
            cmbGameType.Location = new System.Drawing.Point(75, 43);
            cmbGameType.Name = "cmbGameType";
            cmbGameType.Size = new System.Drawing.Size(126, 23);
            cmbGameType.TabIndex = 29;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new System.Drawing.Point(4, 46);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(65, 15);
            label12.TabIndex = 30;
            label12.Text = "Game Type";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 215F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30F));
            tableLayoutPanel1.Controls.Add(txtChatMessage, 1, 2);
            tableLayoutPanel1.Controls.Add(btnSendChat, 2, 2);
            tableLayoutPanel1.Controls.Add(lbConsole, 1, 1);
            tableLayoutPanel1.Controls.Add(label4, 1, 0);
            tableLayoutPanel1.Controls.Add(panel1, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 15F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel1.Size = new System.Drawing.Size(471, 290);
            tableLayoutPanel1.TabIndex = 31;
            // 
            // panel1
            // 
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label12);
            panel1.Controls.Add(txtServerAddress);
            panel1.Controls.Add(cmbGameType);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(label11);
            panel1.Controls.Add(nudPort);
            panel1.Controls.Add(label10);
            panel1.Controls.Add(btnConnect);
            panel1.Controls.Add(txtSlotID);
            panel1.Controls.Add(chkSendData);
            panel1.Controls.Add(txtGameName);
            panel1.Controls.Add(chkRecieveData);
            panel1.Controls.Add(label9);
            panel1.Controls.Add(nudPlayer);
            panel1.Controls.Add(label8);
            panel1.Controls.Add(label5);
            panel1.Controls.Add(label7);
            panel1.Controls.Add(txtPassword);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(label6);
            panel1.Controls.Add(chkAllowCheck);
            panel1.Controls.Add(chkProcessData);
            panel1.Controls.Add(chkShowPass);
            panel1.Controls.Add(btnProcessData);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(3, 3);
            panel1.Name = "panel1";
            tableLayoutPanel1.SetRowSpan(panel1, 3);
            panel1.Size = new System.Drawing.Size(209, 284);
            panel1.TabIndex = 13;
            // 
            // NetClient
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(471, 290);
            Controls.Add(tableLayoutPanel1);
            KeyPreview = true;
            Name = "NetClient";
            Text = "NetClient";
            FormClosing += NetClient_FormClosing;
            Load += NetClient_Load;
            KeyDown += NetClient_KeyDown;
            ((System.ComponentModel.ISupportInitialize)nudPort).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudPlayer).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
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
        private TextBox txtGameName;
        private TextBox txtSlotID;
        private Label label10;
        private Label label11;
        private ComboBox cmbGameType;
        private Label label12;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
    }
}