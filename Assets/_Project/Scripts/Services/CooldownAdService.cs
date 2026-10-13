using System;
using UnityEngine;

namespace ParkingMastery.Services
{
    /// <summary>
    /// IAdService sarmalayıcısı: interstitial'a kendi bekleme aralığımızı uygular, reklam sırasında ikinci reklamı engeller,
    /// ödüllü reklam açılamazsa RewardFailed bildirir. Oturum boyunca yaşar; sahneler arası bekleme saati korunur.
    /// </summary>
    public sealed class CooldownAdService : IAdService, IDisposable
    {
        /// <summary>Editör/Development build'de bekleme kısa tutulur (test); prod'da <see cref="AdPolicy.DefaultInterstitialCooldown"/>.</summary>
        public const float TestModeInterstitialCooldown = 10f;

        private readonly IAdService _inner;
        private readonly AdPolicy _policy;
        private readonly Func<float> _now;
        private readonly bool _testMode;

        public event Action<string> RewardGranted;
        public event Action<string> RewardFailed;
        public event Action AdFinished;

        public CooldownAdService(IAdService inner)
            : this(inner, () => Time.realtimeSinceStartup, Application.isEditor || Debug.isDebugBuild) { }

        public CooldownAdService(IAdService inner, Func<float> now, bool testMode)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _now = now ?? throw new ArgumentNullException(nameof(now));
            _testMode = testMode;
            _policy = new AdPolicy(testMode ? TestModeInterstitialCooldown : AdPolicy.DefaultInterstitialCooldown, _now());

            _inner.RewardGranted += OnRewardGranted;
            _inner.RewardFailed += OnRewardFailed;
            _inner.AdFinished += OnAdFinished;
        }

        public void Dispose()
        {
            _inner.RewardGranted -= OnRewardGranted;
            _inner.RewardFailed -= OnRewardFailed;
            _inner.AdFinished -= OnAdFinished;
            (_inner as IDisposable)?.Dispose();
        }

        public bool IsAdShowing => _inner.IsAdShowing;

        public bool ShowInterstitial()
        {
            float now = _now();
            var decision = _policy.EvaluateInterstitial(now, _inner.IsAdShowing);
            if (decision != InterstitialDecision.Allow)
            {
                Log("Interstitial atlandı: " + decision + " (kalan bekleme " + _policy.RemainingCooldown(now).ToString("0.0") + " sn)");
                return false;
            }

            bool requested = _inner.ShowInterstitial();
            if (requested) _policy.NotifyAd(now);
            Log(requested ? "Interstitial istendi." : "Interstitial platform tarafından geçilmedi (kendi zamanlayıcısı).");
            return requested;
        }

        public void ShowRewarded(string rewardId)
        {
            if (_inner.IsAdShowing)
            {
                Log("Ödüllü reklam istenmedi: başka reklam açık.");
                RewardFailed?.Invoke(rewardId);
                return;
            }
            Log("Ödüllü reklam istendi: " + rewardId);
            _inner.ShowRewarded(rewardId);
        }

        private void OnRewardGranted(string id)
        {
            Log("Ödül verildi: " + id);
            RewardGranted?.Invoke(id);
        }

        private void OnRewardFailed(string id)
        {
            Log("Ödül verilmedi/reklam gösterilemedi: " + id);
            RewardFailed?.Invoke(id);
        }

        private void OnAdFinished()
        {
            _policy.NotifyAd(_now());
            AdFinished?.Invoke();
        }

        private void Log(string message)
        {
            if (_testMode) Debug.Log("[Ads] " + message);
        }
    }
}
