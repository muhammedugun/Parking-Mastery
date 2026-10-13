using System.Collections;
using ParkingMastery.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ParkingMastery.Core
{
    /// <summary>
    /// Boot sahnesindeki giriş noktası: servisleri erken başlatır, kayıt hazır olana kadar (en fazla maxWaitSeconds) bekler,
    /// sonra ilk sahneyi yükler.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private string _firstScene = "MainMenu";
        [SerializeField] private float _maxWaitSeconds = 5f;

        private IEnumerator Start()
        {
            // Servisleri burada oluştur → WebGL'de SDK/kayıt yüklemesi ilk sahneden önce başlar.
            _ = GameServices.Platform;
            _ = GameServices.Localization;
            _ = GameServices.Ads;
            var saves = GameServices.Saves;

            float waited = 0f;
            while (!saves.IsReady && waited < _maxWaitSeconds)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(_firstScene);
        }
    }
}
