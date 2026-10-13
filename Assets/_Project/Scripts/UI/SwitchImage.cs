using ParkingMastery.Services;
using UnityEngine;
using UnityEngine.UI;

namespace ParkingMastery.UI
{
    /// <summary>
    /// Düğme ikonunu kayıtlı bool ayara göre açık/kapalı sprite'a çevirir.
    /// saveVariableName: "isMusicOn" veya "isEngineSoundOn" (ISaveService üzerinden okunur).
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class SwitchImage : MonoBehaviour
    {
        [SerializeField] private Sprite spriteOn, spriteOff;

        /// <summary>Ayar açıkken gösterilen sprite.</summary>
        public Sprite SpriteOn => spriteOn;

        /// <summary>Ayar kapalıyken gösterilen sprite.</summary>
        public Sprite SpriteOff => spriteOff;

        [SerializeField] string saveVariableName;

        private void Start()
        {
            var saves = GameServices.Saves;
            bool value;

            switch (saveVariableName)
            {
                case "isMusicOn": value = saves.IsMusicOn; break;
                case "isEngineSoundOn": value = saves.IsEngineSoundOn; break;
                default:
                    Debug.LogWarning($"{saveVariableName} adında desteklenen bir kayıt ayarı yok.");
                    return;
            }

            if (TryGetComponent(out Image image))
                image.sprite = value ? spriteOn : spriteOff;
        }
    }
}
