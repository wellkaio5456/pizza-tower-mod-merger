namespace SugarySpireGMLoader.Models;

public sealed class ModManifest
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "1.0.0";
    public string Author { get; set; } = "Unknown";
    public string Description { get; set; } = "";
    public bool Enabled { get; set; } = true;
    public int Priority { get; set; }
    public List<ModPatch> Patches { get; set; } = new();
    // Legacy loose-file entries are retained for the existing installer, but are not applied to data.win.
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
