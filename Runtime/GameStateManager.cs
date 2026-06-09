using System;
using UnityEngine;

namespace BaseForJams
{
    /// <summary>
    /// Owns the game's pause state as a simple state machine.
    ///
    /// Design notes:
    ///   • Does NOT manipulate Time.timeScale — systems subscribe to
    ///     <see cref="OnStateChanged"/> and suspend themselves accordingly.
    ///   • Does NOT read input — that is the job of <see cref="PauseInput"/>.
    ///     This keeps the state logic platform-agnostic and trivially testable.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────────
        public static GameStateManager Instance { get; private set; }

        // Support "Enter Play Mode Options" with domain reload disabled:
        // statics survive between editor play sessions and must be reset manually.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;

        // ── State ────────────────────────────────────────────────────────────
        public enum GameState { Playing, Paused }

        public GameState State { get; private set; } = GameState.Playing;
        public bool IsPaused => State == GameState.Paused;

        // ── Events ───────────────────────────────────────────────────────────
        /// <summary>Fired whenever the game state changes. Subscribe here to
        /// pause/resume your own systems (AI, physics, animations, etc.).</summary>
        public event Action<GameState> OnStateChanged;

        // ── Unity lifecycle ──────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        // ── Public API ───────────────────────────────────────────────────────
        public void Pause()  => SetState(GameState.Paused);
        public void Resume() => SetState(GameState.Playing);
        public void Toggle() => SetState(IsPaused ? GameState.Playing : GameState.Paused);

        private void SetState(GameState next)
        {
            if (State == next) return;
            State = next;
            OnStateChanged?.Invoke(State);
        }
    }
}
