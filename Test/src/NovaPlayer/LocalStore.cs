using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace NovaPlayer;

public static class LocalStore
{
    public static readonly string AppDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NovaPlayer");
    public static readonly string StatePath = Path.Combine(AppDirectory, "state.json");

    // The old location is used only if Nova has not saved a settings file yet.
    // Keeping the old file unchanged protects existing Luma Player libraries.
    private static readonly string LegacyStatePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LumaPlayer", "state.json");

    public static PlayerState Load()
    {
        try
        {
            string path = File.Exists(StatePath) ? StatePath : LegacyStatePath;
            if (!File.Exists(path)) return new PlayerState();
            var state = JsonSerializer.Deserialize<PlayerState>(File.ReadAllText(path)) ?? new PlayerState();
            state.Library ??= new List<MediaItem>();
            state.Library = state.Library.Where(x => !string.IsNullOrWhiteSpace(x.Path))
                .GroupBy(x => x.Path, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First()).ToList();
            return state;
        }
        catch (Exception ex)
        {
            StartupDiagnostics.Write("Unable to read saved library settings.", ex);
            return new PlayerState();
        }
    }

    public static void Save(PlayerState state)
    {
        Directory.CreateDirectory(AppDirectory);
        string temporary = StatePath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporary, StatePath, true);
    }
}
