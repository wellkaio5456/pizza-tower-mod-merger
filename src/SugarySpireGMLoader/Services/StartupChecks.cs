using System.IO;
using SugarySpireGMLoader.GameSpecificData;
using SugarySpireGMLoader.Models;

namespace SugarySpireGMLoader.Services;

public static class StartupChecks
{
    public static GameProfile ValidateGameFolder(string gameDirectory)
    {
        if (string.IsNullOrWhiteSpace(gameDirectory))
            throw new InvalidOperationException("Game directory was not supplied.");

        if (!Directory.Exists(gameDirectory))
            throw new DirectoryNotFoundException(gameDirectory);

        var profile = GameProfileDetector.Detect(gameDirectory);

        foreach (var required in profile.RequiredFiles)
        {
            var fullPath = Path.Combine(gameDirectory, required);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Required file missing: {required}", fullPath);
        }

        return profile;
    }
}
