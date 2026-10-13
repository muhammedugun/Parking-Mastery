using YG;
using ParkingMastery.Services;

namespace ParkingMastery.Yandex
{
    /// <summary>IPlatformService'in YG2.envir uygulaması.</summary>
    public sealed class YandexPlatformService : IPlatformService
    {
        public bool IsMobile => YG2.envir.isMobile;
    }
}
