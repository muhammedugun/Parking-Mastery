using UnityEngine;
using ParkingMastery.Services;
using ParkingMastery.Core;

/// <summary>
/// Aracın motor sesini hıza göre ayarlar: araç dururken kısık, hızlandıkça daha yüksek ve tiz.
/// Ses, oyuncunun motor sesi ayarına (ISaveService.IsEngineSoundOn) ve oyun içi sessize alma olaylarına uyar.
/// Sessize alma için tek giriş noktası MuteCarSound / UnMuteCarSound olaylarıdır.
/// </summary>

namespace ParkingMastery.Car
{
    public class CarSoundControl : MonoBehaviour
    {
        [SerializeField] private AudioSource _carEngineSound;

        [Header("Hıza göre ses")]
        [Tooltip("Araç dururken ses, AudioSource'taki orijinal ses seviyesinin bu oranı kadardır. 0 = tamamen sessiz.")]
        [SerializeField, Range(0f, 1f)] private float _idleVolumeFactor = 0.25f;
        [Tooltip("Maksimum hızda perdenin, başlangıç perdesine eklenen miktarı.")]
        [SerializeField] private float _pitchRange = 0.4f;
        [Tooltip("Ses ve perde geçişlerinin yumuşaklığı. Büyük değer daha hızlı tepki verir.")]
        [SerializeField] private float _smoothing = 6f;

        private Rigidbody _carRigidbody;
        private CarControl _carControl;
        private float _initialPitch;
        private float _initialVolume;
        private float _speedFactor;
        private bool _isMutedByGame;
        private bool _isInitialized;

        private void Awake()
        {
            _carRigidbody = GetComponent<Rigidbody>();
            _carControl = GetComponent<CarControl>();

            if (_carEngineSound == null)
            {
                enabled = false;
                return;
            }

            _initialPitch = _carEngineSound.pitch;
            _initialVolume = _carEngineSound.volume;
            _isInitialized = true;
        }

        private void OnEnable()
        {
            if (!_isInitialized)
                return;

            EventBus.Subscribe(GameEventType.MuteCarSound, OnMuteRequested);
            EventBus.Subscribe(GameEventType.UnMuteCarSound, OnUnMuteRequested);
            ApplyMute();
        }

        private void OnDisable()
        {
            if (!_isInitialized)
                return;

            EventBus.Unsubscribe(GameEventType.MuteCarSound, OnMuteRequested);
            EventBus.Unsubscribe(GameEventType.UnMuteCarSound, OnUnMuteRequested);
        }

        // Ses/perde görsel-işitsel bir güncellemedir; fizik adımına (50 Hz) bağlı kalınca perde basamaklı duyulabiliyordu.
        private void Update()
        {
            float maxSpeed = _carControl != null ? _carControl.MaxSpeed : 10f;
            float targetFactor = _carRigidbody != null
                ? Mathf.Clamp01(_carRigidbody.linearVelocity.magnitude / Mathf.Max(maxSpeed, 0.01f))
                : 0f;
            _speedFactor = Mathf.Lerp(_speedFactor, targetFactor, 1f - Mathf.Exp(-_smoothing * Time.deltaTime));

            _carEngineSound.volume = _initialVolume * Mathf.Lerp(_idleVolumeFactor, 1f, _speedFactor);
            _carEngineSound.pitch = _initialPitch + _speedFactor * _pitchRange;

            // Oyuncu motor sesi ayarını sonradan değiştirebilir; yalnız değer farklıysa AudioSource'a yazılır.
            ApplyMute();
        }

        private void OnMuteRequested()
        {
            _isMutedByGame = true;
            ApplyMute();
        }

        private void OnUnMuteRequested()
        {
            _isMutedByGame = false;
            ApplyMute();
        }

        /// <summary>
        /// Motor sesi, oyun tarafından sessize alınmışsa veya oyuncu ayarlardan kapattıysa duyulmaz.
        /// </summary>
        private void ApplyMute()
        {
            bool shouldMute = _isMutedByGame || !IsEngineSoundEnabled;
            if (_carEngineSound.mute != shouldMute)
                _carEngineSound.mute = shouldMute;
        }

        /// <summary>
        /// Kayıt henüz yüklenmediyse servis motor sesini varsayılan olarak açık döndürür (WebGL'de SDK gecikmesi).
        /// </summary>
        private static bool IsEngineSoundEnabled => GameServices.Audio.IsEngineSoundOn;
    }
}
