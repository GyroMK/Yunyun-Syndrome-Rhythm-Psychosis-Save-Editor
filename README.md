# Yunyun Syndrome · Save Editor

A GUI save editor for **Yunyun Syndrome**. It decrypts the `save_global` and
`save_slotN` files, lets you edit your progress, songs, theories and unlocks,
and re-encrypts them so the game can read them back.

> ⚠️ **Disclaimer:** unofficial tool, not affiliated with Alliance Arts. Edit your
> own saves at your own risk. Keep backups (the app makes one automatically in
> `backups/` every time you save).

![Yunyun Save Editor](docs/screenshot.png)

## Features

- 🔓 Automatically decrypts/encrypts the saves (AES‑256‑CBC).
- 🎚️ **Progress** tab: edit `DenpaPlayPoint` (the denpa bar), parameters and flags.
- 🎵 **Songs** tab: editable table with each song's score (with real song titles).
- 🧩 **Theories** tab: editable table of conspiracy theories (with real titles).
- 🌳 **Advanced** tab: a tree with **every** field in the save; edit, add and delete any value.
- ⚡ Quick actions: complete all songs, unlock all songs, get all theories.
- 🎯 **Set denpa to %**: type a target % and it computes the exact `DenpaPlayPoint` for you.
- 💾 Automatic backup before every save.
- 🌍 Multi-language UI: **English, Español, 日本語** (live switch; remembers your choice).

## Usage

1. **Close the game.**
2. In Steam, turn off the cloud for this game while editing:
   *Library → right-click Yunyun Syndrome → Properties → General →
   uncheck "Keep games saves in the Steam Cloud"* (otherwise the cloud may
   overwrite your changes).
3. Open **`YunyunSaveEditor.exe`**.
4. Pick the file (`save_global` or a slot) and click **Open / Reload**.
5. Edit whatever you want and click **Save to game**.
6. Launch the game and check. Not what you wanted? Repeat.

The save folder is detected automatically at:
`%USERPROFILE%\AppData\LocalLow\AllianceArts\Yunyun_Syndrome\player`
(otherwise use **Change folder…**).

### About the denpa percentage
The denpa % does **not** come from `DenpaPlayPoint` alone, but from the **total**:

```
total = DenpaPlayPoint + song points + theory points + ending points
```

(each "perfect" song = 480 pts, each ending = 2,500 pts, theories depend on rarity).
The editor computes your total and current %, and the **"Apply %"** button sets the
exact `DenpaPlayPoint` needed to reach the % you ask for (accounting for what songs,
theories and endings already contribute). Reference totals:

| %  | points | %   | points  |
|----|--------|-----|---------|
| 10 | 5,800  | 70  | 85,100  |
| 30 | 22,300 | 90  | 125,600 |
| 50 | 49,600 | 100 | 150,600 |

The real song/theory names and the denpa table were extracted from the game's own
data (master data and localization tables).

## Download

Grab `YunyunSaveEditor.exe` from the **Releases** section. It's a single
self-contained executable: no need to install .NET or anything else (Windows x64).

## Build from source

Requires the **.NET 9** SDK.

```bash
dotnet build                       # development build
# Single-file, self-contained executable (no .NET needed to run):
dotnet publish -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:EnableCompressionInSingleFile=true
# -> bin/Release/net9.0-windows/win-x64/publish/YunyunSaveEditor.exe
# (IncludeNativeLibrariesForSelfExtract is REQUIRED for a WPF single-file build,
#  otherwise it crashes at startup with System.DllNotFoundException.)
```

## Save format (technical notes)

- Encryption: **AES‑256‑CBC + PKCS7**. The **IV is the first 16 bytes** of the file;
  the rest is the ciphertext. The content is **UTF‑8 JSON** (no BOM).
- `save_global`: collection and progress (`DenpaPlayPoint`, `ScoreRecords`,
  `ConspiracyTheory`, `SongUnlockDatas`, `PictureUnlockDatas`, `EndingData`…).
- `save_slotN`: story progress (`Episode`, `FlagData`, …).

## Translations / Languages

The UI is localized (English, Español, 日本語). Switch it with the selector in the
top-right corner; your choice is saved to `language.txt`.

**Add a language** (PRs welcome!): in [`Loc.cs`](Loc.cs)
1. Add the code to `Loc.Available`, e.g. `("fr", "Français")`.
2. Copy the `["en"] = new() { ... }` dictionary as `["fr"] = new() { ... }`
   and translate the values (keep the **keys** unchanged).

Every string in the app comes from there, so nothing else needs changing.

## License

MIT. See [LICENSE](LICENSE).
