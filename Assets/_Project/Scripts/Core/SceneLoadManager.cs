using UnityEngine;
using UnityEngine.SceneManagement;

namespace ParkingMastery.Core
{
    /// <summary>Sahne geçişlerini yapar: sahne adı sayıysa seviye numarası olarak kaydedip Gameplay sahnesini yükler; yükleme sonrası zaman ölçeğini 1'e döndürür.</summary>
    public class SceneLoadManager : MonoBehaviour
    {
        public const string GameplaySceneName = "Gameplay";

        /// <summary>Sahne adı sayıysa (ör. "5") seviye numarasıdır: seviye kaydedilir, Gameplay sahnesi yüklenir (B7).</summary>
        public void LoadScene(string sceneName)
        {
            Load(sceneName);
        }

        /// <summary>LoadScene'in statik sürümü (kod içinden çağrı için).</summary>
        public static void LoadSceneStatic(string sceneName)
        {
            Load(sceneName);
        }

        /// <summary>Aktif sahneyi yeniden yükler; Gameplay'de LevelSession aynı seviyeyi korur.</summary>
        public static void RestartScene()
        {
            // Gameplay sahnesinde aynı sahne yeniden yüklenir; LevelSession aynı seviyeyi korur.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private static void Load(string sceneName)
        {
            if (int.TryParse(sceneName, out int levelId))
            {
                LevelSession.CurrentLevelId = levelId;
                sceneName = GameplaySceneName;
            }
            SceneManager.LoadScene(sceneName);
            Time.timeScale = 1f;
        }
    }
}
