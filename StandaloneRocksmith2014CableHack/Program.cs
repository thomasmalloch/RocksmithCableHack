using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RocksmithCableHack;

static class Program
{
    private static Mutex? AppMutex;

    [STAThread]
    [SupportedOSPlatform("windows10.0.17763")]
    static async Task Main()
    {
        AppMutex = new Mutex(true, @"Global\RocksmithCableHack_SingleInstance", out bool created);
        if(!created)
        {
            _ = MessageBox.Show("Rocksmith Cable Hack is already running.", "Cable Hack", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        using var tray = new TrayApplicationContext();
        EventHandler? idleHandler = null;
        idleHandler = async (s, e) =>
        {
            Application.Idle -= idleHandler;
            _ = tray.InitializeAsync().ConfigureAwait(false);
        };

        Application.Idle += idleHandler;
        Application.Run(tray);
        GC.KeepAlive(AppMutex);
    }
}