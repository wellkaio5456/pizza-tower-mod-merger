using System.Text.Json;
using SugarySpireGMLoader.Models;

namespace SugarySpireGMLoader.Services;

public static class ModRegistryService
{
    private const string RegistryFileName = "gmloader-mods.json";
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    public static string GetRegistryPath(string gameDirectory) => Path.Combine(Path.GetFullPath(gameDirectory), RegistryFileName);

    public static ModList LoadInstalledMods(string gameDirectory)
    {
        var path = GetRegistryPath(gameDirectory);
        if (!File.Exists(path)) return new ModList();
        var list = JsonSerializer.Deserialize<ModList>(File.ReadAllText(path), Options)
            ?? throw new InvalidDataException($"Invalid mod registry: {path}");
        if (list.SchemaVersion != 1) throw new InvalidDataException($"Unsupported mod registry schema {list.SchemaVersion}.");
        if (list.Mods is null) throw new InvalidDataException("Mod registry has a null mods list.");
        return list;
    }

    public static void RegisterMod(string gameDirectory, string modName, string version, bool enabled)
    {
        if (string.IsNullOrWhiteSpace(modName)) throw new ArgumentException("Mod name is required.", nameof(modName));
        var root = Path.GetFullPath(gameDirectory);
        var list = LoadInstalledMods(root);
        var existing = list.Mods.FirstOrDefault(x => x.Name.Equals(modName, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            existing = new RegisteredMod { Name = modName };
            list.Mods.Add(existing);
        }
        existing.Version = version;
        existing.Enabled = enabled;
        existing.FolderPath = Path.Combine(root, "mods", modName);
        Save(list, root);
    }

    public static void SetModEnabled(string gameDirectory, string modName, bool enabled)
    {
        var list = LoadInstalledMods(gameDirectory);
        var found = list.Mods.SingleOrDefault(x => x.Name.Equals(modName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Mod not found in registry: {modName}");
        found.Enabled = enabled;
        Save(list, gameDirectory);
        Console.WriteLine($"Mod '{modName}' is now {(enabled ? "enabled" : "disabled")}");
    }

    public static void Save(ModList list, string gameDirectory)
    {
        var path = GetRegistryPath(gameDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temp = path + ".tmp";
        try
        {
            File.WriteAllText(temp, JsonSerializer.Serialize(list, Options));
            File.Move(temp, path, overwrite: true);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
}
