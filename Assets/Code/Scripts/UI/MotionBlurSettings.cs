using UnityEngine;
using UnityEngine.UI;

public class MotionBlurSettings : MonoBehaviour
{
    [SerializeField] private Toggle motionBlurToggle;
    private const string MotionBlurKey = "MotionBlur";

    private void Start()
    {
        LoadSettings();
    }

    public void SetMotionBlur(bool enabled)
    {
        PlayerPrefs.SetInt(MotionBlurKey, enabled ? 1 : 0);
        Debug.Log("Motion Blur: " + enabled);
    }

    private void LoadSettings()
    {
        bool motionBlurEnabled = PlayerPrefs.GetInt(MotionBlurKey, 0) == 1;
        motionBlurToggle.SetIsOnWithoutNotify(motionBlurEnabled);
        
        motionBlurToggle.onValueChanged.RemoveListener(SetMotionBlur);
        motionBlurToggle.onValueChanged.AddListener(SetMotionBlur);
        
        SetMotionBlur(motionBlurEnabled);
    }
}