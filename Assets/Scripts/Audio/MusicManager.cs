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
        EventBus.Subscribe(EventType.ResumeGame, UnPauseLevelMusic);
        YG2.onGetSDKData += Initialization;
        if (YG2.isSDKEnabled)
            Initialization();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        EventBus.Unsubscribe(EventType.ParkingSuccessful, UnPauseMenuMusicInvoke);
        EventBus.Unsubscribe(EventType.CarContactObstacle, UnPauseMenuMusicInvoke);
        EventBus.Unsubscribe(EventType.ResumeGame, UnPauseLevelMusic);
        YG2.onGetSDKData -= Initialization;
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

        ApplyMuteState();

        MusicControl(scene.buildIndex);
    }

    /// <summary>
    /// Kayıtlı müzik durumunu sadece ses kaynaklarına uygular. Kayıt (SaveProgress) yapmaz.
    /// </summary>
    private void ApplyMuteState()
    {
        bool mute = CheckMute();
        menuAudioSource.mute = mute;
        levelAudioSource.mute = mute;
    }

    private void Initialization()
    {
        ApplyMuteState();
    }

    /// <summary>
    /// Müzik sesinin kapalı olup olmadığını kontrol eder.
    /// </summary>
    /// <returns>Müzik kapalıysa true, açıksa false döner.</returns>
    public static bool CheckMute()
    {
        return YG2.saves.isMusicOn == false;
    }

    public void Switch()
    {
        if (CheckMute())
            UnMuteMusic();
        else
            MuteMusic();
    }

    /// <summary>
    /// Müzikleri kapalı duruma getirir.
    /// </summary>
    public void MuteMusic()
    {
        YG2.saves.isMusicOn = false;
        if (YG2.isSDKEnabled)
            YG2.SaveProgress();

        menuAudioSource.mute = true;
        levelAudioSource.mute = true;
    }

    /// <summary>
    /// Müzikleri açık duruma getirir.
    /// </summary>
    public void UnMuteMusic()
    {
        YG2.saves.isMusicOn = true;
        if (YG2.isSDKEnabled)
            YG2.SaveProgress();

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