using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NovaPlayer;

public partial class MainWindow
{
    private string _section = "home";

    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button { Tag: string section }) return;
        if (_compact) Fullscreen_Click(this, new RoutedEventArgs());
        switch (section)
        {
            case "home":
                ShowHome();
                break;
            case "music":
            case "videos":
                ShowLibraryTab(section);
                break;
            case "settings":
            case "updates":
            case "about":
                NavigateChrome(section);
                if (section == "settings") SettingsPage.Visibility = Visibility.Visible;
                if (section == "updates") UpdatesPage.Visibility = Visibility.Visible;
                if (section == "about") AboutPage.Visibility = Visibility.Visible;
                break;
        }
    }

    private void NavigateChrome(string section)
    {
        _section = section;
        HomeDashboard.Visibility = Visibility.Collapsed;
        SettingsPage.Visibility = Visibility.Collapsed;
        UpdatesPage.Visibility = Visibility.Collapsed;
        AboutPage.Visibility = Visibility.Collapsed;
        LibraryLanding.Visibility = Visibility.Collapsed;
        EmptyVisual.Visibility = Visibility.Collapsed;
        MusicVisual.Visibility = Visibility.Collapsed;
        VideoSurface.Visibility = Visibility.Collapsed;

        bool library = section is "home" or "music" or "videos" or "player";
        LibraryPanel.Visibility = library ? Visibility.Visible : Visibility.Collapsed;
        ShellGrid.ColumnDefinitions[2].Width = library ? new GridLength(268) : new GridLength(0);
        PlaybackTools.Visibility = section == "player" ? Visibility.Visible : Visibility.Collapsed;
        TransportDock.Visibility = library ? Visibility.Visible : Visibility.Collapsed;

        (string title, string subtitle) = section switch
        {
            "home" => ("Good to see you", "Your music and movies in one place."),
            "music" => ("Music", "All your favorite sounds."),
            "videos" => ("Videos", "Your movies, ready to watch."),
            "settings" => ("Settings", "Playback and appearance."),
            "updates" => ("Updates", "Your player, always improving."),
            "about" => ("About", "Made for your moments."),
            _ => ("Now playing", "Enjoy the show.")
        };
        PageTitle.Text = title;
        PageSubtitle.Text = subtitle;

        foreach (var button in new[] { NavHome, NavMusic, NavVideos, NavSettings, NavUpdates, NavAbout })
        {
            bool selected = button.Tag is string tag && tag == section;
            button.Background = new SolidColorBrush(selected
                ? System.Windows.Media.Color.FromRgb(233, 242, 255)
                : System.Windows.Media.Colors.Transparent);
            button.Foreground = new SolidColorBrush(selected
                ? System.Windows.Media.Color.FromRgb(46, 107, 216)
                : System.Windows.Media.Color.FromRgb(81, 100, 124));
        }
    }

    private void ShowLibraryTab(string section)
    {
        NavigateChrome(section);
        _filter = section == "music" ? "Music" : "Videos";
        RefreshLibrary();
        LibraryLanding.Visibility = Visibility.Visible;
        LibraryIcon.Text = section == "music" ? "♫" : "▶";
        LibraryWelcome.Text = section == "music" ? "Your music" : "Your videos";
        LibraryHint.Text = "Double-click an item in your library to play,\nor choose Open files to add more.";
    }

    private void AutoFullscreen_Changed(object sender, RoutedEventArgs e)
    {
        if (_loading || _state == null) return;
        _state.AutoFullscreenVideo = AutoFullscreenCheck.IsChecked == true;
        LocalStore.Save(_state);
    }

    private void Window_ContentRendered(object? sender, EventArgs e)
    {
        // Render the shell first. Only load the heavyweight VLC engine when the
        // user plays media; idle library browsing should stay memory-light.
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (!Dispatcher.HasShutdownStarted) ScanHomeVideos();
        }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        TopStatus.Text = "Ready — choose a video or song";
    }

    private void EnsurePlaybackEngine()
    {
        if (_player != null) return;
        StartupDiagnostics.InitializeVlc();
        _engine = new LibVLCSharp.Shared.LibVLC(
            "--avcodec-hw=any", "--no-video-title-show",
            "--file-caching=350", "--network-caching=1000");
        _player = new LibVLCSharp.Shared.MediaPlayer(_engine);
        _player.Volume = Math.Clamp(_state.Volume, 0, 100);
        VideoSurface.MediaPlayer = _player;
        _player.EndReached += (_, _) => Dispatcher.BeginInvoke(new Action(NextAfterEnd));
        _player.Playing += (_, _) => Dispatcher.BeginInvoke(new Action(() =>
        {
            if (_pendingResume > 0 && _player.Length > _pendingResume + 5_000)
            {
                _player.Time = _pendingResume;
                _pendingResume = 0;
            }
            PlayPauseButton.Content = "Ⅱ Pause";
        }));
        _player.Paused += (_, _) => Dispatcher.BeginInvoke(new Action(() => PlayPauseButton.Content = "▶ Play"));
        _player.EncounteredError += (_, _) => Dispatcher.BeginInvoke(new Action(() =>
            TopStatus.Text = "Could not play this file; try another format."));
        TopStatus.Text = "Ready to play";
    }
}
