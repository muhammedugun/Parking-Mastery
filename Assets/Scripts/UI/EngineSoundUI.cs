using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using YG;

/// <summary>
/// Motor sesini açıp kapatan buton için kullanılır. Ayar YG2.saves.isEngineSoundOn içinde saklanır.
/// Script, aynı GameObject'teki Button'a kendi tıklama dinleyicisini ekler; İkon ayar değiştikçe güncellenir.
/// </summary>
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
        YG2.onGetSDKData += RefreshIcon;
        RefreshIcon();
    }

    private void OnDisable()
    {
        YG2.onGetSDKData -= RefreshIcon;
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(Switch);
    }

    public void Switch()
    {
        YG2.saves.isEngineSoundOn = !YG2.saves.isEngineSoundOn;
        if (YG2.isSDKEnabled)
            YG2.SaveProgress();

        RefreshIcon();
    }

    private void RefreshIcon()
    {
        if (_icon == null)
            return;

        _icon.sprite = YG2.saves.isEngineSoundOn ? _spriteOn : _spriteOff;
    }
}
