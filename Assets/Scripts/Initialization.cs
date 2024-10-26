using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initialozation : MonoBehaviour
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
