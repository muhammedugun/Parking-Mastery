namespace ParkingMastery.Services
{
    /// <summary>Lider tablosu servisi (AUDIT CC-6). Oyun kodu platform SDK'sını bilmez.</summary>
    public interface ILeaderboardService
    {
        /// <summary>Skoru verilen tabloya gönderir. SDK hazır değilse sessizce yok sayılır.</summary>
        void SubmitScore(string boardName, int score);
    }
}
