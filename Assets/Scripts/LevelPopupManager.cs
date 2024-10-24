using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPopupManager : MonoBehaviour
{
    [SerializeField] private GameObject _winPopup, _losePopup;
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
    }

    public void ClosePopup(GameObject popup)
    {
        popup.SetActive(false);
    }

    private void OpenWinPopup()
    {
        OpenPopup(_winPopup);
    }

    private void OpenLosePopup()
    {
        OpenPopup(_losePopup);
    }

}
