using UnityEngine;
using TMPro;

public class AntiAliasingSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown antiAliasingDropdown;
    
    private const string AntiAliasingKey = "AntiAliasing";
    private readonly int[] antiAliasingOptions = { 0, 2, 4, 8 };

    private void Start()
    {
        LoadSettings();
    }

    public void SetAntiAliasing(int index)
    {
        if (index < 0 || index >= antiAliasingOptions.Length)
            return;
        
        int samples = antiAliasingOptions[index];
        
        QualitySettings.antiAliasing = samples;
        
        PlayerPrefs.SetInt(AntiAliasingKey, samples);
        PlayerPrefs.Save();
        
        Debug.Log("Anti Aliasing set to: " + (samples == 0 ? "off" : samples + "x MSAA"));
        
    }
    private void LoadSettings()
    {
        int savedValue = PlayerPrefs.GetInt(AntiAliasingKey, 4);

        int selectedIndex = 2;

        for (int i = 0; i < antiAliasingOptions.Length; i++)
        {
            if (antiAliasingOptions[i] == savedValue)
            {
                selectedIndex = i;
                break;
            }
        }
        
        antiAliasingDropdown.SetValueWithoutNotify(selectedIndex);
        antiAliasingDropdown.RefreshShownValue();
        
        antiAliasingDropdown.onValueChanged.RemoveListener(SetAntiAliasing);
        antiAliasingDropdown.onValueChanged.AddListener(SetAntiAliasing);
        
        SetAntiAliasing(selectedIndex);
    }
}