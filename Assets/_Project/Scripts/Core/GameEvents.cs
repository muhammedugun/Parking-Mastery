namespace ParkingMastery.Core
{
    /// <summary>Bariyer/engel çarpması sayıldı. RemainingHits: kalan hak (0 ise son hak bitti → ardından LevelFailed gelir).</summary>
    public readonly struct ObstacleHitEvent : IGameEvent
    {
        public readonly int RemainingHits;
        /// <summary>Çarpma şiddeti 0..1 (çarpma hızına göre; hafif temas ≥ 0,25). Ses, kamera sarsıntısı ve ekran uyarısı bunu ölçekler.</summary>
        public readonly float Intensity;
        public ObstacleHitEvent(int remainingHits, float intensity = 1f) { RemainingHits = remainingHits; Intensity = intensity; }
    }

    /// <summary>Oyun durumu değişti (GameStateMachine yayınlar).</summary>
    public readonly struct GameStateChanged : IGameEvent
    {
        public readonly GameState Previous;
        public readonly GameState Current;
        public GameStateChanged(GameState previous, GameState current) { Previous = previous; Current = current; }
    }
}
