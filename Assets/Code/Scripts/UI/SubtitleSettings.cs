using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SubtitleSettings : MonoBehaviour
{
    [SerializeField] private Toggle subtitleToggle;
    private const string SubtitlesKey = "Subtitles";

    private void Start()
    {
        LoadSettings();
    }

    public void SetSubtitles(bool enabled)
    {
        // subtitleManager.SetSubtitlesEnabled(enabled);
        PlayerPrefs.SetInt(SubtitlesKey, enabled ? 1 : 0);
        Debug.Log("Subtitles: " + enabled);
    }

    private void LoadSettings()
    {
        bool subtitlesSaved = PlayerPrefs.GetInt(SubtitlesKey, 0) == 1;
        subtitleToggle.SetIsOnWithoutNotify(subtitlesSaved);
        
        subtitleToggle.onValueChanged.RemoveListener(SetSubtitles);
        subtitleToggle.onValueChanged.AddListener(SetSubtitles);
        
        SetSubtitles(subtitlesSaved);
    }
}