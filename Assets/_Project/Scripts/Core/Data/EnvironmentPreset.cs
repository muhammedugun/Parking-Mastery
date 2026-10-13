using UnityEngine;

namespace ParkingMastery.Core
{
    /// <summary>
    /// Hava / ışık preset'i (B6). Bir LevelData bunu referans alır; uygulayıcı (RenderSettings + ana ışık) Faz G'de yazılır.
    /// Yalnızca veri taşır.
    /// </summary>
    [CreateAssetMenu(menuName = "Parking-Mastery/Environment Preset", fileName = "Env_")]
    public class EnvironmentPreset : ScriptableObject
    {
        [Header("Ana ışık")]
        public Color lightColor = Color.white;
        [Min(0f)] public float lightIntensity = 1f;
        public Vector3 lightEulerAngles = new Vector3(50f, -30f, 0f);

        [Header("Ortam")]
        public Color ambientColor = new Color(0.5f, 0.5f, 0.5f);
        [Tooltip("Boşsa mevcut skybox korunur.")]
        public Material skybox;

        [Header("Sis")]
        public bool fogEnabled;
        public Color fogColor = new Color(0.7f, 0.75f, 0.8f);
        [Min(0f)] public float fogDensity = 0.01f;

        [Header("Hava efekti (isteğe bağlı)")]
        [Tooltip("Yağmur/kar gibi parçacık prefab'ı. Boşsa efekt yok.")]
        public GameObject weatherEffectPrefab;
    }
}
