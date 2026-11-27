using UnityEngine;
using UnityEngine.UI;

public class CameraSmoothnessSettings : MonoBehaviour
{
    [SerializeField] private Slider cameraSmoothnessSlider;
   // [SerializeField] CameraController cameraController;
   private const string CameraSmoothnessKey = "CameraSmoothness";

   private void Start()
   {
       LoadSettings();
   }

   public void SetCameraSmoothness(float value)
   {
       //cameraController.SetSmoothness(value);
       PlayerPrefs.SetFloat("CameraSmoothness", value);
       
       Debug.Log("Camera Smoothness set to: " + value);
   }

   private void LoadSettings()
   {
       float smoothness = PlayerPrefs.GetFloat(CameraSmoothnessKey, 10f);
       
       smoothness = Mathf.Clamp(smoothness, 0f, 20f);
       cameraSmoothnessSlider.SetValueWithoutNotify(smoothness);
       
       cameraSmoothnessSlider.onValueChanged.RemoveListener(SetCameraSmoothness);
       cameraSmoothnessSlider.onValueChanged.AddListener(SetCameraSmoothness);
       
       SetCameraSmoothness(smoothness);
       // cameraController.SetSmoothness(smoothness);
   }
}