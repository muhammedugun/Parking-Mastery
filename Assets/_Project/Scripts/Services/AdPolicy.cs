using System;

namespace ParkingMastery.Services
{
    /// <summary>Tam ekran reklam kararı: gösterilebilir, başka reklam açık olduğu için engellendi veya bekleme süresi dolmadığı için engellendi.</summary>
    public enum InterstitialDecision { Allow, BlockedAdShowing, BlockedCooldown }

    /// <summary>
    /// Interstitial bekleme kuralı (Yandex 0B.5: kendi aralığımız ≥ 60–90 sn). Zamanı dışarıdan alan saf mantıktır.
    /// Saat oturum başında başlar (ilk reklam da bekleme sonrası) ve her reklamdan sonra yeniden başlar.
    /// </summary>
    public sealed class AdPolicy
    {
        public const float DefaultInterstitialCooldown = 75f;

        private float _lastAdTime;

        public float CooldownSeconds { get; set; }

        public AdPolicy(float cooldownSeconds, float nowSeconds)
        {
            CooldownSeconds = Math.Max(0f, cooldownSeconds);
            _lastAdTime = nowSeconds;
        }

        public float RemainingCooldown(float nowSeconds)
        {
            return Math.Max(0f, _lastAdTime + CooldownSeconds - nowSeconds);
        }

        public InterstitialDecision EvaluateInterstitial(float nowSeconds, bool adShowing)
        {
            if (adShowing) return InterstitialDecision.BlockedAdShowing;
            return RemainingCooldown(nowSeconds) > 0f ? InterstitialDecision.BlockedCooldown : InterstitialDecision.Allow;
        }

        /// <summary>Bir reklam istendiğinde/bittiğinde çağrılır; bekleme saatini sıfırlar.</summary>
        public void NotifyAd(float nowSeconds)
        {
            _lastAdTime = nowSeconds;
        }
    }
}
