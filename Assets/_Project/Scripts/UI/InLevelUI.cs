using UnityEngine;
using UnityEngine.UI;
using ParkingMastery.Services;
using ParkingMastery.Car;
using ParkingMastery.Core;
using ParkingMastery.Gameplay;

namespace ParkingMastery.UI
{
    public class InLevelUI : MonoBehaviour
    {
        [SerializeField] private Image _waitingCircle;
        [SerializeField] private GameObject _mobileCarController;

        [SerializeField] private ParkingZone _parkingZone;
        private CarCameraControl _carCameraControl;

        private void Start()
        {
            _carCameraControl = FindFirstObjectByType<CarCameraControl>();


            if (GameServices.Platform.IsMobile)
            {
                _mobileCarController.SetActive(true);
            }
        }

        void Update()
        {
            if (_parkingZone != null && _parkingZone.IsCarInside)
            {
                _waitingCircle.fillAmount = _parkingZone.StayProgress;
                if (_parkingZone.StayProgress >= 1f)
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
            LevelController.RestartLevel();
            LevelController.ResumeGame();
        }

        public void NextLevelButton()
        {
            LevelController.LoadNextLevel();
        }

        public void ResumeButton()
        {
            LevelController.ResumeGame();
        }

        public void SwitchCamera()
        {
            _carCameraControl.SwitchCamera();
        }
    }
}
