using ParkingMastery.Core;
using ParkingMastery.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ParkingMastery.UI
{
    /// <summary>
    /// Ana menü ekranı: Oyna, Ayarlar, Liderlik tablosu, Dil, Ses, Krediler.
    /// Çıkış butonu bilerek yok (web oyununda işlevsiz).
    /// </summary>
    public sealed class MainMenuScreen : ScreenBase
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button languageButton;
        [SerializeField] private Button soundButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Image soundIcon;
        [SerializeField] private TMP_Text languageCode;
        [SerializeField] private UITheme theme;
        [SerializeField] private string levelSelectionScene = "LevelSelectionMenu";

        private bool _wired;
        private IAudioService _audio;
        private ILocalizationService _localization;

        protected override void OnInitialize() => Wire();

        protected override void OnShowing()
        {
            Wire();
            Unsubscribe();
            _audio = GameServices.Audio;
            _localization = GameServices.Localization;
            _audio.SettingsChanged += Refresh;
            _localization.LanguageChanged += OnLanguageChanged;
            Refresh();
        }

        protected override void OnHidden() => Unsubscribe();

        private void Unsubscribe()
        {
            if (_audio != null) _audio.SettingsChanged -= Refresh;
            if (_localization != null) _localization.LanguageChanged -= OnLanguageChanged;
            _audio = null;
            _localization = null;
        }

        private void Wire()
        {
            if (_wired) return;
            _wired = true;
            if (playButton != null) playButton.onClick.AddListener(OnPlay);
            if (settingsButton != null) settingsButton.onClick.AddListener(() => Open<SettingsPopup>());
            if (leaderboardButton != null) leaderboardButton.onClick.AddListener(() => Open<LeaderboardPopup>());
            if (languageButton != null) languageButton.onClick.AddListener(() => Open<LanguagePopup>());
            if (creditsButton != null) creditsButton.onClick.AddListener(() => Open<CreditsPopup>());
            if (soundButton != null) soundButton.onClick.AddListener(OnSoundToggle);
        }

        private static void Open<T>() where T : ScreenBase
        {
            if (UIManager.Instance != null)
                UIManager.Instance.Open<T>();
        }

        private void OnPlay() => SceneLoadManager.LoadSceneStatic(levelSelectionScene);

        /// <summary>Hızlı ses düğmesi: müzik + motor sesi birlikte açılır/kapanır (ayrı ayar: Ayarlar).</summary>
        private void OnSoundToggle()
        {
            var audio = GameServices.Audio;
            bool turnOn = !(audio.IsMusicOn || audio.IsEngineSoundOn);
            audio.SetMusicOn(turnOn);
            audio.SetEngineSoundOn(turnOn);
            Refresh();
        }

        private void OnLanguageChanged(string _) => Refresh();

        private void Refresh()
        {
            if (soundIcon != null && theme != null)
            {
                var audio = GameServices.Audio;
                soundIcon.sprite = audio.IsMusicOn || audio.IsEngineSoundOn ? theme.IconMusicOn : theme.IconMusicOff;
            }

            if (languageCode != null)
                languageCode.text = UILanguage.Normalize(GameServices.Localization.Language).ToUpperInvariant();
        }
    }
}
