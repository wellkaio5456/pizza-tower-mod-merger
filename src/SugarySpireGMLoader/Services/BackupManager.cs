using System.IO;

namespace SugarySpireGMLoader.Services;

public static class BackupManager
{
    public static void CreateBackup(string gameDirectory, string dataFileName, string backupFileName)
    {
        var source = Path.Combine(gameDirectory, dataFileName);
        var backup = Path.Combine(gameDirectory, backupFileName);

        if (!File.Exists(source))
            throw new FileNotFoundException($"Data file missing: {source}", source);

        if (File.Exists(backup))
            return;

        File.Copy(source, backup, overwrite: false);
        Console.WriteLine($"Created backup: {backup}");
    }

    public static void RestoreBackup(string gameDirectory, string dataFileName, string backupFileName)
    {
        var source = Path.Combine(gameDirectory, backupFileName);
        var target = Path.Combine(gameDirectory, dataFileName);

        if (!File.Exists(source))
            return;

        File.Copy(source, target, overwrite: true);
        Console.WriteLine($"Restored backup: {source}");
    }
}
