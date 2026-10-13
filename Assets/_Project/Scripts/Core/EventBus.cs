using System;
using System.Collections.Generic;
using UnityEngine;

namespace ParkingMastery.Core
{
    /// <summary>
    /// Parametresiz / tek parametreli olay anahtarları. Önceki adı EventType idi (UnityEngine.EventType ile çakışıyordu, B5'te yeniden adlandırıldı).
    /// Yeni olaylar için tipli olay (IGameEvent) tercih edilir: bkz. GameEvents.cs.
    /// </summary>
    public enum GameEventType
    {
        //Car
        CarDirectionChanged,
        // LevelFailed: son bariyer hakkı bitince (seviye kaybı) yayınlanır. Eski adı CarContactObstacle idi (AUDIT CC-7).
        LevelFailed, CarInsideParkingArea, CarOutsideParkingArea, MuteCarSound, UnMuteCarSound,
        //Other
        LevelTimeUpdated, StarCountChanged, ParkingSuccessful,
        ResumeGame
        // ObstacleHit artık tipli olay: ObstacleHitEvent (GameEvents.cs).
    }

    /// <summary>Tipli olay işaretçisi. Olaylar `readonly struct` olmalı (ayırma/GC yok).</summary>
    public interface IGameEvent { }

    // ---- Dinleyici listeleri: aynı dinleyici iki kez eklenmez, yayın sırasında abonelik değişimi güvenlidir,
    // ---- bir dinleyicideki hata diğerlerini engellemez.
    internal sealed class Signal
    {
        private readonly List<Action> _list = new List<Action>();
        private Action[] _snapshot;

        public void Add(Action a) { if (a == null || _list.Contains(a)) return; _list.Add(a); _snapshot = null; }
        public void Remove(Action a) { if (_list.Remove(a)) _snapshot = null; }
        public void Clear() { _list.Clear(); _snapshot = null; }

        public void Invoke()
        {
            if (_list.Count == 0) return;
            var snap = _snapshot ??= _list.ToArray();
            for (int i = 0; i < snap.Length; i++)
            {
                if (!_list.Contains(snap[i])) continue; // yayın sırasında çıkmış
                try { snap[i](); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }
    }

    internal sealed class Signal<T>
    {
        private readonly List<Action<T>> _list = new List<Action<T>>();
        private Action<T>[] _snapshot;

        public void Add(Action<T> a) { if (a == null || _list.Contains(a)) return; _list.Add(a); _snapshot = null; }
        public void Remove(Action<T> a) { if (_list.Remove(a)) _snapshot = null; }
        public void Clear() { _list.Clear(); _snapshot = null; }

        public void Invoke(T arg)
        {
            if (_list.Count == 0) return;
            var snap = _snapshot ??= _list.ToArray();
            for (int i = 0; i < snap.Length; i++)
            {
                if (!_list.Contains(snap[i])) continue;
                try { snap[i](arg); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }
    }

    /// <summary>
    /// Olay otobüsü. İki kullanım:
    ///  • Anahtarlı: EventBus.Subscribe(GameEventType.X, Method) / Publish(GameEventType.X)
    ///  • Tipli:     EventBus.Subscribe&lt;ObstacleHitEvent&gt;(Method) / Publish(new ObstacleHitEvent(...))
    /// Abone olan her bileşen OnDisable'da Unsubscribe etmelidir. Play moduna girerken tüm abonelikler sıfırlanır.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<GameEventType, Signal> Events = new Dictionary<GameEventType, Signal>();
        private static readonly List<Action> Resetters = new List<Action>();

        internal static void RegisterResetter(Action reset) => Resetters.Add(reset);

        // ---- Anahtarlı, parametresiz ----
        public static void Subscribe(GameEventType type, Action listener)
        {
            if (!Events.TryGetValue(type, out var signal)) Events[type] = signal = new Signal();
            signal.Add(listener);
        }

        public static void Unsubscribe(GameEventType type, Action listener)
        {
            if (Events.TryGetValue(type, out var signal)) signal.Remove(listener);
        }

        public static void Clear(GameEventType type)
        {
            if (Events.TryGetValue(type, out var signal)) signal.Clear();
        }

        public static void Publish(GameEventType type)
        {
            if (Events.TryGetValue(type, out var signal)) signal.Invoke();
        }

        // ---- Tipli olaylar ----
        public static void Subscribe<TEvent>(Action<TEvent> listener) where TEvent : struct, IGameEvent
            => Channel<TEvent>.Signal.Add(listener);

        public static void Unsubscribe<TEvent>(Action<TEvent> listener) where TEvent : struct, IGameEvent
            => Channel<TEvent>.Signal.Remove(listener);

        public static void Publish<TEvent>(TEvent evt) where TEvent : struct, IGameEvent
            => Channel<TEvent>.Signal.Invoke(evt);

        private static class Channel<TEvent> where TEvent : struct, IGameEvent
        {
            public static readonly Signal<TEvent> Signal = CreateSignal();

            private static Signal<TEvent> CreateSignal()
            {
                var s = new Signal<TEvent>();
                RegisterResetter(s.Clear);
                return s;
            }
        }

        /// <summary>Tüm abonelikleri siler (Play moduna girerken otomatik çağrılır).</summary>
        public static void ClearAll()
        {
            foreach (var signal in Events.Values) signal.Clear();
            for (int i = 0; i < Resetters.Count; i++) Resetters[i]();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() => ClearAll();
    }

    /// <summary>Tek parametreli anahtarlı olay otobüsü (yük tipi T0). Tipli olaylar için EventBus.Subscribe&lt;TEvent&gt; tercih edilir.</summary>
    public static class EventBus<T0>
    {
        private static readonly Dictionary<GameEventType, Signal<T0>> Events = RegisterNew();

        private static Dictionary<GameEventType, Signal<T0>> RegisterNew()
        {
            var d = new Dictionary<GameEventType, Signal<T0>>();
            EventBus.RegisterResetter(() => { foreach (var s in d.Values) s.Clear(); });
            return d;
        }

        public static void Subscribe(GameEventType type, Action<T0> listener)
        {
            if (!Events.TryGetValue(type, out var signal)) Events[type] = signal = new Signal<T0>();
            signal.Add(listener);
        }

        public static void Unsubscribe(GameEventType type, Action<T0> listener)
        {
            if (Events.TryGetValue(type, out var signal)) signal.Remove(listener);
        }

        public static void Publish(GameEventType type, T0 arg0)
        {
            if (Events.TryGetValue(type, out var signal)) signal.Invoke(arg0);
        }
    }
}
