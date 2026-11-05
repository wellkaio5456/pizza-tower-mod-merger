# Sugary Spire GMLoader + Mod Merger

A local mod manager and deterministic `data.win` patch builder for the Sugary Spire: Exhibition Night demo. It uses UndertaleModLib from the official UndertaleModTool source tree. This repository's earlier Pizza Tower mod-merger description is retained as project history; the new profile targets Sugary Spire, and no Pizza Tower-specific compatibility is implied.

## What the real data pipeline does

- Reads and writes GameMaker data with `UndertaleIO.Read` / `UndertaleIO.Write`.
- Creates a one-time `data.win.gmloader-original` base backup and builds every patch pass from that clean base, avoiding cumulative corruption.
- Applies enabled mod manifests in deterministic order: ascending `priority`, then mod name, then folder path.
- Supports exact replacement of existing STRG string entries (`replaceString`).
- Writes to a temporary file, parses the output again for validation, then atomically replaces `data.win`.
- Keeps loose-file install support separate from the `data.win` patch pipeline.

This is a working resource-level data.win rewrite path, not a universal importer for every GameMaker resource. It does **not** currently import sprites, texture pages, rooms, audio groups, arbitrary GML source, or compile arbitrary mod scripts. Those need dedicated importers and game-specific tests; a blind full rewrite of all resource types would be unsafe. Use a copy of the demo and retain the original backup.

## Dependency setup

UndertaleModLib is part of UndertaleModTool and currently targets .NET 10. It is not referenced as a random NuGet package. Clone its source including submodules:

```bash
git clone --recurse-submodules https://github.com/UnderminersTeam/UndertaleModTool.git
```

Build the CLI against that checkout (Windows PowerShell example):

```powershell
dotnet build SugarySpireGMLoader.sln -p:UndertaleModToolRoot="C:\src\UndertaleModTool"
```

Or set `UMT_ROOT` to the checkout path. The current UndertaleModTool project uses .NET 10; install the .NET 10 SDK. Without that source checkout, the tool intentionally refuses `rebuild` rather than pretending to perform a patch.

## Mod format

Put one `mod.json` in each mod folder below `<game-folder>/mods/`:

```json
{
  "name": "sample-text-mod",
  "version": "1.0.0",
  "enabled": true,
  "priority": 10,
  "patches": [
    {
      "type": "replaceString",
      "target": "Original exact STRG text",
      "value": "Replacement text"
    }
  ]
}
```

Targets are exact, ordinal matches against STRG resources. If an earlier mod changes a target, a later mod can target that earlier replacement. Missing targets and unknown operation types abort the build before `data.win` is replaced.

## CLI

```bash
dotnet run --project src/SugarySpireGMLoader/SugarySpireGMLoader.csproj -- detect "C:/Games/SugarySpire"
dotnet run --project src/SugarySpireGMLoader/SugarySpireGMLoader.csproj -- backup "C:/Games/SugarySpire"
dotnet run --project src/SugarySpireGMLoader/SugarySpireGMLoader.csproj -- rebuild "C:/Games/SugarySpire"
dotnet run --project src/SugarySpireGMLoader/SugarySpireGMLoader.csproj -- launch "C:/Games/SugarySpire"
```

`rebuild` applies enabled manifests found recursively in the game's `mods` directory. It starts from `data.win.gmloader-original` every time. To reset the backup, manually remove that backup only after restoring the game's pristine `data.win` from your own installer/source.

## GUI

The WPF GUI is Windows-only and provides folder selection, detection, backup, install, and launch controls. Build the solution with the .NET 10 Windows targeting pack installed.

## Licensing

The original merger project is MIT-licensed. UndertaleModLib is GPL-3.0; distributing a combined binary that includes it may impose GPL obligations. Review both licenses before redistribution. This project does not include UndertaleModLib source or proprietary game data.
