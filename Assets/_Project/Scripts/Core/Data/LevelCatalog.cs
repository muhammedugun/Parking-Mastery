using System.Collections.Generic;
using UnityEngine;

namespace ParkingMastery.Core
{
    /// <summary>Tüm seviyelerin listesi (B7). LevelLoader seviyeyi buradan numarasına göre bulur.</summary>
    [CreateAssetMenu(menuName = "Parking-Mastery/Level Catalog", fileName = "LevelCatalog")]
    public class LevelCatalog : ScriptableObject
    {
        [SerializeField] private List<LevelData> levels = new List<LevelData>();

        public int Count => levels.Count;

        /// <summary>En büyük seviye numarası (seviyeler 1'den ardışık varsayılır).</summary>
        public int MaxLevelId
        {
            get
            {
                int max = 0;
                for (int i = 0; i < levels.Count; i++)
                    if (levels[i] != null && levels[i].LevelId > max) max = levels[i].LevelId;
                return max;
            }
        }

        public bool TryGet(int levelId, out LevelData data)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i] != null && levels[i].LevelId == levelId) { data = levels[i]; return true; }
            }
            data = null;
            return false;
        }

        /// <summary>Boş giriş, yinelenen numara, 1..Max arasında boşluk ve her LevelData'nın kendi denetimini yapar.</summary>
        public bool Validate(List<string> problems)
        {
            int before = problems.Count;
            var seen = new HashSet<int>();
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i] == null) { problems.Add("LevelCatalog: #" + i + " boş."); continue; }
                if (!seen.Add(levels[i].LevelId)) problems.Add("LevelCatalog: yinelenen levelId " + levels[i].LevelId + " ('" + levels[i].name + "').");
                levels[i].Validate(problems);
            }
            int max = MaxLevelId;
            for (int id = 1; id <= max; id++)
                if (!seen.Contains(id)) problems.Add("LevelCatalog: levelId " + id + " eksik.");
            return problems.Count == before;
        }
    }
}
