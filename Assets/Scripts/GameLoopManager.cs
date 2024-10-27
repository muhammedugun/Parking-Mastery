using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class GameLoopManager : MonoBehaviour
{
    private LevelPopupManager _levelPopupManager;
    private LevelTimeCounter _levelTimeCounter;
    private StarCounter _starCounter;

    private void Start()
    {
        _levelPopupManager = FindFirstObjectByType<LevelPopupManager>();
        _levelTimeCounter = FindFirstObjectByType<LevelTimeCounter>();
        _starCounter = FindFirstObjectByType<StarCounter>();
    }

    private void OnEnable()
    {
        YandexGame.RewardVideoEvent += ResumeLevel;
        EventBus.Subscribe(EventType.ParkingSuccessful, UnlockLevel);
    }

    private void OnDisable()
    {
        YandexGame.RewardVideoEvent -= ResumeLevel;
        EventBus.Unsubscribe(EventType.ParkingSuccessful, UnlockLevel);
    }

    private void ResumeLevel(int id)
    {
        _levelPopupManager.CloseAllPopupLevel();
        ResumeGame();
        EventBus.Publish(EventType.ResumeGame);
    }

    public static void RestartLevel()
    {
        SceneLoadManager.RestartScene();
    }

    public static void PauseGame()
    {
        Time.timeScale = 0;
    }
    public static void ResumeGame()
    {
        Time.timeScale = 1;
    }

    public static void LoadNextLevel()
    {
        int nextLevelNumber = int.Parse(SceneManager.GetActiveScene().name) + 1;
        SceneLoadManager.LoadSceneStatic(nextLevelNumber.ToString());
    }

    private void UnlockLevel()
    {
        int nextLevelNumber = int.Parse(SceneManager.GetActiveScene().name) + 1;
        YandexGame.savesData.unlockLevels[nextLevelNumber] = true;

        int rewardedStarCount;

        if (_levelTimeCounter.time > _starCounter._twoStarTime)
        {
            rewardedStarCount = 1;
        }
        else if (_levelTimeCounter.time > _starCounter._threeStarTime)
        {
            rewardedStarCount = 2;
        }
        else
        {
            rewardedStarCount = 3;
        }

        if (rewardedStarCount > YandexGame.savesData.levelsStarCount[nextLevelNumber - 1])
        {
            YandexGame.savesData.levelsStarCount[nextLevelNumber - 1] = rewardedStarCount;
            YandexGame.SaveProgress();

            int totalStarCount = 0;

            foreach (var starCount in YandexGame.savesData.levelsStarCount)
            {
                totalStarCount += starCount;
            }

            YandexGame.NewLeaderboardScores("Leaderboard", totalStarCount);

        }

        YandexGame.SaveProgress();





    }


}
