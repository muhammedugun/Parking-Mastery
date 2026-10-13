using UnityEngine;
using UnityEngine.UI;

namespace ParkingMastery.UI
{
    /// <summary>Butona basınca en üstteki ekranı/popup'ı kapatır (<see cref="UIManager.Back"/>).</summary>
    [RequireComponent(typeof(Button))]
    public sealed class UICloseButton : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private static void OnClick()
        {
            if (UIManager.Instance != null)
                UIManager.Instance.Back();
        }
    }
}