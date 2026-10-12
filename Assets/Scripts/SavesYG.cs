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

        private static void EnsureSize<T>(List<T> list, int index)
        {
            while (list.Count <= index)
            {
                list.Add(default);
            }
        }
    }
}
