namespace SugarySpireGMLoader.Models;

public sealed class ModManifest
{
    public int ManifestVersion { get; set; } = 1;
    public string Name { get; set; } = "";
    public string Version { get; set; } = "1.0.0";
    public string Author { get; set; } = "Unknown";
    public string Description { get; set; } = "";
    // Manifest default; the install registry can override this value.
    public bool Enabled { get; set; } = true;
    public int Priority { get; set; }
    public List<string> Dependencies { get; set; } = new();
    public List<string> Conflicts { get; set; } = new();
    public List<ModPatch> Patches { get; set; } = new();
    public List<ModFileEntry> Files { get; set; } = new();
}

public sealed class ModPatch
{
    public string Type { get; set; } = "replaceString";
    public string Target { get; set; } = "";
    public string? Value { get; set; }
}

public sealed class ModFileEntry
{
    public string Source { get; set; } = "";
    public string Dest { get; set; } = "";
}
