using UnityEngine;

namespace BaseForJams
{
    /// <summary>
    /// Abstract base for every settings panel section.
    /// To add a new settings category:
    ///   1. Create a new MonoBehaviour that inherits SettingPanel.
    ///   2. Override Initialize() to wire up your UI controls to SettingsData.
    ///   3. Add the component to a child GameObject inside the PauseMenu panel.
    ///
    /// The PauseMenu will call Initialize(data) on every SettingPanel it finds.
    /// </summary>
    public abstract class SettingPanel : MonoBehaviour
    {
        /// <summary>Called once by PauseMenu after SettingsData is assigned.</summary>
        public abstract void Initialize(SettingsData data);
    }
}
