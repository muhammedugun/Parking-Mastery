using System;

namespace ParkingMastery.Services
{
    /// <summary>
    /// Ses ayarları servisi (B4). Müzik ve motor sesi açık/kapalı durumunu yönetir; ayarı kaydeder ve
    /// çalan ses kaynaklarına uygular. UI/oyun kodu MusicManager'a ya da kayıt alanlarına doğrudan dokunmaz.
    /// </summary>
    public interface IAudioService
    {
        bool IsMusicOn { get; }
        bool IsEngineSoundOn { get; }

        void SetMusicOn(bool on);
        void SetEngineSoundOn(bool on);
        void ToggleMusic();
        void ToggleEngineSound();

        /// <summary>Müzik ya da motor sesi ayarı değiştiğinde tetiklenir.</summary>
        event Action SettingsChanged;
    }
}
