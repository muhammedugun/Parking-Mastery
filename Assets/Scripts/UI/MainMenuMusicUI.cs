using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuMusicUI : MonoBehaviour
{
    public MusicManager musicManager;

    public void Switch()
    {
        musicManager.Switch();
    }
}
