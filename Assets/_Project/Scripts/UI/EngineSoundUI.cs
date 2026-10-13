using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ParkingMastery.Services;

/// <summary>
/// Motor sesini açıp kapatan buton için kullanılır. Ayar ISaveService.IsEngineSoundOn içinde saklanır.
/// Script, aynı GameObject'teki Button'a kendi tıklama dinleyicisini ekler; İkon ayar değiştikçe güncellenir.
/// </summary>

namespace ParkingMastery.UI
{
    [RequireComponent(typeof(Button))]
    public class EngineSoundUI : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Sprite _spriteOn;
        [SerializeField] private Sprite _spriteOff;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();

            // Buton başka bir butondan kopyalandıysa (ör. müzik butonu) eski kalıcı tıklama olaylarını devre dışı bırak.
            for (int i = 0; i < _button.onClick.GetPersistentEventCount(); i++)
            {
                _button.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);
            }
            _button.onClick.AddListener(Switch);
        }

        private void OnEnable()
        {
            GameServices.Saves.Loaded += RefreshIcon;
            RefreshIcon();
        }

        private void OnDisable()
        {
            GameServices.Saves.Loaded -= RefreshIcon;
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(Switch);
        }

        public void Switch()
        {
            GameServices.Audio.ToggleEngineSound();

            RefreshIcon();
        }

        private void RefreshIcon()
        {
            if (_icon == null)
                return;

            _icon.sprite = GameServices.Audio.IsEngineSoundOn ? _spriteOn : _spriteOff;
        }
    }
}
