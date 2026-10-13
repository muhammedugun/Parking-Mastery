using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ParkingMastery.Services;

namespace ParkingMastery.UI
{
    /// <summary>
    /// Seviye seçim ekranında düğmelere kilit simgesi veya kayıtlı yıldızları yerleştirir.
    /// </summary>
    public class LevelSelectionMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _levelLockIconPrefab, _levelStarsGroupPrefab;
        [SerializeField] private Transform _groupLevels;
        [SerializeField] private Sprite _starSprite;
        private Button[] _levelsButtons;

        private void Start()
        {
            if (_groupLevels == null)
            {
                Debug.LogError("LevelSelectionMenu: _groupLevels atanmamış.", this);
                return;
            }

            _levelsButtons = _groupLevels.GetComponentsInChildren<Button>();
            foreach (var levelButton in _levelsButtons)
            {
                var label = levelButton.GetComponentInChildren<Text>();
                if (label == null || !int.TryParse(label.text, out int levelNumber))
                {
                    Debug.LogWarning($"LevelSelectionMenu: '{levelButton.name}' düğmesinde seviye numarası okunamadı.", levelButton);
                    continue;
                }

                if (GameServices.Saves.IsLevelUnlocked(levelNumber))
                {
                    int levelStarCount = GameServices.Saves.GetLevelStars(levelNumber);
                    if (levelStarCount > 0 && _levelStarsGroupPrefab != null)
                    {
                        var levelStarsGroup = Instantiate(_levelStarsGroupPrefab, parent: levelButton.transform);
                        int starSlots = Mathf.Min(levelStarCount, levelStarsGroup.transform.childCount);
                        for (int i = 0; i < starSlots; i++)
                        {
                            if (levelStarsGroup.transform.GetChild(i).TryGetComponent(out Image star))
                                star.sprite = _starSprite;
                        }
                    }
                }
                else
                {
                    levelButton.interactable = false;
                    if (levelButton.transform.childCount > 0)
                        levelButton.transform.GetChild(0).gameObject.SetActive(false);
                    if (_levelLockIconPrefab != null)
                        Instantiate(_levelLockIconPrefab, parent: levelButton.transform);
                }
            }
        }

    }
}
