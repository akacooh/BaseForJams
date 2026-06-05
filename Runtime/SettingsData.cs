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

        public void NotifyChanged() => OnSettingsChanged?.Invoke();
    }
}
