namespace ParkingMastery.Services
{
    /// <summary>Çalışma ortamı bilgisi (AUDIT CC-6): mobil/masaüstü gibi.</summary>
    public interface IPlatformService
    {
        bool IsMobile { get; }
    }
}
