using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSFXManager : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _sfxs;

    private void OnEnable()
    {
        EventBus.Subscribe(EventType.CarInsideParkingArea, PlayCarInsideParkingArea);
        EventBus.Subscribe(EventType.CarOutsideParkingArea, PlayCarOutsideParkingArea);
        EventBus.Subscribe(EventType.ParkingSuccessful, PlayParkingSuccessful);
        EventBus.Subscribe(EventType.CarContactObstacle, PlayCarContactObstacle);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(EventType.CarInsideParkingArea, PlayCarInsideParkingArea);
        EventBus.Unsubscribe(EventType.CarOutsideParkingArea, PlayCarOutsideParkingArea);
        EventBus.Unsubscribe(EventType.ParkingSuccessful, PlayParkingSuccessful);
        EventBus.Unsubscribe(EventType.CarContactObstacle, PlayCarContactObstacle);
    }

    private void PlayCarInsideParkingArea()
    {
        _audioSource.PlayOneShot(_sfxs[0]);
    }

    private void PlayCarOutsideParkingArea()
    {
        _audioSource.PlayOneShot(_sfxs[1]);
    }

    private void PlayParkingSuccessful()
    {
        _audioSource.PlayOneShot(_sfxs[2]);
    }

    private void PlayCarContactObstacle()
    {
        _audioSource.PlayOneShot(_sfxs[3]);
    }
}
