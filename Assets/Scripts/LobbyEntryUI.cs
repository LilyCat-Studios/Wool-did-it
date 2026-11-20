// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public class LobbyEntryUI : MonoBehaviour
// {
//     [SerializeField] private TMP_Text lobbyNameText;
//     [SerializeField] private TMP_Text detailsText;
//     [SerializeField] private Button joinButton;

//     public void Setup(
//         string lobbyId,
//         string lobbyName,
//         string roomCode,
//         int currentPlayers,
//         int maxPlayers)
//     {
//         lobbyNameText.text = lobbyName;

//         detailsText.text =
//             $"Code: {roomCode} | {currentPlayers}/{maxPlayers}";

//         joinButton.onClick.RemoveAllListeners();

//         joinButton.onClick.AddListener(() =>
//         {
//             EOSLobbyManager.Instance.JoinLobby(lobbyId);
//         });
//     }
// }