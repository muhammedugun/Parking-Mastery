using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YG;

public class LevelPopupManager : MonoBehaviour
{
    [SerializeField] private GameObject _winPopup, _askRewardAdPopup, _pausePopup;
    [SerializeField] private GameObject _pauseButton, _cameraButton;
    private void OnEnable()
    {
        EventBus.Subscribe(EventType.ParkingSuccessful, OpenWinPopupInvoke);
        EventBus.Subscribe(EventType.CarContactObstacle, OpenAskRewardAdPopupInvoke);
        YandexGame.RewardVideoEvent += ShowPauseAndCameraButtons;
    }
    private void OnDisable()
    {
        EventBus.Unsubscribe(EventType.ParkingSuccessful, OpenWinPopupInvoke);
        EventBus.Unsubscribe(EventType.CarContactObstacle, OpenAskRewardAdPopupInvoke);
        YandexGame.RewardVideoEvent -= ShowPauseAndCameraButtons;
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
        HidePauseAndCameraButtons();
        OpenPopup(_winPopup);
        EventBus.Publish(EventType.MuteCarSound);
    }

    private void HidePauseAndCameraButtons()
    {
        _pauseButton.SetActive(false);
        _cameraButton.SetActive(false);
    }

    private void ShowPauseAndCameraButtons(int id)
    {
        _pauseButton.SetActive(true);
        _cameraButton.SetActive(true);
    }

    private void OpenWinPopupInvoke()
    {
        Invoke(nameof(OpenWinPopup), 1f);
    }

    private void OpenAskRewardAdPopup()
    {
        HidePauseAndCameraButtons();
        OpenPopup(_askRewardAdPopup);
    }

    private void OpenAskRewardAdPopupInvoke()
    {
        Invoke(nameof(OpenAskRewardAdPopup), 1f);
    }

    public void OpenPausePopup()
    {
        EventBus.Publish(EventType.MuteCarSound);
        Invoke(nameof(InvokePause), 0.05f);
    }

    private void InvokePause()
    {
        OpenPopup(_pausePopup);
        GameLoopManager.PauseGame();
        YandexGame.FullscreenShow();
    }

    public void ClosePausePopup()
    {
        ClosePopup(_pausePopup);
        GameLoopManager.ResumeGame();
        EventBus.Publish(EventType.UnMuteCarSound);
    }

    public void CloseAllPopupLevel()
    {
        ClosePausePopup();
        ClosePopup(_askRewardAdPopup);
        ClosePopup(_winPopup);
        EventBus.Publish(EventType.UnMuteCarSound);
    }

}
