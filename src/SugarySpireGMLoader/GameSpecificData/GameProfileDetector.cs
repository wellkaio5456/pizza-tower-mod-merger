using System.IO;
using SugarySpireGMLoader.GameSpecificData.Definitions;
using SugarySpireGMLoader.Models;

namespace SugarySpireGMLoader.GameSpecificData;

public static class GameProfileDetector
{
    public static GameProfile Detect(string gameDirectory)
    {
        if (string.IsNullOrWhiteSpace(gameDirectory))
            throw new InvalidOperationException("Game directory was not supplied.");

        var full = Path.GetFullPath(gameDirectory);

        if (!Directory.Exists(full))
            throw new DirectoryNotFoundException($"Game directory does not exist: {full}");

        if (!SugarySpireProfile.IsLikelyGameRoot(full))
            throw new InvalidOperationException(
                "This folder does not appear to contain a valid Sugary Spire Exhibition Night install.");

        var profile = SugarySpireProfile.BuildProfile();

        foreach (var required in profile.RequiredFiles)
        {
            var filePath = Path.Combine(full, required);
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Required file is missing: {required}", filePath);
        }

        return profile;
    }
}
