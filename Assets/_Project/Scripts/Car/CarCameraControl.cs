using System.Collections.Generic;
using UnityEngine;
using ParkingMastery.Core;

/// <summary>
/// Arabayı takip eden kamera.
/// Hedef araç ya da kamera noktaları henüz yoksa hata seli üretmeden bekler ve periyodik olarak yeniden arar.
/// Kamera noktaları: arabanın altındaki 'CameraPositions' objesinin çocukları. Son çocuk **arka kameradır**
/// (geri giderken kullanılır), önceki çocuklar ileri kameralardır ve SwitchCamera() ile sırayla gezilir.
/// </summary>

namespace ParkingMastery.Car
{
    public class CarCameraControl : MonoBehaviour
    {
        private const string CameraPositionsName = "CameraPositions";
        private const float TargetSearchInterval = 0.5f;

        [Tooltip("Takip keskinliği (1/sn). Büyük değer kamerayı hedefe daha hızlı yaklaştırır. Kare hızından bağımsızdır.")]
        [SerializeField] private float _followSharpness = 6.7f;

        private CarControl _carControl;
        private readonly List<Transform> _positions = new List<Transform>();
        private int _currentIndex;
        private float _nextTargetSearchTime;
        private bool _warnedMissingTarget;
        private bool _warnedMissingPositions;
        private bool _needsSnap;

        private bool IsReady => _carControl != null && _positions.Count > 0;
        private bool HasRearCamera => _positions.Count >= 2;
        private int RearIndex => _positions.Count - 1;
        private int ForwardCount => Mathf.Max(1, _positions.Count - 1);

        private void OnEnable()
        {
            EventBus.Subscribe(GameEventType.CarDirectionChanged, RearCameraControl);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(GameEventType.CarDirectionChanged, RearCameraControl);
        }

        private void Start()
        {
            TryFindTarget();
        }

        /// <summary>
        /// Takip edilecek arabayı dışarıdan verir (araba sonradan spawn edildiğinde kullanılır).
        /// </summary>
        public void SetTarget(CarControl car)
        {
            _carControl = car;
            _warnedMissingTarget = false;
            _warnedMissingPositions = false;
            BindCameraPositions();
        }

        // Takip, fizik adımından sonra ve kare hızına bağlı (dt) çalışır: araç interpolation açıkken bile titremez.
        private void LateUpdate()
        {
            if (!IsReady)
            {
                // Hedef henüz hazır değil: log seli yapmadan aralıklarla tekrar dene
                if (Time.time >= _nextTargetSearchTime)
                    TryFindTarget();

                if (!IsReady)
                    return;
            }

            if (_currentIndex < 0 || _currentIndex >= _positions.Count)
                _currentIndex = 0;

            Transform target = _positions[_currentIndex];
            if (target == null)
            {
                // Çocuklar çalışma anında silinmiş olabilir: yeniden bağla
                BindCameraPositions();
                return;
            }

            if (_needsSnap)
            {
                // Hedef ilk bağlandığında editör konumundan araca uçmak yerine doğrudan oraya git
                transform.SetPositionAndRotation(target.position, target.rotation);
                _needsSnap = false;
                return;
            }

            float t = 1f - Mathf.Exp(-_followSharpness * Time.deltaTime);
            transform.SetPositionAndRotation(
                Vector3.Lerp(transform.position, target.position, t),
                Quaternion.Slerp(transform.rotation, target.rotation, t));
        }

        private void TryFindTarget()
        {
            _nextTargetSearchTime = Time.time + TargetSearchInterval;

            if (_carControl == null)
                _carControl = FindFirstObjectByType<CarControl>();

            if (_carControl == null)
            {
                if (!_warnedMissingTarget)
                {
                    _warnedMissingTarget = true;
                    Debug.LogWarning("CarCameraControl: sahnede CarControl bulunamadı, araç görünene kadar bekleniyor.", this);
                }
                return;
            }

            BindCameraPositions();
        }

        /// <summary>
        /// Kamera noktalarını bir kez bulup önbelleğe alır (her karede isimle arama / GetChild yok).
        /// </summary>
        private void BindCameraPositions()
        {
            _positions.Clear();
            _currentIndex = 0;

            if (_carControl == null)
                return;

            Transform root = _carControl.transform.Find(CameraPositionsName);
            if (root != null)
            {
                for (int i = 0; i < root.childCount; i++)
                    _positions.Add(root.GetChild(i));
            }

            if (_positions.Count == 0)
            {
                if (!_warnedMissingPositions)
                {
                    _warnedMissingPositions = true;
                    Debug.LogWarning("CarCameraControl: arabanın altında dolu bir '" + CameraPositionsName + "' objesi bulunamadı!", this);
                }
                return;
            }

            _needsSnap = true;
        }

        /// <summary>
        /// Bir sonraki ileri kameraya geçer (arka kamera sırada değildir; yön değişince otomatik seçilir).
        /// </summary>
        public void SwitchCamera()
        {
            if (!IsReady)
                return;

            // Arka kameradaysa ileri kameraların başına dön; değilse sıradakine geç (modulo ile döngü)
            _currentIndex = (HasRearCamera && _currentIndex == RearIndex) ? 0 : (_currentIndex + 1) % ForwardCount;
            RearCameraControl();
        }

        public void SwitchCamera(int index)
        {
            if (!IsReady || index < 0 || index >= _positions.Count)
                return;

            _currentIndex = index;
        }

        /// <summary>
        /// Arka kamera kontrolü: geri giderken arka kameraya, ileri giderken ileri kameraya geçer.
        /// </summary>
        public void RearCameraControl()
        {
            if (!IsReady || !HasRearCamera)
                return;

            if (_carControl.IsGoingForward && _currentIndex == RearIndex)
                SwitchCamera(0);
            else if (!_carControl.IsGoingForward && _currentIndex == 0)
                SwitchCamera(RearIndex);
        }
    }
}
