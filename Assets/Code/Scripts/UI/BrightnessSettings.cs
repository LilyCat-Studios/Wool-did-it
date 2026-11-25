using UnityEngine;
using UnityEngine.UI;

public class BrightnessSettings : MonoBehaviour
{
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Image brightnessOverlay;
    
    private const string BrightnessKey = "Brightness";

    private void Start()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        float savedBrightness = PlayerPrefs.GetFloat(BrightnessKey, 1f);
        
        savedBrightness = Mathf.Clamp(savedBrightness, 0f, 2f);
        brightnessSlider.SetValueWithoutNotify(savedBrightness);
        
        brightnessSlider.onValueChanged.RemoveListener(SetBrightness);
        brightnessSlider.onValueChanged.AddListener(SetBrightness);
        
        SetBrightness(savedBrightness);
    }

    public void SetBrightness(float value)
    {
        float alpha = 1f - (value / 2f);

        Color overlayColor = Color.black;
        overlayColor.a = alpha;
        
        brightnessOverlay.color = overlayColor;
        
        PlayerPrefs.SetFloat(BrightnessKey, value);
        PlayerPrefs.Save();
        
        Debug.Log($"Brightness: {value}");
        
    }
}