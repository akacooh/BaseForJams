using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace BaseForJams
{
    /// <summary>
    /// Settings panel for master volume.
    /// Drives either an AudioMixer exposed parameter or AudioListener.volume directly.
    /// </summary>
    public class VolumePanel : SettingPanel
    {
        [Header("UI References")]
        [SerializeField] private Slider volumeSlider;

        [Header("Audio (optional)")]
        [Tooltip("Assign an AudioMixer to control its exposed 'MasterVolume' parameter. " +
                 "If left empty, AudioListener.volume is used instead.")]
        [SerializeField] private AudioMixer audioMixer;

        [Tooltip("The exact exposed parameter name on the AudioMixer.")]
        [SerializeField] private string mixerParameter = "MasterVolume";

        private SettingsData _data;

        // ── SettingPanel ─────────────────────────────────────────────────────
        public override void Initialize(SettingsData data)
        {
            _data = data;

            if (volumeSlider == null)
            {
                Debug.LogWarning("[VolumePanel] Slider reference is missing.", this);
                return;
            }

            // Sync slider to saved value
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value    = _data.masterVolume;

            volumeSlider.onValueChanged.RemoveListener(OnSliderChanged);
            volumeSlider.onValueChanged.AddListener(OnSliderChanged);

            ApplyVolume(_data.masterVolume);
        }

        // ── Internal ─────────────────────────────────────────────────────────
        private void OnSliderChanged(float value)
        {
            _data.masterVolume = value;
            _data.NotifyChanged();
            ApplyVolume(value);
        }

        private void ApplyVolume(float linear)
        {
            if (audioMixer != null)
            {
                // AudioMixers use decibels; guard against log(0)
                float db = linear > 0.0001f
                    ? Mathf.Log10(linear) * 20f
                    : -80f;
                audioMixer.SetFloat(mixerParameter, db);
            }
            else
            {
                AudioListener.volume = linear;
            }
        }
    }
}
