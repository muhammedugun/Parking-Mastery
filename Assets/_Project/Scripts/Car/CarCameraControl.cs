using System.Collections.Generic;
using UnityEngine;
using ParkingMastery.Core;
using ParkingMastery.Services;

namespace ParkingMastery.Car
{
    /// <summary>
    /// Arabayı takip eden kamera.
    /// Hedef araç ya da kamera noktaları henüz yoksa hata seli üretmeden bekler ve periyodik olarak yeniden arar.
    /// Kamera noktaları: arabanın altındaki 'CameraPositions' objesinin çocukları. Son çocuk **arka kameradır**
    /// (geri giderken kullanılır), önceki çocuklar ileri kameralardır ve SwitchCamera() ile sırayla gezilir.
    /// </summary>
    public class CarCameraControl : MonoBehaviour
    {
        private const string CameraPositionsName = "CameraPositions";
        private const float TargetSearchInterval = 0.5f;

        [Tooltip("Takip keskinliği (1/sn). Büyük değer kamerayı hedefe daha hızlı yaklaştırır. Kare hızından bağımsızdır.")]
        [SerializeField] private float _followSharpness = 6.7f;

        [SerializeField] private CarControl _carControl;
        private readonly List<Transform> _positions = new List<Transform>();
        private int _currentIndex;
        private float _nextTargetSearchTime;
        private bool _warnedMissingTarget;
        private bool _warnedMissingPositions;
        private bool _needsSnap;

        [Header("Çarpma sarsıntısı")]
        [Tooltip("Tam şiddetli çarpmada sarsıntının sönme süresi (sn). 0'a yakın değer sarsıntıyı neredeyse kapatır.")]
        [SerializeField, Min(0.05f)] private float _shakeDuration = 0.4f;
        [Tooltip("Tam şiddette kamera ekseninde en büyük kayma (metre). 0 = sarsıntı kapalı.")]
        [SerializeField, Min(0f)] private float _shakeMaxOffset = 0.15f;
        [Tooltip("Tam şiddette en büyük yalpa (derece, kamera ekseni).")]
        [SerializeField, Min(0f)] private float _shakeMaxRoll = 1.5f;
        [Tooltip("Sarsıntı titreşim sıklığı (Hz benzeri).")]
        [SerializeField, Min(1f)] private float _shakeFrequency = 28f;

        /// <summary>Kalan sarsıntı (0..1); çarpmada yükselir, zamanla söner. Zaman ölçeğinden bağımsızdır (popup'ta timeScale 0 olsa da kamera ofsette kalmaz).</summary>
        private float _trauma;
        /// <summary>Sarsıntısız takip pozu; takip bunun üzerinden yürür, sarsıntı yalnız görüntüye eklenir (ofset takibe sızmaz).</summary>
        private Vector3 _basePosition;
        private Quaternion _baseRotation = Quaternion.identity;

        [Header("Görüş açısı (FOV)")]
        [Tooltip("Araç tam hızdayken tabana eklenen görüş açısı (derece): hız hissi verir. 0 = hıza bağlı zoom kapalı.")]
        [SerializeField, Min(0f)] private float _speedFovBonus = 8f;
        [Tooltip("FOV değişiminin yumuşaklığı (1/sn). Büyük değer daha çabuk uyum sağlar.")]
        [SerializeField, Min(0.1f)] private float _fovSharpness = 4f;
        [Tooltip("Mobil cihazda tabana eklenen görüş açısı (derece): küçük ekranda daha geniş alan.")]
        [SerializeField, Min(0f)] private float _mobileFovBonus = 6f;
        [Tooltip("Referans en-boy oranı. Ekran bundan darsa yatay görüş korunacak şekilde FOV büyütülür.")]
        [SerializeField, Min(0.5f)] private float _referenceAspect = 16f / 9f;
        [Tooltip("Ekran uyarlamasından sonra taban FOV'un üst sınırı (derece).")]
        [SerializeField, Range(40f, 110f)] private float _maxFov = 85f;

        [Header("Park sinematiği")]
        [Tooltip("Park tamamlanınca kameranın araca doğru ilerleyeceği en fazla mesafe (metre). 0 = yaklaşma kapalı.")]
        [SerializeField, Min(0f)] private float _parkApproachDistance = 1.5f;
        [Tooltip("Yaklaşırken kamera ile araç arasında kalacak en az mesafe (metre).")]
        [SerializeField, Min(0.5f)] private float _parkMinDistance = 2.5f;
        [Tooltip("Kameranın baktığı nokta: araç merkezinin üstünde (metre).")]
        [SerializeField] private float _parkFocusHeight = 0.6f;
        [Tooltip("Park tamamlanınca FOV'un daralacağı miktar (derece). 0 = zoom kapalı.")]
        [SerializeField, Min(0f)] private float _parkFovReduction = 12f;
        [Tooltip("Sinematik yaklaşmanın süresi (sn).")]
        [SerializeField, Min(0.1f)] private float _parkDuration = 0.9f;

        private Camera _camera;
        private float _baseFov = 60f;
        private float _currentFov = 60f;
        private bool _fovInitialized;
        private bool _isMobile;
        private bool _parkingActive;
        /// <summary>Park yaklaşmasının ilerlemesi (0..1); zaman ölçeğinden bağımsız ilerler.</summary>
        private float _parkBlend;

        private bool IsReady => _carControl != null && _positions.Count > 0;
        private bool HasRearCamera => _positions.Count >= 2;
        private int RearIndex => _positions.Count - 1;
        private int ForwardCount => Mathf.Max(1, _positions.Count - 1);

        private void Awake()
        {
            if (TryGetComponent(out _camera))
                _baseFov = _camera.fieldOfView;
        }

        private void OnEnable()
        {
            EventBus.Subscribe(GameEventType.CarDirectionChanged, RearCameraControl);
            EventBus.Subscribe<ObstacleHitEvent>(OnObstacleHit);
            EventBus.Subscribe(GameEventType.ParkingSuccessful, OnParkingSuccessful);
            EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
            _parkingActive = false;
            _parkBlend = 0f;
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(GameEventType.CarDirectionChanged, RearCameraControl);
            EventBus.Unsubscribe<ObstacleHitEvent>(OnObstacleHit);
            EventBus.Unsubscribe(GameEventType.ParkingSuccessful, OnParkingSuccessful);
            EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
        }

        private void Start()
        {
            _isMobile = GameServices.Platform.IsMobile;
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
                _basePosition = target.position;
                _baseRotation = target.rotation;
                ApplyShake();
                _needsSnap = false;
                UpdateFov(true);
                return;
            }

            float t = 1f - Mathf.Exp(-_followSharpness * Time.deltaTime);
            _basePosition = Vector3.Lerp(_basePosition, target.position, t);
            _baseRotation = Quaternion.Slerp(_baseRotation, target.rotation, t);
            ApplyShake();
            UpdateFov(false);
        }

        /// <summary>Çarpma sayıldı: şiddeti kadar sarsıntı ekler (üst üste çarpmada büyük olan korunur).</summary>
        private void OnObstacleHit(ObstacleHitEvent evt)
        {
            _trauma = Mathf.Max(_trauma, Mathf.Clamp01(evt.Intensity));
        }

        /// <summary>Takip pozuna (varsa) sönümlü Perlin sarsıntısı ekleyip kameraya yazar. Şiddet karesel uygulanır: hafif çarpma belli belirsiz, sert çarpma belirgin.</summary>
        private void ApplyShake()
        {
            Vector3 position = _basePosition;
            Quaternion rotation = _baseRotation;
        
            // Park sonrası sinematik yaklaşma: kamera araca doğru ilerler (en az _parkMinDistance kalır)
            if (_parkBlend > 0f && _carControl != null)
            {
                Vector3 toFocus = _carControl.transform.position + Vector3.up * _parkFocusHeight - position;
                float distance = toFocus.magnitude;
                if (distance > 0.01f)
                {
                    float move = Mathf.Min(_parkApproachDistance * ParkEase(), Mathf.Max(0f, distance - _parkMinDistance));
                    position += toFocus / distance * move;
                }
            }

            if (_trauma > 0f)
            {
                _trauma = Mathf.Max(0f, _trauma - Time.unscaledDeltaTime / _shakeDuration);
                float strength = _trauma * _trauma;
                float n = Time.unscaledTime * _shakeFrequency;
                Vector3 offset = new Vector3(ShakeNoise(n, 0f), ShakeNoise(n, 10f), ShakeNoise(n, 20f)) * (_shakeMaxOffset * strength);
                position += _baseRotation * offset;
                rotation = _baseRotation * Quaternion.Euler(0f, 0f, ShakeNoise(n, 30f) * _shakeMaxRoll * strength);
            }

            transform.SetPositionAndRotation(position, rotation);
        }

        private static float ShakeNoise(float x, float seed)
        {
            return (Mathf.PerlinNoise(x, seed) - 0.5f) * 2f;
        }

        /// <summary>Park tamamlandı: sinematik yaklaşma başlar (zoom + araca yaklaşma).</summary>
        private void OnParkingSuccessful()
        {
            _parkingActive = true;
        }

        /// <summary>Yeni bir deneme/seviye oynanmaya başlayınca sinematik yaklaşma sıfırlanır (kamera normale döner).</summary>
        private void OnGameStateChanged(GameStateChanged evt)
        {
            if (evt.Current == GameState.Playing)
                _parkingActive = false;
        }

        /// <summary>0..1 yaklaşma ilerlemesini yumuşak başlayıp yumuşak biten (smoothstep) eğriye çevirir.</summary>
        private float ParkEase()
        {
            return _parkBlend * _parkBlend * (3f - 2f * _parkBlend);
        }

        /// <summary>Görüş açısı = (ekrana uyarlanmış taban) + hız payı − park yaklaşması. Zaman ölçeğinden bağımsız yumuşatılır (popup'ta da yaklaşma tamamlanır).</summary>
        private void UpdateFov(bool immediate)
        {
            if (_camera == null)
                return;
        
            _parkBlend = Mathf.MoveTowards(_parkBlend, _parkingActive ? 1f : 0f, Time.unscaledDeltaTime / _parkDuration);
        
            float speed01 = _carControl != null ? Mathf.Clamp01(_carControl.CurrentSpeed / Mathf.Max(0.1f, _carControl.MaxSpeed)) : 0f;
            float target = ScreenBaseFov() + _speedFovBonus * speed01 - _parkFovReduction * ParkEase();
            target = Mathf.Clamp(target, 20f, 100f);
        
            _currentFov = (immediate || !_fovInitialized)
                ? target
                : Mathf.Lerp(_currentFov, target, 1f - Mathf.Exp(-_fovSharpness * Time.unscaledDeltaTime));
            _fovInitialized = true;
            _camera.fieldOfView = _currentFov;
        }

        /// <summary>Taban görüş açısı: mobilde biraz geniş; ekran referanstan (16:9) darsa yatay görüş korunacak şekilde büyütülür (üst sınır _maxFov).</summary>
        private float ScreenBaseFov()
        {
            float fov = _baseFov + (_isMobile ? _mobileFovBonus : 0f);
            float aspect = _camera.aspect;
            if (aspect > 0.01f && aspect < _referenceAspect)
            {
                float halfTan = Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) * _referenceAspect / aspect;
                fov = 2f * Mathf.Atan(halfTan) * Mathf.Rad2Deg;
            }
            return Mathf.Min(fov, _maxFov);
        }

        private void TryFindTarget()
        {
            _nextTargetSearchTime = Time.time + TargetSearchInterval;

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
