using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace BaseForJams
{
    /// <summary>
    /// Listens for the pause toggle input and forwards it to the <see cref="GameStateManager"/>.
    /// All platform-specific input code is isolated here so GameStateManager stays pure state.
    ///
    /// Supports both the new Input System and the legacy Input Manager automatically
    /// via the ENABLE_INPUT_SYSTEM compile define.
    /// </summary>
    [RequireComponent(typeof(GameStateManager))]
    public class PauseInput : MonoBehaviour
    {
        private GameStateManager _state;

#if !ENABLE_INPUT_SYSTEM
        [Header("Legacy Input Manager")]
        [Tooltip("Key that toggles the pause menu.")]
        [SerializeField] private KeyCode toggleKey = KeyCode.Escape;
#endif

        private void Awake() => _state = GetComponent<GameStateManager>();

        private void Update()
        {
            // Ignore the toggle while the title screen is up — pausing there is
            // meaningless and Escape opening a menu over the title looks broken.
            if (StartScreen.Instance != null && StartScreen.Instance.IsShowing)
                return;

            if (WasTogglePressed())
                _state.Toggle();
        }

        private bool WasTogglePressed()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // Browsers reserve Escape for exiting pointer lock / fullscreen, so the
            // key fights the browser on WebGL — the on-screen pause button is the
            // only toggle there.
            return false;
#elif ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(toggleKey);
#endif
        }
    }
}
