using ParkingMastery.Services;
using TMPro;
using UnityEngine;

namespace ParkingMastery.UI
{
    /// <summary>
    /// TMP metni için RU/EN/TR çevirisi ve dil değişince otomatik güncelleme.
    /// Geçici çözüm: E8'de merkezi metin tablosuyla değiştirilecek. Desteklenmeyen dil → EN.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class UILocalizedLabel : MonoBehaviour
    {
        [SerializeField, TextArea] private string ru;
        [SerializeField, TextArea] private string en;
        [SerializeField, TextArea] private string tr;

        private TMP_Text _label;
        private ILocalizationService _localization;

        /// <summary>Metinleri çalışma anında / kurulumda ayarlar.</summary>
        public void Set(string russian, string english, string turkish)
        {
            ru = russian;
            en = english;
            tr = turkish;
            Refresh();
        }

        private void OnEnable()
        {
            _localization = GameServices.Localization;
            _localization.LanguageChanged += OnLanguageChanged;
            Refresh();
        }

        private void OnDisable()
        {
            if (_localization != null)
                _localization.LanguageChanged -= OnLanguageChanged;
            _localization = null;
        }

        private void OnLanguageChanged(string _) => Refresh();

        private void Refresh()
        {
            if (_label == null && !TryGetComponent(out _label))
                return;

            string lang = UILanguage.Normalize(GameServices.Localization.Language);
            string text = lang == "ru" ? ru : lang == "tr" ? tr : en;
            _label.text = string.IsNullOrEmpty(text) ? en : text;
        }
    }

    /// <summary>Dil kodu yardımcıları (geçici; E8'de merkezi yerelleştirmeye taşınır).</summary>
    public static class UILanguage
    {
        public const string Russian = "ru";
        public const string English = "en";
        public const string Turkish = "tr";

        /// <summary>'ru-RU' → 'ru'; bilinmeyen/boş dil → 'en'.</summary>
        public static string Normalize(string language)
        {
            if (string.IsNullOrEmpty(language) || language.Length < 2)
                return English;
            string code = language.Substring(0, 2).ToLowerInvariant();
            return code == Russian || code == Turkish ? code : English;
        }

        /// <summary>Açık/Kapalı metni geçerli dilde döndürür.</summary>
        public static string OnOff(bool on, string language)
        {
            string lang = Normalize(language);
            if (lang == Russian) return on ? "Вкл" : "Выкл";
            if (lang == Turkish) return on ? "Açık" : "Kapalı";
            return on ? "On" : "Off";
        }
    }
}