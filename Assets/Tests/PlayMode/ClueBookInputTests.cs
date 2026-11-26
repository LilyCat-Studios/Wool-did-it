using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

[Category("Player Input")]
public class ClueBookInputTests : InputTestFixture
{
    private const string ScenePath = "Assets/Scenes/SheepPen.unity";
    private const string StartButtonName = "Start";
    private const string ClueBookName = "Book";

    private Keyboard _keyboard;
    private PlayerInput _playerInput;
    private GameObject _clueBook;

    [TearDown]
    public void DisablePlayerInput()
    {
        foreach (PlayerInput playerInput in Object.FindObjectsByType<PlayerInput>())
        {
            playerInput.enabled = false;
        }
    }

    [UnityTest]
    public IEnumerator PressingQ_OpensClueBook()
    {
        yield return LoadDemoScene();
        Assert.That(_clueBook.activeSelf, Is.False, "Cluebook was already open at the start.");

        yield return PressQ();

        Assert.That(_clueBook.activeSelf, Is.True);
    }

    [UnityTest]
    public IEnumerator PressingQTwice_ClosesClueBook()
    {
        yield return LoadDemoScene();

        yield return PressQ();
        yield return PressQ();

        Assert.That(_clueBook.activeSelf, Is.False);
    }

    private IEnumerator LoadDemoScene()
    {
        _keyboard = InputSystem.AddDevice<Keyboard>();

#if UNITY_EDITOR
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#endif
        yield return null;

        GameObject startButton = GameObject.Find(StartButtonName);
        Assert.That(startButton != null, Is.True, "No Start button found.");
        startButton.GetComponent<Button>().onClick.Invoke();
        yield return null;

        playerController controller = Object.FindAnyObjectByType<playerController>();
        Assert.That(controller != null, Is.True, "No playerController found.");

        _playerInput = controller.GetComponent<PlayerInput>();
        InputUser.PerformPairingWithDevice(_keyboard, _playerInput.user);

        _clueBook = FindIncludingInactive(ClueBookName);
        Assert.That(_clueBook != null, Is.True, $"No object named '{ClueBookName}' found.");
    }

    private IEnumerator PressQ()
    {
        Press(_keyboard.qKey);
        yield return null;
        Assert.That(_playerInput.actions["Clues"].IsPressed(), Is.True, "The Clues action did not receive Q.");

        Release(_keyboard.qKey);
        yield return null;
    }

    private static GameObject FindIncludingInactive(string objectName)
    {
        foreach (Transform transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
        {
            if (transform.name == objectName)
            {
                return transform.gameObject;
            }
        }
        return null;
    }
}