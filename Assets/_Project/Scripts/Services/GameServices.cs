using System;
using UnityEngine;

namespace ParkingMastery.Services
{
    /// <summary>
    /// Servislere tek erişim noktası. Varsayılanlar Yandex uygulamalarıdır; test/editor için Override ile değiştirilebilir.
    /// Play moduna girerken (domain reload kapalı olsa da) sıfırlanır.
    /// </summary>
    public static class GameServices
    {
        private static IAdService _ads;
        private static ISaveService _saves;
        private static ILeaderboardService _leaderboard;
        private static ILocalizationService _localization;
        private static IPlatformService _platform;
        private static IAudioService _audio;

        // Varsayılan uygulamalar platform katmanı tarafından kaydedilir (ör. ParkingMastery.Yandex) → Services, YG eklentisine bağımlı değildir.
        private static Func<IAdService> _adsFactory;
        private static Func<ISaveService> _savesFactory;
        private static Func<ILeaderboardService> _leaderboardFactory;
        private static Func<ILocalizationService> _localizationFactory;
        private static Func<IPlatformService> _platformFactory;
        private static Func<IAudioService> _audioFactory;

        public static IAdService Ads => _ads ??= Create(_adsFactory, nameof(Ads));
        public static ISaveService Saves => _saves ??= Create(_savesFactory, nameof(Saves));
        public static ILeaderboardService Leaderboard => _leaderboard ??= Create(_leaderboardFactory, nameof(Leaderboard));
        public static ILocalizationService Localization => _localization ??= Create(_localizationFactory, nameof(Localization));
        public static IPlatformService Platform => _platform ??= Create(_platformFactory, nameof(Platform));
        public static IAudioService Audio => _audio ??= Create(_audioFactory, nameof(Audio));

        /// <summary>Platform katmanının varsayılan servis üreticilerini kaydeder (null olan dokunulmaz).</summary>
        public static void SetDefaultFactories(Func<IAdService> ads = null, Func<ISaveService> saves = null,
            Func<ILeaderboardService> leaderboard = null, Func<ILocalizationService> localization = null,
            Func<IPlatformService> platform = null, Func<IAudioService> audio = null)
        {
            if (ads != null) _adsFactory = ads;
            if (saves != null) _savesFactory = saves;
            if (leaderboard != null) _leaderboardFactory = leaderboard;
            if (localization != null) _localizationFactory = localization;
            if (platform != null) _platformFactory = platform;
            if (audio != null) _audioFactory = audio;
        }

        private static T Create<T>(Func<T> factory, string name) where T : class
        {
            if (factory == null)
                throw new InvalidOperationException("GameServices." + name + ": varsayılan uygulama kayıtlı değil. Platform katmanı SetDefaultFactories çağırmalı veya Override kullanılmalı.");
            return factory();
        }

        /// <summary>Verilen servis(ler)i değiştirir; null olan parametre dokunulmaz bırakılır.</summary>
        public static void Override(IAdService ads = null, ISaveService saves = null,
            ILeaderboardService leaderboard = null, ILocalizationService localization = null,
            IPlatformService platform = null, IAudioService audio = null)
        {
            if (ads != null) { Release(_ads); _ads = ads; }
            if (saves != null) { Release(_saves); _saves = saves; }
            if (leaderboard != null) { Release(_leaderboard); _leaderboard = leaderboard; }
            if (localization != null) { Release(_localization); _localization = localization; }
            if (platform != null) { Release(_platform); _platform = platform; }
            if (audio != null) { Release(_audio); _audio = audio; }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            Release(_ads);
            Release(_saves);
            Release(_leaderboard);
            Release(_localization);
            Release(_platform);
            Release(_audio);
            _ads = null;
            _saves = null;
            _leaderboard = null;
            _localization = null;
            _platform = null;
            _audio = null;
        }

        private static void Release(object service)
        {
            (service as IDisposable)?.Dispose();
        }
    }
}
