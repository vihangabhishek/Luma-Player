using System.IO;
using System.Text.Json.Serialization;

namespace LumaPlayer;

public sealed class MediaItem
{
    public string Path { get; set; } = "";
    public bool Favorite { get; set; }
    public long ResumeMs { get; set; }
    public DateTime LastPlayedUtc { get; set; }
    [JsonIgnore] public string Title => System.IO.Path.GetFileNameWithoutExtension(Path);
    [JsonIgnore] public string Kind => MediaCatalog.IsVideo(Path) ? "VIDEO" : "MUSIC";
    [JsonIgnore] public string Secondary => $"{Kind}  ·  {System.IO.Path.GetExtension(Path).TrimStart('.').ToUpperInvariant()}";
}

public sealed class PlayerState
{
    public List<MediaItem> Library { get; set; } = new();
    public int Volume { get; set; } = 75;
    public string Theme { get; set; } = "Blue";
    public string Repeat { get; set; } = "All";
    public bool Shuffle { get; set; }
    public bool ResumeEnabled { get; set; } = true;
}

public static class MediaCatalog
{
    public static readonly HashSet<string> Video = new(StringComparer.OrdinalIgnoreCase)
    {".mp4",".mkv",".avi",".mov",".webm",".wmv",".flv",".m4v",".ts",".mpeg",".mpg",".3gp"};
    public static readonly HashSet<string> Audio = new(StringComparer.OrdinalIgnoreCase)
    {".mp3",".flac",".wav",".m4a",".aac",".ogg",".opus",".wma",".aiff",".ape",".alac",".mid"};
    public static bool IsVideo(string path) => Video.Contains(System.IO.Path.GetExtension(path));
    public static bool Supported(string path) => IsVideo(path) || Audio.Contains(System.IO.Path.GetExtension(path));
}
