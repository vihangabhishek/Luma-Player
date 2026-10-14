using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace LumaPlayer;

public sealed class VideoPreviewItem
{
    public string Path { get; init; } = "";
    public string Title { get; init; } = "";
    public string Details { get; init; } = "";
    public BitmapSource? Thumbnail { get; init; }
}

// Uses Explorer's own video thumbnail providers. No external ffmpeg process,
// temporary video copies, video playback, telemetry, or network calls.
internal static class ExplorerThumbnail
{
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeSize
    {
        public int Width, Height;
        public NativeSize(int w, int h) { Width = w; Height = h; }
    }

    [ComImport, Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItemImageFactory
    {
        void GetImage(NativeSize size, int flags, out IntPtr bitmapHandle);
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    private static extern void SHCreateItemFromParsingName(
        [MarshalAs(UnmanagedType.LPWStr)] string path,
        IntPtr bindContext,
        ref Guid interfaceId,
        [MarshalAs(UnmanagedType.Interface)] out IShellItemImageFactory factory);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(IntPtr objectHandle);

    internal static BitmapSource? TryRead(string path)
    {
        IShellItemImageFactory? factory = null;
        IntPtr bitmap = IntPtr.Zero;
        try
        {
            Guid guid = typeof(IShellItemImageFactory).GUID;
            SHCreateItemFromParsingName(path, IntPtr.Zero, ref guid, out factory);
            // 8 = actual thumbnail only; 64 = preferred widescreen media preview.
            // This avoids generic file icons masquerading as video previews.
            factory.GetImage(new NativeSize(256, 144), 8 | 64, out bitmap);
            if (bitmap == IntPtr.Zero) return null;
            var source = Imaging.CreateBitmapSourceFromHBitmap(bitmap, IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(256, 144));
            source.Freeze(); // Can now be safely passed from the STA worker to WPF.
            return source;
        }
        catch (Exception) { return null; } // Media without a thumbnail uses the fallback visual.
        finally
        {
            if (bitmap != IntPtr.Zero) DeleteObject(bitmap);
            if (factory != null) Marshal.ReleaseComObject(factory);
        }
    }
}

internal static class LocalVideoDiscovery
{
    private const int MaxDirectories = 260;
    private const int MaxResults = 120;

    internal static IReadOnlyList<string> Discover(IEnumerable<string> knownLibraryPaths)
    {
        var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string path in knownLibraryPaths)
        {
            if (MediaCatalog.IsVideo(path) && File.Exists(path)) results.Add(path);
        }

        // Scan only the current user's Videos folder and the top level of Downloads.
        // Never crawl arbitrary drives or other users' files.
        var videos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        var queue = new Queue<(string Folder, int Depth)>();
        if (Directory.Exists(videos)) queue.Enqueue((videos, 0));
        if (Directory.Exists(downloads)) queue.Enqueue((downloads, 2));
        int directories = 0;

        while (queue.Count > 0 && directories++ < MaxDirectories && results.Count < MaxResults)
        {
            var (folder, depth) = queue.Dequeue();
            try
            {
                foreach (var file in Directory.EnumerateFiles(folder))
                {
                    if (MediaCatalog.IsVideo(file)) results.Add(file);
                    if (results.Count >= MaxResults) break;
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }

            if (depth >= 2 || results.Count >= MaxResults) continue;
            try
            {
                foreach (var subdir in Directory.EnumerateDirectories(folder))
                {
                    try
                    {
                        // Avoid junctions, symlink loops and cloud redirected folders.
                        if ((File.GetAttributes(subdir) & FileAttributes.ReparsePoint) == 0)
                            queue.Enqueue((subdir, depth + 1));
                    }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        return results.OrderByDescending(x =>
        {
            try { return File.GetLastWriteTimeUtc(x); }
            catch { return DateTime.MinValue; }
        }).Take(MaxResults).ToArray();
    }
}
