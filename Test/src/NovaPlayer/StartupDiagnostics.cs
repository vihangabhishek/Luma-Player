using System;
using System.IO;
using System.Windows;
using LibVLCSharp.Shared;

namespace NovaPlayer;

/// <summary>Local-only startup diagnostics. Never uploads logs or media details.</summary>
internal static class StartupDiagnostics
{
    internal static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "NovaPlayer", "logs");
    internal static readonly string LogPath = Path.Combine(LogDirectory, "startup.log");

    internal static void Write(string message, Exception? exception = null)
    {
        try
        {
            Directory.CreateDirectory(LogDirectory);
            File.AppendAllText(LogPath, $"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}] {message}{Environment.NewLine}" +
                (exception == null ? "" : exception + Environment.NewLine));
        }
        catch { /* Diagnostic logging must not crash the application. */ }
    }

    internal static void InitializeVlc()
    {
        var appDir = AppContext.BaseDirectory;
        var nativeDir = Path.Combine(appDir, "libvlc", "win-x64");
        var nativeDll = Path.Combine(nativeDir, "libvlc.dll");
        var nativeCore = Path.Combine(nativeDir, "libvlccore.dll");
        Write("Starting native VLC engine. Path available: " +
              File.Exists(nativeDll) + "; core available: " + File.Exists(nativeCore) +
              "; application is 64-bit: " + Environment.Is64BitProcess);
        if (File.Exists(nativeDll) && File.Exists(nativeCore))
            Core.Initialize(nativeDir);
        else
            Core.Initialize();
        Write("LibVLC native initialization succeeded.");
    }

    internal static int RunSelfTest()
    {
        try
        {
            Write("Windows startup self-test started.");
            InitializeVlc();
            using var engine = new LibVLC("--no-video-title-show");
            using var player = new LibVLCSharp.Shared.MediaPlayer(engine);
            Write("Windows startup self-test passed.");
            return 0;
        }
        catch (Exception ex)
        {
            Write("Windows startup self-test failed.", ex);
            return 1;
        }
    }

    internal static void ReportCrash(Exception ex)
    {
        Write("Unhandled application startup/runtime exception.", ex);
        try
        {
            System.Windows.MessageBox.Show(
                "Nova Player could not start correctly.\n\n" +
                ex.GetType().Name + ": " + ex.Message + "\n\n" +
                "Diagnostic log (stored on your PC):\n" + LogPath + "\n\n" +
                "Please report this error on the GitHub Issues page. The log is NOT uploaded automatically.",
                "Nova Player startup error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        catch { }
    }
}
