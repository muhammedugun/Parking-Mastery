using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParkingMastery.Core
{
    /// <summary>
    /// Oyun açılışında bir kez uygulanan genel ayarlar (dikey senkronizasyon kapalı, hedef kare hızı 60).
    /// </summary>
    public class Initialization : MonoBehaviour
    {
        private static bool _isSettingsSet;
        void Start()
        {
            if (!_isSettingsSet)
            {
                _isSettingsSet = true;
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
            }

        }

    }
}
