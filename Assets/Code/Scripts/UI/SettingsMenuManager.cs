using UnityEngine;

public class SettingsMenuManager : MonoBehaviour
{
    [Header("Settings Panels")]
    [SerializeField]  private GameObject videoPanel;
    [SerializeField]  private GameObject audioPanel;
    [SerializeField]  private GameObject controlsPanel;
    [SerializeField]  private GameObject gameplayPanel;
    void Start()
    {
        OpenVideo();
    }

    public void OpenVideo()
    {
        ShowPanel(videoPanel);
    }

    public void OpenAudio()
    {
        ShowPanel(audioPanel);
    }

    public void OpenControls()
    {
        ShowPanel(controlsPanel);
    }

    public void OpenGameplay()
    {
        ShowPanel(gameplayPanel);
    }

    private void ShowPanel(GameObject selectedPanel)
    {
        videoPanel.SetActive(selectedPanel == videoPanel);
        audioPanel.SetActive(selectedPanel == audioPanel);
        controlsPanel.SetActive(selectedPanel == controlsPanel);
        gameplayPanel.SetActive(selectedPanel == gameplayPanel);
    }
}
