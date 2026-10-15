using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using LibVLCSharp.Shared;

namespace NovaPlayer;

internal static class VideoSmoke
{
    // Generate a tiny, deterministic, uncompressed AVI with real video frames.
    // No external test media, network download, or FFmpeg installation is needed.
    internal static string CreateTestClip()
    {
        const int width = 64, height = 48, fps = 8, frames = 32;
        const int frameBytes = width * height * 3;
        string path = Path.Combine(Path.GetTempPath(), "nova-video-" + Guid.NewGuid().ToString("N") + ".avi");
        using var writer = new BinaryWriter(File.Create(path), Encoding.ASCII);

        static void FourCC(BinaryWriter b, string code) => b.Write(Encoding.ASCII.GetBytes(code));
        static long BeginChunk(BinaryWriter b, string id)
        {
            FourCC(b, id);
            long position = b.BaseStream.Position;
            b.Write(0);
            return position;
        }
        static void EndChunk(BinaryWriter b, long sizePosition)
        {
            long end = b.BaseStream.Position;
            b.BaseStream.Position = sizePosition;
            b.Write(checked((int)(end - sizePosition - 4)));
            b.BaseStream.Position = end;
            if ((end - sizePosition - 4) % 2 != 0) b.Write((byte)0);
        }

        long riff = BeginChunk(writer, "RIFF"); FourCC(writer, "AVI ");
        long hdrl = BeginChunk(writer, "LIST"); FourCC(writer, "hdrl");
        long avih = BeginChunk(writer, "avih");
        foreach (uint v in new uint[] {
            1000000 / fps, frameBytes * fps, 0, 0x10, frames, 0, 1,
            frameBytes, width, height, 0, 0, 0, 0 })
            writer.Write(v);
        EndChunk(writer, avih);

        long strl = BeginChunk(writer, "LIST"); FourCC(writer, "strl");
        long strh = BeginChunk(writer, "strh");
        FourCC(writer, "vids"); FourCC(writer, "DIB ");
        writer.Write((uint)0); writer.Write((ushort)0); writer.Write((ushort)0);
        writer.Write((uint)0); writer.Write((uint)1); writer.Write((uint)fps);
        writer.Write((uint)0); writer.Write((uint)frames);
        writer.Write((uint)frameBytes); writer.Write(uint.MaxValue); writer.Write((uint)0);
        writer.Write((short)0); writer.Write((short)0);
        writer.Write((short)width); writer.Write((short)height);
        EndChunk(writer, strh);

        long strf = BeginChunk(writer, "strf");
        writer.Write((uint)40);
        writer.Write(width); writer.Write(height);
        writer.Write((ushort)1); writer.Write((ushort)24);
        writer.Write((uint)0); writer.Write((uint)frameBytes);
        writer.Write(0); writer.Write(0); writer.Write((uint)0); writer.Write((uint)0);
        EndChunk(writer, strf); EndChunk(writer, strl); EndChunk(writer, hdrl);

        long movi = BeginChunk(writer, "LIST"); FourCC(writer, "movi");
        var pixels = new byte[frameBytes];
        for (int frame = 0; frame < frames; frame++)
        {
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int offset = (y * width + x) * 3;
                pixels[offset] = 20; // blue
                pixels[offset + 1] = (byte)((x + frame) % 64 * 4);
                pixels[offset + 2] = 165;
            }
            long db = BeginChunk(writer, "00db");
            writer.Write(pixels);
            EndChunk(writer, db);
        }
        EndChunk(writer, movi); EndChunk(writer, riff);
        return path;
    }
}

public partial class MainWindow
{
    internal async Task RunVideoSmokeAsync()
    {
        string? sample = null;
        try
        {
            sample = VideoSmoke.CreateTestClip();
            await EnsurePlaybackEngineAsync();
            _current = new MediaItem { Path = sample };
            ShowPlayer();
            await Dispatcher.InvokeAsync(() => VideoHost.UpdateLayout(), DispatcherPriority.Loaded);
            var hwnd = VideoSurface.Handle;
            VideoSurface.MediaPlayer = _player;
            _player.Hwnd = hwnd;
            if (hwnd == IntPtr.Zero || _player.Hwnd != hwnd)
                throw new InvalidOperationException("The native video renderer was not attached to the visible Windows HWND.");
            using var media = new Media(_engine, new Uri(sample));
            bool started = await Task.Run(() => _player.Play(media));
            if (!started) throw new InvalidOperationException("Video renderer refused a valid AVI video clip.");
            await Task.Delay(850);
            if (!_player.IsPlaying) throw new InvalidOperationException("Video playback never entered playing state.");
            StartupDiagnostics.Write("Native Windows video renderer and offline AVI playback smoke test PASSED.");
        }
        finally
        {
            if (_player != null) { try { await Task.Run(() => _player.Stop()); } catch { } }
            if (sample != null) { try { File.Delete(sample); } catch { } }
            _current = null;
        }
    }
}
