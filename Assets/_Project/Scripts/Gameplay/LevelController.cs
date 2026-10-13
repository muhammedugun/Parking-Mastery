using ParkingMastery.Core;
using ParkingMastery.Services;
using UnityEngine;

namespace ParkingMastery.Gameplay
{
    /// <summary>
    /// Level akışı (C1, eski GameLoopManager): başlat (Playing) → kazan/kaybet → yeniden dene / sonraki level.
    /// Seviye verisi LevelLoader'dan gelir; yıldız LevelData.CalculateStars ile hesaplanır.
    /// </summary>
    public class LevelController : MonoBehaviour
    {
        /// <summary>Pop-up'ların kapatılmasını ister (UI katmanı abone olur; Gameplay, UI'ya bağımlı değildir).</summary>
        public static event System.Action CloseAllPopupsRequested;

        private const string LeaderboardName = "Leaderboard";

        [SerializeField] private LevelLoader _loader;
        [SerializeField] private LevelScoring _scoring;

        private void Start()
        {
            // GameStateMachine sahne yüklenince Loading yapar (sceneLoaded Start'tan önce); seviye hazırsa başlat.
            if (_loader != null && _loader.Current != null) GameStateMachine.TryChange(GameState.Playing);
        }

        private void OnEnable()
        {
            GameServices.Ads.RewardGranted += ResumeLevel;
            EventBus.Subscribe(GameEventType.ParkingSuccessful, CompleteLevel);
        }

        private void OnDisable()
        {
            GameServices.Ads.RewardGranted -= ResumeLevel;
            EventBus.Unsubscribe(GameEventType.ParkingSuccessful, CompleteLevel);
        }

        private void ResumeLevel(string id)
        {
            if (id != AdRewardIds.Resume) return;

            CloseAllPopupsRequested?.Invoke();
            ResumeGame();
            EventBus.Publish(GameEventType.ResumeGame);
        }

        /// <summary>Aynı seviyeyi baştan kurar (LevelSession seviyeyi korur).</summary>
        public static void RestartLevel()
        {
            SceneLoadManager.RestartScene();
        }

        public static void PauseGame()
        {
            Time.timeScale = 0;
            GameStateMachine.TryChange(GameState.Paused);
        }

        public static void ResumeGame()
        {
            Time.timeScale = 1;
            GameStateMachine.TryChange(GameState.Playing);
        }

        public static void LoadNextLevel()
        {
            int nextLevelNumber = LevelSession.CurrentLevelId + 1;
            // Son seviyeden sonra seviye seçimine dön (B7).
            if (nextLevelNumber > LevelSession.LevelCount)
                SceneLoadManager.LoadSceneStatic("LevelSelectionMenu");
            else
                SceneLoadManager.LoadSceneStatic(nextLevelNumber.ToString());
        }

        /// <summary>Park başarılı: sonraki leveli açar, yıldızı (iyileştiyse) kaydeder, skor tablosunu günceller.</summary>
        private void CompleteLevel()
        {
            if (_loader == null || _loader.Current == null || _scoring == null)
            {
                Debug.LogError("[LevelController] LevelLoader/LevelScoring atanmamış veya seviye yüklenmemiş; ilerleme kaydedilmedi.");
                return;
            }

            var saves = GameServices.Saves;
            int levelId = _loader.Current.LevelId;
            saves.UnlockLevel(levelId + 1);

            int rewardedStarCount = _scoring.CalculateStars();

            // En iyi süre (C4): yalnız tamamlanan seviyede, daha kısaysa güncellenir.
            saves.TrySetBestTime(levelId, _scoring.ElapsedSeconds);

            // Kayıt dizini: yıldızlar seviye numarasıyla (1 tabanlı) tutulur.
            bool improved = rewardedStarCount > saves.GetLevelStars(levelId);
            if (improved)
            {
                saves.SetLevelStars(levelId, rewardedStarCount);
            }

            saves.Save();

            if (improved)
            {
                GameServices.Leaderboard.SubmitScore(LeaderboardName, saves.GetTotalStars());
            }
        }
    }
}
