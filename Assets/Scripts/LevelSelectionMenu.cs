using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class LevelSelectionMenu : MonoBehaviour
{
    [SerializeField] private GameObject _levelLockIconPrefab, _levelStarsGroupPrefab;
    [SerializeField] private Transform _groupLevels;
    [SerializeField] private Sprite _starSprite;
    private Button[] _levelsButtons;

    private void Start()
    {
        _levelsButtons = _groupLevels.GetComponentsInChildren<Button>();
        foreach (var levelButton in _levelsButtons)
        {
            int levelNumber = int.Parse(levelButton.GetComponentInChildren<Text>().text);
            if (YandexGame.savesData.unlockLevels[levelNumber])
            {
                int levelStarCount = YandexGame.savesData.levelsStarCount[levelNumber];
                if (levelStarCount > 0)
                {
                    var levelStarsGroup = Instantiate(_levelStarsGroupPrefab, parent: levelButton.transform);
                    for (int i = 0; i < levelStarCount; i++)
                    {
                        levelStarsGroup.transform.GetChild(i).GetComponent<Image>().sprite = _starSprite;
                    }
                }

            }
            else
            {
                levelButton.interactable = false;
                levelButton.transform.GetChild(0).gameObject.SetActive(false);
                Instantiate(_levelLockIconPrefab, parent: levelButton.transform);
            }

        }
    }

}
