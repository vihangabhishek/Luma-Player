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

    internal static int RunPlaybackSmokeTest()
    {
        string? sample = null;
        try
        {
            Write("Running offline PCM playback smoke test.");
            InitializeVlc();
            sample = Path.Combine(Path.GetTempPath(), "nova-smoke-" + Guid.NewGuid().ToString("N") + ".wav");
            const int rate = 8000, seconds = 2;
            using (var stream = File.Create(sample))
            using (var wav = new BinaryWriter(stream))
            {
                int dataSize = rate * seconds * sizeof(short);
                wav.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                wav.Write(36 + dataSize);
                wav.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                wav.Write(16); wav.Write((short)1); wav.Write((short)1);
                wav.Write(rate); wav.Write(rate * sizeof(short));
                wav.Write((short)sizeof(short)); wav.Write((short)16);
                wav.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                wav.Write(dataSize);
                for (int i = 0; i < rate * seconds; i++)
                    wav.Write((short)(Math.Sin(2.0 * Math.PI * 440.0 * i / rate) * 3000));
            }

            using var engine = new LibVLC("--aout=dummy", "--no-video-title-show");
            using var player = new LibVLCSharp.Shared.MediaPlayer(engine);
            using var media = new Media(engine, new Uri(sample));
            if (!player.Play(media))
                throw new InvalidOperationException("LibVLC refused the sample media.");
            bool playing = false;
            for (int i = 0; i < 25; i++)
            {
                System.Threading.Thread.Sleep(100);
                if (player.IsPlaying) { playing = true; break; }
            }
            player.Stop();
            if (!playing) throw new TimeoutException("LibVLC never started sample playback.");
            Write("Offline PCM playback smoke test passed.");
            return 0;
        }
        catch (Exception ex)
        {
            Write("Offline PCM playback smoke test failed.", ex);
            return 1;
        }
        finally
        {
            if (sample != null) { try { File.Delete(sample); } catch { } }
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
