using UnityEngine;
using UnityEngine.SceneManagement;
using ParkingMastery.Services;
using ParkingMastery.Core;

namespace ParkingMastery.Audio
{
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
            // Systems prefab'ı altındayken kalıcılığı SystemsRoot sağlar; yalnız kök nesneyse kendisi işaretler.
            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            EventBus.Subscribe(GameEventType.ParkingSuccessful, UnPauseMenuMusicInvoke);
            EventBus.Subscribe(GameEventType.LevelFailed, UnPauseMenuMusicInvoke);
            EventBus.Subscribe(GameEventType.ResumeGame, UnPauseLevelMusic);
            GameServices.Saves.Loaded += Initialization;
            if (GameServices.Saves.IsReady)
                Initialization();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            EventBus.Unsubscribe(GameEventType.ParkingSuccessful, UnPauseMenuMusicInvoke);
            EventBus.Unsubscribe(GameEventType.LevelFailed, UnPauseMenuMusicInvoke);
            EventBus.Unsubscribe(GameEventType.ResumeGame, UnPauseLevelMusic);
            GameServices.Saves.Loaded -= Initialization;
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
        public void ApplyMuteState()
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
            return !GameServices.Audio.IsMusicOn;
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
            GameServices.Audio.SetMusicOn(false);
        }

        /// <summary>
        /// Müzikleri açık duruma getirir.
        /// </summary>
        public void UnMuteMusic()
        {
            GameServices.Audio.SetMusicOn(true);
        }

        private void MusicControl(int sceneBuildIndex)
        {
            string sceneName = SceneManager.GetSceneByBuildIndex(sceneBuildIndex).name;
            if (sceneName == "Boot" || sceneName == "MainMenu" || sceneName == "LevelSelectionMenu")
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
}
