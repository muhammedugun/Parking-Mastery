using ParkingMastery.Core;
using ParkingMastery.Services;
using UnityEngine;

namespace ParkingMastery.Gameplay
{
    /// <summary>
    /// Seviye puanlaması (C3, eski LevelTimeCounter + StarCounter): geçen süreyi sayar, süreye göre yıldızı hesaplar.
    /// Her saniye <c>LevelTimeUpdated</c>, yıldız kaybedildikçe <c>StarCountChanged</c> (kaybedilen yıldızın 0 tabanlı sırası) yayınlar.
    /// Yıldız eşikleri <see cref="LevelData"/>'dan gelir (<see cref="Configure"/>); park başarılı/seviye başarısız olunca sayım durur, ödüllü reklamdan sonra devam eder.
    /// </summary>
    public class LevelScoring : MonoBehaviour
    {
        // Eşikler LevelLoader tarafından LevelData'dan yazılır; LevelData'sız (eski) sahneler için yedek ve editör araçları içindir.
        [SerializeField, Min(1)] private int _threeStarTime = 45;
        [SerializeField, Min(1)] private int _twoStarTime = 75;

        private LevelData _data;
        private float _fraction;
        private int _seconds;
        private int _stars = 3;
        private bool _running = true;

        /// <summary>Geçen tam saniye (sayaç duruyorsa son değer).</summary>
        public int ElapsedSeconds => _seconds;

        /// <summary>Şu anki yıldız sayısı (3 → 1; süre aştıkça düşer).</summary>
        public int CurrentStars => _stars;

        public int ThreeStarTime => _threeStarTime;
        public int TwoStarTime => _twoStarTime;

        /// <summary>LevelLoader: seviye verisini bağlar, süreyi ve yıldızları sıfırlar.</summary>
        public void Configure(LevelData data)
        {
            _data = data;
            if (data != null) SetThresholds(data.ThreeStarTime, data.TwoStarTime);
            _fraction = 0f;
            _seconds = 0;
            _stars = 3;
            _running = true;
        }

        /// <summary>Eşikleri doğrudan ayarlar (LevelData'sız eski sahneler / editör araçları).</summary>
        public void SetThresholds(int threeStarTime, int twoStarTime)
        {
            _threeStarTime = Mathf.Max(1, threeStarTime);
            _twoStarTime = Mathf.Max(1, twoStarTime);
        }

        /// <summary>Geçen süreye göre ödül yıldızı (1–3). Kural LevelData.CalculateStars ile aynıdır.</summary>
        public int CalculateStars()
        {
            if (_data != null) return _data.CalculateStars(_seconds);
            if (_seconds > _twoStarTime) return 1;
            return _seconds > _threeStarTime ? 2 : 3;
        }

        private void OnEnable()
        {
            EventBus.Subscribe(GameEventType.ParkingSuccessful, StopCounting);
            EventBus.Subscribe(GameEventType.LevelFailed, StopCounting);
            GameServices.Ads.RewardGranted += ResumeCounting;
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(GameEventType.ParkingSuccessful, StopCounting);
            EventBus.Unsubscribe(GameEventType.LevelFailed, StopCounting);
            GameServices.Ads.RewardGranted -= ResumeCounting;
        }

        private void Update()
        {
            if (!_running) return;

            _fraction += Time.deltaTime;
            while (_fraction >= 1f)
            {
                _fraction -= 1f;
                _seconds++;
                EventBus<int>.Publish(GameEventType.LevelTimeUpdated, _seconds);
                UpdateStars();
            }
        }

        private void UpdateStars()
        {
            int now = CalculateStars();
            while (_stars > now)
            {
                _stars--;
                EventBus<int>.Publish(GameEventType.StarCountChanged, _stars);
            }
        }

        private void StopCounting() { _running = false; }

        private void ResumeCounting(string id) { if (id == AdRewardIds.Resume) _running = true; }
    }
}
