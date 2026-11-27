using UnityEngine;
using UnityEngine.UI;

public class HUDSettings : MonoBehaviour
{
     [SerializeField] private Toggle hudToggle;
     [SerializeField] private Toggle crossHairToggle;

    // [SerializeField] private GameObject hudObject;
    // [SerializeField] private GameObject crossHairObject;
     
     private const string HUDKey = "HUD";
     private const string CrossHair = "Crosshair";

     private void Start()
     {
      LoadSettings();    
     }

     public void SetHUD(bool enabled)
     {
          // hudObject.SetHUD(enabled);
          PlayerPrefs.SetInt(HUDKey, enabled ? 1 : 0);
          Debug.Log("HUD: " + enabled);
     }

     public void SetCrossHair(bool enabled)
     {
         //crosshairObject.SetCrossHair(enabled);
         PlayerPrefs.SetInt(CrossHair, enabled ? 1 : 0);
         Debug.Log( "Crosshair" + enabled);
     }

     private void LoadSettings()
     {
          bool hudEnabled = PlayerPrefs.GetInt(HUDKey, 0) == 1;
          bool crossHairEnabled = PlayerPrefs.GetInt(CrossHair, 0) == 1;
          
          hudToggle.SetIsOnWithoutNotify(hudEnabled);
          crossHairToggle.SetIsOnWithoutNotify(crossHairEnabled);
          
          hudToggle.onValueChanged.RemoveListener(SetHUD);
          crossHairToggle.onValueChanged.RemoveListener(SetCrossHair);
          
          hudToggle.onValueChanged.AddListener(SetHUD);
          crossHairToggle.onValueChanged.AddListener(SetCrossHair);
          
          SetHUD(hudEnabled);
          SetCrossHair(crossHairEnabled);
          
     }
}
