using UnityEngine;
using YG;

public class AdManager : MonoBehaviour
{
    public void OpenRewardAd()
    {
        YandexGame.RewVideoShow(0);
    }

    public void OpenFullscreenAd()
    {
        YandexGame.FullscreenShow();
    }
}
