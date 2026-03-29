using Microsoft.Win32;
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
    private static readonly string SettingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RocksmithCableHack");
    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");
    private const string RegistryRunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string RegistryAppName = "RocksmithCableHack";

    // Variables
    private bool HaveSettingsChanged = false;
    private readonly JsonSerializerOptions JsonOptions = new()
    {
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,       
        WriteIndented = true,
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
            this.ApplyStartWithWindows();
            if(!this.HaveSettingsChanged)
                return;

            if(!Directory.Exists(SettingsDir))
                Directory.CreateDirectory(SettingsDir);

            await File.WriteAllTextAsync(SettingsPath, JsonSerializer.Serialize(this, this.JsonOptions)).ConfigureAwait(false);            
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
            {
                return new AppSettings()
                {
                    HaveSettingsChanged = true,
                };
            }

            var json = await File.ReadAllTextAsync(SettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    private bool ApplyStartWithWindows()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryRunKey, true);
            if(key is null)
                return false;

            if(this.StartWithWindows)
                key.SetValue(RegistryAppName, $"\"{Application.ExecutablePath}\"");
            else
                key.DeleteValue(RegistryAppName, false);

            return true;
        }
        catch
        {
            return false;
        }
    }
}