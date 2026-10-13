using ParkingMastery.Services;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Düğme ikonunu kayıtlı bool ayara göre açık/kapalı sprite'a çevirir.
/// saveVariableName: "isMusicOn" veya "isEngineSoundOn" (ISaveService üzerinden okunur).
/// </summary>

namespace ParkingMastery.UI
{
    public class SwitchImage : MonoBehaviour
    {
        public Sprite spriteOn, spriteOff;

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

            GetComponent<Image>().sprite = value ? spriteOn : spriteOff;
        }
    }
}
