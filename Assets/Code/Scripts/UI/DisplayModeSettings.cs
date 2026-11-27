using System;
using UnityEngine;
using TMPro;

public class DisplayModeSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown displayModeDropdown;
    private const string DisplayModekey = "DisplayMode";

    private void Start()
    {
       LoadSettings();
    }

    public void SetDisplayMode(int index)
    {
        FullScreenMode mode;

        switch (index)
        {
            case 0:
                mode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1:
                mode = FullScreenMode.Windowed;
                break;
            case 2:
                mode = FullScreenMode.FullScreenWindow;
                break;
            default:
                mode = FullScreenMode.ExclusiveFullScreen;
                break;
        }
        Screen.fullScreenMode = mode;
        PlayerPrefs.SetInt(DisplayModekey, index);
        PlayerPrefs.Save();
        
        Debug.Log("Changed display mode to: " + mode);
    }
    private void LoadSettings()
    {
        int savedMode = PlayerPrefs.GetInt(DisplayModekey, 0);
        
        displayModeDropdown.SetValueWithoutNotify(savedMode);
        displayModeDropdown.RefreshShownValue();
        
        displayModeDropdown.onValueChanged.RemoveListener(SetDisplayMode);
        displayModeDropdown.onValueChanged.AddListener(SetDisplayMode);
        
        SetDisplayMode(savedMode);
    }
}