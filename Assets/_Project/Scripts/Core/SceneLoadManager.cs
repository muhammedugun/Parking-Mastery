using UnityEngine;
using UnityEngine.SceneManagement;

namespace ParkingMastery.Core
{
    public class SceneLoadManager : MonoBehaviour
    {
        public const string GameplaySceneName = "Gameplay";

        /// <summary>Sahne adı sayıysa (ör. "5") seviye numarasıdır: seviye kaydedilir, Gameplay sahnesi yüklenir (B7).</summary>
        public void LoadScene(string sceneName)
        {
            Load(sceneName);
        }

        public static void LoadSceneStatic(string sceneName)
        {
            Load(sceneName);
        }

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
