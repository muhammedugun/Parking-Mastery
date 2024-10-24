using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSoundControl : MonoBehaviour
{
    [SerializeField] private AudioSource _carEngineSound;

    private Rigidbody _carRigidbody;
    private float _initialCarEngineSoundPitch;

    private void Start()
    {
        _carRigidbody = GetComponent<Rigidbody>();
        _initialCarEngineSoundPitch = _carEngineSound.pitch;
    }
    private void FixedUpdate()
    {
        if (_carEngineSound != null)
        {
            float engineSoundPitch = _initialCarEngineSoundPitch + (Mathf.Abs(_carRigidbody.velocity.magnitude) / 25f);
            _carEngineSound.pitch = engineSoundPitch;
        }
    }
}
