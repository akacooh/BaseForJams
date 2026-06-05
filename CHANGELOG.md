# Changelog

All notable changes to this package are documented here.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-06-05

### Added
- State-based `GameStateManager` (pause without `Time.timeScale`) with `OnStateChanged` event.
- `PauseInput` — Escape/keyboard toggle supporting both the new Input System and the legacy Input Manager.
- `PauseMenu` UI — top-right pause button, settings container, Resume and Quit buttons.
- `StartScreen` — title screen with Play button and `OnGameStarted` event.
- Expandable settings system: `SettingsData` (ScriptableObject), abstract `SettingPanel`, plus `VolumePanel` and `SensitivityPanel`.
- `SceneSetup` editor tool: `Tools ▶ Base For Jams ▶ Setup Scene` (idempotent; auto-assigns `SettingsData`).
- Assembly definitions for runtime and editor code.
