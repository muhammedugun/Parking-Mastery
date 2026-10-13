using ParkingMastery.Services;
using UnityEngine;

namespace ParkingMastery.Services
{
    public class AdManager : MonoBehaviour
    {
        public void OpenRewardAd()
        {
            GameServices.Ads.ShowRewarded("resume");
        }

        public void OpenFullscreenAd()
        {
            GameServices.Ads.ShowInterstitial();
        }
    }
}
