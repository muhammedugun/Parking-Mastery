using System;

namespace ParkingMastery.Services
{
    /// <summary>
    /// Kayıt servisi. Kayıt henüz yüklenmediyse (WebGL'de SDK gecikmesi) okumalar güvenli varsayılan döndürür,
    /// yazmalar yok sayılır; hiçbiri hata fırlatmaz.
    /// </summary>
    public interface ISaveService
    {
        /// <summary>Kayıt yüklendi mi?</summary>
        bool IsReady { get; }

        /// <summary>Kayıt (yeniden) yüklendiğinde tetiklenir.</summary>
        event Action Loaded;

        /// <summary>Müzik açık mı? Yüklenmediyse true.</summary>
        bool IsMusicOn { get; set; }

        /// <summary>Motor sesi açık mı? Yüklenmediyse true.</summary>
        bool IsEngineSoundOn { get; set; }

        bool IsLevelUnlocked(int level);
        void UnlockLevel(int level);
        int GetLevelStars(int level);
        void SetLevelStars(int level, int stars);
        int GetTotalStars();

        /// <summary>Kayıt şeması sürümü (0 = sürümsüz eski kayıt; yüklemede en güncele taşınır).</summary>
        int SaveVersion { get; }

        /// <summary>Seviyedeki en iyi süre (sn); henüz yoksa 0.</summary>
        int GetBestTime(int level);

        /// <summary>Verilen süre daha kısaysa (veya hiç süre yoksa) en iyi süreyi günceller; güncellendiyse true.</summary>
        bool TrySetBestTime(int level, int seconds);

        /// <summary>Değişiklikleri kalıcı yazar (SDK hazır değilse yok sayılır).</summary>
        void Save();
    }
}
