using UnityEngine;
using UnityEngine.SceneManagement;

namespace ParkingMastery.Core
{
    public enum GameState { Loading, Playing, Paused, Won, Failed }

    /// <summary>
    /// Oyun durumu makinesi. Geçerli geçişler:
    ///   Loading → Playing
    ///   Playing → Paused | Won | Failed | Loading
    ///   Paused  → Playing | Loading
    ///   Won     → Loading
    ///   Failed  → Playing (ödüllü reklamla devam) | Loading
    /// Geçersiz geçiş yok sayılır (editör/development'ta uyarı verir). Her değişiklikte GameStateChanged yayınlanır.
    /// Sahne yüklenince Loading olur; seviyeyi kuran LevelLoader hazır olunca (Start) Playing yapar (B7).
    /// ParkingSuccessful → Won, LevelFailed → Failed, ResumeGame → Playing. Menü sahnelerinde durum Loading kalır.
    /// </summary>
    public static class GameStateMachine
    {
        public static GameState Current { get; private set; } = GameState.Loading;

        public static bool CanTransition(GameState from, GameState to)
        {
            switch (from)
            {
                case GameState.Loading: return to == GameState.Playing;
                case GameState.Playing: return to == GameState.Paused || to == GameState.Won || to == GameState.Failed || to == GameState.Loading;
                case GameState.Paused: return to == GameState.Playing || to == GameState.Loading;
                case GameState.Won: return to == GameState.Loading;
                case GameState.Failed: return to == GameState.Playing || to == GameState.Loading;
                default: return false;
            }
        }

        /// <summary>Durumu değiştirir. Zaten o durumdaysa true (olay yayınlanmaz); geçişe izin yoksa false.</summary>
        public static bool TryChange(GameState next)
        {
            if (Current == next) return true;
            if (!CanTransition(Current, next))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning("[GameState] Geçersiz geçiş yok sayıldı: " + Current + " → " + next);
#endif
                return false;
            }

            var previous = Current;
            Current = next;
            EventBus.Publish(new GameStateChanged(previous, next));
            return true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            Current = GameState.Loading;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Hook()
        {
            // Abonelikler EventBus'ta tekilleştirilir; tekrar çağrı çift abonelik yapmaz.
            EventBus.Subscribe(GameEventType.ParkingSuccessful, OnParkingSuccessful);
            EventBus.Subscribe(GameEventType.LevelFailed, OnLevelFailed);
            EventBus.Subscribe(GameEventType.ResumeGame, OnResumeGame);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Editörde doğrudan bir seviye sahnesinden Play'e basılırsa ilk sahne için sceneLoaded kaçırılmış olabilir.
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryChange(GameState.Loading);
        }

        private static void OnParkingSuccessful() => TryChange(GameState.Won);
        private static void OnLevelFailed() => TryChange(GameState.Failed);
        private static void OnResumeGame() => TryChange(GameState.Playing);
    }
}
