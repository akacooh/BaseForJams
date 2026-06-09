using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BaseForJams
{
    /// <summary>
    /// Full-screen title screen shown at launch. Displays the game name and a
    /// Play button that starts the game. The pause menu remains usable while
    /// this screen is visible (the pause button renders above it).
    ///
    /// Hook gameplay startup by subscribing to <see cref="OnGameStarted"/>.
    /// </summary>
    public class StartScreen : MonoBehaviour
    {
        public static StartScreen Instance { get; private set; }

        // Support "Enter Play Mode Options" with domain reload disabled:
        // statics survive between editor play sessions and must be reset manually.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;

        [Header("References")]
        [Tooltip("The root object to show/hide for the whole start screen.")]
        [SerializeField] private GameObject screenRoot;
        [SerializeField] private Button     playButton;
        [SerializeField] private Text       titleText;

        [Header("Title")]
        [Tooltip("Game title. Leave empty to use the Project's Product Name.")]
        [SerializeField] private string titleOverride = "";

        /// <summary>True while the start screen is visible.</summary>
        public bool IsShowing => screenRoot != null && screenRoot.activeSelf;

        /// <summary>Raised when the player presses Play. Subscribe to begin gameplay.</summary>
        public event Action OnGameStarted;

        /// <summary>Inspector-friendly mirror of <see cref="OnGameStarted"/> —
        /// wire scene reactions here without writing code.</summary>
        [Header("Inspector Events")]
        [SerializeField] private UnityEvent onGameStarted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (titleText != null)
                titleText.text = string.IsNullOrEmpty(titleOverride)
                    ? Application.productName
                    : titleOverride;

            if (playButton != null)
                playButton.onClick.AddListener(StartGame);

            Show();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        // ── Public API ───────────────────────────────────────────────────────
        public void StartGame()
        {
            Hide();
            OnGameStarted?.Invoke();
            onGameStarted?.Invoke();
        }

        public void Show()
        {
            if (screenRoot != null) screenRoot.SetActive(true);
        }

        public void Hide()
        {
            if (screenRoot != null) screenRoot.SetActive(false);
        }
    }
}
