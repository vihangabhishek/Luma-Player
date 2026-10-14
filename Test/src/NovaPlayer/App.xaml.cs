using System;
using System.Windows;
using System.Windows.Threading;

namespace NovaPlayer;

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
        bool uiSmoke = Array.Exists(e.Args, arg => arg.Equals("--ui-smoke-test", StringComparison.OrdinalIgnoreCase));
        StartupDiagnostics.Write(uiSmoke ? "Starting Windows UI smoke test." : "Launching Nova Player window.");
        base.OnStartup(e);
        if (uiSmoke)
        {
            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                StartupDiagnostics.Write("Windows UI smoke test passed.");
                Shutdown(0);
            };
            timer.Start();
        }
    }

    private void OnUnhandled(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        StartupDiagnostics.ReportCrash(e.Exception);
        e.Handled = true;
        Shutdown(1);
    }
}
