using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

[Category("Player Message")]
public class PlayerMessageTests
{
    private const string _ScenePath = "Assets/Level/Scenes/SheepExampleScene.unity";

    private PlayerMessage _playerMessage;
    private Canvas _messageCanvas;

    private IEnumerator Load()
    {
#if UNITY_EDITOR
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#endif
        yield return null;

        // Wait for the local player's PlayerMessage component to spawn
        float timeout = Time.realtimeSinceStartup + 5f;
        while (_playerMessage == null && Time.realtimeSinceStartup < timeout)
        {
            foreach (var pm in Object.FindObjectsByType<PlayerMessage>(FindObjectsSortMode.None))
            {
                if (pm.isOwner)
                {
                    _playerMessage = pm;
                    break;
                }
            }
            yield return null;
        }

        Assert.That(_playerMessage, Is.Not.Null, "PlayerMessage never spawned.");
        Assert.That(_playerMessage.isOwner, Is.True, "Local client does not own PlayerMessage.");
        Assert.That(_playerMessage.canvasGroup, Is.Not.Null, "CanvasGroup reference is missing on prefab.");
        Assert.That(_playerMessage.messageText, Is.Not.Null, "TMP_Text reference is missing on prefab.");

        _messageCanvas = _playerMessage.canvasGroup.GetComponentInParent<Canvas>();
        Assert.That(_messageCanvas, Is.Not.Null, "Parent Canvas is missing on prefab.");
    }

    [UnityTest]
    public IEnumerator InitialState_StartsHidden()
    {
        yield return Load();

        Assert.That(_playerMessage.canvasGroup.alpha, Is.EqualTo(0f));
        Assert.That(_messageCanvas.enabled, Is.False);
    }

    [UnityTest]
    public IEnumerator ShowMessage_ValidText_PopInHoldAndPopOutWorks()
    {
        yield return Load();

        _playerMessage.ShowMessage("Hello world!");

        // Wait for ServerRpc -> TargetRpc roundtrip
        yield return new WaitUntil(
            () => _messageCanvas.enabled && _playerMessage.messageText.text == "Hello world!",
            System.TimeSpan.FromSeconds(2),
            () => Assert.Fail("Message never appeared on canvas after calling ShowMessage.")
        );

        // Wait for pop-in to reach full alpha
        yield return new WaitUntil(
            () => Mathf.Approximately(_playerMessage.canvasGroup.alpha, 1f),
            System.TimeSpan.FromSeconds(_playerMessage.popInDuration + 1f),
            () => Assert.Fail($"Alpha never reached 1 after pop-in (was {_playerMessage.canvasGroup.alpha}).")
        );

        // Wait for hold duration + pop-out to complete and disable the canvas
        float totalRemaining = _playerMessage.messageDuration + _playerMessage.popOutDuration;
        yield return new WaitUntil(
            () => !_messageCanvas.enabled,
            System.TimeSpan.FromSeconds(totalRemaining + 1f),
            () => Assert.Fail($"Canvas did not disable after pop-out (alpha was {_playerMessage.canvasGroup.alpha}).")
        );

        Assert.That(_playerMessage.canvasGroup.alpha, Is.EqualTo(0f));
    }


    [UnityTest]
    public IEnumerator ShowMessage_WhitespaceOrEmpty_IsIgnored()
    {
        yield return Load();

        _playerMessage.ShowMessage("   ");
        yield return new WaitForSeconds(0.2f);

        Assert.That(_messageCanvas.enabled, Is.False, "Canvas should not enable for whitespace-only messages.");
        Assert.That(_playerMessage.canvasGroup.alpha, Is.EqualTo(0f));
    }

    [UnityTest]
    public IEnumerator ShowMessage_TrimsWhitespaceAndTruncatesToMaxLength()
    {
        yield return Load();

        // Send a message with leading/trailing spaces and 150 characters
        string longText = "   " + new string('A', 150) + "   ";
        _playerMessage.ShowMessage(longText);

        yield return new WaitUntil(
            () => _messageCanvas.enabled,
            System.TimeSpan.FromSeconds(2),
            () => Assert.Fail("Truncated message never appeared.")
        );

        // ServerRpc should trim spaces and truncate to maxCharacterLength (120 by default)
        Assert.That(_playerMessage.messageText.text.Length, Is.EqualTo(120));
        Assert.That(_playerMessage.messageText.text, Is.EqualTo(new string('A', 120)));
    }
}
