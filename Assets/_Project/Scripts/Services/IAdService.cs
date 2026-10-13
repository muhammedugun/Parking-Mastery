using System;

namespace ParkingMastery.Services
{
    /// <summary>
    /// Reklam servisi. Oyun/araç kodu Yandex SDK'sını doğrudan bilmez, bu arayüzü kullanır (AUDIT CC-6).
    /// </summary>
    public interface IAdService
    {
        /// <summary>Ödüllü reklam izlenip ödül hak edildiğinde tetiklenir; parametre ödül kimliğidir (bkz. <see cref="AdRewardIds"/>).</summary>
        event Action<string> RewardGranted;

        /// <summary>
        /// Ödüllü reklam ödülsüz bitti ya da gösterilemedi (erken kapatıldı, hata, reklam yok); parametre ödül kimliğidir.
        /// Oyuncunun ilerlemesi ödüle bağlı olmamalı: bu olayda ücretsiz seçenekler (Tekrar dene / Menü) açık kalmalıdır.
        /// </summary>
        event Action<string> RewardFailed;

        /// <summary>Herhangi bir reklam gösterilip kapandığında tetiklenir.</summary>
        event Action AdFinished;

        /// <summary>Şu an ekranda bir reklam var mı? (Bu sırada oyun ve ses duraklatılmış olmalıdır.)</summary>
        bool IsAdShowing { get; }

        /// <summary>Ödüllü reklam gösterir. rewardId, RewardGranted olayında geri döner.</summary>
        void ShowRewarded(string rewardId);

        /// <summary>
        /// Tam ekran (interstitial) reklam ister. Yalnız oyun dışı eylemden sonra çağrılmalı (Yandex kuralı).
        /// Bekleme aralığı/başka reklam nedeniyle gösterilmeyecekse false döner.
        /// </summary>
        bool ShowInterstitial();
    }
}
