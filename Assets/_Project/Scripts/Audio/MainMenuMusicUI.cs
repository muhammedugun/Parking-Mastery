using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParkingMastery.Audio
{
    public class MainMenuMusicUI : MonoBehaviour
    {
        public MusicManager musicManager;

        public void Switch()
        {
            musicManager.Switch();
        }
    }
}
