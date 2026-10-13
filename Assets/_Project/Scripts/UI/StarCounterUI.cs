
using UnityEngine;
using UnityEngine.UI;
using ParkingMastery.Core;

namespace ParkingMastery.UI
{
    /// <summary>
    /// Kalan yıldız sayısını (StarCountChanged olayı) HUD ve kazanma popup'ındaki yıldız görsellerine yansıtır.
    /// </summary>
    public class StarCounterUI : MonoBehaviour
    {
        [SerializeField] private Image[] _starsImage, _winPopupStarsImage;
        [SerializeField] private Sprite _faintStarSprite;

        private void OnEnable()
        {
            EventBus<int>.Subscribe(GameEventType.StarCountChanged, UpdateStars);
        }
        private void OnDisable()
        {
            EventBus<int>.Unsubscribe(GameEventType.StarCountChanged, UpdateStars);
        }

        private void UpdateStars(int starCount)
        {
            if (_starsImage[starCount].sprite != _faintStarSprite)
                _starsImage[starCount].sprite = _faintStarSprite;

            if (_winPopupStarsImage[starCount].sprite != _faintStarSprite)
                _winPopupStarsImage[starCount].sprite = _faintStarSprite;

            if (starCount == 1)
                EventBus<int>.Unsubscribe(GameEventType.StarCountChanged, UpdateStars);
        }
    }
}
