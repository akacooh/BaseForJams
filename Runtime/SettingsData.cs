using System;
using UnityEngine;

namespace BaseForJams
{
    /// <summary>
    /// ScriptableObject that holds all user-configurable settings.
    /// Add new fields here when expanding the settings system.
    /// Create via: Assets ▶ Create ▶ Base For Jams ▶ Settings Data
    /// </summary>
    [CreateAssetMenu(fileName = "SettingsData", menuName = "Base For Jams/Settings Data")]
    public class SettingsData : ScriptableObject
    {
        // ── Audio ─────────────────────────────────────────────────────────────
        [Header("Audio")]
        [Range(0f, 1f)]
        public float masterVolume = 1f;

        // ── Gameplay ──────────────────────────────────────────────────────────
        [Header("Gameplay")]
        [Range(0.1f, 10f)]
        public float mouseSensitivity = 1f;

        // ── Add future setting categories below, e.g.: ────────────────────────
        // [Header("Graphics")]
        // public int  qualityLevel = 2;
        // public bool fullscreen   = true;

        // ── Change notification ───────────────────────────────────────────────
        /// <summary>Fired whenever any setting is modified at runtime.</summary>
        public event Action OnSettingsChanged;

        /// <summary>Persists the current values and notifies listeners.
        /// Call after modifying any setting.</summary>
        public void NotifyChanged()
        {
            Save();
            OnSettingsChanged?.Invoke();
        }

        // ── Persistence (PlayerPrefs) ─────────────────────────────────────────
        private const string PrefsPrefix = "BaseForJams.";

        /// <summary>Restores saved values from PlayerPrefs. The values serialized
        /// in the asset act as defaults for keys that were never saved.
        /// Called by PauseMenu on startup.</summary>
        public void Load()
        {
            masterVolume     = PlayerPrefs.GetFloat(PrefsPrefix + nameof(masterVolume),     masterVolume);
            mouseSensitivity = PlayerPrefs.GetFloat(PrefsPrefix + nameof(mouseSensitivity), mouseSensitivity);
            OnSettingsChanged?.Invoke();
        }

        private void Save()
        {
            PlayerPrefs.SetFloat(PrefsPrefix + nameof(masterVolume),     masterVolume);
            PlayerPrefs.SetFloat(PrefsPrefix + nameof(mouseSensitivity), mouseSensitivity);
            PlayerPrefs.Save();
        }
    }
}
