namespace ParkingMastery.Core
{
    /// <summary>Bariyer/engel çarpması sayıldı. RemainingHits: kalan hak (0 ise son hak bitti → ardından LevelFailed gelir).</summary>
    public readonly struct ObstacleHitEvent : IGameEvent
    {
        public readonly int RemainingHits;
        public ObstacleHitEvent(int remainingHits) { RemainingHits = remainingHits; }
    }

    /// <summary>Oyun durumu değişti (GameStateMachine yayınlar).</summary>
    public readonly struct GameStateChanged : IGameEvent
    {
        public readonly GameState Previous;
        public readonly GameState Current;
        public GameStateChanged(GameState previous, GameState current) { Previous = previous; Current = current; }
    }
}
