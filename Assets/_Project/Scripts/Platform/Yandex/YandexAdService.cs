using System;
using YG;
using ParkingMastery.Services;

namespace ParkingMastery.Yandex
{
    /// <summary>IAdService'in PluginYourGames (YG2) uygulaması. Yandex'e bağlı tek yer burasıdır.</summary>
    public sealed class YandexAdService : IAdService, IDisposable
    {
        public event Action<string> RewardGranted;

        public YandexAdService()
        {
            YG2.onRewardAdv += OnReward;
        }

        public void Dispose()
        {
            YG2.onRewardAdv -= OnReward;
        }

        private void OnReward(string rewardId)
        {
            RewardGranted?.Invoke(rewardId);
        }

        public void ShowRewarded(string rewardId)
        {
            YG2.RewardedAdvShow(rewardId);
        }

        public void ShowInterstitial()
        {
            YG2.InterstitialAdvShow();
        }
    }
}
