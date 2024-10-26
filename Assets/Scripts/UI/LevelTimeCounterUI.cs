using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelTimeCounterUI : MonoBehaviour
{
    [SerializeField] private Text _timeCounterText;

    private void OnEnable()
    {
        EventBus<int>.Subscribe(EventType.LevelTimeUpdated, UpdateText);
    }

    private void OnDisable()
    {
        EventBus<int>.Unsubscribe(EventType.LevelTimeUpdated, UpdateText);
    }

    private void UpdateText(int time)
    {
        _timeCounterText.text = time.ToString();
    }

}
