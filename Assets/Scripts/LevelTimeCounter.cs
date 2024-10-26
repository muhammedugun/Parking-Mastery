using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YG;

public class LevelTimeCounter : MonoBehaviour
{
    [HideInInspector] public int time;

    private void OnEnable()
    {
        EventBus.Subscribe(EventType.ParkingSuccessful, StopCounting);
        EventBus.Subscribe(EventType.CarContactObstacle, StopCounting);
        YandexGame.RewardVideoEvent += ResumeCounting;
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(EventType.ParkingSuccessful, StopCounting);
        EventBus.Unsubscribe(EventType.CarContactObstacle, StopCounting);
        YandexGame.RewardVideoEvent -= ResumeCounting;
    }

    void Start()
    {
        InvokeRepeating(nameof(UpdateTime), 1f, 1f);
    }

    private void UpdateTime()
    {
        time++;
        EventBus<int>.Publish(EventType.LevelTimeUpdated, time);
    }

    private void StopCounting()
    {
        CancelInvoke();
    }
    private void ResumeCounting(int id)
    {
        InvokeRepeating(nameof(UpdateTime), 1f, 1f);
    }

}
