using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class SwitchImage : MonoBehaviour
{
    public Sprite spriteOn, spriteOff;

    [SerializeField] string saveVariableName;

    private void Start()
    {
        // YandexGame sınıfından savesData nesnesini alalım.
        var savesDataInstance = YandexGame.savesData;

        // savesData nesnesindeki alana string ismine göre ulaşalım.
        FieldInfo fieldInfo = savesDataInstance.GetType().GetField(saveVariableName, BindingFlags.Public | BindingFlags.Instance);

        if (fieldInfo != null)
        {
            // Değeri okuyalım ve bool olarak dönüştürelim.
            object value = fieldInfo.GetValue(savesDataInstance);

            if (value is bool boolValue)
            {
                // boolValue'yu kullanarak işlem yapabilirsiniz
                if (boolValue == false)
                {
                    GetComponent<Image>().sprite = spriteOff;
                }
                else
                {
                    GetComponent<Image>().sprite = spriteOn;
                }
            }
            else
            {
                Debug.LogWarning($"{saveVariableName} değişkeni bir bool türünde değil.");
            }
        }
        else
        {
            Debug.LogWarning($"{saveVariableName} adında bir değişken bulunamadı.");
        }
    }


}
