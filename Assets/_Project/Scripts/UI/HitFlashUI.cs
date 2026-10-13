using UnityEngine;
using UnityEngine.UI;
using ParkingMastery.Core;

namespace ParkingMastery.UI
{
    /// <summary>
    /// Çarpma uyarısı: ObstacleHitEvent gelince ekranı kısaca kırmızıya boyar (dilden bağımsız uyarı).
    /// Şiddet ne kadar yüksekse parlama o kadar belirgindir. Image yalnız görseldir, dokunmayı engellemez.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class HitFlashUI : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [Tooltip("Tam şiddetli çarpmada en yüksek kırmızı opaklık (0..1).")]
        [SerializeField, Range(0f, 1f)] private float _maxAlpha = 0.35f;
        [Tooltip("Parlamanın sönme süresi (sn).")]
        [SerializeField, Min(0.05f)] private float _duration = 0.4f;

        private float _alpha;

        private void Awake()
        {
            if (_image == null && !TryGetComponent(out _image))
            {
                enabled = false;
                return;
            }

            _image.raycastTarget = false;
            SetAlpha(0f);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<ObstacleHitEvent>(OnObstacleHit);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ObstacleHitEvent>(OnObstacleHit);
            _alpha = 0f;
            if (_image != null)
                SetAlpha(0f);
        }

        private void OnObstacleHit(ObstacleHitEvent evt)
        {
            _alpha = Mathf.Max(_alpha, Mathf.Clamp01(evt.Intensity) * _maxAlpha);
            SetAlpha(_alpha);
        }

        // Zaman ölçeğinden bağımsız: Lose popup'ı timeScale'i 0 yapsa da parlama sönüp ekran kırmızı kalmaz.
        private void Update()
        {
            if (_alpha <= 0f)
                return;

            _alpha = Mathf.Max(0f, _alpha - _maxAlpha * Time.unscaledDeltaTime / _duration);
            SetAlpha(_alpha);
        }

        private void SetAlpha(float alpha)
        {
            Color c = _image.color;
            c.a = alpha;
            _image.color = c;
        }
    }
}
