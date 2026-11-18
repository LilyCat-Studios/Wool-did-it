using System.Collections.Generic;
using UnityEngine;

public class LobbyBrowser : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;
    [SerializeField] private LobbyEntryUI lobbyPrefab;

    private readonly List<GameObject> spawnedEntries =
        new();

    private void Start()
    {
        EOSLobbyManager.Instance.SetLobbyBrowser(this);
    }

    public void ClearList()
    {
        foreach (GameObject obj in spawnedEntries)
        {
            Destroy(obj);
        }

        spawnedEntries.Clear();
    }

    public void AddLobby(
        string lobbyId,
        string lobbyName,
        string roomCode,
        int currentPlayers,
        int maxPlayers)
    {
        LobbyEntryUI entry =
            Instantiate(lobbyPrefab, contentRoot);

        entry.Setup(
            lobbyId,
            lobbyName,
            roomCode,
            currentPlayers,
            maxPlayers);

        spawnedEntries.Add(entry.gameObject);
    }
}