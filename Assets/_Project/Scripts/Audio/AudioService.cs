using System;
using ParkingMastery.Services;

namespace ParkingMastery.Audio
{
    /// <summary>IAudioService uygulaması: ayarı ISaveService'e yazar, çalan müziğe (MusicManager) uygular.</summary>
    public sealed class AudioService : IAudioService
    {
        public event Action SettingsChanged;

        public bool IsMusicOn => GameServices.Saves.IsMusicOn;
        public bool IsEngineSoundOn => GameServices.Saves.IsEngineSoundOn;

        public void SetMusicOn(bool on)
        {
            var saves = GameServices.Saves;
            saves.IsMusicOn = on;
            saves.Save();

            var music = MusicManager.Instance;
            if (music != null) music.ApplyMuteState();

            SettingsChanged?.Invoke();
        }

        public void SetEngineSoundOn(bool on)
        {
            var saves = GameServices.Saves;
            saves.IsEngineSoundOn = on;
            saves.Save();
            SettingsChanged?.Invoke();
        }

        public void ToggleMusic() => SetMusicOn(!IsMusicOn);
        public void ToggleEngineSound() => SetEngineSoundOn(!IsEngineSoundOn);
    }
}
