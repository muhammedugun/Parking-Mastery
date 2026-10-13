using ParkingMastery.Core;
using UnityEngine;

namespace ParkingMastery.Gameplay
{
    /// <summary>
    /// Park bölgesi (C2, eski ParkingArea): aracın tam içinde olması + yön/açı + bekleme süresi kontrolü ve görsel geri bildirim.
    /// Başarı: araç koşulları <see cref="_requiredStayTime"/> saniye boyunca sağlarsa ParkingSuccessful yayınlanır.
    /// </summary>
    public class ParkingZone : MonoBehaviour
    {
        private enum Fit { Outside, WrongAngle, Ok }

        [SerializeField] private GameObject _arrowDown;

        [Header("Kurallar")]
        [Tooltip("Araç kutusunun park alanı sınırından taşabileceği pay (dünya birimi). Alan aracın boyutuna çok yakınsa artır.")]
        [SerializeField, Min(0f)] private float _fitTolerance = 0.1f;
        [Tooltip("Aracın yönü ile bölge yönü arasındaki en büyük açı (derece). 90 = eski kural (aynı yöne bakmak).")]
        [SerializeField, Range(1f, 180f)] private float _maxAngle = 90f;
        [Tooltip("Araç koşulları bu kadar saniye sağlarsa park başarılı sayılır.")]
        [SerializeField, Min(0.1f)] private float _requiredStayTime = 2f;

        [Header("Görsel geri bildirim")]
        [SerializeField] private Color _alignedColor = Color.blue;
        [SerializeField] private Color _wrongAngleColor = new Color(1f, 0.85f, 0.1f);
        [SerializeField] private Color _successColor = Color.green;

        private Collider _collider;
        private MeshRenderer _meshRenderer;
        private Color _defaultColor;
        private bool _cached;

        /// <summary>Araç şu an bölgenin içinde ve yönü doğru (süre sayıyor).</summary>
        public bool IsCarInside { get; private set; }

        /// <summary>Koşulların kesintisiz sağlandığı süre (sn).</summary>
        public float StayTime { get; private set; }

        public float RequiredStayTime => _requiredStayTime;

        /// <summary>0..1 bekleme ilerlemesi (UI halkası için).</summary>
        public float StayProgress => Mathf.Clamp01(StayTime / _requiredStayTime);

        /// <summary>Park tamamlandı (ParkingSuccessful yayınlandı).</summary>
        public bool IsCompleted { get; private set; }

        /// <summary>LevelLoader: araç kutusunun alan sınırından taşabileceği payı ayarlar (LevelData.ParkingTarget.fitTolerance).</summary>
        public void SetFitTolerance(float tolerance) { _fitTolerance = Mathf.Max(0f, tolerance); }

        private void Start()
        {
            EnsureCached();
        }

        private void EnsureCached()
        {
            if (_cached) return;
            _cached = true;
            _collider = GetComponent<Collider>();
            _meshRenderer = GetComponent<MeshRenderer>();
            if (_meshRenderer != null) _defaultColor = _meshRenderer.material.color;
        }

        private void OnTriggerStay(Collider other)
        {
            if (IsCompleted || !other.CompareTag("Player")) return;

            switch (Evaluate(other))
            {
                case Fit.Ok:
                    if (!IsCarInside)
                    {
                        IsCarInside = true;
                        EventBus.Publish(GameEventType.CarInsideParkingArea);
                    }

                    if (_arrowDown != null && _arrowDown.activeSelf) _arrowDown.SetActive(false);
                    ChangeColor(_alignedColor);
                    StayTime += Time.deltaTime;
                    if (StayTime >= _requiredStayTime)
                    {
                        IsCompleted = true;
                        ChangeColor(_successColor);
                        EventBus.Publish(GameEventType.ParkingSuccessful);
                    }
                    break;

                case Fit.WrongAngle:
                    // İçeride ama yön yanlış: sayaç sıfırlanır, sarı uyarı gösterilir.
                    ResetParkingState(_wrongAngleColor);
                    break;

                default:
                    ResetParkingState(_defaultColor);
                    break;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Araç alandan çıktığında OnTriggerStay artık çağrılmaz; durumu burada sıfırla (AUDIT PA-1).
            if (other.CompareTag("Player") && !IsCompleted)
            {
                ResetParkingState(_defaultColor);
            }
        }

        private Fit Evaluate(Collider car)
        {
            EnsureCached();
            if (!CheckCarInsideBounds(car)) return Fit.Outside;
            return IsAngleOk(car.transform) ? Fit.Ok : Fit.WrongAngle;
        }

        /// <summary>Park durumunu başa alır: süre, renk, ok ve olay. Başarıdan sonra çağrılmamalı.</summary>
        private void ResetParkingState(Color color)
        {
            if (IsCarInside)
            {
                IsCarInside = false;
                EventBus.Publish(GameEventType.CarOutsideParkingArea);
            }

            if (_arrowDown != null && !_arrowDown.activeSelf)
                _arrowDown.SetActive(true);

            ChangeColor(color);
            StayTime = 0f;
        }

        private void ChangeColor(Color color)
        {
            if (_meshRenderer != null && _meshRenderer.material != null && _meshRenderer.material.color != color)
            {
                _meshRenderer.material.color = color;
            }
        }

        /// <summary>Aracın yönü ile bölge yönü arasındaki yatay açı <see cref="_maxAngle"/> içinde mi? (Eğim/yatış yok sayılır.)</summary>
        private bool IsAngleOk(Transform carTransform)
        {
            Vector3 zoneDir = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            Vector3 carDir = Vector3.ProjectOnPlane(carTransform.forward, Vector3.up);
            if (zoneDir.sqrMagnitude < 1e-6f || carDir.sqrMagnitude < 1e-6f) return true;
            return Vector3.Angle(zoneDir, carDir) <= _maxAngle;
        }

        /// <summary>Arabanın park alanının içerisinde olup olmadığını kontrol et.</summary>
        private bool CheckCarInsideBounds(Collider carCollider)
        {
            var areaBox = _collider as BoxCollider;
            if (areaBox == null || !TryGetLocalBox(carCollider, out Vector3 carCenter, out Vector3 carExtents))
            {
                // Kutu olmayan collider'larda eski yönteme dön
                Bounds carBounds = carCollider.bounds;
                return _collider.bounds.Contains(carBounds.min) && _collider.bounds.Contains(carBounds.max);
            }

            // Park alanının ve araç kutusunun dönüşüne göre kontrol: araç eğik olsa bile sınır kutusu şişmez.
            // Araç kutusunun 8 köşesi park alanının yerel eksenlerinde karşılaştırılır. Dikey eksen yok sayılır.
            int upAxis = GetUpAxis();
            Vector3 areaHalfSize = areaBox.size * 0.5f;
            Vector3 areaScale = transform.lossyScale;
            Transform carTransform = carCollider.transform;

            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = carCenter + new Vector3(
                    (i & 1) == 0 ? -carExtents.x : carExtents.x,
                    (i & 2) == 0 ? -carExtents.y : carExtents.y,
                    (i & 4) == 0 ? -carExtents.z : carExtents.z);

                Vector3 local = transform.InverseTransformPoint(carTransform.TransformPoint(corner)) - areaBox.center;

                for (int axis = 0; axis < 3; axis++)
                {
                    if (axis == upAxis)
                        continue;

                    float margin = _fitTolerance / Mathf.Max(Mathf.Abs(areaScale[axis]), 0.0001f);
                    if (Mathf.Abs(local[axis]) > areaHalfSize[axis] + margin)
                        return false;
                }
            }

            return true;
        }

        /// <summary>Park alanının yerel eksenlerinden hangisinin dünya yukarısına en yakın olduğunu bulur.</summary>
        private int GetUpAxis()
        {
            float dotX = Mathf.Abs(Vector3.Dot(transform.right, Vector3.up));
            float dotY = Mathf.Abs(Vector3.Dot(transform.up, Vector3.up));
            float dotZ = Mathf.Abs(Vector3.Dot(transform.forward, Vector3.up));

            if (dotY >= dotX && dotY >= dotZ)
                return 1;
            return dotX >= dotZ ? 0 : 2;
        }

        /// <summary>Collider'ın kendi yerel uzayındaki kutusunu (merkez ve yarı boyutlar) döndürür.</summary>
        private static bool TryGetLocalBox(Collider collider, out Vector3 center, out Vector3 extents)
        {
            if (collider is BoxCollider box)
            {
                center = box.center;
                extents = box.size * 0.5f;
                return true;
            }

            if (collider is MeshCollider meshCollider && meshCollider.sharedMesh != null)
            {
                center = meshCollider.sharedMesh.bounds.center;
                extents = meshCollider.sharedMesh.bounds.extents;
                return true;
            }

            if (collider is CapsuleCollider capsule)
            {
                center = capsule.center;
                extents = new Vector3(capsule.radius, capsule.radius, capsule.radius);
                extents[capsule.direction] = Mathf.Max(capsule.height * 0.5f, capsule.radius);
                return true;
            }

            if (collider is SphereCollider sphere)
            {
                center = sphere.center;
                extents = Vector3.one * sphere.radius;
                return true;
            }

            center = Vector3.zero;
            extents = Vector3.zero;
            return false;
        }
    }
}
