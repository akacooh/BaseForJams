using UnityEngine;
using UnityEngine.UI;

namespace BaseForJams
{
    /// <summary>
    /// Settings panel for mouse/look sensitivity.
    /// Reads/writes SettingsData.mouseSensitivity and notifies listeners on change.
    ///
    /// Your camera/controller can read the live value via:
    ///     SettingsData.mouseSensitivity
    /// or subscribe to SettingsData.OnSettingsChanged to react immediately.
    /// </summary>
    public class SensitivityPanel : SettingPanel
    {
        [Header("UI References")]
        [SerializeField] private Slider sensitivitySlider;

        [Header("Range")]
        [SerializeField] private float minSensitivity = 0.1f;
        [SerializeField] private float maxSensitivity = 10f;

        private SettingsData _data;

        public override void Initialize(SettingsData data)
        {
            _data = data;

            if (sensitivitySlider == null)
            {
                Debug.LogWarning("[SensitivityPanel] Slider reference is missing.", this);
                return;
            }

            sensitivitySlider.minValue = minSensitivity;
            sensitivitySlider.maxValue = maxSensitivity;
            sensitivitySlider.value    = _data.mouseSensitivity;

            sensitivitySlider.onValueChanged.RemoveListener(OnSliderChanged);
            sensitivitySlider.onValueChanged.AddListener(OnSliderChanged);
        }

        private void OnSliderChanged(float value)
        {
            _data.mouseSensitivity = value;
            _data.NotifyChanged();
        }
    }
}
