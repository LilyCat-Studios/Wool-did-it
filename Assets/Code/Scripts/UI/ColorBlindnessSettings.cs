using UnityEngine;
using TMPro;

public class ColorBlindnessSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown colorBlindnessDropdown;
    private const string ColorBlindnessKey = "ColorBlindness";
    
    private void Start()
    {
        LoadSettings();
    }

    public void SetColorBlindness(int mode)
    {
        PlayerPrefs.SetInt(ColorBlindnessKey, mode);
        PlayerPrefs.Save();
        Debug.Log("ColorBlindness: " + mode);
    }

    private void LoadSettings()
    {
        int savedMode = PlayerPrefs.GetInt(ColorBlindnessKey, 0);
        
        colorBlindnessDropdown.SetValueWithoutNotify(savedMode);
        colorBlindnessDropdown.RefreshShownValue();
        
        colorBlindnessDropdown.onValueChanged.RemoveListener(SetColorBlindness);
        colorBlindnessDropdown.onValueChanged.AddListener(SetColorBlindness);
        
        SetColorBlindness(savedMode);
    }
}