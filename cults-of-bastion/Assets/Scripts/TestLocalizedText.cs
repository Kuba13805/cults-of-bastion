using System;
using Managers;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class TestLocalizedText : MonoBehaviour
{
    public static event Action<string> OnGetLocalizedString;
    [Button]
    public void GetLocalizedText()
    {
        LocalizationManager.OnSendLocalizedString += LocalizeText;
        OnGetLocalizedString?.Invoke(GetComponent<TextMeshProUGUI>().text);
    }

    private void OnDestroy()
    {
        LocalizationManager.OnSendLocalizedString -= LocalizeText;
    }

    private void LocalizeText(string translationKey)
    {
        GetComponent<TextMeshProUGUI>().text = translationKey;
        LocalizationManager.OnSendLocalizedString -= LocalizeText;
    }
}
