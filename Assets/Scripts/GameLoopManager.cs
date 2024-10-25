using UnityEngine;
using YG;

public class GameLoopManager : MonoBehaviour
{
    private LevelPopupManager _levelPopupManager;

    private void Start()
    {
        _levelPopupManager = FindFirstObjectByType<LevelPopupManager>();
    }

    private void OnEnable()
    {
        YandexGame.RewardVideoEvent += ResumeLevel;
    }

    private void OnDisable()
    {
        YandexGame.RewardVideoEvent -= ResumeLevel;
    }

    private void ResumeLevel(int id)
    {
        _levelPopupManager.CloseAllPopupLevel();
        ResumeGame();
    }

    public static void PauseGame()
    {
        Time.timeScale = 0;
    }
    public static void ResumeGame()
    {
        Time.timeScale = 1;
    }
}
