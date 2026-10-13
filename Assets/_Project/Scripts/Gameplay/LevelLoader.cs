using ParkingMastery.Car;
using ParkingMastery.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace ParkingMastery.Gameplay
{
    /// <summary>
    /// Tek Gameplay sahnesinde seviyeyi LevelData'dan kurar (B7): araç, park alanı, yıldız eşikleri, çarpma hakkı, engeller.
    /// Seviye numarası LevelSession.CurrentLevelId'dir. Awake'te (diğerlerinden önce) kurar; oyunu LevelController başlatır.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class LevelLoader : MonoBehaviour
    {
        [SerializeField] private LevelCatalog _catalog;
        [SerializeField] private Transform _car;
        [SerializeField] private CarControl _carControl;
        [FormerlySerializedAs("_parkingArea")]
        [SerializeField] private ParkingZone _parkingZone;
        [FormerlySerializedAs("_starCounter")]
        [SerializeField] private LevelScoring _scoring;
        [SerializeField] private Transform _obstaclesRoot;

        /// <summary>Şu an yüklü seviye verisi.</summary>
        public LevelData Current { get; private set; }

        private void Awake()
        {
            if (_catalog == null)
            {
                Debug.LogError("[LevelLoader] LevelCatalog atanmamış.");
                return;
            }

            LevelSession.LevelCount = _catalog.MaxLevelId;
            int id = LevelSession.CurrentLevelId;
            if (!_catalog.TryGet(id, out var data))
            {
                Debug.LogError("[LevelLoader] Seviye bulunamadı: " + id + " → seviye seçimine dönülüyor.");
                SceneLoadManager.LoadSceneStatic("LevelSelectionMenu");
                return;
            }
            Apply(data);
        }

        /// <summary>Seviyeyi sahneye uygular (önceki engelleri temizler). Editörde doğrulama için de çağrılabilir.</summary>
        public void Apply(LevelData data)
        {
            ClearObstacles();

            // Araç
            var spawn = data.CarSpawn;
            _car.SetPositionAndRotation(spawn.position, Quaternion.Euler(0f, spawn.yaw, 0f));
            _carControl.SetMaxObstacleHits(data.MaxObstacleHits);

            // Park alanı: boyut = BoxCollider.size × ölçek
            var target = data.ParkingTarget;
            var areaTransform = _parkingZone.transform;
            areaTransform.SetPositionAndRotation(target.position, Quaternion.Euler(0f, target.yaw, 0f));
            var box = _parkingZone.GetComponent<BoxCollider>();
            Vector3 unit = box != null ? box.size : Vector3.one;
            areaTransform.localScale = new Vector3(
                unit.x > 0f ? target.size.x / unit.x : 1f,
                unit.y > 0f ? target.size.y / unit.y : 1f,
                unit.z > 0f ? target.size.z / unit.z : 1f);
            _parkingZone.SetFitTolerance(target.fitTolerance);

            // Puanlama (yıldız eşikleri LevelData'dan)
            _scoring.Configure(data);

            // Engeller
            var list = data.Obstacles;
            for (int i = 0; i < list.Count; i++)
            {
                var p = list[i];
                if (p.prefab == null) continue;
                var go = Instantiate(p.prefab, p.position, Quaternion.Euler(p.eulerAngles), _obstaclesRoot);
                go.name = p.prefab.name;
                go.transform.localScale = p.scale == Vector3.zero ? Vector3.one : p.scale;
            }

            Physics.SyncTransforms();
            Current = data;
            LevelSession.CurrentLevelId = data.LevelId;
        }

        private void ClearObstacles()
        {
            for (int i = _obstaclesRoot.childCount - 1; i >= 0; i--)
            {
                var child = _obstaclesRoot.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child); else DestroyImmediate(child);
            }
        }
    }
}
