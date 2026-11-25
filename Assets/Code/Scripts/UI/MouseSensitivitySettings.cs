using UnityEngine;
using UnityEngine.UI;

public class MouseSensitivitySettings : MonoBehaviour
{
    [SerializeField] private Slider sensitivitySlider;
    private const string SensitivityKey = "MouseSensitivity";

    public static float MouseSensitivity { get; private set; } = 1f;
    private void Start()
    {
       LoadSettings(); 
    }
    private void LoadSettings()
    {
        float savedSensitivity = PlayerPrefs.GetFloat(SensitivityKey, 1f);
        
        savedSensitivity = Mathf.Clamp(savedSensitivity, 0.1f, 10f);
        
        sensitivitySlider.SetValueWithoutNotify(savedSensitivity);
        
        sensitivitySlider.onValueChanged.RemoveListener(SetSensitivity);
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        
        SetSensitivity(savedSensitivity);
    }

    public void SetSensitivity(float value)
    {
        MouseSensitivity = value;
        
        PlayerPrefs.SetFloat(SensitivityKey, value);
        PlayerPrefs.Save();

        Debug.Log($"Mouse Sensitivity: {value}");
    }
}