using System;
using System.Collections.Generic;
using UnityEngine;

namespace ParkingMastery.Core
{
    /// <summary>Dünya teması (5 dünya × 10 level).</summary>
    public enum WorldTheme { CityParking, Harbor, ConstructionSite, WinterRoad, NightCity }

    /// <summary>Aracın başlangıç konumu ve Y ekseni yönü.</summary>
    [Serializable]
    public struct SpawnPoint
    {
        public Vector3 position;
        [Tooltip("Y ekseni dönüşü (derece).")] public float yaw;
    }

    /// <summary>Park alanının konumu, yönü, boyutu ve taşma toleransı.</summary>
    [Serializable]
    public struct ParkingTarget
    {
        public Vector3 position;
        public float yaw;
        [Tooltip("Park alanı boyutu (dünya birimi, ölçek uygulanmış).")] public Vector3 size;
        [Min(0f), Tooltip("Araç kutusunun alan sınırından taşabileceği pay.")] public float fitTolerance;
    }

    /// <summary>Bir engelin prefab'ı ile dünya uzayındaki konumu, açıları ve ölçeği.</summary>
    [Serializable]
    public struct ObstaclePlacement
    {
        public GameObject prefab;
        public Vector3 position;
        [Tooltip("Dünya uzayı euler açıları (derece).")] public Vector3 eulerAngles;
        public Vector3 scale;
    }

    /// <summary>
    /// Bir seviyenin tüm verisi (B6). Tek Gameplay sahnesi + LevelLoader (B7) seviyeyi bu veriden kurar;
    /// Level Editor aracı (H1) bu veriyi üretir/doğrular. Yalnızca veri taşır (sahne nesnesi/oyun mantığı içermez).
    /// </summary>
    [CreateAssetMenu(menuName = "Parking-Mastery/Level Data", fileName = "Level_")]
    public class LevelData : ScriptableObject
    {
        [Header("Kimlik")]
        [SerializeField, Min(1)] private int levelId = 1;
        [SerializeField] private WorldTheme theme = WorldTheme.CityParking;

        [Header("Süre ve yıldız (saniye)")]
        [Tooltip("Hedef süre: tasarım referansı (H2: 30–120 sn).")]
        [SerializeField, Min(0)] private int targetTimeSeconds = 60;
        [Tooltip("Bu süre ve altında 3 yıldız.")]
        [SerializeField, Min(1)] private int threeStarTime = 45;
        [Tooltip("Bu süre ve altında 2 yıldız; üstü 1 yıldız.")]
        [SerializeField, Min(1)] private int twoStarTime = 75;
        [Tooltip("0 = süre sınırı yok (zamanlı levellerde > 0).")]
        [SerializeField, Min(0)] private int timeLimitSeconds = 0;

        [Header("Kurallar")]
        [Tooltip("Seviyeyi kaybetmeden önceki bariyer/engel çarpma hakkı.")]
        [SerializeField, Min(1)] private int maxObstacleHits = 3;

        [Header("Yerleşim")]
        [SerializeField] private SpawnPoint carSpawn;
        [SerializeField] private ParkingTarget parkingTarget = new ParkingTarget { size = Vector3.one, fitTolerance = 0.1f };
        [Tooltip("Zemin prefab'ı. Boşsa dünya varsayılanı kullanılır.")]
        [SerializeField] private GameObject groundPrefab;
        [SerializeField] private List<ObstaclePlacement> obstacles = new List<ObstaclePlacement>();

        [Header("Atmosfer")]
        [Tooltip("Level müziği. Boşsa dünya varsayılanı.")]
        [SerializeField] private AudioClip music;
        [Tooltip("Hava/ışık preset'i. Boşsa dünya varsayılanı.")]
        [SerializeField] private EnvironmentPreset environment;

        public int LevelId => levelId;
        public WorldTheme Theme => theme;
        public int TargetTimeSeconds => targetTimeSeconds;
        public int ThreeStarTime => threeStarTime;
        public int TwoStarTime => twoStarTime;
        public int TimeLimitSeconds => timeLimitSeconds;
        public int MaxObstacleHits => maxObstacleHits;
        public SpawnPoint CarSpawn => carSpawn;
        public ParkingTarget ParkingTarget => parkingTarget;
        public GameObject GroundPrefab => groundPrefab;
        public IReadOnlyList<ObstaclePlacement> Obstacles => obstacles;
        public AudioClip Music => music;
        public EnvironmentPreset Environment => environment;

        /// <summary>Geçen süreye göre yıldız (1–3). Eski GameLoopManager kuralıyla aynı (süre kesirli karşılaştırılır).</summary>
        public int CalculateStars(float elapsedSeconds)
        {
            if (elapsedSeconds > twoStarTime) return 1;
            if (elapsedSeconds > threeStarTime) return 2;
            return 3;
        }

        /// <summary>Veriyi denetler; sorunları `problems`'e ekler. Sorun yoksa true. (Ayrıntılı oynanabilirlik kontrolü H1/H13'te.)</summary>
        public bool Validate(List<string> problems)
        {
            int before = problems.Count;
            string who = "LevelData '" + name + "': ";

            if (levelId < 1) problems.Add(who + "levelId >= 1 olmalı.");
            if (threeStarTime >= twoStarTime) problems.Add(who + "3 yıldız süresi, 2 yıldız süresinden küçük olmalı (" + threeStarTime + " / " + twoStarTime + ").");
            if (timeLimitSeconds > 0 && timeLimitSeconds <= twoStarTime) problems.Add(who + "süre sınırı, 2 yıldız süresinden büyük olmalı.");
            if (maxObstacleHits < 1) problems.Add(who + "maxObstacleHits >= 1 olmalı.");
            if (parkingTarget.size.x <= 0f || parkingTarget.size.y <= 0f || parkingTarget.size.z <= 0f)
                problems.Add(who + "park alanı boyutu sıfırdan büyük olmalı.");
            for (int i = 0; i < obstacles.Count; i++)
                if (obstacles[i].prefab == null) problems.Add(who + "engel #" + i + " prefab'ı boş.");

            return problems.Count == before;
        }

        private void OnValidate()
        {
            if (twoStarTime < 1) twoStarTime = 1;
            if (threeStarTime < 1) threeStarTime = 1;
            // Inspector'da yeni eklenen engelin ölçeği (0,0,0) gelir → 1'e çek.
            for (int i = 0; i < obstacles.Count; i++)
            {
                var o = obstacles[i];
                if (o.scale == Vector3.zero) { o.scale = Vector3.one; obstacles[i] = o; }
            }
        }
    }
}
