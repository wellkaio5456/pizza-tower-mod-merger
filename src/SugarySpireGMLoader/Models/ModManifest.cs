namespace SugarySpireGMLoader.Models;

public sealed class ModManifest
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "1.0.0";
    public string Author { get; set; } = "Unknown";
    public string Description { get; set; } = "";
    public bool Enabled { get; set; } = true;
    public List<ModFileEntry> Files { get; set; } = new();
}

public sealed class ModFileEntry
{
    public string Source { get; set; } = "";
    public string Dest { get; set; } = "";
}
