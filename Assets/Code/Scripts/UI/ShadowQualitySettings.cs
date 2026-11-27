using UnityEngine;
using TMPro;

public class ShadowQualitySettings : MonoBehaviour
{
	[SerializeField] private TMP_Dropdown shadowQualityDropdown;

	private const string ShadowQualityKey = "ShadowQuality";

	private void Start()
	{
		LoadSettings();
	}

	public void SetShadowQuality(int index)
	{
		if (index < 0 || index > 3)
			return;

		switch (index)
		{
			case 0:
				QualitySettings.shadows = ShadowQuality.Disable;
				QualitySettings.shadowResolution = ShadowResolution.Low;
				break;
			case 1:
				QualitySettings.shadows = ShadowQuality.HardOnly;
				QualitySettings.shadowResolution = ShadowResolution.Low;
				break;
			case 2:
				QualitySettings.shadows = ShadowQuality.All;
				QualitySettings.shadowResolution = ShadowResolution.Medium;
				break;
			case 3:
				QualitySettings.shadows = ShadowQuality.All;
				QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
				break;
		}
		
		PlayerPrefs.SetInt(ShadowQualityKey, index);
		PlayerPrefs.Save();
		
		Debug.Log($"Shadow Quality Changed to: {shadowQualityDropdown.options[index].text}");
	}
	private void LoadSettings()
	{
		int savedQuality = PlayerPrefs.GetInt(ShadowQualityKey, 2);
		savedQuality = Mathf.Clamp(savedQuality, 0, 3);

		shadowQualityDropdown.SetValueWithoutNotify(savedQuality);
		shadowQualityDropdown.RefreshShownValue();

		shadowQualityDropdown.onValueChanged.RemoveListener(
			SetShadowQuality);
		shadowQualityDropdown.onValueChanged.AddListener(
			SetShadowQuality);

		SetShadowQuality(savedQuality);
	}
}
		

