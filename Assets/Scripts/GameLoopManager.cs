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
        YG2.onRewardAdv += ResumeLevel;
        EventBus.Subscribe(EventType.ParkingSuccessful, UnlockLevel);
    }

    private void OnDisable()
    {
        YG2.onRewardAdv -= ResumeLevel;
        EventBus.Unsubscribe(EventType.ParkingSuccessful, UnlockLevel);
    }

    private void ResumeLevel(string id)
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
        YG2.saves.UnlockLevel(nextLevelNumber);

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

        if (rewardedStarCount > YG2.saves.GetLevelStars(nextLevelNumber - 1))
        {
            YG2.saves.SetLevelStars(nextLevelNumber - 1, rewardedStarCount);
            YG2.SaveProgress();

            int totalStarCount = 0;

            foreach (var starCount in YG2.saves.levelsStarCount)
            {
                totalStarCount += starCount;
            }

            YG2.SetLeaderboard("Leaderboard", totalStarCount);

        }

        YG2.SaveProgress();





    }


}
