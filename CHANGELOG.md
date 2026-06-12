# Changelog

All notable changes to this package are documented here.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.1] - 2026-06-12

### Fixed
- Compile error in `SceneSetup` (`FindObjectsByType` was called with `FindObjectsInactive` instead of `FindObjectsSortMode`).

## [1.1.0] - 2026-06-10

### Added
- Settings now persist via PlayerPrefs (`SettingsData.Load()`, auto-saved on change).
- Restart button in the pause menu (resumes, then reloads the active scene).
- UnityEvent mirrors for Inspector wiring: `onPaused`/`onResumed` on `GameStateManager`, `onGameStarted` on `StartScreen`.
- TextMeshPro support — used automatically when the TMP package (or uGUI 2.0+) is present, with legacy `Text` fallback.
- Support for Enter Play Mode Options with domain reload disabled (statics and SO event subscribers reset on play start).

### Changed
- `SceneSetup` creates `Assets/SettingsData.asset` when none exists and no longer picks the (immutable) asset from inside packages.
- Generated Canvas uses `ScaleWithScreenSize` (1920×1080 reference).
- `PauseMenu` wires up in `Start` instead of `Awake` (avoids singleton ordering race).
- Sensitivity range is defined once in `SettingsData` (`Min`/`MaxMouseSensitivity` constants).
- Minimum Unity version declared as 2021.3.18f1.

### Fixed
- WebGL: Quit button is hidden (`Application.Quit` is a no-op in browsers) and the Escape pause toggle is disabled (browsers reserve Escape).
- Pause toggle key is ignored while the start screen is showing.
- `StartScreen` destroys duplicate instances instead of silently overwriting the singleton.

### Removed
- Bundled `SettingsData.asset` (created in the user project instead).

## [1.0.0] - 2026-06-05

### Added
- State-based `GameStateManager` (pause without `Time.timeScale`) with `OnStateChanged` event.
- `PauseInput` — Escape/keyboard toggle supporting both the new Input System and the legacy Input Manager.
- `PauseMenu` UI — top-right pause button, settings container, Resume and Quit buttons.
- `StartScreen` — title screen with Play button and `OnGameStarted` event.
- Expandable settings system: `SettingsData` (ScriptableObject), abstract `SettingPanel`, plus `VolumePanel` and `SensitivityPanel`.
- `SceneSetup` editor tool: `Tools ▶ Base For Jams ▶ Setup Scene` (idempotent; auto-assigns `SettingsData`).
- Assembly definitions for runtime and editor code.
