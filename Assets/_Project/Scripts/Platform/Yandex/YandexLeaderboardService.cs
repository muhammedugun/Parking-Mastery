using YG;
using ParkingMastery.Services;

namespace ParkingMastery.Yandex
{
    /// <summary>ILeaderboardService'in YG2 uygulaması. Yandex'e bağlı yerlerden biridir.</summary>
    public sealed class YandexLeaderboardService : ILeaderboardService
    {
        public void SubmitScore(string boardName, int score)
        {
            if (YG2.isSDKEnabled) YG2.SetLeaderboard(boardName, score);
        }
    }
}
