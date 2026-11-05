# Sugary Spire GMLoader

A starter adaptation of the GMLoader pattern for Sugary Spire: Exhibition Night Demo.

This project is intended for a local, user-owned copy of the demo. It acts as a safe mod-loader scaffold and does not attempt to bypass protections or anti-tamper systems.

## Included
- Game directory detection
- Required file validation
- Backup of the GameMaker data file
- Mod manifest reading and installation
- Active mod registry with enable/disable tracking
- Launch support for the demo executable
- Rebuild hook for deeper GameMaker data patching
- Optional GUI launcher

## Build
```bash
dotnet build SugarySpireGMLoader.sln
```

## CLI usage
```bash
dotnet run --project src/SugarySpireGMLoader/SugarySpireGMLoader.csproj -- detect "C:/Games/SugarySpire"

dotnet run --project src/SugarySpireGMLoader/SugarySpireGMLoader.csproj -- backup "C:/Games/SugarySpire"

dotnet run --project src/SugarySpireGMLoader/SugarySpireGMLoader.csproj -- install "C:/Games/SugarySpire" "C:/Games/SugarySpire/mods/example_mod"

dotnet run --project src/SugarySpireGMLoader/SugarySpireGMLoader.csproj -- launch "C:/Games/SugarySpire"
```

## GUI
```bash
dotnet run --project src/SugarySpireGMLoader.Gui/SugarySpireGMLoader.Gui.csproj
```

## Notes
This is a scaffolded fork of the GMLoader concept, adapted to a Sugary Spire demo profile.
The actual GameMaker recompilation and asset patching will require deeper integration with the demo's resource layout and a future UndertaleModLib or Underanalyzer workflow.

## Important
Use only on a version of the game that you own or are legally authorized to modify.
