using System;
using YG;
using ParkingMastery.Services;

namespace ParkingMastery.Yandex
{
    /// <summary>ILocalizationService'in YG2.lang uygulaması.</summary>
    public sealed class YandexLocalizationService : ILocalizationService, IDisposable
    {
        public event Action<string> LanguageChanged;

        public YandexLocalizationService()
        {
            YG2.onSwitchLang += OnSwitchLang;
        }

        public void Dispose()
        {
            YG2.onSwitchLang -= OnSwitchLang;
        }

        public string Language => string.IsNullOrEmpty(YG2.lang) ? "en" : YG2.lang;

        public void SetLanguage(string language)
        {
            if (!string.IsNullOrEmpty(language)) YG2.SwitchLanguage(language);
        }

        private void OnSwitchLang(string language)
        {
            LanguageChanged?.Invoke(string.IsNullOrEmpty(language) ? "en" : language);
        }
    }
}
