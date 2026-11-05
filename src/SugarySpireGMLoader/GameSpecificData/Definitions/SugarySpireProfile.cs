namespace SugarySpireGMLoader.GameSpecificData.Definitions;

public static class SugarySpireProfile
{
    public const string GameName = "Sugary Spire: Exhibition Night Demo";
    public const string ExecutableName = "SugarySpire_ExhibitionNight.exe";
    public const string DataFileName = "data.win";
    public const string BackupFileName = "data.win.backup";
    public const string ModFolderName = "mods";

    public static readonly string[] SupportedRuntimes =
    {
        "GameMaker 2023.11",
        "GameMaker 2023.11.1.160",
        "GM:Studio / GM2 2023.11"
    };

    public static readonly string[] RequiredFiles =
    {
        DataFileName,
        ExecutableName
    };

    public static readonly string[] OptionalFiles =
    {
        "fmod.dll",
        "fmodstudio.dll",
        "steam_api.dll",
        "config.ini",
        "language.ini"
    };

    public static readonly string[] AudioFiles =
    {
        "fmod.dll",
        "fmodstudio.dll"
    };

    public static bool IsLikelyGameRoot(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
            return false;

        var full = System.IO.Path.GetFullPath(rootPath);

        if (!System.IO.Directory.Exists(full))
            return false;

        var hasDataWin = System.IO.File.Exists(System.IO.Path.Combine(full, DataFileName));
        var hasExe = System.IO.File.Exists(System.IO.Path.Combine(full, ExecutableName));

        return hasDataWin || hasExe;
    }

    public static GameProfile BuildProfile()
    {
        return new GameProfile(
            Name: GameName,
            ExecutableName: ExecutableName,
            DataFileName: DataFileName,
            BackupFileName: BackupFileName,
            ModFolderName: ModFolderName,
            RequiredFiles: RequiredFiles,
            OptionalFiles: OptionalFiles,
            SupportedRuntimes: SupportedRuntimes);
    }
}
