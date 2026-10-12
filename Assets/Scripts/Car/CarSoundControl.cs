using UnityEngine;
using YG;

/// <summary>
/// Aracın motor sesini hıza göre ayarlar: araç dururken kısık, hızlandıkça daha yüksek ve tiz.
/// Ses, oyuncunun motor sesi ayarına (YG2.saves.isEngineSoundOn) ve oyun içi sessize alma olaylarına uyar.
/// </summary>
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

    private void Start()
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
        ApplyMute();
    }

    private void OnEnable()
    {
        EventBus.Subscribe(EventType.MuteCarSound, Mute);
        EventBus.Subscribe(EventType.UnMuteCarSound, UnMute);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(EventType.MuteCarSound, Mute);
        EventBus.Unsubscribe(EventType.UnMuteCarSound, UnMute);
    }

    private void FixedUpdate()
    {
        if (_carEngineSound == null)
            return;

        float maxSpeed = _carControl != null ? _carControl.MaxSpeed : 10f;
        float targetFactor = Mathf.Clamp01(_carRigidbody.linearVelocity.magnitude / Mathf.Max(maxSpeed, 0.01f));
        _speedFactor = Mathf.Lerp(_speedFactor, targetFactor, 1f - Mathf.Exp(-_smoothing * Time.fixedDeltaTime));

        _carEngineSound.volume = _initialVolume * Mathf.Lerp(_idleVolumeFactor, 1f, _speedFactor);
        _carEngineSound.pitch = _initialPitch + _speedFactor * _pitchRange;

        ApplyMute();
    }

    public void Mute()
    {
        _isMutedByGame = true;
        ApplyMute();
    }

    public void UnMute()
    {
        _isMutedByGame = false;
        ApplyMute();
    }

    /// <summary>
    /// Motor sesi, oyun tarafından sessize alınmışsa veya oyuncu ayarlardan kapattıysa duyulmaz.
    /// </summary>
    private void ApplyMute()
    {
        if (_carEngineSound != null)
            _carEngineSound.mute = _isMutedByGame || !YG2.saves.isEngineSoundOn;
    }
}
