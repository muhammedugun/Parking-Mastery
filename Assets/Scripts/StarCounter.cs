using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarCounter : MonoBehaviour
{
    public int _threeStarTime, _twoStarTime;

    private void OnEnable()
    {
        EventBus<int>.Subscribe(EventType.LevelTimeUpdated, ControlStars);
    }

    private void OnDisable()
    {
        EventBus<int>.Unsubscribe(EventType.LevelTimeUpdated, ControlStars);
    }

    private void ControlStars(int time)
    {
        if (time > _twoStarTime)
        {
            EventBus<int>.Publish(EventType.StarCountChanged, 1);
        }
        else if (time > _threeStarTime)
        {
            EventBus<int>.Publish(EventType.StarCountChanged, 2);
        }
    }

}
