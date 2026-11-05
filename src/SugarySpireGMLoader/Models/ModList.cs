namespace SugarySpireGMLoader.Models;

public sealed class ModList
{
    public List<RegisteredMod> Mods { get; set; } = new();
}

public sealed class RegisteredMod
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "1.0.0";
    public bool Enabled { get; set; } = true;
    public string FolderPath { get; set; } = "";
}
