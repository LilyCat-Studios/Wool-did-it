using UnityEngine;
using UnityEngine.UI;

public class UIScaleSettings : MonoBehaviour
{
    [SerializeField] Slider uiScaleSlider;
   // [SerializeField] RectTransform uiRoot;

    private void Start()
    {
        LoadSettings();
    }

    public void SetUIScale(float value)
    {
     //   uiRoot.localScale =  Vector3.one * value;
        PlayerPrefs.SetFloat("UIScale", value);
        PlayerPrefs.Save();
        Debug.Log("UI Scale: " + value);
    }

    private void LoadSettings()
    {
        float uiScale = PlayerPrefs.GetFloat("UIScale", 1f);
        uiScale = Mathf.Clamp(uiScale, 0.75f, 1.5f);
        uiScaleSlider.SetValueWithoutNotify(uiScale);
        
        uiScaleSlider.onValueChanged.RemoveListener(SetUIScale);
        uiScaleSlider.onValueChanged.AddListener(SetUIScale);
    //    uiRoot.localScale = Vector3.one * uiScale;
    SetUIScale(uiScale);
    }
}