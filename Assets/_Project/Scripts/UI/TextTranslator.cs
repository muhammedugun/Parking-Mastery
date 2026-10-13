using UnityEngine;
using UnityEngine.UI;
using ParkingMastery.Services;
using System.Collections.Generic;

namespace ParkingMastery.UI
{
    /// <summary>
    /// TextTranslator sınıfı, farklı dillerde çevirileri destekler ve kullanıcı diline göre uygun metni gösterir.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class TextTranslator : MonoBehaviour
    {
        [SerializeField] private string _defaultText;
        [SerializeField] private List<LanguageText> _translations;

        private Dictionary<string, string> _translationDictionary;

        void Start()
        {
            InitializeTranslations();
            UpdateText();
        }

        private void InitializeTranslations()
        {
            _translationDictionary = new Dictionary<string, string>();

            if (_translations == null)
                return;

            foreach (var translation in _translations)
            {
                _translationDictionary[translation.LanguageCode] = translation.Text;
            }
        }

        private void UpdateText()
        {
            if (!TryGetComponent(out Text textComponent))
            {
                Debug.LogWarning("TextTranslator: aynı nesnede Text bileşeni yok.", this);
                return;
            }

            textComponent.text = GetText();
        }

        /// <summary>Kullanıcı diline uygun çeviriyi döndürür; çeviri yoksa varsayılan metni verir.</summary>
        public string GetText()
        {
            if (_translationDictionary == null)
                InitializeTranslations();

            return _translationDictionary.TryGetValue(GameServices.Localization.Language, out string translatedText)
                ? translatedText
                : _defaultText;
        }
    }

    /// <summary>Tek bir dil için çeviri: dil kodu ve o dildeki metin.</summary>
    [System.Serializable]
    public class LanguageText
    {
        public string LanguageCode;
        public string Text;
    }
}
