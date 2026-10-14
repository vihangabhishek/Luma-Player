using System.IO;
using System.Text.Json;
namespace LumaPlayer;

public static class LocalStore
{
    public static readonly string AppDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LumaPlayer");
    public static readonly string StatePath = System.IO.Path.Combine(AppDirectory,"state.json");
    public static PlayerState Load()
    {
        try
        {
            if (!File.Exists(StatePath)) return new PlayerState();
            var state = JsonSerializer.Deserialize<PlayerState>(File.ReadAllText(StatePath)) ?? new PlayerState();
            state.Library = state.Library.Where(x => !string.IsNullOrWhiteSpace(x.Path)).GroupBy(x => x.Path,StringComparer.OrdinalIgnoreCase).Select(x=>x.First()).ToList();
            return state;
        }
        catch { return new PlayerState(); }
    }
    public static void Save(PlayerState state)
    {
        Directory.CreateDirectory(AppDirectory);
        string tmp = StatePath + ".tmp";
        File.WriteAllText(tmp,JsonSerializer.Serialize(state,new JsonSerializerOptions{WriteIndented=true}));
        File.Move(tmp,StatePath,true);
    }
}
