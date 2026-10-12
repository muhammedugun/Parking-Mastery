using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingArea : MonoBehaviour
{
    [HideInInspector] public float _carStayTime;
    [HideInInspector] public bool _carInsideParkingPoint;

    [SerializeField] private GameObject _arrowDown;

    [Tooltip("Araç kutusunun park alanı sınırından taşabileceği pay (dünya birimi). Alan aracın boyutuna çok yakınsa artır.")]
    [SerializeField, Min(0f)] private float _fitTolerance = 0.1f;

    private Collider _collider;
    private MeshRenderer _meshRenderer;
    private Color _defaultColor;
    private bool _isParkingSuccessful;

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
            if (!_isParkingSuccessful)
            {
                if (CheckCarInsideParkingPoint(other) && DirectionControl(other.transform))
                {
                    if (!_carInsideParkingPoint)
                    {
                        _carInsideParkingPoint = true;
                        EventBus.Publish(EventType.CarInsideParkingArea);
                    }

                    if (_arrowDown.gameObject.activeSelf)
                        _arrowDown.gameObject.SetActive(false);
                    ChangeColor(Color.blue);
                    _carStayTime += Time.deltaTime;
                    if (_carStayTime >= 2f && !_isParkingSuccessful)
                    {
                        _isParkingSuccessful = true;
                        ChangeColor(Color.green);
                        EventBus.Publish(EventType.ParkingSuccessful);
                    }

                }
                else
                {
                    if (_carInsideParkingPoint)
                    {
                        _carInsideParkingPoint = false;
                        EventBus.Publish(EventType.CarOutsideParkingArea);
                    }
                    _carInsideParkingPoint = false;
                    if (!_arrowDown.gameObject.activeSelf)
                        _arrowDown.gameObject.SetActive(true);
                    ChangeColor(_defaultColor);
                    _carStayTime = 0f;
                }
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
    private bool CheckCarInsideParkingPoint(Collider carCollider)
    {
        var areaBox = _collider as BoxCollider;
        if (areaBox == null || !TryGetLocalBox(carCollider, out Vector3 carCenter, out Vector3 carExtents))
        {
            // Kutu olmayan collider'larda eski yönteme dön
            Bounds carBounds = carCollider.bounds;
            return _collider.bounds.Contains(carBounds.min) && _collider.bounds.Contains(carBounds.max);
        }

        // Park alanının ve araç kutusunun dönüşüne göre kontrol: araç eğik olsa bile sınır kutusu şişmez.
        // Araç kutusunun 8 köşesi park alanının yerel eksenlerinde karşılaştırılır. Dikey eksen yok sayılır.
        int upAxis = GetUpAxis();
        Vector3 areaHalfSize = areaBox.size * 0.5f;
        Vector3 areaScale = transform.lossyScale;
        Transform carTransform = carCollider.transform;

        for (int i = 0; i < 8; i++)
        {
            Vector3 corner = carCenter + new Vector3(
                (i & 1) == 0 ? -carExtents.x : carExtents.x,
                (i & 2) == 0 ? -carExtents.y : carExtents.y,
                (i & 4) == 0 ? -carExtents.z : carExtents.z);

            Vector3 local = transform.InverseTransformPoint(carTransform.TransformPoint(corner)) - areaBox.center;

            for (int axis = 0; axis < 3; axis++)
            {
                if (axis == upAxis)
                    continue;

                float margin = _fitTolerance / Mathf.Max(Mathf.Abs(areaScale[axis]), 0.0001f);
                if (Mathf.Abs(local[axis]) > areaHalfSize[axis] + margin)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Park alanının yerel eksenlerinden hangisinin dünya yukarısına en yakın olduğunu bulur.
    /// </summary>
    private int GetUpAxis()
    {
        float dotX = Mathf.Abs(Vector3.Dot(transform.right, Vector3.up));
        float dotY = Mathf.Abs(Vector3.Dot(transform.up, Vector3.up));
        float dotZ = Mathf.Abs(Vector3.Dot(transform.forward, Vector3.up));

        if (dotY >= dotX && dotY >= dotZ)
            return 1;
        return dotX >= dotZ ? 0 : 2;
    }

    /// <summary>
    /// Collider'ın kendi yerel uzayındaki kutusunu (merkez ve yarı boyutlar) döndürür.
    /// </summary>
    private static bool TryGetLocalBox(Collider collider, out Vector3 center, out Vector3 extents)
    {
        if (collider is BoxCollider box)
        {
            center = box.center;
            extents = box.size * 0.5f;
            return true;
        }

        if (collider is MeshCollider meshCollider && meshCollider.sharedMesh != null)
        {
            center = meshCollider.sharedMesh.bounds.center;
            extents = meshCollider.sharedMesh.bounds.extents;
            return true;
        }

        if (collider is CapsuleCollider capsule)
        {
            center = capsule.center;
            extents = new Vector3(capsule.radius, capsule.radius, capsule.radius);
            extents[capsule.direction] = Mathf.Max(capsule.height * 0.5f, capsule.radius);
            return true;
        }

        if (collider is SphereCollider sphere)
        {
            center = sphere.center;
            extents = Vector3.one * sphere.radius;
            return true;
        }

        center = Vector3.zero;
        extents = Vector3.zero;
        return false;
    }

    private bool DirectionControl(Transform carTransform)
    {
        return Vector3.Dot(transform.forward, carTransform.forward) >= 0;
    }
}
