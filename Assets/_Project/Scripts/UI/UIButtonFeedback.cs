using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ParkingMastery.UI
{
    /// <summary>
    /// Butonlara basma animasyonu (küçülüp geri dönme) ve tıklama sesi ekler. Button/Toggle/Slider gibi
    /// herhangi bir <see cref="Selectable"/> ile (ya da tek başına) çalışır; etkileşime kapalı butonda tepki vermez.
    /// Animasyon <c>Time.unscaledDeltaTime</c> kullanır (pause menüsünde de çalışır).
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Parking Mastery/UI/Button Feedback")]
    public sealed class UIButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Ses")]
        [SerializeField] private AudioClip clickClip;
        [SerializeField, Range(0f, 1f)] private float clickVolume = 1f;

        [Header("Animasyon")]
        [SerializeField, Range(0.7f, 1f)] private float pressedScale = 0.94f;
        [SerializeField, Min(0f)] private float animationTime = 0.08f;

        private Selectable _selectable;
        private Vector3 _baseScale;
        private Coroutine _routine;
        private bool _pressed;

        private bool IsInteractable => _selectable == null || _selectable.IsInteractable();

        private void Awake()
        {
            TryGetComponent(out _selectable);
            _baseScale = transform.localScale;
        }

        private void OnDisable()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }

            _pressed = false;
            transform.localScale = _baseScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!IsInteractable)
                return;

            _pressed = true;
            ScaleTo(pressedScale);
        }

        public void OnPointerUp(PointerEventData eventData) => Release();

        public void OnPointerExit(PointerEventData eventData) => Release();

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsInteractable)
                UISfx.Play(clickClip, clickVolume);
        }

        private void Release()
        {
            if (!_pressed)
                return;

            _pressed = false;
            ScaleTo(1f);
        }

        private void ScaleTo(float factor)
        {
            if (!isActiveAndEnabled)
                return;

            if (_routine != null)
                StopCoroutine(_routine);

            _routine = StartCoroutine(Animate(_baseScale * factor));
        }

        private IEnumerator Animate(Vector3 target)
        {
            var from = transform.localScale;
            float elapsed = 0f;
            while (elapsed < animationTime)
            {
                elapsed += Time.unscaledDeltaTime;
                transform.localScale = Vector3.Lerp(from, target, Mathf.Clamp01(elapsed / animationTime));
                yield return null;
            }

            transform.localScale = target;
            _routine = null;
        }

#if UNITY_EDITOR
        // Bileşen eklenince varsayılan tıklama sesini otomatik ata.
        private void Reset()
        {
            clickClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/UI/UI_Click.wav");
        }
#endif
    }
}
