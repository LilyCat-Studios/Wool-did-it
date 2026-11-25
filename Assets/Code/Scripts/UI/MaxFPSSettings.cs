using UnityEngine;
using TMPro;

public class MaxFPSSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown maxFPSDropdown;

    private const string MaxFPSKey = "MaxFPS";
    private readonly int[] fpsOptions = { 30, 60, 120, 144, 240, 0 };

    private void Start()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        int savedFPS = PlayerPrefs.GetInt(MaxFPSKey, 60);
        int selectedIndex = 1;

        for (int i = 0; i < fpsOptions.Length; i++)
        {
            if (fpsOptions[i] == savedFPS)
            {
                selectedIndex = i;
                break;
            }
        }
        
        maxFPSDropdown.SetValueWithoutNotify(selectedIndex);
        maxFPSDropdown.RefreshShownValue();
        
        maxFPSDropdown.onValueChanged.RemoveListener(SetMaxFPS);
        maxFPSDropdown.onValueChanged.AddListener(SetMaxFPS);
        
        SetMaxFPS(selectedIndex);
    }

    public void SetMaxFPS(int index)
    {
        if (index < 0 || index >= fpsOptions.Length)
            return;
        
        int fps =  fpsOptions[index];
        
        Application.targetFrameRate = fps;
        
        PlayerPrefs.SetInt(MaxFPSKey, fps);
        PlayerPrefs.Save();
        
        Debug.Log("Max FPS set to: " + (fps == 0 ? "Unlimited" : fps.ToString()));
    }

}