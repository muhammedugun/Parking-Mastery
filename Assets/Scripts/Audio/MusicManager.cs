using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class MusicManager : MonoBehaviour
{
    public AudioSource menuAudioSource, levelAudioSource;

    /// <summary>
    /// Singleton deseni kullanarak MusicManager örneğini alır.
    /// </summary>
    public static MusicManager Instance => instance;

    private static MusicManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        EventBus.Subscribe(EventType.ParkingSuccessful, UnPauseMenuMusicInvoke);
        EventBus.Subscribe(EventType.CarContactObstacle, UnPauseMenuMusicInvoke);
        YandexGame.GetDataEvent += Initialization;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        EventBus.Unsubscribe(EventType.ParkingSuccessful, UnPauseMenuMusicInvoke);
        EventBus.Unsubscribe(EventType.CarContactObstacle, UnPauseMenuMusicInvoke);
        YandexGame.GetDataEvent -= Initialization;
    }

    private void Start()
    {
        MusicControl(SceneManager.GetActiveScene().buildIndex);

        if (SceneManager.GetActiveScene().name == "MainMenu")
            FindAnyObjectByType<MainMenuMusicUI>().musicManager = this;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CancelInvoke();
        if (scene.name == "MainMenu")
            FindAnyObjectByType<MainMenuMusicUI>().musicManager = this;

        if (CheckMute())
        {
            MuteMusic();
        }
        else
        {
            UnMuteMusic();
        }

        MusicControl(scene.buildIndex);
    }

    private void Initialization()
    {
        if (CheckMute())
        {
            MuteMusic();
        }
        else
        {
            UnMuteMusic();
        }
    }

    /// <summary>
    /// M�zik sesinin kapal� olup olmad���n� kontrol eder.
    /// </summary>
    /// <returns>M�zik kapal�ysa true, a��ksa false d�ner.</returns>
    public static bool CheckMute()
    {
        return YandexGame.savesData.isMusicOn == false;
    }

    public void Switch()
    {
        if (CheckMute())
            UnMuteMusic();
        else
            MuteMusic();
    }

    /// <summary>
    /// M�zikleri kapal� duruma getirir.
    /// </summary>
    public void MuteMusic()
    {
        YandexGame.savesData.isMusicOn = false;
        YandexGame.SaveProgress();

        menuAudioSource.mute = true;
        levelAudioSource.mute = true;
    }

    /// <summary>
    /// M�zikleri a��k duruma getirir.
    /// </summary>
    public void UnMuteMusic()
    {
        YandexGame.savesData.isMusicOn = true;
        YandexGame.SaveProgress();

        menuAudioSource.mute = false;
        levelAudioSource.mute = false;
    }

    private void MusicControl(int sceneBuildIndex)
    {
        string sceneName = SceneManager.GetSceneByBuildIndex(sceneBuildIndex).name;
        if (sceneName == "MainMenu" || sceneName == "LevelSelectionMenu")
        {
            UnPauseMenuMusic();
        }
        else
        {
            menuAudioSource.Pause();
            Invoke(nameof(UnPauseLevelMusic), 1f);
        }
    }

    private void UnPauseMenuMusicInvoke()
    {
        levelAudioSource.Pause();
        Invoke(nameof(UnPauseMenuMusic), 1f);
    }

    private void UnPauseMenuMusic()
    {

        levelAudioSource.Pause();
        if (!menuAudioSource.isPlaying)
            menuAudioSource.UnPause();

    }

    private void UnPauseLevelMusic()
    {

        menuAudioSource.Pause();
        if (!levelAudioSource.isPlaying)
            levelAudioSource.UnPause();

    }
}