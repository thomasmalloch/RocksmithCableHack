namespace RocksmithCableHack;

partial class FormSettings
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSettings));
        this.chkAutoHack = new System.Windows.Forms.CheckBox();
        this.numDelay = new System.Windows.Forms.NumericUpDown();
        this.lblDevices = new System.Windows.Forms.Label();
        this.cboDevices = new System.Windows.Forms.ComboBox();
        this.lblDelay = new System.Windows.Forms.Label();
        this.btnRefresh = new System.Windows.Forms.Button();
        this.chkDetect = new System.Windows.Forms.CheckBox();
        this.chkStartWithWindows = new System.Windows.Forms.CheckBox();
        this.label1 = new System.Windows.Forms.Label();
        this.btnOK = new System.Windows.Forms.Button();
        this.chkNotifications = new System.Windows.Forms.CheckBox();
        ((System.ComponentModel.ISupportInitialize)this.numDelay).BeginInit();
        this.SuspendLayout();
        // 
        // chkAutoHack
        // 
        this.chkAutoHack.Location = new System.Drawing.Point(16, 184);
        this.chkAutoHack.Name = "chkAutoHack";
        this.chkAutoHack.Size = new System.Drawing.Size(400, 30);
        this.chkAutoHack.TabIndex = 6;
        this.chkAutoHack.Text = "Hack Rocksmith on detection";
        this.chkAutoHack.UseVisualStyleBackColor = true;
        this.chkAutoHack.Checked = true;
        this.chkAutoHack.CheckedChanged += this.chkAutoHack_CheckedChanged;
        // 
        // numDelay
        // 
        this.numDelay.Location = new System.Drawing.Point(16, 256);
        this.numDelay.Maximum = new decimal(new int[] { 30000, 0, 0, 0 });
        this.numDelay.Name = "numDelay";
        this.numDelay.Size = new System.Drawing.Size(150, 27);
        this.numDelay.TabIndex = 8;
        this.numDelay.Value = 5000;
        this.numDelay.ValueChanged += this.numDelay_ValueChanged;
        // 
        // lblDevices
        // 
        this.lblDevices.Location = new System.Drawing.Point(16, 16);
        this.lblDevices.Name = "lblDevices";
        this.lblDevices.Size = new System.Drawing.Size(400, 25);
        this.lblDevices.TabIndex = 0;
        this.lblDevices.Text = "USB Audio Input Devices";
        this.lblDevices.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // cboDevices
        // 
        this.cboDevices.FormattingEnabled = true;
        this.cboDevices.Location = new System.Drawing.Point(16, 48);
        this.cboDevices.Name = "cboDevices";
        this.cboDevices.Size = new System.Drawing.Size(296, 28);
        this.cboDevices.TabIndex = 1;
        this.cboDevices.SelectionChangeCommitted += this.cboDevices_SelectionChangeCommitted;
        // 
        // lblDelay
        // 
        this.lblDelay.Location = new System.Drawing.Point(16, 224);
        this.lblDelay.Name = "lblDelay";
        this.lblDelay.Size = new System.Drawing.Size(400, 25);
        this.lblDelay.TabIndex = 7;
        this.lblDelay.Text = "Delay before automatically hacking:";
        this.lblDelay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // btnRefresh
        // 
        this.btnRefresh.Location = new System.Drawing.Point(320, 48);
        this.btnRefresh.Name = "btnRefresh";
        this.btnRefresh.Size = new System.Drawing.Size(94, 29);
        this.btnRefresh.TabIndex = 2;
        this.btnRefresh.Text = "Refresh";
        this.btnRefresh.UseVisualStyleBackColor = true;
        this.btnRefresh.Click += this.btnRefresh_Click;
        // 
        // chkDetect
        // 
        this.chkDetect.Location = new System.Drawing.Point(16, 152);
        this.chkDetect.Name = "chkDetect";
        this.chkDetect.Size = new System.Drawing.Size(400, 30);
        this.chkDetect.TabIndex = 5;
        this.chkDetect.Text = "Detect Rocksmith";
        this.chkDetect.UseVisualStyleBackColor = true;
        this.chkDetect.Checked = true;
        this.chkDetect.CheckedChanged += this.chkDetect_CheckedChanged;
        // 
        // chkStartWithWindows
        // 
        this.chkStartWithWindows.Location = new System.Drawing.Point(16, 88);
        this.chkStartWithWindows.Name = "chkStartWithWindows";
        this.chkStartWithWindows.Size = new System.Drawing.Size(400, 30);
        this.chkStartWithWindows.TabIndex = 3;
        this.chkStartWithWindows.Text = "Start with Windows";
        this.chkStartWithWindows.UseVisualStyleBackColor = true;
        this.chkStartWithWindows.CheckedChanged += this.chkStartWithWindows_CheckedChanged;
        // 
        // label1
        // 
        this.label1.Location = new System.Drawing.Point(168, 256);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(248, 25);
        this.label1.TabIndex = 9;
        this.label1.Text = "milliseconds";
        this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // btnOK
        // 
        this.btnOK.Location = new System.Drawing.Point(320, 336);
        this.btnOK.Name = "btnOK";
        this.btnOK.Size = new System.Drawing.Size(94, 29);
        this.btnOK.TabIndex = 10;
        this.btnOK.Text = "OK";
        this.btnOK.UseVisualStyleBackColor = true;
        this.btnOK.Click += this.btnOK_Click;
        // 
        // chkNotifications
        // 
        this.chkNotifications.Location = new System.Drawing.Point(16, 120);
        this.chkNotifications.Name = "chkNotifications";
        this.chkNotifications.Size = new System.Drawing.Size(400, 30);
        this.chkNotifications.TabIndex = 4;
        this.chkNotifications.Text = "Show tray notifications";
        this.chkNotifications.UseVisualStyleBackColor = true;
        this.chkNotifications.Checked = true;
        this.chkNotifications.CheckedChanged += this.chkNotifications_CheckedChanged;
        // 
        // FormSettings
        // 
        this.AcceptButton = this.btnOK;
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.btnOK;
        this.ClientSize = new System.Drawing.Size(427, 379);
        this.Controls.Add(this.chkNotifications);
        this.Controls.Add(this.btnOK);
        this.Controls.Add(this.label1);
        this.Controls.Add(this.chkStartWithWindows);
        this.Controls.Add(this.chkDetect);
        this.Controls.Add(this.btnRefresh);
        this.Controls.Add(this.lblDelay);
        this.Controls.Add(this.cboDevices);
        this.Controls.Add(this.lblDevices);
        this.Controls.Add(this.numDelay);
        this.Controls.Add(this.chkAutoHack);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
        this.MaximizeBox = false;
        this.Name = "FormSettings";
        this.Text = "Settings";
        ((System.ComponentModel.ISupportInitialize)this.numDelay).EndInit();
        this.ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.NumericUpDown numDelay;
    private System.Windows.Forms.Label lblDelay;
    private System.Windows.Forms.ComboBox cboDevices;
    private System.Windows.Forms.Label lblDevices;
    private System.Windows.Forms.Button btnRefresh;
    private System.Windows.Forms.CheckBox chkDetect;
    private System.Windows.Forms.CheckBox chkAutoHack;
    private System.Windows.Forms.CheckBox chkStartWithWindows;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.CheckBox chkNotifications;
}