using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AudioSource menuAudioSource, levelAudioSource;

    private static MusicManager instance;
    private float _defaultMenuMusicVolume, _defaultLevelMusicVolume;

    /// <summary>
    /// Singleton deseni kullanarak MusicManager örneğini alır.
    /// </summary>
    public static MusicManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MusicManager>();

                if (instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(MusicManager).Name);
                    instance = singletonObject.AddComponent<MusicManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe(EventType.ParkingSuccessful, UnPauseMenuMusic);
        EventBus.Subscribe(EventType.CarContactObstacle, UnPauseMenuMusic);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(EventType.ParkingSuccessful, UnPauseMenuMusic);
        EventBus.Unsubscribe(EventType.CarContactObstacle, UnPauseMenuMusic);
    }


    private void Start()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("SoundVolume", 1f);

        _defaultMenuMusicVolume = menuAudioSource.volume;
        _defaultLevelMusicVolume = levelAudioSource.volume;

        if (CheckMute())
        {
            MuteMusic();
        }
        else
        {
            UnMuteMusic();
        }


        MusicControl(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// M�zik sesinin kapal� olup olmad���n� kontrol eder.
    /// </summary>
    /// <returns>M�zik kapal�ysa true, a��ksa false d�ner.</returns>
    public static bool CheckMute()
    {
        return PlayerPrefs.GetFloat("MusicVolume", 1f) == 0f;
    }

    /// <summary>
    /// M�zikleri kapal� duruma getirir.
    /// </summary>
    public void MuteMusic()
    {
        PlayerPrefs.SetFloat("MusicVolume", 0f);
        menuAudioSource.volume = 0f;
        levelAudioSource.volume = 0f;
    }

    /// <summary>
    /// M�zikleri a��k duruma getirir.
    /// </summary>
    public void UnMuteMusic()
    {
        PlayerPrefs.SetFloat("MusicVolume", 1f);
        menuAudioSource.volume = _defaultMenuMusicVolume;
        levelAudioSource.volume = _defaultLevelMusicVolume;
    }

    private void OnLevelWasLoaded(int level)
    {
        MusicControl(level);
    }

    private void MusicControl(int sceneBuildIndex)
    {
        string name = SceneManager.GetSceneByBuildIndex(sceneBuildIndex).name;
        if (name == "MainMenu" || name == "LevelSelectionMenu")
        {
            UnPauseMenuMusic();
        }
        else
        {
            UnPauseLevelMusic();
        }
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