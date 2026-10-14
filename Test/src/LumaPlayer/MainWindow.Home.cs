using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Button = System.Windows.Controls.Button;
using System.Windows.Threading;

namespace LumaPlayer;

public partial class MainWindow
{
    private readonly ObservableCollection<VideoPreviewItem> _homeVideos = new();
    private int _videoScanGeneration;

    private void InitializeHome()
    {
        HomeVideosList.ItemsSource = _homeVideos;
        ShowHome();
        ScanHomeVideos();
    }

    private void ShowHome()
    {
        // Keep Home and video stage mutually exclusive. No hidden background video playback.
        if (_player?.IsPlaying == true) _player.Pause();
        HomeDashboard.Visibility = Visibility.Visible;
        EmptyVisual.Visibility = Visibility.Collapsed;
        VideoSurface.Visibility = Visibility.Collapsed;
        MusicVisual.Visibility = Visibility.Collapsed;
    }

    private void ShowPlayer()
    {
        HomeDashboard.Visibility = Visibility.Collapsed;
        if (_current != null)
        {
            bool video = MediaCatalog.IsVideo(_current.Path);
            VideoSurface.Visibility = video ? Visibility.Visible : Visibility.Collapsed;
            MusicVisual.Visibility = video ? Visibility.Collapsed : Visibility.Visible;
            EmptyVisual.Visibility = Visibility.Collapsed;
        }
        else if (_streamTitle != null)
        {
            VideoSurface.Visibility = Visibility.Visible;
            MusicVisual.Visibility = Visibility.Collapsed;
            EmptyVisual.Visibility = Visibility.Collapsed;
        }
        else
        {
            EmptyVisual.Visibility = Visibility.Visible;
            VideoSurface.Visibility = Visibility.Collapsed;
            MusicVisual.Visibility = Visibility.Collapsed;
        }
    }

    private void Home_Click(object sender, RoutedEventArgs e) => ShowHome();

    private void RefreshHome_Click(object sender, RoutedEventArgs e) => ScanHomeVideos();

    private void HomeVideo_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: VideoPreviewItem item } && File.Exists(item.Path))
            AddMediaPaths(new[] { item.Path }, true);
    }

    private void ScanHomeVideos()
    {
        int generation = Interlocked.Increment(ref _videoScanGeneration);
        var library = _state.Library.Select(x => x.Path).ToArray();
        _homeVideos.Clear();
        HomeVideoCount.Text = "Finding your videos…";
        HomeScanStatus.Text = "Scanning Videos and Downloads (only your files)";

        var thread = new Thread(() =>
        {
            try
            {
                var paths = LocalVideoDiscovery.Discover(library);
                if (Dispatcher.HasShutdownStarted) return;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (generation != _videoScanGeneration) return;
                    HomeVideoCount.Text = $"{paths.Count} videos found";
                    HomeScanStatus.Text = paths.Count == 0
                        ? "No videos yet. Use Open Files or Add Folder to include other locations."
                        : "Video previews are loading…";
                    HomeNoVideos.Visibility = paths.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                }), DispatcherPriority.Background);

                int count = 0;
                foreach (string path in paths)
                {
                    if (generation != _videoScanGeneration || Dispatcher.HasShutdownStarted) break;
                    string title = System.IO.Path.GetFileNameWithoutExtension(path);
                    string extension = System.IO.Path.GetExtension(path).TrimStart('.').ToUpperInvariant();
                    string folder = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(path)) ?? "Video";
                    var item = new VideoPreviewItem
                    {
                        Path = path,
                        Title = title,
                        Details = extension + " · " + folder,
                        Thumbnail = count < 50 ? ExplorerThumbnail.TryRead(path) : null
                    };
                    ++count;
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (generation == _videoScanGeneration) _homeVideos.Add(item);
                    }), DispatcherPriority.Background);
                }
                if (!Dispatcher.HasShutdownStarted)
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (generation == _videoScanGeneration && paths.Count > 0)
                            HomeScanStatus.Text = "Click a preview to play · Scan again to refresh";
                    }), DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                StartupDiagnostics.Write("Home video scan failed.", ex);
                if (!Dispatcher.HasShutdownStarted)
                    Dispatcher.BeginInvoke(new Action(() => HomeScanStatus.Text = "Could not scan videos. You can still open files manually."));
            }
        })
        { IsBackground = true, Name = "Luma local video previews" };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }
}
