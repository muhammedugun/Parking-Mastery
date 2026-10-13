using System;
using System.Collections.Generic;
using UnityEngine;

namespace ParkingMastery.Services
{
    /// <summary>
    /// ISaveService sarmalayıcısı: yazılan her değeri PlayerPrefs'e (JSON) yedekler. Bulut kaydı hazır değilken
    /// (WebGL'de SDK gecikmesi / çevrimdışı) okumalar yedekten gelir; kayıt yüklenince ilerleme birleştirilir
    /// (açık seviyeler birleşimi, seviye başına en yüksek yıldız) ve buluta geri yazılır. Ayarlar (müzik/motor sesi)
    /// bulut hazırsa buluttan gelir. Yandex'e bağlı değildir.
    /// </summary>
    public sealed class LocalBackupSaveService : ISaveService, IDisposable
    {
        private const string Key = "PM_SaveBackup_v1";
        private const int MaxLevels = 64;

        [Serializable]
        private class Snapshot
        {
            public bool music = true;
            public bool engine = true;
            public List<bool> unlocked = new List<bool>();
            public List<int> stars = new List<int>();
            // Eski (v1) yedeklerde yoktur; JsonUtility eksik alanda varsayılanı korur.
            public List<int> bestTimes = new List<int>();
        }

        private readonly ISaveService _inner;
        private Snapshot _local;

        public event Action Loaded;

        public LocalBackupSaveService(ISaveService inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _local = ReadLocal();
            _inner.Loaded += OnInnerLoaded;
            if (_inner.IsReady) Merge();
        }

        public void Dispose()
        {
            _inner.Loaded -= OnInnerLoaded;
            (_inner as IDisposable)?.Dispose();
        }

        /// <summary>Bulut kaydı hazır mı? (Yedek her zaman vardır; GameBootstrap bulutu bekler.)</summary>
        public bool IsReady => _inner.IsReady;

        public bool IsMusicOn
        {
            get => _inner.IsReady ? _inner.IsMusicOn : _local.music;
            set { _local.music = value; WriteLocal(); if (_inner.IsReady) _inner.IsMusicOn = value; }
        }

        public bool IsEngineSoundOn
        {
            get => _inner.IsReady ? _inner.IsEngineSoundOn : _local.engine;
            set { _local.engine = value; WriteLocal(); if (_inner.IsReady) _inner.IsEngineSoundOn = value; }
        }

        public bool IsLevelUnlocked(int level)
        {
            if (_inner.IsReady) return _inner.IsLevelUnlocked(level);
            return level == 1 || LocalUnlocked(level);
        }

        public void UnlockLevel(int level)
        {
            if (level < 0 || level >= MaxLevels) return;
            Ensure(_local.unlocked, level);
            _local.unlocked[level] = true;
            WriteLocal();
            if (_inner.IsReady) _inner.UnlockLevel(level);
        }

        public int GetLevelStars(int level)
        {
            return _inner.IsReady ? _inner.GetLevelStars(level) : LocalStars(level);
        }

        public void SetLevelStars(int level, int stars)
        {
            if (level < 0 || level >= MaxLevels) return;
            Ensure(_local.stars, level);
            _local.stars[level] = stars;
            WriteLocal();
            if (_inner.IsReady) _inner.SetLevelStars(level, stars);
        }

        public int GetTotalStars()
        {
            if (_inner.IsReady) return _inner.GetTotalStars();
            int total = 0;
            foreach (var s in _local.stars) total += s;
            return total;
        }

        public int SaveVersion => _inner.SaveVersion;

        public int GetBestTime(int level)
        {
            return _inner.IsReady ? _inner.GetBestTime(level) : LocalBest(level);
        }

        public bool TrySetBestTime(int level, int seconds)
        {
            if (level < 0 || level >= MaxLevels || seconds <= 0) return false;
            Ensure(_local.bestTimes, level);
            int current = _local.bestTimes[level];
            bool improvedLocal = current <= 0 || seconds < current;
            if (improvedLocal)
            {
                _local.bestTimes[level] = seconds;
                WriteLocal();
            }
            bool improvedInner = _inner.IsReady && _inner.TrySetBestTime(level, seconds);
            return improvedLocal || improvedInner;
        }

        public void Save()
        {
            _inner.Save();
            WriteLocal();
            PlayerPrefs.Save();
        }

        private void OnInnerLoaded()
        {
            Merge();
            Loaded?.Invoke();
        }

        /// <summary>Yerel yedek ile bulutu birleştirir; sonra yedeği buluttaki son duruma eşitler.</summary>
        private void Merge()
        {
            bool changed = false;
            for (int level = 1; level < MaxLevels; level++)
            {
                if (LocalUnlocked(level) && !_inner.IsLevelUnlocked(level)) { _inner.UnlockLevel(level); changed = true; }
                int localStars = LocalStars(level);
                if (localStars > _inner.GetLevelStars(level)) { _inner.SetLevelStars(level, localStars); changed = true; }
                int localBest = LocalBest(level);
                if (localBest > 0 && _inner.TrySetBestTime(level, localBest)) changed = true;
            }

            var snap = new Snapshot { music = _inner.IsMusicOn, engine = _inner.IsEngineSoundOn };
            for (int level = 0; level < MaxLevels; level++)
            {
                snap.unlocked.Add(_inner.IsLevelUnlocked(level));
                snap.stars.Add(_inner.GetLevelStars(level));
                snap.bestTimes.Add(_inner.GetBestTime(level));
            }
            _local = snap;
            WriteLocal();

            if (changed) _inner.Save();
        }

        private bool LocalUnlocked(int level) => level >= 0 && level < _local.unlocked.Count && _local.unlocked[level];
        private int LocalStars(int level) => level >= 0 && level < _local.stars.Count ? _local.stars[level] : 0;
        private int LocalBest(int level) => level >= 0 && level < _local.bestTimes.Count ? _local.bestTimes[level] : 0;

        private static void Ensure<T>(List<T> list, int index)
        {
            while (list.Count <= index) list.Add(default);
        }

        private static Snapshot ReadLocal()
        {
            try
            {
                var json = PlayerPrefs.GetString(Key, null);
                if (!string.IsNullOrEmpty(json))
                {
                    var s = JsonUtility.FromJson<Snapshot>(json);
                    if (s != null) { s.unlocked ??= new List<bool>(); s.stars ??= new List<int>(); s.bestTimes ??= new List<int>(); return s; }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[LocalBackupSaveService] Yedek okunamadı, sıfırdan başlanıyor: " + e.Message);
            }
            return new Snapshot();
        }

        private void WriteLocal()
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(_local));
        }
    }
}
