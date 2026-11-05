# Mod format (version 1)

Each mod directory under `<game-folder>/mods/` contains a `mod.json`. Mods are ordered with dependencies first, then ascending priority, then case-insensitive name for deterministic conflict resolution. A mod's registry enabled state overrides the manifest default. Missing dependencies, conflicts, duplicate names, cycles, unsupported patch types, or missing patch targets stop the rebuild before replacing the current data file.

Supported patch operation:

```json
{
  "type": "replaceString",
  "target": "Exact existing STRG content",
  "value": "Replacement"
}
```

This first release intentionally limits binary resource mutation to exact string replacement. It does not claim support for script compilation or sprite, texture, room, font, shader, or audio imports.
