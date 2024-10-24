
using UnityEngine;
using UnityEngine.UI;

public class StarCounterUI : MonoBehaviour
{
    [SerializeField] private Image[] _starsImage;
    [SerializeField] private Sprite _faintStarSprite;

    private void OnEnable()
    {
        EventBus<int>.Subscribe(EventType.StarCountChanged, UpdateStars);
    }
    private void OnDisable()
    {
        EventBus<int>.Unsubscribe(EventType.StarCountChanged, UpdateStars);
    }

    private void UpdateStars(int starCount)
    {
        if (_starsImage[starCount].sprite != _faintStarSprite)
            _starsImage[starCount].sprite = _faintStarSprite;

        if (starCount == 1)
            EventBus<int>.Unsubscribe(EventType.StarCountChanged, UpdateStars);
    }
}
