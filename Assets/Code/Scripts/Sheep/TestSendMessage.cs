
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMessage))]
public class TestSendMessage : MonoBehaviour
{
    [Header("Random Messages")]
    [SerializeField]
    private string[] messages =
    {
        "Hello everyone!",
        "Nice to meet you!",
        "Help me!",
        "Follow me!",
        "Wait for me!",
        "Good job!",
        "Haha!",
        "What are you doing?",
        "Let's go!",
        "I'm here!"
    };

    private PlayerMessage _playerMessage;

    private void Awake()
    {
        _playerMessage = GetComponent<PlayerMessage>();
    }

    private void OnInteract() => SendRandomMessage();

    private void SendRandomMessage()
    {
        if (messages == null || messages.Length == 0 || _playerMessage == null) return;

        int randomIndex = Random.Range(0, messages.Length);
        _playerMessage.ShowMessage(messages[randomIndex]);
    }
}
