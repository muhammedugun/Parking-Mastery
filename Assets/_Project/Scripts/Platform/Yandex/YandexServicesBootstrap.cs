using ParkingMastery.Services;
using UnityEngine;

namespace ParkingMastery.Yandex
{
    /// <summary>Yandex uygulamalarını GameServices'e varsayılan olarak kaydeder (Services, YG eklentisini bilmez).</summary>
    internal static class YandexServicesBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Register()
        {
            GameServices.SetDefaultFactories(
                ads: () => new CooldownAdService(new YandexAdService()),
                saves: () => new LocalBackupSaveService(new YandexSaveService()),
                leaderboard: () => new YandexLeaderboardService(),
                localization: () => new YandexLocalizationService(),
                platform: () => new YandexPlatformService());
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void RegisterInEditor() => Register();
#endif
    }
}
