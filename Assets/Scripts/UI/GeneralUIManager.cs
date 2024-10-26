using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneralUIManager : MonoBehaviour
{
    public static void OpenPopup(GameObject popup)
    {
        popup.SetActive(true);
    }

    public static void ClosePopup(GameObject popup)
    {
        popup.SetActive(false);
    }

    public static void SwitchImage(SwitchImage switchImage)
    {
        var imageComponent = switchImage.GetComponent<Image>();

        if (imageComponent.sprite == switchImage.spriteOn)
            imageComponent.sprite = switchImage.spriteOff;
        else
            imageComponent.sprite = switchImage.spriteOn;
    }

    public static void SwitchImageOn(SwitchImage switchImage)
    {
        switchImage.GetComponent<Image>().sprite = switchImage.spriteOn;
    }

    public static void SwitchImageOff(SwitchImage switchImage)
    {
        switchImage.GetComponent<Image>().sprite = switchImage.spriteOff;
    }
}
