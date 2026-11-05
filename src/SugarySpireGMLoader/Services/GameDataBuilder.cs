using System.IO;

namespace SugarySpireGMLoader.Services;

public static class GameDataBuilder
{
    public static void RebuildDataFile(string gameDirectory, string dataFileName)
    {
        var dataPath = Path.Combine(gameDirectory, dataFileName);

        if (!File.Exists(dataPath))
            throw new FileNotFoundException($"Game data file not found: {dataPath}", dataPath);

        Console.WriteLine($"Game data rebuild requested for: {dataPath}");
        Console.WriteLine("This is a scaffold hook for a deeper GameMaker patching pipeline.");
    }
}
