using UnityEngine;
using UnityEngine.UI;

public class InvertMouseSettings : MonoBehaviour
{
    [SerializeField] private Toggle invertXToggle;
    [SerializeField] private Toggle invertYToggle;
    
    private const string InvertXKey = "InvertX";
    private const string InvertYKey = "InvertY";

    public static bool InvertX { get; private set; } = false;
    public static bool InvertY { get; private set; } = false;

    private void Start()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        bool savedInvertX = PlayerPrefs.GetInt(InvertXKey, 0) == 1;
        bool savedInvertY = PlayerPrefs.GetInt(InvertYKey, 0) == 1;
        
        invertXToggle.SetIsOnWithoutNotify(savedInvertX);
        invertYToggle.SetIsOnWithoutNotify(savedInvertY);

        invertXToggle.onValueChanged.RemoveListener(SetInvertX);
        invertXToggle.onValueChanged.AddListener(SetInvertX);
        
        invertYToggle.onValueChanged.RemoveListener(SetInvertY);
        invertYToggle.onValueChanged.AddListener(SetInvertY);
        
        SetInvertX(savedInvertX);
        SetInvertY(savedInvertY);
    }

    public void SetInvertX(bool isEnabled)
    {
        InvertX = isEnabled;

        PlayerPrefs.SetInt(InvertXKey, isEnabled ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("Invert X: " + isEnabled);
    }

    public void SetInvertY(bool isEnabled)
    {
        InvertY = isEnabled;
        
        PlayerPrefs.SetInt(InvertYKey, isEnabled ? 1 : 0);
        PlayerPrefs.Save();
        
        Debug.Log("Invert Y: " + isEnabled);
    }
}