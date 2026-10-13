using UnityEngine;
using ParkingMastery.Core;
using ParkingMastery.Services;

namespace ParkingMastery.UI
{
    /// <summary>
    /// Düğmelerin (UnityEvent) reklam çağrıları için sahne bileşeni. Bekleme aralığı ve reklam sırasında duraklatma
    /// <see cref="CooldownAdService"/> / eklenti tarafında uygulanır; burada yalnızca 'level içinde reklam yok' kuralı vardır.
    /// </summary>
    public class AdManager : MonoBehaviour
    {
        /// <summary>Ayrı 'Evet' düğmesi (reklam simgeli, ödül metni popup'ta yazılı). Reklam izlemek devam etmenin tek yolu değildir.</summary>
        public void OpenRewardAd()
        {
            GameServices.Ads.ShowRewarded(AdRewardIds.Resume);
        }

        /// <summary>Yalnız oyun dışı eylemlerde (Sonraki / Tekrar / Menü) kullanılır; level oynanırken asla reklam göstermez.</summary>
        public void OpenFullscreenAd()
        {
            if (GameStateMachine.Current == GameState.Playing)
            {
                Debug.LogWarning("[Ads] Level oynanırken interstitial istendi; Yandex kuralı gereği yok sayıldı.");
                return;
            }
            GameServices.Ads.ShowInterstitial();
        }
    }
}
