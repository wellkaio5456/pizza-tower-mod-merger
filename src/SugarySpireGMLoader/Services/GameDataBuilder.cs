using System.Security.Cryptography;
using System.Text.Json;
using SugarySpireGMLoader.Models;

namespace SugarySpireGMLoader.Services;

/// <summary>Deterministic, transactional UndertaleModLib data.win patch builder.</summary>
public static class GameDataBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static void RebuildDataFile(string gameDirectory, string dataFileName)
    {
#if UNDERTALEMODLIB
        RebuildWithUndertaleModLib(gameDirectory, dataFileName);
#else
        throw new InvalidOperationException("UndertaleModLib is not configured. Build with -p:UndertaleModToolRoot=<UndertaleModTool clone path>.");
#endif
    }

#if UNDERTALEMODLIB
    private static void RebuildWithUndertaleModLib(string gameDirectory, string dataFileName)
    {
        var root = Path.GetFullPath(gameDirectory);
        var input = Path.Combine(root, dataFileName);
        if (!File.Exists(input)) throw new FileNotFoundException("Game data file was not found.", input);

        var backup = Path.Combine(root, "data.win.gmloader-original");
        if (!File.Exists(backup)) File.Copy(input, backup, overwrite: false);
        if (new FileInfo(backup).Length == 0) throw new InvalidDataException($"Original backup is empty: {backup}");

        var mods = LoadEnabledMods(root);
        var temp = input + ".gmloader.tmp";
        try
        {
            using (var stream = new FileStream(backup, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var data = UndertaleModLib.UndertaleIO.Read(stream, (warning, important) =>
                {
                    Console.Error.WriteLine($"UndertaleModLib {(important ? "warning" : "notice")}: {warning}");
                    if (important) throw new InvalidDataException($"Input data.win warning: {warning}");
                });

                foreach (var mod in mods)
                {
                    Console.WriteLine($"Applying {mod.Manifest.Name} {mod.Manifest.Version} (priority {mod.Manifest.Priority})");
                    foreach (var patch in mod.Manifest.Patches)
                    {
                        if (!patch.Type.Equals("replaceString", StringComparison.OrdinalIgnoreCase))
                            throw new InvalidDataException($"Unsupported patch type '{patch.Type}' in {mod.Manifest.Name}; supported: replaceString.");
                        ApplyStringReplacement(data, patch, mod.Manifest.Name);
                    }
                }

                using var output = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                UndertaleModLib.UndertaleIO.Write(output, data);
                output.Flush(flushToDisk: true);
            }

            using (var check = new FileStream(temp, FileMode.Open, FileAccess.Read, FileShare.Read))
                _ = UndertaleModLib.UndertaleIO.Read(check, (warning, important) =>
                {
                    if (important) throw new InvalidDataException($"Output validation warning: {warning}");
                });

            File.Move(temp, input, overwrite: true);
            Console.WriteLine($"Rebuild successful. data.win SHA-256: {Sha256(input)}");
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }

    private static void ApplyStringReplacement(UndertaleModLib.Models.UndertaleData data, ModPatch patch, string modName)
    {
        if (string.IsNullOrEmpty(patch.Target) || patch.Value is null)
            throw new InvalidDataException($"replaceString in {modName} requires non-empty target and value fields.");
        var matches = data.Strings.Where(s => string.Equals(s.Content, patch.Target, StringComparison.Ordinal)).ToArray();
        if (matches.Length == 0) throw new InvalidDataException($"String target not found for {modName}: {patch.Target}");
        foreach (var entry in matches) entry.Content = patch.Value;
        Console.WriteLine($"  STRG replacements: {matches.Length}");
    }

    private static List<LoadedMod> LoadEnabledMods(string root)
    {
        var modsRoot = Path.GetFullPath(Path.Combine(root, "mods"));
        var registry = ModRegistryService.LoadInstalledMods(root);
        var overrides = registry.Mods.ToDictionary(x => x.Name, x => x.Enabled, StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(modsRoot)) return new();

        var all = new Dictionary<string, LoadedMod>(StringComparer.OrdinalIgnoreCase);
        foreach (var manifestPath in Directory.EnumerateFiles(modsRoot, "mod.json", SearchOption.AllDirectories))
        {
            var folder = Path.GetDirectoryName(manifestPath)!;
            EnsureUnder(modsRoot, folder);
            var manifest = JsonSerializer.Deserialize<ModManifest>(File.ReadAllText(manifestPath), JsonOptions)
                ?? throw new InvalidDataException($"Invalid manifest: {manifestPath}");
            if (manifest.ManifestVersion != 1) throw new InvalidDataException($"Unsupported manifest version in {manifestPath}");
            if (string.IsNullOrWhiteSpace(manifest.Name)) throw new InvalidDataException($"Missing mod name in {manifestPath}");
            if (!all.TryAdd(manifest.Name, new LoadedMod(folder, manifest)))
                throw new InvalidDataException($"Duplicate mod name (case-insensitive): {manifest.Name}");
        }

        var enabled = all.Values.Where(m => overrides.TryGetValue(m.Manifest.Name, out var state) ? state : m.Manifest.Enabled).ToDictionary(m => m.Manifest.Name, StringComparer.OrdinalIgnoreCase);
        foreach (var mod in enabled.Values)
        {
            foreach (var dependency in mod.Manifest.Dependencies)
                if (!enabled.ContainsKey(dependency)) throw new InvalidDataException($"Enabled mod '{mod.Manifest.Name}' requires missing or disabled mod '{dependency}'.");
            foreach (var conflict in mod.Manifest.Conflicts)
                if (enabled.ContainsKey(conflict)) throw new InvalidDataException($"Enabled mods conflict: '{mod.Manifest.Name}' and '{conflict}'.");
        }

        // Stable Kahn topological sort. Dependencies always precede dependents; priority orders otherwise-ready mods.
        var result = new List<LoadedMod>();
        var remaining = new HashSet<string>(enabled.Keys, StringComparer.OrdinalIgnoreCase);
        while (remaining.Count > 0)
        {
            var next = remaining.Select(id => enabled[id])
                .Where(m => m.Manifest.Dependencies.All(d => !remaining.Contains(d)))
                .OrderBy(m => m.Manifest.Priority)
                .ThenBy(m => m.Manifest.Name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
            if (next is null) throw new InvalidDataException("Mod dependency cycle detected: " + string.Join(", ", remaining.OrderBy(x => x)));
            result.Add(next);
            remaining.Remove(next.Manifest.Name);
        }
        return result;
    }

    private static void EnsureUnder(string root, string candidate)
    {
        var prefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!Path.GetFullPath(candidate).StartsWith(prefix, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new InvalidDataException($"Mod manifest escapes mods directory: {candidate}");
    }

    private static string Sha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private sealed record LoadedMod(string Directory, ModManifest Manifest);
#endif
}
