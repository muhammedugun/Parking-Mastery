using UnityEngine;

namespace ParkingMastery.Core
{
    /// <summary>
    /// Sahneler arası seviye bilgisi (B7). SceneLoadManager sayısal sahne adı ("5") alınca CurrentLevelId'yi yazıp
    /// Gameplay sahnesini yükler; LevelLoader buradan okur. LevelCount'u LevelLoader doldurur.
    /// </summary>
    public static class LevelSession
    {
        /// <summary>Oynanan / yüklenecek seviye numarası. Editörde Gameplay'den doğrudan Play'de 1'dir.</summary>
        public static int CurrentLevelId { get; set; } = 1;

        /// <summary>Toplam seviye sayısı (en büyük numara). Henüz yüklenmediyse 0.</summary>
        public static int LevelCount { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            CurrentLevelId = 1;
            LevelCount = 0;
        }
    }
}
