using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class VSyncSettings : MonoBehaviour
{
    [SerializeField] private Toggle vSyncToggle;
    private const string VSyncKey = "VSync";

    private void Start()
    {
        LoadSettings();
    }

    public void SetVSync(bool isEnabled)
    {
        QualitySettings.vSyncCount = isEnabled ? 1 : 0;
        
        PlayerPrefs.SetInt(VSyncKey, isEnabled ? 1 : 0);
        PlayerPrefs.Save();
        
        Debug.Log("V-Sync: " + isEnabled);
    }
    private void LoadSettings()
    {
        int savedValue = PlayerPrefs.GetInt(VSyncKey, 1);
        
        vSyncToggle.SetIsOnWithoutNotify(savedValue == 1);
        
        vSyncToggle.onValueChanged.RemoveListener(SetVSync);
        vSyncToggle.onValueChanged.AddListener(SetVSync);
        
        SetVSync(savedValue == 1);
        
    }
}