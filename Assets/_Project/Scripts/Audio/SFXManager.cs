using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParkingMastery.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SFXManager : MonoBehaviour
    {
        private AudioSource _audioSource;

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void PlaySFX(AudioClip audioClip)
        {
            _audioSource.PlayOneShot(audioClip);
        }
    }
}
