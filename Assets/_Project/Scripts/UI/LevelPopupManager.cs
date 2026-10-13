using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkingMastery.Services;
using ParkingMastery.Core;
using ParkingMastery.Gameplay;

namespace ParkingMastery.UI
{
    public class LevelPopupManager : MonoBehaviour
    {
        [SerializeField] private GameObject _winPopup, _askRewardAdPopup, _pausePopup;
        [SerializeField] private GameObject _pauseButton, _cameraButton;
        private void OnEnable()
        {
            EventBus.Subscribe(GameEventType.ParkingSuccessful, OpenWinPopupInvoke);
            LevelController.CloseAllPopupsRequested += CloseAllPopupLevel;
            EventBus.Subscribe(GameEventType.LevelFailed, OpenAskRewardAdPopupInvoke);
            GameServices.Ads.RewardGranted += ShowPauseAndCameraButtons;
        }
        private void OnDisable()
        {
            EventBus.Unsubscribe(GameEventType.ParkingSuccessful, OpenWinPopupInvoke);
            LevelController.CloseAllPopupsRequested -= CloseAllPopupLevel;
            EventBus.Unsubscribe(GameEventType.LevelFailed, OpenAskRewardAdPopupInvoke);
            GameServices.Ads.RewardGranted -= ShowPauseAndCameraButtons;
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
            EventBus.Publish(GameEventType.MuteCarSound);
        }

        private void HidePauseAndCameraButtons()
        {
            _pauseButton.SetActive(false);
            _cameraButton.SetActive(false);
        }

        private void ShowPauseAndCameraButtons(string id = null)
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
            HidePauseAndCameraButtons();
            EventBus.Publish(GameEventType.MuteCarSound);
            Invoke(nameof(InvokePause), 0.05f);
        }

    private void InvokePause()
        {
            OpenPopup(_pausePopup);
            LevelController.PauseGame();
        }

        public void ClosePausePopup()
        {
            ShowPauseAndCameraButtons();
            ClosePopup(_pausePopup);
            LevelController.ResumeGame();
            EventBus.Publish(GameEventType.UnMuteCarSound);
        }

        public void CloseAllPopupLevel()
        {
            ClosePausePopup();
            ClosePopup(_askRewardAdPopup);
            ClosePopup(_winPopup);
            EventBus.Publish(GameEventType.UnMuteCarSound);
        }

    }
}
