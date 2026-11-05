using SugarySpireGMLoader.Services;

var args = Environment.GetCommandLineArgs().Skip(1).ToArray();

if (args.Length == 0)
{
    PrintHelp();
    return 1;
}

try
{
    var command = args[0].ToLowerInvariant();

    switch (command)
    {
        case "detect":
            var detectDir = args.Length > 1 ? args[1] : ".";
            var profile = StartupChecks.ValidateGameFolder(detectDir);
            Console.WriteLine($"Detected: {profile.Name}");
            Console.WriteLine($"Executable: {profile.ExecutableName}");
            Console.WriteLine($"Game data: {profile.DataFileName}");
            return 0;

        case "backup":
            var backupDir = args.Length > 1 ? args[1] : ".";
            var backupProfile = StartupChecks.ValidateGameFolder(backupDir);
            BackupManager.CreateBackup(backupDir, backupProfile.DataFileName, backupProfile.BackupFileName);
            return 0;

        case "install":
            var installDir = args.Length > 1 ? args[1] : ".";
            var modDir = args.Length > 2 ? args[2] : Path.Combine(installDir, "mods", "example_mod");
            StartupChecks.ValidateGameFolder(installDir);
            ModInstaller.Install(installDir, modDir);
            return 0;

        case "listmods":
            var listDir = args.Length > 1 ? args[1] : ".";
            var mods = ModRegistryService.LoadInstalledMods(listDir);
            foreach (var mod in mods.Mods)
            {
                Console.WriteLine($"{mod.Name} [{(mod.Enabled ? "enabled" : "disabled")}] v{mod.Version}");
            }
            return 0;

        case "enablemod":
            var enableDir = args.Length > 1 ? args[1] : ".";
            var enableModName = args.Length > 2 ? args[2] : string.Empty;
            ModRegistryService.SetModEnabled(enableDir, enableModName, true);
            return 0;

        case "disablemod":
            var disableDir = args.Length > 1 ? args[1] : ".";
            var disableModName = args.Length > 2 ? args[2] : string.Empty;
            ModRegistryService.SetModEnabled(disableDir, disableModName, false);
            return 0;

        case "rebuild":
            var rebuildDir = args.Length > 1 ? args[1] : ".";
            var rebuildProfile = StartupChecks.ValidateGameFolder(rebuildDir);
            BackupManager.CreateBackup(rebuildDir, rebuildProfile.DataFileName, rebuildProfile.BackupFileName);
            GameDataBuilder.RebuildDataFile(rebuildDir, rebuildProfile.DataFileName);
            return 0;

        case "launch":
            var launchDir = args.Length > 1 ? args[1] : ".";
            var launchProfile = StartupChecks.ValidateGameFolder(launchDir);
            Launcher.LaunchExecutable(launchDir, launchProfile.ExecutableName);
            return 0;

        default:
            PrintHelp();
            return 1;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    return 1;
}

static void PrintHelp()
{
    Console.WriteLine("Sugary Spire GMLoader");
    Console.WriteLine("");
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run -- detect <game-folder>");
    Console.WriteLine("  dotnet run -- backup <game-folder>");
    Console.WriteLine("  dotnet run -- install <game-folder> <mod-folder>");
    Console.WriteLine("  dotnet run -- listmods <game-folder>");
    Console.WriteLine("  dotnet run -- enablemod <game-folder> <mod-name>");
    Console.WriteLine("  dotnet run -- disablemod <game-folder> <mod-name>");
    Console.WriteLine("  dotnet run -- rebuild <game-folder>");
    Console.WriteLine("  dotnet run -- launch <game-folder>");
}
