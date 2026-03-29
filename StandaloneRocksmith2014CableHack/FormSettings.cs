using System;
using System.Management;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace RocksmithCableHack;


[SupportedOSPlatform("windows")]
public partial class FormSettings : Form
{
    private readonly AppSettings Settings;
    private readonly TrayApplicationContext Context;
    private ManagementEventWatcher? USBWatcher;

    public FormSettings(AppSettings settings, TrayApplicationContext context)
    {
        this.InitializeComponent();
        this.Settings = settings;
        this.Context = context;
        this.LoadFromSettings();
        this.StartUSBWatcher();
    }

    protected override void Dispose(bool disposing)
    {
        if(disposing)
        {
            try
            {
                this.USBWatcher?.Stop(); 
            }
            catch { }

            this.USBWatcher?.Dispose();
            this.components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void LoadFromSettings()
    {
        this.RefreshDevices();
        this.chkDetect.Checked = this.Settings.WatchForRocksmith;
        this.chkAutoHack.Checked = this.Settings.AutoHack;
        this.chkStartWithWindows.Checked = this.Settings.StartWithWindows;
        this.chkNotifications.Checked = this.Settings.ShowNotifications;
        this.numDelay.Value = Math.Clamp(this.Settings.HackDelayMs, 0, 30000);
    }

    private async Task SaveSettingsAsync()
    {
        if(this.cboDevices.SelectedItem is AudioDevice device && device.IsConnected)
        {
            this.Settings.SelectedDeviceVid = device.Vid;
            this.Settings.SelectedDevicePid = device.Pid;
            this.Settings.SelectedDeviceName = device.Name;
        }

        this.Settings.WatchForRocksmith = this.chkDetect.Checked;
        this.Settings.AutoHack = this.chkAutoHack.Checked;
        this.Settings.StartWithWindows = this.chkStartWithWindows.Checked;
        this.Settings.ShowNotifications = this.chkNotifications.Checked;
        this.Settings.HackDelayMs = (int)this.numDelay.Value;

        await this.Settings.SaveAsync();
    }

    private void RefreshDevices()
    {
        this.cboDevices.Items.Clear();
        var devices = AudioDeviceHelper.GetUSBAudioDevices(this.Settings);
        string savedKey = $"{this.Settings.SelectedDeviceVid}:{this.Settings.SelectedDevicePid}";
        bool selected = false;

        foreach(var device in devices)
        {
            int index = this.cboDevices.Items.Add(device);

            if(device.DeviceKey == savedKey)
            {
                this.cboDevices.SelectedIndex = index;
                selected = true;
            }
        }

        if(!selected && this.cboDevices.Items.Count > 0)
            this.cboDevices.SelectedIndex = 0;
    }

    private void StartUSBWatcher()
    {
        try
        {
            const string query =
                "SELECT * FROM __InstanceOperationEvent WITHIN 5 " +
                "WHERE TargetInstance ISA 'Win32_SoundDevice' " +
                "AND TargetInstance.DeviceID LIKE '%USB%'";

            this.USBWatcher = new ManagementEventWatcher(new WqlEventQuery(query));
            this.USBWatcher.EventArrived += (s, e) =>
            {
                try
                { this.BeginInvoke(this.RefreshDevices); }
                catch { }
            };

            this.USBWatcher.Start();
        }
        catch
        {
        }
    }

    private async void btnOK_Click(object? sender, EventArgs e)
    {
        await this.SaveSettingsAsync();
        this.Close();
    }

    private void btnRefresh_Click(object? sender, EventArgs e)
    {
        this.RefreshDevices();
    }

    private async void chkStartWithWindows_CheckedChanged(object? sender, EventArgs e)
    {
        await this.SaveSettingsAsync();
    }

    private async void chkNotifications_CheckedChanged(object? sender, EventArgs e)
    {
        await this.SaveSettingsAsync();
    }

    private async void chkDetect_CheckedChanged(object? sender, EventArgs e)
    {
        await this.SaveSettingsAsync();
    }

    private async void chkAutoHack_CheckedChanged(object? sender, EventArgs e)
    {
        await this.SaveSettingsAsync();
    }

    private async void numDelay_ValueChanged(object? sender, EventArgs e)
    {
        await this.SaveSettingsAsync();
    }

    private async void cboDevices_SelectionChangeCommitted(object? sender, EventArgs e)
    {
        await this.SaveSettingsAsync();
    }
}