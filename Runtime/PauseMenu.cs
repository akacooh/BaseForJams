using UnityEngine;
using UnityEngine.UI;

namespace BaseForJams
{
    /// <summary>
    /// Drives the Pause Menu UI.
    ///
    /// Hierarchy expected (created automatically by the SceneSetup editor tool):
    ///
    ///  PauseMenu  [this component]
    ///  ├─ PauseButton   (top-right corner toggle)
    ///  └─ PausePanel    (overlay panel)
    ///      ├─ Header
    ///      ├─ SettingsContainer
    ///      │   └─ VolumePanel  [VolumePanel component → Label + Slider]
    ///      └─ ButtonRow → Resume | Quit
    ///
    /// Adding new setting sections:
    ///   Add a new child GameObject to SettingsContainer, attach your
    ///   SettingPanel subclass — it will be auto-initialized here.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        // ── Inspector refs ───────────────────────────────────────────────────
        [Header("Core References")]
        [SerializeField] private GameStateManager gameState;
        [SerializeField] private SettingsData     settingsData;

        [Header("UI Elements")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button     pauseButton;   // top-right corner
        [SerializeField] private Button     resumeButton;  // inside panel
        [SerializeField] private Button     quitButton;    // inside panel

        // ── Unity lifecycle ──────────────────────────────────────────────────
        // Wiring happens in Start (not Awake) so the GameStateManager singleton
        // is guaranteed to exist regardless of Awake execution order.
        private void Start()
        {
            // Fall back to singleton if not assigned in Inspector
            if (gameState == null)
                gameState = GameStateManager.Instance;

            if (gameState == null)
            {
                Debug.LogError("[PauseMenu] No GameStateManager found. " +
                               "Assign it in the Inspector or ensure one exists in the scene.", this);
                return;
            }

            // Wire buttons
            pauseButton?.onClick.AddListener(gameState.Toggle);
            resumeButton?.onClick.AddListener(gameState.Resume);
#if UNITY_WEBGL && !UNITY_EDITOR
            // Application.Quit is a no-op in browsers — hide the button entirely.
            if (quitButton != null)
                quitButton.gameObject.SetActive(false);
#else
            quitButton?.onClick.AddListener(Quit);
#endif

            // Listen to state changes
            gameState.OnStateChanged += OnStateChanged;

            // Initialize all setting panels found in children
            if (settingsData != null)
            {
                settingsData.Load();
                foreach (var panel in GetComponentsInChildren<SettingPanel>(includeInactive: true))
                    panel.Initialize(settingsData);
            }
            else
            {
                Debug.LogWarning("[PauseMenu] No SettingsData assigned — setting panels will not initialize.", this);
            }

            // Set initial UI state
            SetPanelVisible(gameState.IsPaused);
        }

        private void OnDestroy()
        {
            if (gameState != null)
                gameState.OnStateChanged -= OnStateChanged;
        }

        // ── State handler ────────────────────────────────────────────────────
        private void OnStateChanged(GameStateManager.GameState state)
        {
            SetPanelVisible(state == GameStateManager.GameState.Paused);
        }

        private void SetPanelVisible(bool visible)
        {
            if (pausePanel != null)
                pausePanel.SetActive(visible);
        }

        // ── Quit ─────────────────────────────────────────────────────────────
        /// <summary>Quits the application (stops Play Mode in the Editor).</summary>
        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
