using System.Security.Cryptography;
using System.Text.Json;
using SugarySpireGMLoader.Models;

namespace SugarySpireGMLoader.Services;

/// <summary>
/// Deterministically applies supported manifest operations to a clean copy of data.win.
/// The first supported operation is exact STRG string replacement. This does not claim to
/// import arbitrary sprites, rooms, audio, or GML source; those require resource-specific importers.
/// </summary>
public static class GameDataBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static void RebuildDataFile(string gameDirectory, string dataFileName)
    {
#if UNDERTALEMODLIB
        RebuildWithUndertaleModLib(gameDirectory, dataFileName);
#else
        throw new InvalidOperationException(
            "UndertaleModLib is not configured. Clone UndertaleModTool with submodules and pass " +
            "-p:UndertaleModToolRoot=<clone path> to dotnet build.");
#endif
    }

#if UNDERTALEMODLIB
    private static void RebuildWithUndertaleModLib(string gameDirectory, string dataFileName)
    {
        var root = Path.GetFullPath(gameDirectory);
        var input = Path.Combine(root, dataFileName);
        if (!File.Exists(input))
            throw new FileNotFoundException("Game data file was not found.", input);

        var backup = Path.Combine(root, "data.win.gmloader-original");
        if (!File.Exists(backup))
            File.Copy(input, backup, overwrite: false);
        VerifyBackup(backup);

        var modsRoot = Path.Combine(root, "mods");
        var mods = LoadMods(modsRoot);
        var temp = input + ".gmloader.tmp";
        try
        {
            using (var stream = new FileStream(backup, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var data = UndertaleModLib.UndertaleIO.Read(stream, (warning, important) =>
                {
                    if (important)
                        Console.Error.WriteLine($"UndertaleModLib warning: {warning}");
                });

                foreach (var mod in mods)
                {
                    Console.WriteLine($"Applying {mod.Manifest.Name} ({mod.Manifest.Version}), priority {mod.Manifest.Priority}");
                    foreach (var patch in mod.Manifest.Patches)
                    {
                        if (!string.Equals(patch.Type, "replaceString", StringComparison.OrdinalIgnoreCase))
                            throw new InvalidDataException($"Unsupported patch type '{patch.Type}' in {mod.Manifest.Name}. Supported type: replaceString.");
                        ApplyStringReplacement(data, patch, mod.Manifest.Name);
                    }
                }

                using var output = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                UndertaleModLib.UndertaleIO.Write(output, data);
                output.Flush(true);
            }

            // Validate the generated file through the same parser before replacing the game file.
            using (var check = new FileStream(temp, FileMode.Open, FileAccess.Read, FileShare.Read))
                _ = UndertaleModLib.UndertaleIO.Read(check, (warning, important) =>
                {
                    if (important)
                        Console.Error.WriteLine($"Validation warning: {warning}");
                });

            File.Move(temp, input, overwrite: true);
            Console.WriteLine($"Rebuilt {input} from the original backup. SHA-256: {Sha256(input)}");
        }
        finally
        {
            if (File.Exists(temp))
                File.Delete(temp);
        }
    }

    private static void ApplyStringReplacement(UndertaleModLib.Models.UndertaleData data, ModPatch patch, string modName)
    {
        if (string.IsNullOrEmpty(patch.Target))
            throw new InvalidDataException($"replaceString in {modName} requires a non-empty 'target'.");
        if (patch.Value is null)
            throw new InvalidDataException($"replaceString in {modName} requires a 'value'.");

        var matches = data.Strings.Where(s => string.Equals(s.Content, patch.Target, StringComparison.Ordinal)).ToArray();
        if (matches.Length == 0)
            throw new InvalidDataException($"String target not found for {modName}: {patch.Target}");
        foreach (var entry in matches)
            entry.Content = patch.Value;
        Console.WriteLine($"  Replaced {matches.Length} STRG entry/entries: {patch.Target}");
    }

    private static List<LoadedMod> LoadMods(string modsRoot)
    {
        if (!Directory.Exists(modsRoot))
            return new List<LoadedMod>();

        var found = new List<LoadedMod>();
        foreach (var manifestPath in Directory.EnumerateFiles(modsRoot, "mod.json", SearchOption.AllDirectories))
        {
            var directory = Path.GetDirectoryName(manifestPath)!;
            var manifest = JsonSerializer.Deserialize<ModManifest>(File.ReadAllText(manifestPath), JsonOptions)
                ?? throw new InvalidDataException($"Invalid manifest: {manifestPath}");
            if (string.IsNullOrWhiteSpace(manifest.Name))
                throw new InvalidDataException($"Manifest has no name: {manifestPath}");
            if (manifest.Enabled)
                found.Add(new LoadedMod(directory, manifest));
        }

        // Deterministic ordering: lower priority applies first; name and path break ties.
        return found.OrderBy(m => m.Manifest.Priority)
            .ThenBy(m => m.Manifest.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(m => m.Directory, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void VerifyBackup(string backup)
    {
        using var stream = File.OpenRead(backup);
        if (stream.Length == 0)
            throw new InvalidDataException($"Original backup is empty: {backup}");
        Console.WriteLine($"Using immutable base backup {backup} ({Sha256(backup)})");
    }

    private static string Sha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private sealed record LoadedMod(string Directory, ModManifest Manifest);
#endif
}
