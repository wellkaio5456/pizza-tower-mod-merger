using System.IO;
using System.Text.Json;
using SugarySpireGMLoader.Models;

namespace SugarySpireGMLoader.Services;

public static class ModInstaller
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static void Install(string gameDirectory, string modDirectory)
    {
        var gameRoot = Path.GetFullPath(gameDirectory);
        var sourceRoot = Path.GetFullPath(modDirectory);
        if (!Directory.Exists(gameRoot)) throw new DirectoryNotFoundException(gameRoot);
        if (!Directory.Exists(sourceRoot)) throw new DirectoryNotFoundException(sourceRoot);

        var manifestPath = Path.Combine(sourceRoot, "mod.json");
        if (!File.Exists(manifestPath)) throw new FileNotFoundException("Mod manifest missing: mod.json", manifestPath);
        var manifest = JsonSerializer.Deserialize<ModManifest>(File.ReadAllText(manifestPath), JsonOptions)
            ?? throw new InvalidDataException("Invalid mod.json.");
        ValidateName(manifest.Name);

        // Validate every entry before copying anything. Reject traversal and duplicate destinations.
        var copies = new List<(string Source, string Destination)>();
        var destinations = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        foreach (var entry in manifest.Files)
        {
            if (string.IsNullOrWhiteSpace(entry.Source) || string.IsNullOrWhiteSpace(entry.Dest))
                throw new InvalidDataException("Each file entry requires source and dest paths.");
            var source = ResolveUnder(sourceRoot, entry.Source);
            var destination = ResolveUnder(gameRoot, entry.Dest);
            if (!File.Exists(source)) throw new FileNotFoundException($"Mod file missing: {entry.Source}", source);
            if (!destinations.Add(destination)) throw new InvalidDataException($"Duplicate destination: {entry.Dest}");
            copies.Add((source, destination));
        }

        // Stage content before touching game files; create the registry only after all copies succeed.
        foreach (var (source, destination) in copies)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(source, destination, overwrite: true);
        }
        ModRegistryService.RegisterMod(gameRoot, manifest.Name, manifest.Version, manifest.Enabled);
        Console.WriteLine($"Installed mod: {manifest.Name} v{manifest.Version} ({copies.Count} loose files)");
        if (manifest.Patches.Count > 0)
            Console.WriteLine("Manifest data.win patches are picked up by the rebuild command.");
    }

    private static string ResolveUnder(string root, string relative)
    {
        if (Path.IsPathRooted(relative)) throw new InvalidDataException($"Rooted paths are not allowed: {relative}");
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var full = Path.GetFullPath(Path.Combine(root, relative));
        if (!full.StartsWith(fullRoot, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new InvalidDataException($"Path escapes its allowed directory: {relative}");
        return full;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name is "." or ".." || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name.Contains('/') || name.Contains('\\'))
            throw new InvalidDataException("Mod name must be a simple folder-safe identifier.");
    }
}
