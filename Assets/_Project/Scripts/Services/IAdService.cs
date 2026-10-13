using System;

namespace ParkingMastery.Services
{
    /// <summary>
    /// Reklam servisi. Oyun/araç kodu Yandex SDK'sını doğrudan bilmez, bu arayüzü kullanır (AUDIT CC-6).
    /// </summary>
    public interface IAdService
    {
        /// <summary>Ödüllü reklam izlenip ödül hak edildiğinde tetiklenir; parametre ödül kimliğidir.</summary>
        event Action<string> RewardGranted;

        /// <summary>Ödüllü reklam gösterir. rewardId, RewardGranted olayında geri döner.</summary>
        void ShowRewarded(string rewardId);

        /// <summary>Tam ekran (interstitial) reklam gösterir. Yalnız oyun dışı eylemden sonra çağrılmalı (Yandex kuralı).</summary>
        void ShowInterstitial();
    }
}
