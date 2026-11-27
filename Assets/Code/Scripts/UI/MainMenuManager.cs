using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
  //  [SerializeField] private GameObject mainMenuPanel;
  //  [SerializeField] private GameObject settingsPanel;

    public void StartGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void OpenSettings()
    {
    //    mainMenuPanel.SetActive(false);
    //    settingsPanel.SetActive(true);
        SceneManager.LoadScene("GameSettingsMenu");
    }

    public void BackToMainMenu()
    {
     //   settingsPanel.SetActive(false);
     //   mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}