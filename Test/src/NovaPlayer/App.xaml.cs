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
        if (Array.Exists(e.Args, arg => arg.Equals("--playback-smoke-test", StringComparison.OrdinalIgnoreCase)))
        {
            Shutdown(StartupDiagnostics.RunPlaybackSmokeTest());
            return;
        }
        if (Array.Exists(e.Args, arg => arg.Equals("--diagnose", StringComparison.OrdinalIgnoreCase)))
        {
            int result = StartupDiagnostics.RunSelfTest();
            Shutdown(result);
            return;
        }
        bool videoSmoke = Array.Exists(e.Args, arg => arg.Equals("--video-smoke-test", StringComparison.OrdinalIgnoreCase));
        if (videoSmoke)
        {
            StartupDiagnostics.Write("Starting real native video renderer smoke test.");
            base.OnStartup(e);
            var videoTest = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(700) };
            videoTest.Tick += async (_, _) =>
            {
                videoTest.Stop();
                try
                {
                    if (MainWindow is not MainWindow window)
                        throw new InvalidOperationException("Main window did not load.");
                    await window.RunVideoSmokeAsync();
                    Shutdown(0);
                }
                catch (Exception ex)
                {
                    StartupDiagnostics.Write("Real native video renderer smoke test FAILED.", ex);
                    Shutdown(1);
                }
            };
            videoTest.Start();
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
                try
                {
                    if (this.MainWindow is not MainWindow window)
                        throw new InvalidOperationException("Nova main window failed to load.");
                    window.RunUiSmokeScenario();
                    StartupDiagnostics.Write("Windows navigation and UI smoke test passed.");
                    Shutdown(0);
                }
                catch (Exception ex)
                {
                    StartupDiagnostics.Write("Windows UI navigation smoke test failed.", ex);
                    Shutdown(1);
                }
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
