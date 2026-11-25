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
public class PlayerMovementInputTests : InputTestFixture
{
    private const string ScenePath = "Assets/Scenes/SheepPen.unity"; 
    private const string StartButtonName = "Start";
    private const int Frames = 30;
    private const float HoldSeconds = 1f;
    private const float AllowedDeviationPercent = 10f;

    private Keyboard keyboard;
    // private Mouse mouse;
    private Transform player;
    private int originalVSyncCount;

    private IEnumerator LoadDemoScene()
    {
        keyboard = InputSystem.AddDevice<Keyboard>();

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
        Assert.That(controller.gameObject.activeInHierarchy, Is.True, "Controller game object is not active in hierarchy.");

        PlayerInput playerInput = controller.GetComponent<PlayerInput>();
        InputUser.PerformPairingWithDevice(keyboard, playerInput.user);

        player = controller.transform;
    }

    [SetUp]
    public void StoreOriginalVSyncCount()
    {
        originalVSyncCount = QualitySettings.vSyncCount;
    }

    [TearDown]
    public void DiableInputDevices()
    {
        foreach (PlayerInput playerInput in Object.FindObjectsByType<PlayerInput>())
        {
            playerInput.enabled = false;
        }

        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = originalVSyncCount;
    }

    [UnityTest]
    public IEnumerator HoldingW_MovesPlayerForward()
    {
        yield return LoadDemoScene();
        float startZ = player.position.z;
        PlayerInput playerInput = player.GetComponent<PlayerInput>();

        Press(keyboard.wKey);
        yield return null;
        for (int i = 1; i < Frames; i++)
        {
            yield return null;
        }
        Release(keyboard.wKey);

        Assert.That(player.position.z, Is.GreaterThan(startZ));
    }

    [UnityTest]
    public IEnumerator HoldingW_PlayerSpeedIsConsistentAtDifferentFrameRates()
    {
        float speed30 = 0f;
        float speed60 = 0f;

        yield return MeasureSpeed(30, result => speed30 = result);
        yield return MeasureSpeed(60, result => speed60 = result);

        Debug.Log($"Speed at 30 FPS: {speed30}, Speed at 60 FPS: {speed60}");
        Assert.That(speed30, Is.GreaterThan(0f), "Player did not move at 30 FPS.");
        Assert.That(speed30, Is.EqualTo(speed60).Within(AllowedDeviationPercent), "Player speed is not consistent at different frame rates.");
    }

    [UnityTest]
    public IEnumerator NoInput_PlayerStaysInPlace()
    {
        yield return LoadDemoScene();
        Vector3 start = player.position;

        for (int i = 0; i < Frames; i++)
        {
            yield return null;
        }

        Assert.That(player.position.x, Is.EqualTo(start.x).Within(0.01f));
        Assert.That(player.position.z, Is.EqualTo(start.z).Within(0.01f));
    }

    private IEnumerator MeasureSpeed(int frameRate, System.Action<float> result)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = frameRate;
        yield return LoadDemoScene();

        Vector3 start = player.position;
        float startTime = Time.time;

        Press(keyboard.wKey);
        while (Time.time - startTime < HoldSeconds)
        {
            yield return null;
        }
        Release(keyboard.wKey);

        float elapsed = Time.time - startTime;
        Vector3 end = player.position;
        float distance = Vector2.Distance(new Vector2(start.x, start.z), new Vector2(end.x, end.z));
        result(distance / elapsed);
    }
}