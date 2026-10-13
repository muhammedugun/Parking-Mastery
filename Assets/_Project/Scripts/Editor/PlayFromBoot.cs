using UnityEditor;
using UnityEditor.SceneManagement;

namespace ParkingMastery.EditorTools
{
    /// <summary>Menü: Parking-Mastery ▸ Play From Boot. Açıkken Play her zaman Boot sahnesinden başlar (varsayılan: kapalı).</summary>
    [InitializeOnLoad]
    public static class PlayFromBoot
    {
        private const string Key = "PM_PlayFromBoot";
        private const string Menu = "Parking-Mastery/Play From Boot";
        private const string BootPath = "Assets/_Project/Scenes/Boot.unity";

        static PlayFromBoot() => Apply();

        [MenuItem(Menu)]
        private static void Toggle()
        {
            EditorPrefs.SetBool(Key, !EditorPrefs.GetBool(Key, false));
            Apply();
        }

        [MenuItem(Menu, true)]
        private static bool Validate()
        {
            Menu_SetChecked();
            return true;
        }

        private static void Menu_SetChecked() => UnityEditor.Menu.SetChecked(Menu, EditorPrefs.GetBool(Key, false));

        private static void Apply()
        {
            EditorSceneManager.playModeStartScene = EditorPrefs.GetBool(Key, false)
                ? AssetDatabase.LoadAssetAtPath<SceneAsset>(BootPath)
                : null;
        }
    }
}
