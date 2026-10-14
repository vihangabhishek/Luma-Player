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

    // Only the latest snapshot is relevant. Queue disk writes on one worker so
    // seeking, changing tracks and saving play positions cannot freeze the UI.
    private static readonly object SaveGate = new();
    private static string? _pendingJson;
    private static Task? _saveTask;

    public static void Save(PlayerState state)
    {
        try
        {
            // A short immutable JSON snapshot on the UI thread keeps the background
            // writer independent from subsequent library mutations.
            string json = JsonSerializer.Serialize(state);
            lock (SaveGate)
            {
                _pendingJson = json;
                if (_saveTask == null || _saveTask.IsCompleted)
                    _saveTask = Task.Run(DrainSaves);
            }
        }
        catch (Exception ex) { StartupDiagnostics.Write("Unable to queue player settings.", ex); }
    }

    private static void DrainSaves()
    {
        while (true)
        {
            string? json;
            lock (SaveGate)
            {
                json = _pendingJson;
                _pendingJson = null;
            }
            if (json == null) return;

            try
            {
                Directory.CreateDirectory(AppDirectory);
                string temporary = StatePath + ".tmp";
                File.WriteAllText(temporary, json);
                File.Move(temporary, StatePath, overwrite: true);
            }
            catch (Exception ex) { StartupDiagnostics.Write("Unable to persist player settings.", ex); }
        }
    }

    public static void FlushPending()
    {
        Task? active;
        lock (SaveGate) active = _saveTask;
        try { active?.Wait(TimeSpan.FromSeconds(3)); }
        catch (Exception ex) { StartupDiagnostics.Write("Pending library settings flush failed.", ex); }
    }
}
