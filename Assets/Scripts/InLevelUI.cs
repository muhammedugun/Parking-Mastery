using UnityEngine;
using UnityEngine.UI;

public class InLevelUI : MonoBehaviour
{
    [SerializeField] private Image _waitingCircle;

    private ParkingArea _parkingPoint;

    private void Start()
    {
        _parkingPoint = FindFirstObjectByType<ParkingArea>();
    }

    void Update()
    {
        if (_parkingPoint._carInsideParkingPoint)
        {

            _waitingCircle.fillAmount = Mathf.InverseLerp(0, 3f, _parkingPoint._carStayTime);
        }
        else
        {
            _waitingCircle.fillAmount = 0f;
        }
    }
}
