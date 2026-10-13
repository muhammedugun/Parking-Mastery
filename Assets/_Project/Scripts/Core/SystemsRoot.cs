using UnityEngine;

namespace ParkingMastery.Core
{
    /// <summary>
    /// Kalıcı sistemler kökü (Systems prefab'ı). Boot sahnesinde bir kez yüklenir ve sahneler arası yaşar.
    /// İçinde: Music (MusicManager + ses kaynakları), SceneLoader. Kayıt/Reklam/Yerelleştirme/Platform servisleri
    /// sahne nesnesi değil, GameServices üzerinden erişilen statik servislerdir (GameBootstrap bunları erken başlatır).
    /// </summary>
    public class SystemsRoot : MonoBehaviour
    {
        public static SystemsRoot Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

#if UNITY_EDITOR
        private const string PrefabPath = "Assets/_Project/Prefabs/Systems/Systems.prefab";

        // Editörde Boot dışındaki bir sahneden Play'e basılırsa Systems yine de oluşturulsun (yalnız editör; build'de ilk sahne Boot).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EditorEnsureSystems()
        {
            if (Instance != null) return;
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab != null) Instantiate(prefab);
        }
#endif
    }
}
