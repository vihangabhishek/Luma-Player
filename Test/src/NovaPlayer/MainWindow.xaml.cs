using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using Button = System.Windows.Controls.Button;
using TextBox = System.Windows.Controls.TextBox;
using ComboBox = System.Windows.Controls.ComboBox;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using DataFormats = System.Windows.DataFormats;
using KeyEventHandler = System.Windows.Input.KeyEventHandler;
using LibVLCSharp.Shared;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace NovaPlayer;

public partial class MainWindow : Window
{
    private LibVLC _engine = null!;
    private MediaPlayer _player = null!;
    private readonly DispatcherTimer _clock;
    private readonly PlayerState _state;
    private readonly ObservableCollection<MediaItem> _visible = new();
    private string _filter = "All";
    private MediaItem? _current;
    private string? _streamTitle;
    private bool _seeking;
    private bool _loading = true;
    private long _pendingResume;
    private long _lastResumeSave;
    private bool _compact;
    private bool _nativeMediaOpening;
    private readonly Random _random = new();
    private WindowState _previousWindowState;
    private WindowStyle _previousWindowStyle;
    private ResizeMode _previousResize;
    private GridLength _previousLeft = new(214), _previousRight = new(290);

    public MainWindow()
    {
        InitializeComponent();
        _state = LocalStore.Load();
        // Paint the application before the heavy VLC engine is loaded.
        VolumeSlider.Value = Math.Clamp(_state.Volume, 0, 100);
        Playlist.ItemsSource = _visible;
        SearchBox.Text = "";
        VideoSurface.Loaded += (_, _) => { if (_player != null) VideoSurface.MediaPlayer = _player; };
        _clock = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(750) };
        _clock.Tick += (_, _) => RefreshPosition();
        _clock.Start();
        _loading = false;
        RepeatButton.Content = "Repeat: " + _state.Repeat;
        ShuffleButton.Content = "Shuffle: " + (_state.Shuffle ? "On" : "Off");
        foreach (ComboBoxItem option in ThemePicker.Items)
            if ((string?)option.Content == _state.Theme) { ThemePicker.SelectedItem = option; break; }
        ApplyTheme();
        RefreshLibrary();
        InitializeHome();
        AutoFullscreenCheck.IsChecked = _state.AutoFullscreenVideo;
        AddHandler(Keyboard.KeyDownEvent, new KeyEventHandler(Window_KeyDown), true);
    }

    private void RefreshLibrary()
    {
        var query = SearchBox?.Text?.Trim() ?? "";
        // Avoid synchronous disk checks while drawing the library.
        var queryable = _state.Library.AsEnumerable();
        queryable = _filter switch
        {
            "Videos" => queryable.Where(x => MediaCatalog.IsVideo(x.Path)),
            "Music" => queryable.Where(x => !MediaCatalog.IsVideo(x.Path)),
            "Favorites" => queryable.Where(x => x.Favorite),
            "Recent" => queryable.Where(x => x.LastPlayedUtc != default).OrderByDescending(x => x.LastPlayedUtc),
            _ => queryable
        };
        var result = queryable.Where(x => x.Title.Contains(query, StringComparison.OrdinalIgnoreCase)).Take(650).ToArray();
        var selected = Playlist.SelectedItem as MediaItem;
        _visible.Clear();
        foreach (var item in result) _visible.Add(item);
        if (selected != null && _visible.Contains(selected)) Playlist.SelectedItem = selected;
        LibraryCount.Text = $"{_visible.Count} shown · {_state.Library.Count} saved";
    }

    private void AddMediaPaths(IEnumerable<string> paths, bool autoPlay)
    {
        MediaItem? firstNew = null;
        int added = 0;
        var known = _state.Library.ToDictionary(x => x.Path, StringComparer.OrdinalIgnoreCase);
        foreach (string path in paths)
        {
            if (!File.Exists(path) || !MediaCatalog.Supported(path)) continue;
            if (!known.TryGetValue(path, out var existing))
            {
                existing = new MediaItem { Path = path };
                known[path] = existing;
                _state.Library.Add(existing); added++;
            }
            firstNew ??= existing;
        }
        LocalStore.Save(_state);
        RefreshLibrary();
        ScanHomeVideos();
        TopStatus.Text = $"Added {added} media file(s)";
        if (autoPlay && firstNew != null) OpenMedia(firstNew);
    }

    private void OpenFiles_Click(object sender, RoutedEventArgs e)
    {
        var picker = new OpenFileDialog
        {
            Multiselect = true,
            Title = "Choose videos or music",
            Filter = "Media files|*.mp4;*.mkv;*.avi;*.mov;*.webm;*.wmv;*.flv;*.m4v;*.ts;*.mpeg;*.mpg;*.3gp;*.mp3;*.flac;*.wav;*.m4a;*.aac;*.ogg;*.opus;*.wma;*.aiff;*.ape;*.alac;*.mid|All files|*.*"
        };
        if (picker.ShowDialog(this) == true) AddMediaPaths(picker.FileNames, true);
    }

    private async void AddFolder_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog { Description = "Choose a music or video folder", UseDescriptionForTitle = true };
        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
        TopStatus.Text = "Scanning folder…";
        try
        {
            var files = await Task.Run(() => ScanFolder(dialog.SelectedPath).Take(15_000).ToArray());
            AddMediaPaths(files, false);
            TopStatus.Text = $"Folder scan complete · {files.Length} supported file(s) found";
        }
        catch (Exception ex) { TopStatus.Text = "Folder scan failed: " + ex.Message; }
    }

    private static IEnumerable<string> ScanFolder(string root)
    {
        var pending = new Stack<string>(); pending.Push(root);
        while (pending.Count != 0)
        {
            var folder = pending.Pop();
            string[] paths, dirs;
            try { paths = Directory.GetFiles(folder); dirs = Directory.GetDirectories(folder); }
            catch { continue; }
            foreach (var file in paths) if (MediaCatalog.Supported(file)) yield return file;
            foreach (var dir in dirs) pending.Push(dir);
        }
    }

    private async void OpenUrl_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new UrlDialog { Owner = this };
        if (dialog.ShowDialog() != true || dialog.Url == null) return;
        int request = ++_openGeneration;
        TopStatus.Text = "Connecting to stream…";
        try
        {
            await EnsurePlaybackEngineAsync();
            if (_playbackShuttingDown || request != _openGeneration) return;
            SaveResume();
            _current = null;
            _streamTitle = dialog.Url;
            _pendingResume = 0;
            ShowPlayer();
            NowTitle.Text = "Network stream";
            NowMeta.Text = dialog.Url;
            await _openGate.WaitAsync();
            try
            {
                if (request != _openGeneration) return;
                // Network media opening must not freeze the WPF message queue.
                _nativeMediaOpening = true;
                bool started = await Task.Run(() =>
                {
                    using var media = new Media(_engine, new Uri(dialog.Url));
                    return _player.Play(media);
                });
                _nativeMediaOpening = false;
                if (request == _openGeneration)
                    TopStatus.Text = started ? "Playing network stream" : "Stream could not be opened";
            }
            finally { _nativeMediaOpening = false; _openGate.Release(); }
        }
        catch (Exception ex)
        {
            StartupDiagnostics.Write("Unable to start network stream.", ex);
            TopStatus.Text = "Network stream unavailable";
        }
    }

    private async void OpenMedia(MediaItem item)
    {
        if (!File.Exists(item.Path))
        {
            TopStatus.Text = "File was moved or deleted. Please choose a new location.";
            return;
        }

        int request = ++_openGeneration;
        TopStatus.Text = "Preparing media…";
        try
        {
            await EnsurePlaybackEngineAsync();
            if (_playbackShuttingDown || request != _openGeneration) return;

            if (_current != item) SaveResume();
            _current = item;
            _streamTitle = null;
            _pendingResume = _state.ResumeEnabled ? Math.Max(0, item.ResumeMs) : 0;
            item.LastPlayedUtc = DateTime.UtcNow;
            ShowPlayer();
            bool video = MediaCatalog.IsVideo(item.Path);
            MusicTitle.Text = item.Title;
            NowTitle.Text = item.Title;
            NowMeta.Text = item.Secondary;

            // One native media-open operation at a time; newer selections take precedence.
            await _openGate.WaitAsync();
            try
            {
                if (_playbackShuttingDown || request != _openGeneration) return;
                _nativeMediaOpening = true;
                bool started = await Task.Run(() =>
                {
                    using var media = new Media(_engine, new Uri(item.Path));
                    return _player.Play(media);
                });
                _nativeMediaOpening = false;
                if (_playbackShuttingDown || request != _openGeneration) return;
                if (!started)
                {
                    TopStatus.Text = "Unable to open media — try another file";
                }
                else
                {
                    TopStatus.Text = video ? "Playing video" : "Playing music";
                    if (video && _state.AutoFullscreenVideo && !_compact)
                        Fullscreen_Click(this, new RoutedEventArgs());
                }
            }
            finally { _nativeMediaOpening = false; _openGate.Release(); }

            // Persistence is queued off-thread; update list without rescanning files.
            LocalStore.Save(_state);
            RefreshLibrary();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            StartupDiagnostics.Write("Unable to open media.", ex);
            TopStatus.Text = "Could not play this file — choose another";
        }
    }

    private void RefreshPosition()
    {
        if (_player == null || _seeking || _nativeMediaOpening || _playbackShuttingDown) return;
        long time = Math.Max(0, _player.Time), length = Math.Max(0, _player.Length);
        if (_pendingResume > 0 && length > _pendingResume + 5_000)
        {
            _player.Time = _pendingResume;
            _pendingResume = 0;
        }
        PositionText.Text = TimeLabel(time);
        DurationText.Text = TimeLabel(length);
        if (length > 0) SeekSlider.Value = Math.Clamp((double)time / length * 1000, 0, 1000);
        if (_player.IsPlaying && _current != null && Math.Abs(time - _lastResumeSave) > 12_000)
        {
            _lastResumeSave = time; _current.ResumeMs = time; LocalStore.Save(_state);
        }
    }
    private static string TimeLabel(long ms) { var t = TimeSpan.FromMilliseconds(ms); return t.TotalHours >= 1 ? $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}" : $"{t.Minutes:00}:{t.Seconds:00}"; }
    private void SaveResume()
    {
        if (_current == null || _player == null) return;
        if (_player.Time > 0) _current.ResumeMs = _player.Time;
        LocalStore.Save(_state);
    }
    private void NextAfterEnd()
    {
        if (_current != null) { _current.ResumeMs = 0; LocalStore.Save(_state); }
        if (_state.Repeat == "One" && _current != null) { OpenMedia(_current); return; }
        NextCore(true);
    }
    private void NextCore(bool fromEnd)
    {
        var queue = _visible.ToArray();
        if (queue.Length == 0) { if (fromEnd) PlayPauseButton.Content="▶  Play";return; }
        int current = Array.FindIndex(queue, x => x == _current);
        if (fromEnd && _state.Repeat == "Off" && current == queue.Length - 1 && !_state.Shuffle) { PlayPauseButton.Content = "▶  Play"; return; }
        var next = _state.Shuffle && queue.Length > 1 ? queue.Where(x => x != _current).ElementAt(_random.Next(queue.Length - (current >= 0 ? 1 : 0))) : queue[(current+1+queue.Length)%queue.Length];
        OpenMedia(next);
    }
    private void Previous_Click(object sender,RoutedEventArgs e)
    {
        if (_player.Time > 5_000) { _player.Time = 0; return; }
        var queue = _visible.ToArray();if(queue.Length == 0)return;
        int i = Array.FindIndex(queue,x=>x==_current);
        OpenMedia(queue[(i - 1 + queue.Length) % queue.Length]);
    }
    private void Next_Click(object sender,RoutedEventArgs e)=>NextCore(false);
    private async void PlayPause_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_player == null && _visible.Count > 0) { OpenMedia(_visible[0]); return; }
            if (_player == null) { OpenFiles_Click(sender, e); return; }
            // Pause/resume can synchronously wait on the native playback pipeline.
            await Task.Run(() =>
            {
                if (_player.IsPlaying) _player.Pause();
                else _player.Play();
            });
        }
        catch (Exception ex) { StartupDiagnostics.Write("Playback control failed.", ex); }
    }
    private void Back_Click(object sender,RoutedEventArgs e) => SeekBy(-10_000);
    private void Forward_Click(object sender,RoutedEventArgs e) => SeekBy(10_000);
    private void SeekBy(long delta) { if(_player.Length>0) _player.Time = Math.Clamp(_player.Time+delta,0,_player.Length); }
    private void Seek_Begin(object sender,MouseButtonEventArgs e)=>_seeking=true;
    private void Seek_End(object sender,MouseButtonEventArgs e)
    { if(_player.Length>0)_player.Time=(long)(_player.Length*SeekSlider.Value/1000.0);_seeking=false; }
    private void Volume_Changed(object sender,RoutedPropertyChangedEventArgs<double> e)
    { if(_loading||_player==null)return;_player.Volume=(int)e.NewValue;_state.Volume=(int)e.NewValue; }
    private void Mute_Click(object sender,RoutedEventArgs e)
    { if(_player==null)return;_player.Mute=!_player.Mute;MuteButton.Content=_player.Mute?"Muted":"◖))"; }
    private void Speed_Changed(object sender,SelectionChangedEventArgs e)
    { if(_loading||_player==null||SpeedPicker.SelectedItem is not ComboBoxItem item)return;if(float.TryParse(item.Tag?.ToString(),System.Globalization.CultureInfo.InvariantCulture,out float rate))_player.SetRate(rate); }
    private void Aspect_Changed(object sender,SelectionChangedEventArgs e)
    { if(_loading||_player==null||AspectPicker.SelectedItem is not ComboBoxItem item)return;_player.AspectRatio=string.IsNullOrEmpty(item.Tag?.ToString())?null:item.Tag.ToString(); }
    private void Subtitle_Click(object sender,RoutedEventArgs e)
    {
        if(_player.Media==null)return;
        var f=new OpenFileDialog{Filter="Subtitles|*.srt;*.ass;*.ssa;*.vtt;*.sub|All files|*.*",Title="Select subtitles"};
        if(f.ShowDialog(this)==true)
        {
            bool ok=_player.AddSlave(MediaSlaveType.Subtitle,new Uri(f.FileName).AbsoluteUri,true);
            TopStatus.Text=ok?"Subtitle file added":"Could not attach subtitles";
        }
    }
    private void Snapshot_Click(object sender,RoutedEventArgs e)
    {
        if(_current==null||!MediaCatalog.IsVideo(_current.Path)) { TopStatus.Text="Open a video to take a snapshot";return; }
        var picker=new SaveFileDialog{Filter="PNG image|*.png",FileName="Nova-Player-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".png"};
        if(picker.ShowDialog(this)==true)TopStatus.Text=_player.TakeSnapshot(0,picker.FileName,0,0)?"Screenshot saved":"Screenshot unavailable";
    }
    private void Fullscreen_Click(object sender,RoutedEventArgs e)
    {
        if (!_compact && (_player == null || (_current == null && _streamTitle == null)
            || (_current != null && !MediaCatalog.IsVideo(_current.Path)))) return;
        var outer = ShellGrid;
        if (!_compact)
        {
            _previousWindowState = WindowState;
            _previousWindowStyle = WindowStyle;
            _previousResize = ResizeMode;
            _previousLeft = outer.ColumnDefinitions[0].Width;
            _previousRight = outer.ColumnDefinitions[2].Width;
            NavPanel.Visibility = Visibility.Collapsed;
            LibraryPanel.Visibility = Visibility.Collapsed;
            TopHeader.Visibility = Visibility.Collapsed;
            PlaybackTools.Visibility = Visibility.Collapsed;
            TransportDock.Visibility = Visibility.Collapsed;
            outer.ColumnDefinitions[0].Width = new GridLength(0);
            outer.ColumnDefinitions[2].Width = new GridLength(0);
            outer.Margin = new Thickness(0);
            MainArea.Margin = new Thickness(0);
            PlayerContainer.CornerRadius = new CornerRadius(0);
            PlayerContainer.BorderThickness = new Thickness(0);
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
        }
        else
        {
            WindowState = WindowState.Normal;
            WindowStyle = _previousWindowStyle;
            ResizeMode = _previousResize;
            outer.ColumnDefinitions[0].Width = _previousLeft;
            outer.ColumnDefinitions[2].Width = _previousRight;
            outer.Margin = new Thickness(13, 12, 13, 0);
            MainArea.Margin = new Thickness(13, 0, 13, 0);
            PlayerContainer.CornerRadius = new CornerRadius(17);
            PlayerContainer.BorderThickness = new Thickness(1);
            NavPanel.Visibility = Visibility.Visible;
            TopHeader.Visibility = Visibility.Visible;
            WindowState = _previousWindowState;
            NavigateChrome("player");
            VideoSurface.Visibility = Visibility.Visible;
        }
        _compact = !_compact;
    }

    private void Playlist_DoubleClick(object sender,MouseButtonEventArgs e)
    { if(Playlist.SelectedItem is MediaItem item)OpenMedia(item); }
    private void Playlist_SelectionChanged(object sender,SelectionChangedEventArgs e) { }
    private void Filter_Click(object sender,RoutedEventArgs e)
    { if(sender is Button b && b.Tag is string filter) { _filter=filter;RefreshLibrary();ShowPlayer();TopStatus.Text=filter+" library"; } }
    private void Search_Changed(object sender,TextChangedEventArgs e) { if(!_loading&&_state!=null)RefreshLibrary(); }
    private void Shuffle_Click(object sender,RoutedEventArgs e)
    { _state.Shuffle=!_state.Shuffle;ShuffleButton.Content="Shuffle: "+(_state.Shuffle?"On":"Off");LocalStore.Save(_state); }
    private void Repeat_Click(object sender,RoutedEventArgs e)
    { _state.Repeat=_state.Repeat switch{"All"=>"One","One"=>"Off",_=>"All"};RepeatButton.Content="Repeat: "+_state.Repeat;LocalStore.Save(_state); }
    private void Favorite_Click(object sender,RoutedEventArgs e)
    { if(Playlist.SelectedItem is MediaItem item){item.Favorite=!item.Favorite;LocalStore.Save(_state);RefreshLibrary();TopStatus.Text=item.Favorite?"Added to favorites":"Removed from favorites";} }
    private void Remove_Click(object sender,RoutedEventArgs e)
    { if(Playlist.SelectedItem is MediaItem item){if(_current==item){_player.Stop();_current=null;EmptyVisual.Visibility=Visibility.Visible;VideoSurface.Visibility=Visibility.Collapsed;MusicVisual.Visibility=Visibility.Collapsed;} _state.Library.Remove(item);LocalStore.Save(_state);RefreshLibrary();} }
    private void Theme_Changed(object sender,SelectionChangedEventArgs e)
    { if(_loading||_state==null)return;_state.Theme=(ThemePicker.SelectedItem as ComboBoxItem)?.Content?.ToString()??"Blue";ApplyTheme();LocalStore.Save(_state); }
    private void ApplyTheme()
    {
        var colors=_state.Theme switch
        {
            "Violet"=>("#E9E6FF","#CAC4FA"),"Ocean"=>("#D9F5FF","#B7DDEF"),
            "Rose"=>("#FFE7F1","#DCC7E9"),"Midnight"=>("#C2D4F0","#7996C8"),_=>("#EAF4FF","#C5DDFF")
        };
        AppRoot.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors.Item1));
    }
    private void Window_KeyDown(object sender,System.Windows.Input.KeyEventArgs e)
    {
        if(Keyboard.FocusedElement is TextBox||Keyboard.FocusedElement is ComboBox)return;
        if (_player == null && e.Key != Key.O) return;
        switch(e.Key)
        {
            case Key.Space: PlayPause_Click(this,new RoutedEventArgs());e.Handled=true;break;
            case Key.Left: SeekBy(-5_000);e.Handled=true;break;
            case Key.Right: SeekBy(5_000);e.Handled=true;break;
            case Key.Up: VolumeSlider.Value=Math.Min(100,VolumeSlider.Value+5);e.Handled=true;break;
            case Key.Down: VolumeSlider.Value=Math.Max(0,VolumeSlider.Value-5);e.Handled=true;break;
            case Key.F11: Fullscreen_Click(this,new RoutedEventArgs());e.Handled=true;break;
            case Key.Escape: if(_compact)Fullscreen_Click(this,new RoutedEventArgs());e.Handled=true;break;
            case Key.O when Keyboard.Modifiers==ModifierKeys.Control: OpenFiles_Click(this,new RoutedEventArgs());e.Handled=true;break;
            case Key.N when Keyboard.Modifiers==ModifierKeys.Control: NextCore(false);e.Handled=true;break;
        }
    }
    private void Window_Drop(object sender,System.Windows.DragEventArgs e)
    { if(e.Data.GetDataPresent(DataFormats.FileDrop) && e.Data.GetData(DataFormats.FileDrop) is string[] files)AddMediaPaths(files,true); }
    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        _playbackShuttingDown = true;
        ++_openGeneration;
        _clock.Stop();
        try
        {
            if (!_nativeMediaOpening) SaveResume();
            _state.Volume = (int)VolumeSlider.Value;
            LocalStore.Save(_state);
            LocalStore.FlushPending();
        }
        catch (Exception ex) { StartupDiagnostics.Write("Unable to finish saving on shutdown.", ex); }

        if (_player != null)
        {
            // WPF-owned video view must be detached on the UI thread.
            try { VideoSurface.MediaPlayer = null; }
            catch (Exception ex) { StartupDiagnostics.Write("Video surface could not detach.", ex); }

            var player = _player;
            var engine = _engine;
            // Stop and native disposal can block on a decoder thread. Do not
            // freeze the closing window while libvlc waits for playback to stop.
            _ = Task.Run(async () =>
            {
                await _openGate.WaitAsync();
                try
                {
                    player.Stop();
                    player.Dispose();
                    engine.Dispose();
                }
                catch (Exception ex) { StartupDiagnostics.Write("Playback cleanup failed.", ex); }
                finally { _openGate.Release(); }
            });
        }
    }
}
