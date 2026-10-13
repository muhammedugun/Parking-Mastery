using System;
using ParkingMastery.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ParkingMastery.UI
{
    /// <summary>
    /// Dil seçimi popup'ı (RU / EN / TR). Dil adları kendi dilinde yazılır; bilinmeyen dilde de bulunabilsin.
    /// Dil seçimi <see cref="ILocalizationService.SetLanguage"/> ile uygulanır (merkezi tablo E8'de).
    /// </summary>
    public sealed class LanguagePopup : ScreenBase
    {
        [Serializable]
        private struct Option
        {
            public string code;
            public Button button;
            public Image background;
            public TMP_Text label;
        }

        [SerializeField] private Option[] options;
        [SerializeField] private UITheme theme;

        private bool _wired;

        protected override void OnInitialize() => Wire();

        protected override void OnShowing()
        {
            Wire();
            Refresh();
        }

        private void Wire()
        {
            if (_wired || options == null) return;
            _wired = true;
            for (int i = 0; i < options.Length; i++)
            {
                string code = options[i].code;
                if (options[i].button != null)
                    options[i].button.onClick.AddListener(() => Select(code));
            }
        }

        private void Select(string code)
        {
            GameServices.Localization.SetLanguage(code);
            Refresh();
            if (UIManager.Instance != null)
                UIManager.Instance.Back();
        }

        private void Refresh()
        {
            if (options == null || theme == null) return;
            string current = UILanguage.Normalize(GameServices.Localization.Language);
            foreach (var option in options)
            {
                bool selected = option.code == current;
                if (option.background != null) option.background.color = selected ? theme.Primary : theme.SurfaceAlt;
                if (option.label != null) option.label.color = selected ? theme.PrimaryText : theme.TextPrimary;
            }
        }
    }
}