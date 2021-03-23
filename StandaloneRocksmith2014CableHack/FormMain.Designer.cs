namespace StandaloneRocksmith2014CableHack
{
	partial class FormMain
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
			this.cboAudioDevices = new System.Windows.Forms.ComboBox();
			this.btnRefreshDevices = new System.Windows.Forms.Button();
			this.chkAuto = new System.Windows.Forms.CheckBox();
			this.chkWatchRocksmith = new System.Windows.Forms.CheckBox();
			this.btnHack = new System.Windows.Forms.Button();
			this.txtStatus = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// cboAudioDevices
			// 
			this.cboAudioDevices.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboAudioDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboAudioDevices.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cboAudioDevices.FormattingEnabled = true;
			this.cboAudioDevices.Location = new System.Drawing.Point(16, 256);
			this.cboAudioDevices.Name = "cboAudioDevices";
			this.cboAudioDevices.Size = new System.Drawing.Size(312, 21);
			this.cboAudioDevices.TabIndex = 0;
			this.cboAudioDevices.SelectionChangeCommitted += new System.EventHandler(this.cboAudioDevices_SelectionChangeCommitted);
			// 
			// btnRefreshDevices
			// 
			this.btnRefreshDevices.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnRefreshDevices.Location = new System.Drawing.Point(336, 256);
			this.btnRefreshDevices.Name = "btnRefreshDevices";
			this.btnRefreshDevices.Size = new System.Drawing.Size(80, 24);
			this.btnRefreshDevices.TabIndex = 2;
			this.btnRefreshDevices.Text = "Refresh";
			this.btnRefreshDevices.UseVisualStyleBackColor = true;
			this.btnRefreshDevices.Click += new System.EventHandler(this.btnRefreshDevices_Click);
			// 
			// chkAuto
			// 
			this.chkAuto.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.chkAuto.AutoSize = true;
			this.chkAuto.Location = new System.Drawing.Point(16, 312);
			this.chkAuto.Name = "chkAuto";
			this.chkAuto.Size = new System.Drawing.Size(286, 17);
			this.chkAuto.TabIndex = 4;
			this.chkAuto.Text = "Automatically hack Rocksmith with the selected device";
			this.chkAuto.UseVisualStyleBackColor = true;
			this.chkAuto.CheckedChanged += new System.EventHandler(this.chkAuto_CheckedChanged);
			// 
			// chkWatchRocksmith
			// 
			this.chkWatchRocksmith.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.chkWatchRocksmith.AutoSize = true;
			this.chkWatchRocksmith.Location = new System.Drawing.Point(16, 288);
			this.chkWatchRocksmith.Name = "chkWatchRocksmith";
			this.chkWatchRocksmith.Size = new System.Drawing.Size(170, 17);
			this.chkWatchRocksmith.TabIndex = 5;
			this.chkWatchRocksmith.Text = "Listen for Rocksmith start/stop";
			this.chkWatchRocksmith.UseVisualStyleBackColor = true;
			this.chkWatchRocksmith.CheckedChanged += new System.EventHandler(this.chkWatchRocksmith_CheckedChanged);
			// 
			// btnHack
			// 
			this.btnHack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnHack.Enabled = false;
			this.btnHack.Location = new System.Drawing.Point(336, 288);
			this.btnHack.Name = "btnHack";
			this.btnHack.Size = new System.Drawing.Size(80, 48);
			this.btnHack.TabIndex = 6;
			this.btnHack.Text = "Hack";
			this.btnHack.UseVisualStyleBackColor = true;
			this.btnHack.Click += new System.EventHandler(this.btnHack_Click);
			// 
			// txtStatus
			// 
			this.txtStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtStatus.BackColor = System.Drawing.SystemColors.WindowText;
			this.txtStatus.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtStatus.ForeColor = System.Drawing.SystemColors.Window;
			this.txtStatus.Location = new System.Drawing.Point(16, 16);
			this.txtStatus.Name = "txtStatus";
			this.txtStatus.ReadOnly = true;
			this.txtStatus.Size = new System.Drawing.Size(400, 232);
			this.txtStatus.TabIndex = 7;
			this.txtStatus.Text = "";
			// 
			// FormMain
			// 
			this.AcceptButton = this.btnHack;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(433, 348);
			this.Controls.Add(this.txtStatus);
			this.Controls.Add(this.btnHack);
			this.Controls.Add(this.chkWatchRocksmith);
			this.Controls.Add(this.chkAuto);
			this.Controls.Add(this.btnRefreshDevices);
			this.Controls.Add(this.cboAudioDevices);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "FormMain";
			this.Text = "Cable Hack";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox cboAudioDevices;
		private System.Windows.Forms.Button btnRefreshDevices;
		private System.Windows.Forms.CheckBox chkAuto;
		private System.Windows.Forms.CheckBox chkWatchRocksmith;
		private System.Windows.Forms.Button btnHack;
		private System.Windows.Forms.RichTextBox txtStatus;
	}
}

