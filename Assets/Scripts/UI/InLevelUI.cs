using UnityEngine;
using UnityEngine.UI;
using YG;

public class InLevelUI : MonoBehaviour
{
    [SerializeField] private Image _waitingCircle;
    [SerializeField] private GameObject _mobileCarController;

    private ParkingArea _parkingPoint;
    private CarCameraControl _carCameraControl;

    private void Start()
    {
        _parkingPoint = FindFirstObjectByType<ParkingArea>();
        _carCameraControl = FindFirstObjectByType<CarCameraControl>();


        if (YandexGame.EnvironmentData.isMobile)
        {
            _mobileCarController.SetActive(true);
        }
    }

    void Update()
    {
        if (_parkingPoint._carInsideParkingPoint)
        {

            _waitingCircle.fillAmount = Mathf.InverseLerp(0, 2f, _parkingPoint._carStayTime);
            if (_parkingPoint._carStayTime >= 2f)
            {
                _waitingCircle.color = Color.green;
            }
        }
        else
        {
            _waitingCircle.fillAmount = 0f;
        }
    }

    public void HomeButton()
    {
        SceneLoadManager.LoadSceneStatic("MainMenu");
    }

    public void RestartButton()
    {
        GameLoopManager.RestartLevel();
        GameLoopManager.ResumeGame();
    }

    public void NextLevelButton()
    {
        GameLoopManager.LoadNextLevel();
    }

    public void ResumeButton()
    {
        GameLoopManager.ResumeGame();
    }

    public void SwitchCamera()
    {
        _carCameraControl.SwitchCamera();
    }
}
