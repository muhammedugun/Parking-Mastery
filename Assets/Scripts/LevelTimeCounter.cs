using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTimeCounter : MonoBehaviour
{
    private int time;

    void Start()
    {
        InvokeRepeating(nameof(UpdateTime), 1f, 1f);
    }

    private void UpdateTime()
    {
        time++;
        EventBus<int>.Publish(EventType.LevelTimeUpdated, time);
    }

}
