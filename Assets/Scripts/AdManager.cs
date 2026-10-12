using UnityEngine;
using YG;

public class AdManager : MonoBehaviour
{
    public void OpenRewardAd()
    {
        YG2.RewardedAdvShow("resume");
    }

    public void OpenFullscreenAd()
    {
        YG2.InterstitialAdvShow();
    }
}
