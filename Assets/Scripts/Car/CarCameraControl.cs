using UnityEngine;

public class CarCameraControl : MonoBehaviour
{

    public float smoothSpeed = 0.125f;  // Kameranın yumuşak hareket hızı

    private CarControl _carControl;
    private Transform cameraPositions;
    private int currentCameraIndex;

    private void Start()
    {

        _carControl = FindFirstObjectByType<CarControl>();
        for (int i = 0; i < _carControl.transform.childCount; i++)
        {
            if (_carControl.transform.GetChild(i).name == "CameraPositions")
                cameraPositions = _carControl.transform.GetChild(i);
        }

        if (cameraPositions == null)
            Debug.LogWarning("kamera altında CameraPositions objesi bulunamadı!");


        EventBus.Subscribe(EventType.CarDirectionChanged, RearCameraControl);
    }

    private void FixedUpdate()
    {
        Transform targetTransform = cameraPositions.GetChild(currentCameraIndex);

        // Kameranın pozisyonunu yumuşak geçişle belirle
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetTransform.position, smoothSpeed);

        // Rotasyonu lerp ile ayarla
        transform.rotation = Quaternion.Lerp(transform.rotation, targetTransform.rotation, smoothSpeed);

        // Kamerayı yumuşak pozisyona taşı
        transform.position = smoothedPosition;
    }

    public void SwitchCamera()
    {
        if (currentCameraIndex < cameraPositions.childCount - 2)
        {
            currentCameraIndex++;
        }
        else if (currentCameraIndex == cameraPositions.childCount - 1)
        {
            currentCameraIndex = 1;
        }
        else
        {
            currentCameraIndex = 0;
            RearCameraControl();
        }
    }
    public void SwitchCamera(int index)
    {
        currentCameraIndex = index;
    }

    /// <summary>
    /// Arka kamera kontrolü
    /// </summary>
    public void RearCameraControl()
    {
        if (_carControl.isGoingForward && currentCameraIndex == cameraPositions.childCount - 1)
            SwitchCamera(0);
        else if (!_carControl.isGoingForward && currentCameraIndex == 0)
            SwitchCamera(cameraPositions.childCount - 1);
    }
}
