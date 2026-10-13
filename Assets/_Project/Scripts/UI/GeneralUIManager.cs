using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ParkingMastery.UI
{
    /// <summary>
    /// Popup açma/kapama ve açık/kapalı görsel değiştirme için statik UI yardımcıları.
    /// </summary>
    public class GeneralUIManager : MonoBehaviour
    {
        /// <summary>Verilen popup'ı açar (null ise bir şey yapmaz).</summary>
        public static void OpenPopup(GameObject popup)
        {
            if (popup != null)
                popup.SetActive(true);
        }

        /// <summary>Verilen popup'ı kapatır (null ise bir şey yapmaz).</summary>
        public static void ClosePopup(GameObject popup)
        {
            if (popup != null)
                popup.SetActive(false);
        }

        /// <summary>İkonu açık ↔ kapalı sprite arasında değiştirir.</summary>
        public static void SwitchImage(SwitchImage switchImage)
        {
            if (!TryGetImage(switchImage, out var imageComponent))
                return;

            imageComponent.sprite = imageComponent.sprite == switchImage.SpriteOn ? switchImage.SpriteOff : switchImage.SpriteOn;
        }

        /// <summary>İkonu 'açık' sprite'ına ayarlar.</summary>
        public static void SwitchImageOn(SwitchImage switchImage)
        {
            if (TryGetImage(switchImage, out var imageComponent))
                imageComponent.sprite = switchImage.SpriteOn;
        }

        /// <summary>İkonu 'kapalı' sprite'ına ayarlar.</summary>
        public static void SwitchImageOff(SwitchImage switchImage)
        {
            if (TryGetImage(switchImage, out var imageComponent))
                imageComponent.sprite = switchImage.SpriteOff;
        }

        private static bool TryGetImage(SwitchImage switchImage, out Image image)
        {
            image = null;
            return switchImage != null && switchImage.TryGetComponent(out image);
        }
    }
}
