using System;
using System.Collections;
using UnityEngine;

namespace ParkingMastery.UI
{
    /// <summary>Ekran geçiş animasyonu türü.</summary>
    public enum ScreenTransition
    {
        /// <summary>Animasyonsuz, anında aç/kapa.</summary>
        None,
        /// <summary>Yalnızca CanvasGroup alfa geçişi.</summary>
        Fade,
        /// <summary>Alfa + hafif ölçek geçişi.</summary>
        FadeScale
    }

    /// <summary>
    /// Tüm UI ekranlarının (menü, HUD, popup) ortak tabanı. CanvasGroup ile fade/scale geçişi yapar;
    /// geçişler <c>Time.unscaledDeltaTime</c> kullanır, bu yüzden oyun duraklatılmışken (timeScale 0) de çalışır.
    /// Kendi başına da kullanılabilir (sade paneller); özel davranış için sanal kancalar (<c>OnShowing</c> vb.) geçersiz kılınır.
    /// Ekranları <see cref="UIManager"/> yönetir; doğrudan <see cref="Show"/>/<see cref="Hide"/> çağırmak da güvenlidir.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [DisallowMultipleComponent]
    public class ScreenBase : MonoBehaviour
    {
        [Tooltip("Popup ise altındaki ekran görünür kalır ve 'Geri' yalnızca popup'ı kapatır.")]
        [SerializeField] private bool isPopup;
        [SerializeField] private ScreenTransition transition = ScreenTransition.FadeScale;
        [SerializeField, Min(0f)] private float duration = 0.25f;
        [Tooltip("FadeScale için gizliyken ölçek (1 = ölçeksiz).")]
        [SerializeField, Range(0.5f, 1f)] private float hiddenScale = 0.92f;
        [Tooltip("Ölçeklenecek öğe; boşsa bu nesnenin kendisi.")]
        [SerializeField] private RectTransform scaleTarget;

        private CanvasGroup _group;
        private Coroutine _routine;
        private bool _initialized;

        /// <summary>Popup ekranı mı?</summary>
        public bool IsPopup => isPopup;

        /// <summary>Hedef durum görünür mü (geçiş sürerken de hedefi gösterir).</summary>
        public bool IsVisible { get; private set; }

        /// <summary>Şu anda fade/scale animasyonu oynuyor mu?</summary>
        public bool IsTransitioning => _routine != null;

        /// <summary>Gösterme (animasyon dahil) tamamlanınca tetiklenir.</summary>
        public event Action<ScreenBase> Shown;

        /// <summary>Gizleme (animasyon dahil) tamamlanınca tetiklenir.</summary>
        public event Action<ScreenBase> Hidden;

        /// <summary>Bir kez çağrılır (UIManager ya da ilk Show/Hide). Başlangıç durumunu sahnedeki aktiflikten alır.</summary>
        internal void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;
            _group = GetComponent<CanvasGroup>();
            if (scaleTarget == null)
                scaleTarget = transform as RectTransform;

            IsVisible = gameObject.activeSelf;
            Apply(IsVisible ? 1f : 0f);
            OnInitialize();
        }

        /// <summary>Ekranı gösterir (nesneyi etkinleştirir ve geçişi oynatır).</summary>
        public void Show(bool instant = false)
        {
            Initialize();
            if (IsVisible && !IsTransitioning && gameObject.activeSelf)
                return;

            IsVisible = true;
            StopRoutine();
            gameObject.SetActive(true);
            OnShowing();

            if (instant || !CanAnimate())
            {
                Apply(1f);
                Complete(true);
                return;
            }

            _routine = StartCoroutine(Animate(_group.alpha, 1f, true));
        }

        /// <summary>Ekranı gizler (geçiş bitince nesneyi kapatır).</summary>
        public void Hide(bool instant = false)
        {
            Initialize();
            if (!IsVisible && !IsTransitioning && !gameObject.activeSelf)
                return;

            IsVisible = false;
            StopRoutine();
            OnHiding();
            _group.interactable = false;
            _group.blocksRaycasts = false;

            if (instant || !CanAnimate())
            {
                Apply(0f);
                Complete(false);
                return;
            }

            _routine = StartCoroutine(Animate(_group.alpha, 0f, false));
        }

        /// <summary>Ekran ilk kez hazırlanırken çağrılır.</summary>
        protected virtual void OnInitialize() { }

        /// <summary>Gösterme başlarken (nesne aktif, animasyon öncesi).</summary>
        protected virtual void OnShowing() { }

        /// <summary>Gösterme bittiğinde (ekran etkileşime hazır).</summary>
        protected virtual void OnShown() { }

        /// <summary>Gizleme başlarken.</summary>
        protected virtual void OnHiding() { }

        /// <summary>Gizleme bittiğinde (nesne kapatıldı).</summary>
        protected virtual void OnHidden() { }

        private bool CanAnimate() => transition != ScreenTransition.None && duration > 0f && isActiveAndEnabled;

        private IEnumerator Animate(float from, float to, bool show)
        {
            _group.blocksRaycasts = show;
            _group.interactable = false;

            // Yarım kalan geçişten devam ederken süre kalan mesafeyle orantılı olur.
            float length = Mathf.Max(0.0001f, duration * Mathf.Abs(to - from));
            float elapsed = 0f;
            while (elapsed < length)
            {
                elapsed += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(elapsed / length);
                Apply(Mathf.Lerp(from, to, 1f - (1f - k) * (1f - k) * (1f - k)));
                yield return null;
            }

            Apply(to);
            Complete(show);
        }

        private void Complete(bool shown)
        {
            _routine = null;
            _group.interactable = shown;
            _group.blocksRaycasts = shown;

            if (shown)
            {
                OnShown();
                Shown?.Invoke(this);
            }
            else
            {
                gameObject.SetActive(false);
                OnHidden();
                Hidden?.Invoke(this);
            }
        }

        private void Apply(float value)
        {
            _group.alpha = value;
            if (transition == ScreenTransition.FadeScale && scaleTarget != null)
                scaleTarget.localScale = Vector3.one * Mathf.Lerp(hiddenScale, 1f, value);
        }

        private void StopRoutine()
        {
            if (_routine == null)
                return;

            StopCoroutine(_routine);
            _routine = null;
        }

        private void OnDisable()
        {
            // Dışarıdan kapatılırsa yarım kalan geçiş bitmiş sayılmaz; bayat 'geçiş sürüyor' durumunu temizle.
            _routine = null;
        }
    }
}
