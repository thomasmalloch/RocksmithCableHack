using System;
using System.Diagnostics;
using System.Drawing;
using System.Management;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RocksmithCableHack;

[SupportedOSPlatform("windows10.0.17763")]
public class TrayApplicationContext : ApplicationContext
{
    // Constants
    private const string WmiStartQuery =
        "SELECT * FROM __InstanceCreationEvent WITHIN 2 " +
        "WHERE TargetInstance ISA 'Win32_Process' AND TargetInstance.Name = 'Rocksmith2014.exe'";

    private const string WmiStopQuery =
        "SELECT * FROM __InstanceDeletionEvent WITHIN 10 " +
        "WHERE TargetInstance ISA 'Win32_Process' AND TargetInstance.Name = 'Rocksmith2014.exe'";

    private const string WmiRunningQuery =
        "SELECT Handle FROM Win32_Process WHERE Name = 'Rocksmith2014.exe'";

    private const string WmiUSBDeviceQuery =
        "SELECT * FROM __InstanceOperationEvent WITHIN 5 " +
        "WHERE TargetInstance ISA 'Win32_SoundDevice' " +
        "AND TargetInstance.DeviceID LIKE '%USB%'";

    private static readonly byte[] VIDPattern = [0xF8, 0x06, 0xBA, 0x12];
    private static readonly byte[] PIDPattern = [0xFF, 0x00];

    // Variables
    private readonly ManagementEventWatcher StartWatcher;
    private readonly ManagementEventWatcher StopWatcher;
    private readonly ManagementEventWatcher USBWatcher;
    private AppSettings Settings = null!;
    private FormSettings? SettingsForm;
    private int RocksmithPID;
    private volatile bool IsHacked;
    private volatile bool IsHacking;
    private CancellationTokenSource HackCancellationTokenSource = new();

    // Controls
    private ContextMenuStrip? mnuTray = null;
    private NotifyIcon? icoTray = null;
    private Icon? AppIcon = null;
    private ToolStripMenuItem? mniHack = null;
    private ToolStripMenuItem? mniDevices = null;
    private ToolStripMenuItem? mniDetect = null;
    private ToolStripMenuItem? mniAutoHack = null;

    public TrayApplicationContext()
    {
        this.StartWatcher = new ManagementEventWatcher(new WqlEventQuery(WmiStartQuery));
        this.StopWatcher = new ManagementEventWatcher(new WqlEventQuery(WmiStopQuery));
        this.USBWatcher = new ManagementEventWatcher(new WqlEventQuery(WmiUSBDeviceQuery));
    }

    public async Task InitializeAsync()
    {
        this.Settings = await AppSettings.BuildAsync();
        await this.Settings.SaveAsync();
        this.Settings.SettingsSaved += (s, e) => this.RunOnUi(async ()=> await this.SyncTrayFromSettings());

        this.mniHack = new ToolStripMenuItem("Hack Now", null, this.mniHack_Click) { Enabled = false };
        this.mniDevices = new ToolStripMenuItem("Audio Device");
        this.mniDetect = new ToolStripMenuItem("Detect Rocksmith", null, this.mniDetect_Click)
        {
            Checked = this.Settings.WatchForRocksmith
        };

        this.mniAutoHack = new ToolStripMenuItem("Hack on detection", null, this.mniAutoHack_Click)
        {
            Checked = this.Settings.AutoHack
        };

        this.mnuTray = new ContextMenuStrip();
        this.mnuTray.Items.Add(this.mniHack);
        this.mnuTray.Items.Add(new ToolStripSeparator());
        this.mnuTray.Items.Add(this.mniDevices);
        this.mnuTray.Items.Add(new ToolStripSeparator());
        this.mnuTray.Items.Add(this.mniDetect);
        this.mnuTray.Items.Add(this.mniAutoHack);
        this.mnuTray.Items.Add(new ToolStripSeparator());
        this.mnuTray.Items.Add("Settings", null, this.mniSettings_Click);
        this.mnuTray.Items.Add(new ToolStripSeparator());
        this.mnuTray.Items.Add("Exit", null, this.mniExit_Click);

        this.RefreshDevicesMenu();
        this.AppIcon = LoadEmbeddedIcon();
        this.icoTray = new NotifyIcon
        {
            Icon = this.AppIcon,
            Text = "Rocksmith Cable Hack",
            ContextMenuStrip = this.mnuTray,
            Visible = true
        };

        this.icoTray.DoubleClick += this.mniSettings_Click;
        this.StartWatcher.EventArrived += this.OnRocksmithStarted;
        this.StopWatcher.EventArrived += this.OnRocksmithStopped;
        this.USBWatcher.EventArrived += (s, e) =>
        {
            try
            { 
                this.RunOnUi(this.RefreshDevicesMenu); 
            }
            catch 
            { 
            }
        };

        this.USBWatcher.Start();
        if(this.Settings.WatchForRocksmith)
            this.BeginWatching();

        await this.CheckForRunningRocksmithAsync();
    }

    private static Icon LoadEmbeddedIcon()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("RocksmithCableHack.guitar-pick.ico")
            ?? throw new InvalidOperationException("Embedded icon not found.");

        return new Icon(stream);
    }

    private void ShowTrayNotification(string balloon, ToolTipIcon balloonIcon = ToolTipIcon.Info)
    {
        if(!this.Settings.ShowNotifications)
            return;

        this.icoTray?.ShowBalloonTip(3000, "Rocksmith Cable Hack", balloon, balloonIcon);
    }

    private void RefreshDevicesMenu()
    {
        if(this.mniDevices is null)
            return;

        this.mniDevices.DropDownItems.Clear();
        var devices = AudioDeviceHelper.GetUSBAudioDevices(this.Settings);
        if(devices.Count == 0)
        {
            this.mniDevices.DropDownItems.Add(new ToolStripMenuItem("No devices found") 
            { 
                Enabled = false 
            });

            return;
        }

        string savedKey = $"{this.Settings.SelectedDeviceVid}:{this.Settings.SelectedDevicePid}";
        bool hasSelection = false;
        foreach(var device in devices)
        {
            bool isSelected = (device.DeviceKey == savedKey);
            var item = new ToolStripMenuItem(device.Name + ((device.IsConnected) ? "" : " (disconnected)"))
            {
                Tag = device,
                Checked = isSelected,
                Enabled = device.IsConnected,
                ToolTipText = $"VID: {device.Vid} PID: {device.Pid}"
            };

            item.Click += this.mniDevice_Click;
            this.mniDevices.DropDownItems.Add(item);

            if(isSelected)
                hasSelection = true;
        }

        // if nothing matched, select the first connected device
        if(!hasSelection)
        {
            foreach(ToolStripItem item in this.mniDevices.DropDownItems)
            {
                if((item is not ToolStripMenuItem menuItem) ||
                    (!menuItem.Enabled) ||
                    (menuItem.Tag is not AudioDevice device))
                {
                    continue;
                }

                menuItem.Checked = true;
                SaveSelectedDevice(device);
                break;
            }

            _ = this.Settings.SaveAsync(raiseSavedEvent: false);
        }
    }

    private void SaveSelectedDevice(AudioDevice device)
    {
        this.Settings.SelectedDeviceVid = device.Vid;
        this.Settings.SelectedDevicePid = device.Pid;
        this.Settings.SelectedDeviceName = device.Name;
    }


    private async void mniDevice_Click(object? sender, EventArgs e)
    {
        try
        {
            if((sender is not ToolStripMenuItem clicked) || (clicked.Tag is not AudioDevice device))
                return;

            if(this.mniDevices is null)
                return;

            // uncheck all, check selected
            foreach(ToolStripItem item in this.mniDevices.DropDownItems)
            {
                if(item is ToolStripMenuItem menuItem)
                    menuItem.Checked = false;
            }

            clicked.Checked = true;
            this.SaveSelectedDevice(device);
            await this.Settings.SaveAsync(raiseSavedEvent: false);
        }
        finally 
        {
        
        }
    }

    private async void mniDetect_Click(object? sender, EventArgs e)
    {
        if(this.mniDetect is null)
            return;

        this.mniDetect.Checked = (!this.mniDetect.Checked);
        this.Settings.WatchForRocksmith = this.mniDetect.Checked;
        await this.Settings.SaveAsync(raiseSavedEvent: false);
        if(this.mniDetect.Checked)
        {
            _ = this.CheckForRunningRocksmithAsync();
            if(this.RocksmithPID == 0)
                this.BeginWatching();
        }
        else
        {
            this.EndWatching();
        }
    }

    private async void mniAutoHack_Click(object? sender, EventArgs e)
    {
        if(this.mniAutoHack is null)
            return;

        this.mniAutoHack.Checked = !this.mniAutoHack.Checked;
        this.Settings.AutoHack = this.mniAutoHack.Checked;
        await this.Settings.SaveAsync();
    }

    private async Task SyncTrayFromSettings()
    {
        if(this.mniDetect is not null)
            this.mniDetect.Checked = this.Settings.WatchForRocksmith;

        if(this.mniAutoHack is not null)
            this.mniAutoHack.Checked = this.Settings.AutoHack;

        this.RefreshDevicesMenu();
        this.EndWatching();
        if(this.Settings.WatchForRocksmith)
        {
            await this.CheckForRunningRocksmithAsync();
            if(this.RocksmithPID == 0)
                this.BeginWatching();
        }
    }

    private async Task CheckForRunningRocksmithAsync()
    {
        int pid = GetRocksmithPid();
        if(pid == 0)
            return;

        this.RocksmithPID = pid;
        this.RunOnUi(() =>
        {
            this.ShowTrayNotification($"Rocksmith detected (PID {pid})");
            this.mniHack?.Enabled = true;
        });

        this.StopWatcher.Start();
        if(this.Settings.AutoHack)
            await this.RunHackAsync(pid).ConfigureAwait(false);
    }

    private static int GetRocksmithPid()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(WmiRunningQuery);
            foreach(ManagementBaseObject obj in searcher.Get())
            {
                if(int.TryParse(obj["Handle"]?.ToString(), out int pid))
                    return pid;
            }
        }
        catch
        {
        }

        return 0;
    }

    public void BeginWatching()
    {
        try
        {
            if(this.RocksmithPID == 0)
                this.StartWatcher.Start();
        }
        catch
        {
        }
    }

    public void EndWatching()
    {
        try
        { 
            this.StartWatcher.Stop();
        }
        catch { }

        try
        { 
            this.StopWatcher.Stop();
        }
        catch { }

        this.RocksmithPID = 0;
        this.IsHacked = false;

        if(this.mniHack is not null)
            this.mniHack.Enabled = false;
    }

    private async void OnRocksmithStarted(object sender, EventArrivedEventArgs e)
    {
        try
        {
            await Task.Delay(500);
            int pid = GetRocksmithPid();
            if(pid == 0)
                return;

            this.RocksmithPID = pid;
            this.IsHacked = false;
            this.RunOnUi(() =>
            {
                try
                { 
                    this.StartWatcher.Stop(); 
                }
                catch 
                { }

                this.StopWatcher.Start();
                this.mniHack?.Enabled = true;
                this.ShowTrayNotification($"Rocksmith detected (PID {pid})");
            });

            if(this.Settings.AutoHack)
                await this.RunHackAsync(pid);
        }
        catch
        {
        }
    }

    private void OnRocksmithStopped(object sender, EventArrivedEventArgs e)
    {
        try
        {
            this.RunOnUi(() =>
            {
                try
                { 
                    this.StopWatcher.Stop(); 
                }
                catch { }

                this.RocksmithPID = 0;
                this.IsHacked = false;
                this.mniHack?.Enabled = false;
                this.ShowTrayNotification("Rocksmith exited.");
                if(this.Settings.WatchForRocksmith)
                    this.StartWatcher.Start();
            });
        }
        catch
        {
        }
    }

    private async Task RunHackAsync(int pid)
    {
        if((this.IsHacking) || (this.IsHacked))
            return;

        var devices = AudioDeviceHelper.GetUSBAudioDevices(this.Settings);
        string savedKey = $"{this.Settings.SelectedDeviceVid}:{this.Settings.SelectedDevicePid}";
        AudioDevice? device = null;
        foreach(var d in devices)
        {
            if(!d.IsConnected)
                continue;

            if(d.DeviceKey != savedKey)
                continue;

            device = d;
            break;
        }

        // fall back to first connected device
        if(device is null)
        {
            foreach(var d in devices)
            {
                if(!d.IsConnected)
                    continue;

                device = d;
                break;
            }
        }

        if((device is null) || (!device.IsValid))
        {
            this.RunOnUi(() =>
            {
                this.ShowTrayNotification(
                    "No USB audio device found. Connect your cable and try again.",
                    balloonIcon: ToolTipIcon.Warning);
            });

            return;
        }

        await this.PerformHackAsync(pid, device);
    }

    private async Task PerformHackAsync(int processID, AudioDevice device)
    {
        this.IsHacking = true;
        try
        {
            Process rocksmith;
            try
            {
                rocksmith = Process.GetProcessById(processID);
            }
            catch
            {
                this.RunOnUi(() => this.ShowTrayNotification("Rocksmith not found", ToolTipIcon.Error));
                return;
            }

            if(rocksmith.MainModule is null)
            {
                this.RunOnUi(() => this.ShowTrayNotification("Cannot access Rocksmith", ToolTipIcon.Error));
                return;
            }

            this.RunOnUi(() => this.ShowTrayNotification("Hacking..."));
            if(this.Settings.HackDelayMs > 0)
                await Task.Delay(this.Settings.HackDelayMs);

            if(rocksmith.HasExited)
            {
                this.RunOnUi(() => this.ShowTrayNotification("Rocksmith exited during delay", ToolTipIcon.Warning));
                return;
            }

            var handle = rocksmith.Handle;
            long baseAddr = rocksmith.MainModule.BaseAddress.ToInt64();
            long moduleEnd = baseAddr + rocksmith.MainModule.ModuleMemorySize;
            long cachedVID = this.Settings.CachedVidOffset;
            long cachedPID = this.Settings.CachedPidOffset;
            var result = await Task.Run(
                () => ScanAndPatch(handle, baseAddr, moduleEnd, cachedVID, cachedPID, device),
                cancellationToken: this.HackCancellationTokenSource.Token);

            if(!result.Success)
                throw new Exception(result.Error);

            this.Settings.CachedVidOffset = result.VidOffset;
            this.Settings.CachedPidOffset = result.PidOffset;
            await this.Settings.SaveAsync(raiseSavedEvent: false);
            this.IsHacked = true;
            this.RunOnUi(() => this.ShowTrayNotification($"Rocksmith hacked with {device.Name}"));
        }
        catch(Exception ex)
        {
            this.RunOnUi(() =>
            {
                this.ShowTrayNotification(ex.Message, balloonIcon: ToolTipIcon.Error);
            });
        }
        finally
        {
            this.IsHacking = false;
            this.RunOnUi(() => this.mniHack?.Enabled = ((this.RocksmithPID != 0) && (!this.IsHacked)));
        }
    }

    private static HackResult ScanAndPatch(IntPtr handle, long baseAddr, long moduleEnd, long cachedVIDOffset, long cachedPIDOffset, AudioDevice device)
    {
        long vidAddr = MemoryScanner.TryCachedOffset(handle, baseAddr, cachedVIDOffset, VIDPattern);
        if(vidAddr < 0)
        {
            vidAddr = MemoryScanner.FindPattern(handle, baseAddr, moduleEnd, VIDPattern);
            if(vidAddr < 0)
                return HackResult.Fail("Could not locate VID pattern in memory.");
        }

        long vidWriteAddr = vidAddr + 2;
        if(!MemoryScanner.WriteAndVerify(handle, vidWriteAddr, device.VidBytes))
            return HackResult.Fail("Failed to write VID to process memory.");

        long pidAddr = MemoryScanner.TryCachedOffset(handle, baseAddr, cachedPIDOffset, PIDPattern);
        if(pidAddr < 0)
        {
            pidAddr = MemoryScanner.FindPattern(handle, vidAddr + VIDPattern.Length, moduleEnd, PIDPattern);
            if(pidAddr < 0)
                return HackResult.Fail("Could not locate PID pattern in memory.");
        }

        if(!MemoryScanner.WriteAndVerify(handle, pidAddr, device.PidBytes))
            return HackResult.Fail("Failed to write PID to process memory.");

        return HackResult.Ok(vidAddr - baseAddr, pidAddr - baseAddr);
    }

    private async void mniHack_Click(object? sender, EventArgs e)
    {
        int pid = this.RocksmithPID;
        if(pid == 0)
        {
            pid = GetRocksmithPid();
            if(pid == 0)
            {
                this.RunOnUi(() => this.ShowTrayNotification(
                    "Rocksmith is not running.", balloonIcon: ToolTipIcon.Warning));
                return;
            }

            this.RocksmithPID = pid;
        }

        await this.RunHackAsync(pid);
    }

    private void mniSettings_Click(object? sender, EventArgs e)
    {
        if(this.SettingsForm is not null && !this.SettingsForm.IsDisposed)
        {
            this.SettingsForm.Activate();
            return;
        }

        this.SettingsForm = new FormSettings(this.Settings, this);
        this.SettingsForm.FormClosed += (s, e) =>
        {
            this.SettingsForm?.Dispose();
            this.SettingsForm = null;
        };

        this.SettingsForm.Show();
    }

    private void mniExit_Click(object? sender, EventArgs e)
    {
        Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        if(disposing)
        {
            this.SettingsForm?.Close();            
            this.HackCancellationTokenSource.Dispose();

            try
            { 
                this.StartWatcher.Stop(); 
            }
            catch { }
            try
            {
                this.StopWatcher.Stop(); 
            }
            catch { }
            try
            { 
                this.USBWatcher.Stop(); 
            }
            catch { }

            this.StartWatcher.Dispose();
            this.StopWatcher.Dispose();
            this.USBWatcher.Dispose();
            this.icoTray?.Visible = false;
            this.icoTray?.Dispose();
            this.AppIcon?.Dispose();
            this.mniHack?.Dispose();
            this.mniDevices?.Dispose();
            this.mniDetect?.Dispose();
            this.mniAutoHack?.Dispose();
            this.mnuTray?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void RunOnUi(Action action)
    {
        if(this.mnuTray is null)
            return;

        if(this.mnuTray.InvokeRequired)
            this.mnuTray.BeginInvoke(action);
        else
            action();
    }
}

