using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkingMastery.Services;
using ParkingMastery.Core;
using ParkingMastery.Gameplay;

namespace ParkingMastery.UI
{
    /// <summary>Seviye içi popup'ları yönetir: park başarısında kazanma popup'ını, çarpışmada ödüllü reklam sorusunu açar; ödül alınamazsa kaybetme popup'ına geçer ve duraklatma/kamera düğmelerini gizleyip gösterir.</summary>
    public class LevelPopupManager : MonoBehaviour
    {
        [SerializeField] private GameObject _winPopup, _askRewardAdPopup, _pausePopup;
        [SerializeField] private GameObject _pauseButton, _cameraButton;
        [SerializeField] private GameObject _losePopup;
        private void OnEnable()
        {
            EventBus.Subscribe(GameEventType.ParkingSuccessful, OpenWinPopupInvoke);
            LevelController.CloseAllPopupsRequested += CloseAllPopupLevel;
            EventBus.Subscribe(GameEventType.LevelFailed, OpenAskRewardAdPopupInvoke);
            GameServices.Ads.RewardGranted += OnRewardGranted;
            GameServices.Ads.RewardFailed += OnRewardFailed;
        }
        private void OnDisable()
        {
            EventBus.Unsubscribe(GameEventType.ParkingSuccessful, OpenWinPopupInvoke);
            LevelController.CloseAllPopupsRequested -= CloseAllPopupLevel;
            EventBus.Unsubscribe(GameEventType.LevelFailed, OpenAskRewardAdPopupInvoke);
            GameServices.Ads.RewardGranted -= OnRewardGranted;
            GameServices.Ads.RewardFailed -= OnRewardFailed;
        }

        /// <summary>Verilen popup'ı açar.</summary>
        public void OpenPopup(GameObject popup)
        {
            popup.SetActive(true);
        }

        /// <summary>Verilen popup'ı kapatır.</summary>
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

        private void OnRewardGranted(string id)
        {
            if (id != AdRewardIds.Resume) return;
            ShowPauseAndCameraButtons();
        }

        /// <summary>Ödüllü reklam ödülsüz bitti/gösterilemedi: oyuncu takılmasın, ücretsiz seçenekler (Tekrar / Menü) açılsın.</summary>
        private void OnRewardFailed(string id)
        {
            if (id != AdRewardIds.Resume || _losePopup == null || _askRewardAdPopup == null || !_askRewardAdPopup.activeInHierarchy) return;
            ClosePopup(_askRewardAdPopup);
            OpenPopup(_losePopup);
        }

        private void ShowPauseAndCameraButtons()
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
