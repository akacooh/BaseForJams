# Base For Jams

A drop-in foundation for game jams. Adds, with one click, the boilerplate UI every jam game needs:

- **State-based pause** — pauses via a `GameState` machine, **not** `Time.timeScale`. Systems subscribe to `GameStateManager.OnStateChanged` and suspend themselves.
- **Top-right pause button** + Escape-to-toggle (new Input System or legacy).
- **Start screen** — title + Play button. Fires `StartScreen.OnGameStarted`.
- **Expandable settings menu** — volume & sensitivity sliders included, values persist via PlayerPrefs.
- **Restart button** — resumes and reloads the active scene.
- **Quit button** — quits the build / stops Play Mode in the editor (hidden on WebGL).

## Installation

Requires the **Input System** package (auto-resolved via dependencies).

- **Git:** Package Manager → *+* → *Install package from git URL* → `https://github.com/akacooh/BaseForJams.git`
- **Local:** add to `Packages/manifest.json`:
  `"com.akacooh.baseforjams": "file:/abs/path/to/BaseForJams"`

## Quick start

1. `Tools ▶ Base For Jams ▶ Setup Scene` — builds the GameStateManager, EventSystem, Canvas, StartScreen and PauseMenu, and auto-assigns a `SettingsData` asset (created at `Assets/SettingsData.asset` if none exists).
2. Optionally create your own settings asset: `Assets ▶ Create ▶ Base For Jams ▶ Settings Data`.
3. Hook gameplay to the Play button:
   ```csharp
   StartScreen.Instance.OnGameStarted += BeginGame;
   ```

## Adding a new setting

1. Add a field to `SettingsData` (e.g. `public float mouseSensitivity = 1f;`).
2. Make a panel: `public class MyPanel : SettingPanel { ... }` — override `Initialize(SettingsData)`.
3. Add a child under the PauseMenu's `SettingsContainer` with your panel component (or add a `BuildSettingRow<MyPanel>(...)` line in `SceneSetup`).

`PauseMenu` auto-discovers every `SettingPanel` in its children and initializes them.

## Reading settings at runtime

```csharp
float look = lookInput * settingsData.mouseSensitivity;
settingsData.OnSettingsChanged += ApplySettings; // react to live changes
```
