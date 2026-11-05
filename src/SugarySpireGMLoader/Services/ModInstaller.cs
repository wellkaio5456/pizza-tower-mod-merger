using System.IO;
using System.Text.Json;
using SugarySpireGMLoader.Models;

namespace SugarySpireGMLoader.Services;

public static class ModInstaller
{
    public static void Install(string gameDirectory, string modDirectory)
    {
        if (!Directory.Exists(gameDirectory))
            throw new DirectoryNotFoundException(gameDirectory);

        if (!Directory.Exists(modDirectory))
            throw new DirectoryNotFoundException(modDirectory);

        var manifestPath = Path.Combine(modDirectory, "mod.json");
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException("Mod manifest missing: mod.json", manifestPath);

        var json = File.ReadAllText(manifestPath);
        var manifest = JsonSerializer.Deserialize<ModManifest>(json)
            ?? throw new InvalidOperationException("Invalid mod.json");

        foreach (var entry in manifest.Files)
        {
            var source = Path.Combine(modDirectory, entry.Source);
            var destination = Path.Combine(gameDirectory, entry.Dest);

            if (!File.Exists(source))
                throw new FileNotFoundException($"Missing mod asset: {source}", source);

            var destDir = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(destDir))
                Directory.CreateDirectory(destDir);

            File.Copy(source, destination, overwrite: true);
        }

        ModRegistryService.RegisterMod(gameDirectory, manifest.Name, manifest.Version, true);

        Console.WriteLine($"Installed mod: {manifest.Name} v{manifest.Version}");
    }
}
