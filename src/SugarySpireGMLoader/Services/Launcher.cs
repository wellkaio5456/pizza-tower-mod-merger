using System.Diagnostics;
using System.IO;

namespace SugarySpireGMLoader.Services;

public static class Launcher
{
    public static void LaunchExecutable(string gameDirectory, string executableName)
    {
        var exePath = Path.Combine(gameDirectory, executableName);

        if (!File.Exists(exePath))
            throw new FileNotFoundException($"Executable not found: {exePath}", exePath);

        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            WorkingDirectory = gameDirectory,
            UseShellExecute = true
        };

        Process.Start(psi);
        Console.WriteLine($"Launched: {exePath}");
    }
}
