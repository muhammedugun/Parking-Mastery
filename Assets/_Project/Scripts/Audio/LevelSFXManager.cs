using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkingMastery.Core;

namespace ParkingMastery.Audio
{
    /// <summary>
    /// Seviye içi ses efektlerini (park alanına giriş/çıkış, başarı, kaybetme, engele çarpma) oyun olaylarına abone olarak çalar.
    /// </summary>
    public class LevelSFXManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip[] _sfxs;
        [Tooltip("Bariyer/engel çarpması sesi (isteğe bağlı; boşsa çalmaz). ObstacleHitEvent ile tetiklenir.")]
        [SerializeField] private AudioClip _obstacleHitClip;

        private void OnEnable()
        {
            EventBus.Subscribe(GameEventType.CarInsideParkingArea, PlayCarInsideParkingArea);
            EventBus.Subscribe(GameEventType.CarOutsideParkingArea, PlayCarOutsideParkingArea);
            EventBus.Subscribe(GameEventType.ParkingSuccessful, PlayParkingSuccessful);
            EventBus.Subscribe(GameEventType.LevelFailed, PlayLevelFailed);
            EventBus.Subscribe<ObstacleHitEvent>(PlayObstacleHit);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(GameEventType.CarInsideParkingArea, PlayCarInsideParkingArea);
            EventBus.Unsubscribe(GameEventType.CarOutsideParkingArea, PlayCarOutsideParkingArea);
            EventBus.Unsubscribe(GameEventType.ParkingSuccessful, PlayParkingSuccessful);
            EventBus.Unsubscribe(GameEventType.LevelFailed, PlayLevelFailed);
            EventBus.Unsubscribe<ObstacleHitEvent>(PlayObstacleHit);
        }

        private void PlayObstacleHit(ObstacleHitEvent evt)
        {
            if (_obstacleHitClip != null)
                _audioSource.PlayOneShot(_obstacleHitClip, Mathf.Lerp(0.4f, 1f, evt.Intensity));
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

        private void PlayLevelFailed()
        {
            _audioSource.PlayOneShot(_sfxs[3]);
        }
    }
}
