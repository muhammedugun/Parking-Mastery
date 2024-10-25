using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPopupManager : MonoBehaviour
{
    [SerializeField] private GameObject _winPopup, _losePopup, _pausePopup;
    private void OnEnable()
    {
        EventBus.Subscribe(EventType.ParkingSuccessful, OpenWinPopup);
        EventBus.Subscribe(EventType.CarContactObstacle, OpenLosePopup);
    }
    private void OnDisable()
    {
        EventBus.Unsubscribe(EventType.ParkingSuccessful, OpenWinPopup);
        EventBus.Unsubscribe(EventType.CarContactObstacle, OpenLosePopup);
    }

    public void OpenPopup(GameObject popup)
    {
        popup.SetActive(true);
        EventBus.Publish(EventType.AnyPopupOpened);
    }

    public void ClosePopup(GameObject popup)
    {
        popup.SetActive(false);
        EventBus.Publish(EventType.AnyPopupClosed);
    }

    private void OpenWinPopup()
    {
        OpenPopup(_winPopup);
    }

    private void OpenLosePopup()
    {
        OpenPopup(_losePopup);
    }

    public void OpenPausePopup()
    {
        OpenPopup(_pausePopup);
        GameLoopManager.PauseGame();
    }

    public void ClosePausePopup()
    {
        ClosePopup(_pausePopup);
        GameLoopManager.ResumeGame();
    }

    public void CloseAllPopupLevel()
    {
        ClosePausePopup();
        ClosePopup(_losePopup);
        ClosePopup(_winPopup);
    }

}
