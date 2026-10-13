using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParkingMastery.Audio
{
    /// <summary>
    /// Ana menüdeki müzik aç/kapa düğmesini MusicManager'a bağlar.
    /// </summary>
    public class MainMenuMusicUI : MonoBehaviour
    {
        public void Switch()
        {
            if (MusicManager.Instance != null)
                MusicManager.Instance.Switch();
        }
    }
}
