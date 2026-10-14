using System;
using System.Windows;
using System.Windows.Threading;

namespace LumaPlayer;

public partial class App : System.Windows.Application
{
    public App()
    {
        DispatcherUnhandledException += OnUnhandled;
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                StartupDiagnostics.Write("Fatal domain exception.", ex);
        };
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            StartupDiagnostics.Write("Unobserved background exception.", e.Exception);
            e.SetObserved();
        };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        if (Array.Exists(e.Args, arg => arg.Equals("--diagnose", StringComparison.OrdinalIgnoreCase)))
        {
            int result = StartupDiagnostics.RunSelfTest();
            Shutdown(result);
            return;
        }
        StartupDiagnostics.Write("Launching Luma Player window.");
        base.OnStartup(e);
    }

    private void OnUnhandled(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        StartupDiagnostics.ReportCrash(e.Exception);
        e.Handled = true;
        Shutdown(1);
    }
}
