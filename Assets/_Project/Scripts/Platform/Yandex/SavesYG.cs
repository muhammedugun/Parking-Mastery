using System.Collections.Generic;

namespace YG
{
    /// <summary>
    /// Parking Mastery oyununa ait kayıt verileri. PluginYG2'nin SavesYG partial sınıfını genişletir.
    /// Veriler YG2.saves üzerinden okunur/yazılır, kalıcı olması için YG2.SaveProgress() çağrılmalıdır.
    /// </summary>
    public partial class SavesYG
    {
        /// <summary>
        /// Liste boyutu: indeks 0 kullanılmaz, 1-30 arası seviyeler, bir adet de sonraki seviye için yedek.
        /// Seviye sayısı artarsa yardımcı metotlar listeyi otomatik genişletir.
        /// </summary>
        private const int DefaultLevelSlotCount = 32;

        /// <summary>Seviye numarasına göre kilit durumu. İlk seviye varsayılan olarak açıktır.</summary>
        public List<bool> unlockLevels = CreateDefaultUnlocks();

        /// <summary>Seviye numarasına göre kazanılan yıldız sayısı.</summary>
        public List<int> levelsStarCount = new List<int>(new int[DefaultLevelSlotCount]);

        /// <summary>Müzik açık mı? Varsayılan olarak açık.</summary>
        public bool isMusicOn = true;

        /// <summary>Motor sesi açık mı? Varsayılan olarak açık.</summary>
        public bool isEngineSoundOn = true;

        /// <summary>Güncel kayıt şeması sürümü. Şema değişince artır ve <see cref="MigrateIfNeeded"/>'e adım ekle.</summary>
        public const int CurrentSaveVersion = 1;

        /// <summary>Kayıt şeması sürümü. 0 = bu alan olmadan yazılmış eski kayıt (mevcut oyuncular).</summary>
        public int saveVersion;

        /// <summary>Seviye numarasına göre en iyi süre (sn); 0 = henüz yok.</summary>
        public List<int> levelsBestTime = new List<int>(new int[DefaultLevelSlotCount]);

        private static List<bool> CreateDefaultUnlocks()
        {
            var list = new List<bool>(new bool[DefaultLevelSlotCount]);
            list[1] = true;
            return list;
        }

        public bool IsLevelUnlocked(int level)
        {
            return level >= 0 && level < unlockLevels.Count && unlockLevels[level];
        }

        public void UnlockLevel(int level)
        {
            EnsureSize(unlockLevels, level);
            unlockLevels[level] = true;
        }

        public int GetLevelStars(int level)
        {
            return level >= 0 && level < levelsStarCount.Count ? levelsStarCount[level] : 0;
        }

        public void SetLevelStars(int level, int stars)
        {
            EnsureSize(levelsStarCount, level);
            levelsStarCount[level] = stars;
        }

        public int GetTotalStars()
        {
            int total = 0;
            foreach (var stars in levelsStarCount)
            {
                total += stars;
            }
            return total;
        }

        public int GetBestTime(int level)
        {
            return level >= 0 && level < levelsBestTime.Count ? levelsBestTime[level] : 0;
        }

        /// <summary>Süre daha kısaysa (veya hiç yoksa) en iyi süreyi günceller.</summary>
        public bool TrySetBestTime(int level, int seconds)
        {
            if (level < 0 || seconds <= 0) return false;
            EnsureSize(levelsBestTime, level);
            int current = levelsBestTime[level];
            if (current > 0 && seconds >= current) return false;
            levelsBestTime[level] = seconds;
            return true;
        }

        /// <summary>
        /// Eski kaydı güncel şemaya taşır; hiçbir ilerlemeyi silmez. Listeleri tamamlar/onarır, yıldızı olan seviyeleri açık sayar.
        /// Kayıt değiştiyse true döner.
        /// </summary>
        public bool MigrateIfNeeded()
        {
            if (saveVersion >= CurrentSaveVersion) return false;

            unlockLevels ??= CreateDefaultUnlocks();
            levelsStarCount ??= new List<int>(new int[DefaultLevelSlotCount]);
            levelsBestTime ??= new List<int>(new int[DefaultLevelSlotCount]);

            EnsureSize(unlockLevels, DefaultLevelSlotCount - 1);
            EnsureSize(levelsStarCount, DefaultLevelSlotCount - 1);
            EnsureSize(levelsBestTime, DefaultLevelSlotCount - 1);

            // v0 → v1: ilk seviye hep açık; yıldız 0–3 aralığına sıkıştırılır; yıldızı olan seviye ve sonraki seviye açık sayılır;
            // geçersiz (negatif) süreler temizlenir.
            unlockLevels[1] = true;
            for (int level = 0; level < levelsStarCount.Count; level++)
            {
                levelsStarCount[level] = System.Math.Max(0, System.Math.Min(3, levelsStarCount[level]));
                if (level > 0 && levelsStarCount[level] > 0)
                {
                    UnlockLevel(level);
                    UnlockLevel(level + 1);
                }
            }
            for (int level = 0; level < levelsBestTime.Count; level++)
            {
                if (levelsBestTime[level] < 0) levelsBestTime[level] = 0;
            }

            saveVersion = CurrentSaveVersion;
            return true;
        }

        private static void EnsureSize<T>(List<T> list, int index)
        {
            while (list.Count <= index)
            {
                list.Add(default);
            }
        }
    }
}
