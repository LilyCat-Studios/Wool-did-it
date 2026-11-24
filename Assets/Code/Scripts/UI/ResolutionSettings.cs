using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResolutionSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    private readonly List<Resolution> availableResolutions = new List<Resolution>();

    private void Start()
    {
        if (resolutionDropdown == null)
        {
            Debug.LogError("resolutionDropdown is niet gekoppeld in de Inspector!", this);
            return;
        }

        LoadResolutions();
    }

    private void LoadResolutions()
    {
        availableResolutions.Clear();

        foreach (Resolution resolution in Screen.resolutions)
        {
            AddResolution(resolution.width, resolution.height);
        }
        
        AddResolution(Screen.width, Screen.height);
        
        availableResolutions.Sort((a, b) =>
            (a.width * a.height).CompareTo(b.width * b.height));

        List<string> options = new List<string>();

        foreach (Resolution resolution in availableResolutions)
        {
            string ratio = GetAspectRatio(resolution.width, resolution.height);
            options.Add($"{resolution.width} x {resolution.height} ({ratio})");
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);

        int savedWidth = PlayerPrefs.GetInt("ResolutionWidth", Screen.width);
        int savedHeight = PlayerPrefs.GetInt("ResolutionHeight", Screen.height);

        int selectedIndex = FindResolutionIndex(savedWidth, savedHeight);
        
        resolutionDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
        resolutionDropdown.SetValueWithoutNotify(selectedIndex);
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        
        if (PlayerPrefs.HasKey("ResolutionWidth") && PlayerPrefs.HasKey("ResolutionHeight"))
        {
            ApplyResolution(selectedIndex);
        }
    }

    private void OnDropdownValueChanged(int index)
    {
        Debug.Log($"Dropdown gewijzigd naar index: {index}");
        ApplyResolution(index);
    }

    private void ApplyResolution(int index)
    {
        if (index < 0 || index >= availableResolutions.Count)
        {
            Debug.LogWarning($"Ongeldige resolutie-index: {index}");
            return;
        }

        Resolution target = availableResolutions[index];
        
        PlayerPrefs.SetInt("ResolutionWidth", target.width);
        PlayerPrefs.SetInt("ResolutionHeight", target.height);
        PlayerPrefs.Save();

        FullScreenMode mode = Screen.fullScreenMode;
#if UNITY_EDITOR
        if (mode == FullScreenMode.ExclusiveFullScreen)
        {
            mode = FullScreenMode.FullScreenWindow;
        }
#endif

        Screen.SetResolution(target.width, target.height, mode);
        Debug.Log($"Resolutie ingesteld op: {target.width} x {target.height} ({mode})");
    }

    private void AddResolution(int width, int height)
    {
        if (width <= 0 || height <= 0) return;

        for (int i = 0; i < availableResolutions.Count; i++)
        {
            if (availableResolutions[i].width == width &&
                availableResolutions[i].height == height)
            {
                return;
            }
        }

        availableResolutions.Add(new Resolution
        {
            width = width,
            height = height
        });
    }

    private int FindResolutionIndex(int width, int height)
    {
        for (int i = 0; i < availableResolutions.Count; i++)
        {
            if (availableResolutions[i].width == width &&
                availableResolutions[i].height == height)
            {
                return i;
            }
        }

        for (int i = 0; i < availableResolutions.Count; i++)
        {
            if (availableResolutions[i].width == Screen.width &&
                availableResolutions[i].height == Screen.height)
            {
                return i;
            }
        }

        return 0;
    }

    private string GetAspectRatio(int width, int height)
    {
        int divisor = GreatestCommonDivisor(width, height);
        return $"{width / divisor}:{height / divisor}";
    }

    private int GreatestCommonDivisor(int a, int b)
    {
        while (b != 0)
        {
            int remainder = a % b;
            a = b;
            b = remainder;
        }
        return a;
    }
}