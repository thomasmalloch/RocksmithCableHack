using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RocksmithCableHack;

[SupportedOSPlatform("windows")]
public sealed class AppSettings
{
    // Constants
    private const string ScheduledTaskName = "RocksmithCableHack_StartWithWindows";
    private static readonly string SettingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RocksmithCableHack");
    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    // Variables
    private bool HaveSettingsChanged = false;
    private bool HasStartWithWindowsChanged = false;
    private readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    // Properties
    public string SelectedDeviceVid
    {
        get => field;
        set
        {
            if(field == value)
                return;

            field = value;
            this.HaveSettingsChanged = true;
        }
    } = "";

    public string SelectedDevicePid
    {
        get => field;
        set
        {
            if(field == value)
                return;

            field = value;
            this.HaveSettingsChanged = true;
        }
    } = "";

    public string SelectedDeviceName
    {
        get => field;
        set
        {
            if(field == value)
                return;

            field = value;
            this.HaveSettingsChanged = true;
        }
    } = "";

    [DefaultValue(true)]
    public bool WatchForRocksmith
    {
        get => field;
        set
        {
            if(field == value)
                return;

            field = value;
            this.HaveSettingsChanged = true;
        }
    } = true;

    [DefaultValue(true)]
    public bool AutoHack
    {
        get => field;
        set
        {
            if(field == value)
                return;

            field = value;
            this.HaveSettingsChanged = true;
        }
    } = true;

    public bool StartWithWindows
    {
        get => field;
        set
        {
            if(field == value)
                return;

            field = value;
            this.HaveSettingsChanged = true;
            this.HasStartWithWindowsChanged = true;
        }
    } = false;

    [DefaultValue(true)]
    public bool ShowNotifications
    {
        get => field;
        set
        {
            if(field == value)
                return;

            field = value;
            this.HaveSettingsChanged = true;
        }
    } = true;

    [DefaultValue(5000)]
    public int HackDelayMs
    {
        get => field;
        set
        {
            if(field == value)
                return;

            field = value;
            this.HaveSettingsChanged = true;
        }
    } = 5000;

    public long CachedVidOffset
    {
        get;
        set
        {
            if(field == value)
                return;

            field = value;
            this.HaveSettingsChanged = true;
        }
    } = 0;

    public long CachedPidOffset
    {
        get;
        set
        {
            if(field == value)
                return;

            field = value;
            this.HaveSettingsChanged = true;
        }
    } = 0;

    // Events
    public event EventHandler? SettingsSaved;

    // Methods
    public async Task SaveAsync(bool raiseSavedEvent = true)
    {
        try
        {
            if(!this.HaveSettingsChanged)
                return;

            if(!Directory.Exists(SettingsDir))
                Directory.CreateDirectory(SettingsDir);

            await File.WriteAllTextAsync(SettingsPath, JsonSerializer.Serialize(this, this.JsonOptions)).ConfigureAwait(false);
            await this.ApplyStartWithWindows().ConfigureAwait(false);
            this.HaveSettingsChanged = false;
            if(raiseSavedEvent)
                this.SettingsSaved?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
        }
    }

    public static async Task<AppSettings> BuildAsync()
    {
        try
        {
            if(!File.Exists(SettingsPath))
                return new AppSettings();

            var json = await File.ReadAllTextAsync(SettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    private async Task<bool> ApplyStartWithWindows()
    {
        if(!this.HasStartWithWindowsChanged)
            return true;

        // always remove existing task first (idempotent)
        if(!await RunSchtasksElevated($"/Delete /TN \"{ScheduledTaskName}\" /F"))
        {
            this.StartWithWindows = !this.StartWithWindows;
            this.HasStartWithWindowsChanged = false;
            return false;
        }

        if(!this.StartWithWindows)
        {
            this.HasStartWithWindowsChanged = false;
            return true;
        }

        string exePath = Application.ExecutablePath;
        if(!await RunSchtasksElevated(
            $"/Create /TN \"{ScheduledTaskName}\" " +
            $"/TR \"\\\"{exePath}\\\"\" " +
            $"/SC ONLOGON " +
            $"/RL HIGHEST " +
            $"/F"))
        {
            this.StartWithWindows = false;
            this.HasStartWithWindowsChanged = false;
            return false;
        }

        this.HasStartWithWindowsChanged = false;
        return true;
    }

    private static async Task<bool> RunSchtasksElevated(string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = arguments,
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using var process = Process.Start(startInfo);
            if(process is null)
                return false;

            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}