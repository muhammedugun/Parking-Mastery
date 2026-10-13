using ParkingMastery.Services;
using UnityEngine;

namespace ParkingMastery.Audio
{
    /// <summary>AudioService'i GameServices'e varsayılan olarak kaydeder.</summary>
    internal static class AudioServicesBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Register()
        {
            GameServices.SetDefaultFactories(audio: () => new AudioService());
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void RegisterInEditor() => Register();
#endif
    }
}
