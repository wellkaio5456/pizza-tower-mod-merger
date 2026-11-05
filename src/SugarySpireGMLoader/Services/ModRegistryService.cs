using System.IO;
using System.Text.Json;
using SugarySpireGMLoader.Models;

namespace SugarySpireGMLoader.Services;

public static class ModRegistryService
{
    private const string RegistryFileName = "gmloader-mods.json";

    public static string GetRegistryPath(string gameDirectory)
    {
        return Path.Combine(gameDirectory, RegistryFileName);
    }

    public static ModList LoadInstalledMods(string gameDirectory)
    {
        var path = GetRegistryPath(gameDirectory);
        if (!File.Exists(path))
            return new ModList();

        var json = File.ReadAllText(path);
        var list = JsonSerializer.Deserialize<ModList>(json);
        return list ?? new ModList();
    }

    public static void RegisterMod(string gameDirectory, string modName, string version, bool enabled)
    {
        var mods = LoadInstalledMods(gameDirectory);
        var existing = mods.Mods.FirstOrDefault(x => x.Name == modName);

        if (existing is not null)
        {
            existing.Version = version;
            existing.Enabled = enabled;
            existing.FolderPath = Path.Combine(gameDirectory, "mods", modName);
        }
        else
        {
            mods.Mods.Add(new RegisteredMod
            {
                Name = modName,
                Version = version,
                Enabled = enabled,
                FolderPath = Path.Combine(gameDirectory, "mods", modName)
            });
        }

        Save(mods, gameDirectory);
    }

    public static void SetModEnabled(string gameDirectory, string modName, bool enabled)
    {
        var mods = LoadInstalledMods(gameDirectory);
        var found = mods.Mods.FirstOrDefault(x => x.Name.Equals(modName, StringComparison.OrdinalIgnoreCase));

        if (found is null)
            throw new InvalidOperationException($"Mod not found in registry: {modName}");

        found.Enabled = enabled;
        Save(mods, gameDirectory);
        Console.WriteLine($"Mod '{modName}' is now {(enabled ? "enabled" : "disabled")}");
    }

    public static void Save(ModList mods, string gameDirectory)
    {
        var path = GetRegistryPath(gameDirectory);
        var json = JsonSerializer.Serialize(mods, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(path, json);
    }
}
