using System;
using UnityEngine;
using YG;
using ParkingMastery.Services;

namespace ParkingMastery.Yandex
{
    /// <summary>ISaveService'in YG2.saves (SavesYG) uygulaması. Yandex'e bağlı tek yer burasıdır.</summary>
    public sealed class YandexSaveService : ISaveService, IDisposable
    {
        public event Action Loaded;

        private bool _warned;

        public YandexSaveService()
        {
            YG2.onGetSDKData += OnSdkData;
            // Kayıt zaten yüklüyse yalnız bellekte taşı; buluta yazma, bulut verisi gelince OnSdkData yapar.
            Data?.MigrateIfNeeded();
        }

        public void Dispose()
        {
            YG2.onGetSDKData -= OnSdkData;
        }

        private void OnSdkData()
        {
            // Bulut kaydı yeni yüklendi: eski sürümse taşı ve (değiştiyse) geri yaz, sonra dinleyicilere haber ver.
            if (Data != null && Data.MigrateIfNeeded() && YG2.isSDKEnabled) YG2.SaveProgress();
            Loaded?.Invoke();
        }

        private static SavesYG Data => YG2.saves;

        public bool IsReady => Data != null;

        public bool IsMusicOn
        {
            get => Data == null || Data.isMusicOn;
            set { if (CanWrite()) Data.isMusicOn = value; }
        }

        public bool IsEngineSoundOn
        {
            get => Data == null || Data.isEngineSoundOn;
            set { if (CanWrite()) Data.isEngineSoundOn = value; }
        }

        public bool IsLevelUnlocked(int level)
        {
            // Kayıt yokken yalnız ilk seviye açık (SavesYG varsayılanıyla aynı).
            return Data != null ? Data.IsLevelUnlocked(level) : level == 1;
        }

        public void UnlockLevel(int level)
        {
            if (CanWrite()) Data.UnlockLevel(level);
        }

        public int GetLevelStars(int level)
        {
            return Data != null ? Data.GetLevelStars(level) : 0;
        }

        public void SetLevelStars(int level, int stars)
        {
            if (CanWrite()) Data.SetLevelStars(level, stars);
        }

        public int GetTotalStars()
        {
            return Data != null ? Data.GetTotalStars() : 0;
        }

        public int SaveVersion => Data != null ? Data.saveVersion : 0;

        public int GetBestTime(int level)
        {
            return Data != null ? Data.GetBestTime(level) : 0;
        }

        public bool TrySetBestTime(int level, int seconds)
        {
            return CanWrite() && Data.TrySetBestTime(level, seconds);
        }

        public void Save()
        {
            if (YG2.isSDKEnabled) YG2.SaveProgress();
        }

        private bool CanWrite()
        {
            if (Data != null) return true;
            if (!_warned)
            {
                _warned = true;
                Debug.LogWarning("[ISaveService] Kayıt henüz yüklenmedi; yazma yok sayıldı.");
            }
            return false;
        }
    }
}
