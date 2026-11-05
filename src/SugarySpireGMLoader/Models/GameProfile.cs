namespace SugarySpireGMLoader.Models;

public sealed record GameProfile(
    string Name,
    string ExecutableName,
    string DataFileName,
    string BackupFileName,
    string ModFolderName,
    string[] RequiredFiles,
    string[] OptionalFiles,
    string[] SupportedRuntimes);
