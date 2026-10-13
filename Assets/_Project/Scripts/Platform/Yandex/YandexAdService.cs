using System;
using YG;
using ParkingMastery.Services;

namespace ParkingMastery.Yandex
{
    /// <summary>
    /// IAdService'in PluginYourGames (YG2) uygulaması. Yandex'e bağlı tek yer burasıdır.
    /// Reklam açılırken oyun + ses duraklatması eklentide (Basic.autoPauseGame) yapılır.
    /// </summary>
    public sealed class YandexAdService : IAdService, IDisposable
    {
        public event Action<string> RewardGranted;
        public event Action<string> RewardFailed;
        public event Action AdFinished;

        // null = bekleyen ödüllü reklam yok. Kapanış/hata olayı yalnız bir kez sonuçlandırılır.
        private string _pendingRewardId;
        private bool _pendingRewardGranted;

        public YandexAdService()
        {
            YG2.onRewardAdv += OnReward;
            YG2.onCloseRewardedAdv += OnRewardedClosed;
            YG2.onErrorRewardedAdv += OnRewardedError;
            YG2.onCloseInterAdv += OnInterstitialClosed;
        }

        public void Dispose()
        {
            YG2.onRewardAdv -= OnReward;
            YG2.onCloseRewardedAdv -= OnRewardedClosed;
            YG2.onErrorRewardedAdv -= OnRewardedError;
            YG2.onCloseInterAdv -= OnInterstitialClosed;
        }

        public bool IsAdShowing => YG2.nowAdsShow;

        public void ShowRewarded(string rewardId)
        {
            if (YG2.nowAdsShow)
            {
                RewardFailed?.Invoke(rewardId);
                return;
            }

            _pendingRewardId = rewardId;
            _pendingRewardGranted = false;
            YG2.RewardedAdvShow(rewardId);
        }

        public bool ShowInterstitial()
        {
            // Eklentinin kendi zamanlayıcısı (interAdvInterval) da geçerli; hazır değilse istek yapılmaz.
            if (YG2.nowAdsShow || !YG2.isTimerAdvCompleted) return false;
            YG2.InterstitialAdvShow();
            return true;
        }

        private void OnReward(string rewardId)
        {
            _pendingRewardGranted = true;
            RewardGranted?.Invoke(rewardId);
        }

        private void OnRewardedClosed()
        {
            string id = _pendingRewardId;
            bool granted = _pendingRewardGranted;
            _pendingRewardId = null;
            _pendingRewardGranted = false;

            AdFinished?.Invoke();
            if (id != null && !granted) RewardFailed?.Invoke(id);
        }

        private void OnRewardedError()
        {
            string id = _pendingRewardId;
            _pendingRewardId = null;
            _pendingRewardGranted = false;

            if (id != null) RewardFailed?.Invoke(id);
        }

        private void OnInterstitialClosed()
        {
            AdFinished?.Invoke();
        }
    }
}
