using SugarySpireGMLoader.Services;

var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
if (args.Length == 0) { PrintHelp(); return 1; }

try
{
    var command = args[0].ToLowerInvariant();
    var directory = args.Length > 1 ? args[1] : ".";
    switch (command)
    {
        case "detect":
        {
            var profile = StartupChecks.ValidateGameFolder(directory);
            Console.WriteLine($"Detected: {profile.Name}\nExecutable: {profile.ExecutableName}\nData: {profile.DataFileName}");
            return 0;
        }
        case "backup":
        {
            var profile = StartupChecks.ValidateGameFolder(directory);
            BackupManager.CreateBackup(directory, profile.DataFileName, profile.BackupFileName);
            return 0;
        }
        case "install":
            if (args.Length < 3) throw new ArgumentException("install requires <game-folder> <mod-folder>.");
            StartupChecks.ValidateGameFolder(directory);
            ModInstaller.Install(directory, args[2]);
            return 0;
        case "listmods":
        {
            var list = ModRegistryService.LoadInstalledMods(directory);
            foreach (var mod in list.Mods.OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase))
                Console.WriteLine($"{mod.Name} [{(mod.Enabled ? "enabled" : "disabled")}] v{mod.Version}");
            return 0;
        }
        case "enablemod":
        case "disablemod":
            if (args.Length < 3) throw new ArgumentException($"{command} requires <game-folder> <mod-name>.");
            ModRegistryService.SetModEnabled(directory, args[2], command == "enablemod");
            return 0;
        case "rebuild":
        {
            var profile = StartupChecks.ValidateGameFolder(directory);
            BackupManager.CreateBackup(directory, profile.DataFileName, profile.BackupFileName);
            GameDataBuilder.RebuildDataFile(directory, profile.DataFileName);
            return 0;
        }
        case "launch":
        {
            var profile = StartupChecks.ValidateGameFolder(directory);
            Launcher.LaunchExecutable(directory, profile.ExecutableName);
            return 0;
        }
        default: PrintHelp(); return 1;
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}

static void PrintHelp()
{
    Console.WriteLine("Sugary Spire GMLoader + Mod Merger");
    Console.WriteLine("  detect <game-folder>\n  backup <game-folder>\n  install <game-folder> <mod-folder>\n  listmods <game-folder>\n  enablemod <game-folder> <mod-name>\n  disablemod <game-folder> <mod-name>\n  rebuild <game-folder>\n  launch <game-folder>");
}
