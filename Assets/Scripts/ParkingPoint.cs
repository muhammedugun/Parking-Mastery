using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingPoint : MonoBehaviour
{
    [HideInInspector] public float _carStayTime;
    [HideInInspector] public bool _carInsideParkingPoint;

    [SerializeField] private GameObject _arrowDown;

    private Collider _collider;
    private MeshRenderer _meshRenderer;
    private Color _defaultColor;

    void Start()
    {
        _collider = GetComponent<Collider>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _defaultColor = _meshRenderer.material.color;

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (CheckCarInsideParkingPoint(other.bounds) && DirectionControl(other.transform))
            {
                _carInsideParkingPoint = true;
                if (_arrowDown.gameObject.activeSelf)
                    _arrowDown.gameObject.SetActive(false);
                ChangeColor(Color.blue);
                _carStayTime += Time.deltaTime;
                if (_carStayTime >= 3f)
                    ChangeColor(Color.green);
            }
            else
            {
                _carInsideParkingPoint = false;
                if (!_arrowDown.gameObject.activeSelf)
                    _arrowDown.gameObject.SetActive(true);
                ChangeColor(_defaultColor);
                _carStayTime = 0f;
            }

        }
    }

    private void ChangeColor(Color color)
    {
        if (_meshRenderer != null && _meshRenderer.material != null && _meshRenderer.material.color != color)
        {
            _meshRenderer.material.color = color;
        }
    }

    /// <summary>
    /// Arabanın park alanının içerisinde olup olmadığını kontrol et
    /// </summary>
    private bool CheckCarInsideParkingPoint(Bounds carColliderBounds)
    {
        return _collider.bounds.Contains(carColliderBounds.min) && _collider.bounds.Contains(carColliderBounds.max);
    }

    private bool DirectionControl(Transform carTransform)
    {
        return Vector3.Dot(transform.forward, carTransform.forward) >= 0;
    }
}
