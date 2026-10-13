using System;
using System.Collections.Generic;
using UnityEngine;

namespace ParkingMastery.UI
{
    /// <summary>
    /// Sahne düzeyinde UI ekran yöneticisi. Altındaki <see cref="ScreenBase"/> ekranlarını tipine göre kaydeder,
    /// 'Geri' yığınını tutar. Ekran (tam ekran) açılınca öncekini gizler; popup açılınca altındakini görünür bırakır.
    /// Sahneler arası kalıcı değildir; her sahnenin kendi UIManager'ı olur.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    [DisallowMultipleComponent]
    public sealed class UIManager : MonoBehaviour
    {
        [Tooltip("Başlangıçta açılacak ekran. Boşsa sahnedeki ekran durumuna dokunulmaz.")]
        [SerializeField] private ScreenBase initialScreen;
        [Tooltip("Ekranların aranacağı kök; boşsa bu nesne.")]
        [SerializeField] private Transform screensRoot;

        private readonly Dictionary<Type, ScreenBase> _byType = new Dictionary<Type, ScreenBase>();
        private readonly List<ScreenBase> _all = new List<ScreenBase>();
        private readonly List<ScreenBase> _stack = new List<ScreenBase>();

        /// <summary>Etkin sahnenin UIManager'ı (yoksa null).</summary>
        public static UIManager Instance { get; private set; }

        /// <summary>Yığının tepesindeki ekran (yoksa null).</summary>
        public ScreenBase Current => _stack.Count > 0 ? _stack[_stack.Count - 1] : null;

        /// <summary>Bir ekran açılmak üzere istendiğinde tetiklenir.</summary>
        public event Action<ScreenBase> ScreenOpened;

        /// <summary>Bir ekran kapatılmak üzere istendiğinde tetiklenir.</summary>
        public event Action<ScreenBase> ScreenClosed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("UIManager: sahnede birden fazla UIManager var, bu yok sayılıyor.", this);
                enabled = false;
                return;
            }

            Instance = this;

            var root = screensRoot != null ? screensRoot : transform;
            foreach (var screen in root.GetComponentsInChildren<ScreenBase>(true))
                Register(screen);

            if (initialScreen == null)
                return;

            foreach (var screen in _all)
                if (screen != initialScreen)
                    screen.Hide(true);

            // Sahnede zaten açık gelen başlangıç ekranında Show() erken döner ve OnShowing çalışmaz;
            // önce kapatıp açarak kancanın (olay abonelikleri, ilk yenileme) çalışmasını sağla.
            if (initialScreen.gameObject.activeSelf)
                initialScreen.Hide(true);

            Open(initialScreen, true);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Çalışma anında oluşturulan ekranı kaydeder (sahnedekiler Awake'te otomatik kaydolur).</summary>
        public void Register(ScreenBase screen)
        {
            if (screen == null || _all.Contains(screen))
                return;

            screen.Initialize();
            _all.Add(screen);

            var type = screen.GetType();
            if (_byType.ContainsKey(type))
                Debug.LogWarning($"UIManager: {type.Name} tipinde birden fazla ekran var; tipe göre erişimde ilki kullanılır.", screen);
            else
                _byType.Add(type, screen);
        }

        /// <summary>Tipe göre kayıtlı ekranı döndürür (yoksa null).</summary>
        public T Get<T>() where T : ScreenBase
        {
            return _byType.TryGetValue(typeof(T), out var screen) ? (T)screen : null;
        }

        /// <summary>Tipe göre ekranı açar ve döndürür (bulunamazsa uyarı verir, null döner).</summary>
        public T Open<T>(bool instant = false) where T : ScreenBase
        {
            var screen = Get<T>();
            if (screen == null)
            {
                Debug.LogWarning($"UIManager: {typeof(T).Name} ekranı bulunamadı.", this);
                return null;
            }

            Open(screen, instant);
            return screen;
        }

        /// <summary>Ekranı açar. Tam ekransa önceki ekranlar gizlenir; popup ise üstüne biner.</summary>
        public void Open(ScreenBase screen, bool instant = false)
        {
            if (screen == null)
                return;

            Register(screen);

            if (Current == screen && screen.IsVisible)
                return;

            int index = _stack.IndexOf(screen);

            if (!screen.IsPopup && index >= 0)
            {
                // Yığındaki bir ekrana dönülüyor: üstündekileri kapatıp geri sar.
                while (_stack.Count - 1 > index)
                    RemoveTop(instant);

                screen.Show(instant);
                ScreenOpened?.Invoke(screen);
                return;
            }

            if (index >= 0)
                _stack.RemoveAt(index);

            if (!screen.IsPopup)
            {
                for (int i = _stack.Count - 1; i >= 0; i--)
                {
                    var other = _stack[i];
                    other.Hide(instant);
                    if (other.IsPopup)
                    {
                        _stack.RemoveAt(i);
                        ScreenClosed?.Invoke(other);
                    }
                }
            }

            _stack.Add(screen);
            screen.Show(instant);
            ScreenOpened?.Invoke(screen);
        }

        /// <summary>Tipe göre ekranı kapatır.</summary>
        public void Close<T>(bool instant = false) where T : ScreenBase => Close(Get<T>(), instant);

        /// <summary>Ekranı kapatır; tepedeyse altındaki (gizlenmişse) ekran yeniden gösterilir.</summary>
        public void Close(ScreenBase screen, bool instant = false)
        {
            if (screen == null)
                return;

            bool wasTop = Current == screen;
            bool removed = _stack.Remove(screen);
            if (!removed && !screen.IsVisible)
                return;

            screen.Hide(instant);
            ScreenClosed?.Invoke(screen);

            var below = Current;
            if (wasTop && below != null && !below.IsVisible)
                below.Show(instant);
        }

        /// <summary>
        /// Geri: tepede popup varsa onu, yoksa ve altında ekran varsa tepedeki ekranı kapatır.
        /// Kök ekranı kapatmaz; bir şey kapandıysa true döner.
        /// </summary>
        public bool Back(bool instant = false)
        {
            var top = Current;
            if (top == null)
                return false;

            if (!top.IsPopup && _stack.Count < 2)
                return false;

            Close(top, instant);
            return true;
        }

        /// <summary>Tüm kayıtlı ekranları gizler ve yığını temizler.</summary>
        public void CloseAll(bool instant = false)
        {
            foreach (var screen in _all)
                screen.Hide(instant);

            _stack.Clear();
        }

        private void RemoveTop(bool instant)
        {
            var top = Current;
            _stack.RemoveAt(_stack.Count - 1);
            top.Hide(instant);
            ScreenClosed?.Invoke(top);
        }
    }
}
