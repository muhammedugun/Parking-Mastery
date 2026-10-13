using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ParkingMastery.Core;

namespace ParkingMastery.UI
{
    public class LevelTimeCounterUI : MonoBehaviour
    {
        [SerializeField] private Text _timeCounterText;

        private void OnEnable()
        {
            EventBus<int>.Subscribe(GameEventType.LevelTimeUpdated, UpdateText);
        }

        private void OnDisable()
        {
            EventBus<int>.Unsubscribe(GameEventType.LevelTimeUpdated, UpdateText);
        }

        private void UpdateText(int time)
        {
            _timeCounterText.text = time.ToString();
        }

    }
}
