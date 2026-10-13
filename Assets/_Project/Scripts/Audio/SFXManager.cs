using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParkingMastery.Audio
{
    /// <summary>
    /// Bu nesnedeki AudioSource üzerinden tek seferlik ses efekti çalar.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SFXManager : MonoBehaviour
    {
        private AudioSource _audioSource;

        private void Awake()
        {
            if (!TryGetComponent(out _audioSource))
                Debug.LogError("SFXManager: AudioSource bulunamadı.", this);
        }

        /// <summary>Verilen ses klibini tek seferlik çalar; klip ya da AudioSource yoksa sessizce çıkar.</summary>
        public void PlaySFX(AudioClip audioClip)
        {
            if (audioClip == null || _audioSource == null)
                return;

            _audioSource.PlayOneShot(audioClip);
        }
    }
}
