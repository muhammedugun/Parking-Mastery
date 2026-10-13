using ParkingMastery.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ParkingMastery.UI
{
    /// <summary>
    /// Ayarlar popup'ı. E3 kapsamı: müzik ve motor sesi aç/kapa.
    /// E7'de müzik/SFX slider, titreşim ve grafik kalitesi eklenecek.
    /// </summary>
    public sealed class SettingsPopup : ScreenBase
    {
        [SerializeField] private Button musicButton;
        [SerializeField] private Image musicPill;
        [SerializeField] private TMP_Text musicState;
        [SerializeField] private Button engineButton;
        [SerializeField] private Image enginePill;
        [SerializeField] private TMP_Text engineState;
        [SerializeField] private UITheme theme;

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
            if (musicButton != null)
                musicButton.onClick.AddListener(() => { GameServices.Audio.ToggleMusic(); Refresh(); });
            if (engineButton != null)
                engineButton.onClick.AddListener(() => { GameServices.Audio.ToggleEngineSound(); Refresh(); });
        }

        private void OnLanguageChanged(string _) => Refresh();

        private void Refresh()
        {
            var audio = GameServices.Audio;
            string lang = GameServices.Localization.Language;
            Apply(audio.IsMusicOn, musicPill, musicState, lang);
            Apply(audio.IsEngineSoundOn, enginePill, engineState, lang);
        }

        private void Apply(bool on, Image pill, TMP_Text label, string lang)
        {
            if (pill != null && theme != null) pill.color = on ? theme.Success : theme.SurfaceAlt;
            if (label == null) return;
            label.text = UILanguage.OnOff(on, lang);
            if (theme != null) label.color = on ? theme.PrimaryText : theme.TextMuted;
        }
    }
}