using UnityEngine;
using UnityEngine.SceneManagement;
using ParkingMastery.Services;
using ParkingMastery.Core;

namespace ParkingMastery.Audio
{
    /// <summary>Menü ve seviye müziğini yönetir: sahneye göre doğru müziği açar, kayıtlı müzik ayarına uyar; park başarısı/başarısızlığında menü müziğine, oyuna devam edilince seviye müziğine geçer. Tekil örnektir (Instance).</summary>
    public class MusicManager : MonoBehaviour
    {
        [SerializeField] private AudioSource menuAudioSource, levelAudioSource;

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
            GameServices.Audio.SettingsChanged += ApplyMuteState;
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
            GameServices.Audio.SettingsChanged -= ApplyMuteState;
        }

        private void Start()
        {
            MusicControl(SceneManager.GetActiveScene().buildIndex);

        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CancelInvoke();
            ApplyMuteState();

            MusicControl(scene.buildIndex);
        }

        /// <summary>
        /// Kayıtlı müzik durumunu sadece ses kaynaklarına uygular. Kayıt (SaveProgress) yapmaz.
        /// </summary>
        public void ApplyMuteState()
        {
            bool mute = CheckMute();
            if (menuAudioSource != null) menuAudioSource.mute = mute;
            if (levelAudioSource != null) levelAudioSource.mute = mute;
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

        /// <summary>Müziği açık ↔ kapalı arasında değiştirir.</summary>
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
                if (menuAudioSource != null) menuAudioSource.Pause();
                Invoke(nameof(UnPauseLevelMusic), 1f);
            }
        }

        private void UnPauseMenuMusicInvoke()
        {
            if (levelAudioSource != null) levelAudioSource.Pause();
            Invoke(nameof(UnPauseMenuMusic), 1f);
        }

        private void UnPauseMenuMusic()
        {

            if (levelAudioSource != null) levelAudioSource.Pause();
            if (menuAudioSource != null && !menuAudioSource.isPlaying)
                menuAudioSource.UnPause();

        }

        private void UnPauseLevelMusic()
        {

            if (menuAudioSource != null) menuAudioSource.Pause();
            if (levelAudioSource != null && !levelAudioSource.isPlaying)
                levelAudioSource.UnPause();

        }
    }
}
