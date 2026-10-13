using System;

namespace ParkingMastery.Services
{
    /// <summary>Yerelleştirme servisi (AUDIT CC-6, B4). Aktif dil kodunu ("tr", "en", "ru"...) verir ve dil değişimini bildirir.</summary>
    public interface ILocalizationService
    {
        /// <summary>Aktif dil kodu. Bilinmiyorsa "en" döner.</summary>
        string Language { get; }

        /// <summary>Dil değiştiğinde tetiklenir; parametre yeni dil kodudur.</summary>
        event Action<string> LanguageChanged;

        /// <summary>Dili değiştirir (boş kod yok sayılır).</summary>
        void SetLanguage(string language);
    }
}
