using System.Collections;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Lobby;
using PlayEveryWare.EpicOnlineServices;
using PurrNet;
using PurrNet.EOSTransport;
using PurrNet.Transports;
using UnityEngine;
using Attribute = Epic.OnlineServices.Lobby.Attribute;

public class EOSLobbyManager : MonoBehaviour
{
    public static EOSLobbyManager Instance { get; private set; }

    private const string BucketId = "WoolDidItBucket_v0.0.1";
    private const string GameTag = "WOOLDIDIT";
    private const string RoomCodeChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int RoomCodeLength = 6;
    private const uint MaxPlayers = 8;

    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("References")]
    [SerializeField] private LobbyBrowser lobbyBrowser;
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private EOSTransport eosTransport;

    private ProductUserId localUserId;
    private string currentLobbyId;
    private bool isHost;
    private bool deviceIdRetried;

    public bool IsLoggedIn => localUserId != null && localUserId.IsValid();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        ResolveReferences();
    }

    private IEnumerator Start()
    {
        while (EOSManager.Instance == null || EOSManager.Instance.GetEOSPlatformInterface() == null)
            yield return null;

        CreateDeviceId(EOSManager.Instance.GetEOSConnectInterface());
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        // Clean up if this object is destroyed mid-connection
        if (networkManager != null)
        {
            networkManager.onServerConnectionState -= OnServerConnectionState;
        }
    }

    private void OnApplicationQuit()
    {
        LeaveCurrentLobby();
    }

    public void SetLobbyBrowser(LobbyBrowser browser)
    {
        lobbyBrowser = browser;
        if (IsLoggedIn)
            RefreshLobbies();
    }

    private void ResolveReferences()
    {
        if (networkManager == null)
            networkManager = FindFirstObjectByType<NetworkManager>();

        if (eosTransport == null)
            eosTransport = FindFirstObjectByType<EOSTransport>();
    }

    #region Login

    private void CreateDeviceId(ConnectInterface connect)
    {
        CreateDeviceIdOptions options = new() { DeviceModel = SystemInfo.deviceModel };

        connect.CreateDeviceId(ref options, null, (ref CreateDeviceIdCallbackInfo info) =>
        {
            if (info.ResultCode is Result.Success or Result.DuplicateNotAllowed)
                ConnectLogin(connect);
            else
                Debug.LogError($"[EOS] CreateDeviceId failed: {info.ResultCode}");
        });
    }

    private void ConnectLogin(ConnectInterface connect)
    {
        LoginOptions options = new()
        {
            Credentials = new Credentials
            {
                Type = ExternalCredentialType.DeviceidAccessToken,
                Token = null
            },
            UserLoginInfo = new UserLoginInfo
            {
                DisplayName = $"Player_{Random.Range(1000, 9999)}"
            }
        };

        connect.Login(ref options, null, (ref LoginCallbackInfo info) =>
        {
            switch (info.ResultCode)
            {
                case Result.Success:
                    OnLoggedIn(info.LocalUserId);
                    break;

                case Result.InvalidUser when info.ContinuanceToken != null:
                    CreateUser(connect, info.ContinuanceToken);
                    break;

                case Result.NotFound when !deviceIdRetried:
                    ResetDeviceId(connect);
                    break;

                default:
                    Debug.LogError($"[EOS] Connect Login failed: {info.ResultCode}");
                    break;
            }
        });
    }

    private void ResetDeviceId(ConnectInterface connect)
    {
        deviceIdRetried = true;
        Debug.LogWarning("[EOS] Device ID not found. Recreating it...");

        DeleteDeviceIdOptions options = new();
        connect.DeleteDeviceId(ref options, null, (ref DeleteDeviceIdCallbackInfo _) => CreateDeviceId(connect));
    }

    private void CreateUser(ConnectInterface connect, ContinuanceToken token)
    {
        CreateUserOptions options = new() { ContinuanceToken = token };

        connect.CreateUser(ref options, null, (ref CreateUserCallbackInfo info) =>
        {
            if (info.ResultCode == Result.Success)
                OnLoggedIn(info.LocalUserId);
            else
                Debug.LogError($"[EOS] CreateUser failed: {info.ResultCode}");
        });
    }

    private void OnLoggedIn(ProductUserId userId)
    {
        localUserId = userId;
        Debug.Log($"[EOS] Logged in: {localUserId}");
        RefreshLobbies();
    }

    #endregion

    #region Host

    public void CreateDefaultLobby()
    {
        if (!IsLoggedIn)
        {
            Debug.LogWarning("[EOS] Not logged in yet.");
            return;
        }

        ResolveReferences();

        if (networkManager == null)
        {
            Debug.LogError("[EOS] NetworkManager reference is missing.");
            return;
        }

        string roomCode = GenerateRoomCode();
        LobbyInterface lobby = EOSManager.Instance.GetEOSLobbyInterface();

        CreateLobbyOptions options = new()
        {
            LocalUserId = localUserId,
            MaxLobbyMembers = MaxPlayers,
            PermissionLevel = LobbyPermissionLevel.Publicadvertised,
            PresenceEnabled = false,
            BucketId = BucketId,
            AllowInvites = true
        };

        lobby.CreateLobby(ref options, null, (ref CreateLobbyCallbackInfo info) =>
        {
            if (info.ResultCode != Result.Success)
            {
                Debug.LogError($"[EOS] CreateLobby failed: {info.ResultCode}");
                return;
            }

            currentLobbyId = info.LobbyId;
            isHost = true;
            SetLobbyAttributes(lobby, info.LobbyId, "Wool Did It Room", roomCode);
        });
    }

    private void SetLobbyAttributes(LobbyInterface lobby, string lobbyId, string lobbyName, string roomCode)
    {
        UpdateLobbyModificationOptions modOptions = new()
        {
            LobbyId = lobbyId,
            LocalUserId = localUserId
        };

        if (lobby.UpdateLobbyModification(ref modOptions, out LobbyModification mod) != Result.Success)
            return;

        AddAttribute(mod, "HOST_PUID", localUserId.ToString());
        AddAttribute(mod, "LOBBY_NAME", lobbyName);
        AddAttribute(mod, "ROOM_CODE", roomCode);
        AddAttribute(mod, "GAME_TAG", GameTag);

        UpdateLobbyOptions updateOptions = new() { LobbyModificationHandle = mod };

        lobby.UpdateLobby(ref updateOptions, null, (ref UpdateLobbyCallbackInfo info) =>
        {
            mod.Release();

            if (info.ResultCode != Result.Success)
            {
                Debug.LogError($"[EOS] UpdateLobby failed: {info.ResultCode}");
                return;
            }

            Debug.Log($"[EOS] Lobby ready ({roomCode}). Starting host...");
            StartHostSafe();
        });
    }

    private bool isStartingHost = false;

    private void StartHostSafe()
    {
        if (networkManager == null)
        {
            Debug.LogError("[EOS] Cannot start host: NetworkManager is null.");
            return;
        }

        if (isStartingHost || networkManager.isServer)
        {
            Debug.LogWarning("[EOS] Host is already starting or running.");
            return;
        }

        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogError("[EOS] Cannot start host: gameSceneName is empty.");
            return;
        }

        isStartingHost = true;

        // Ensure we don't accidentally double-subscribe
        networkManager.onServerConnectionState -= OnServerConnectionState;
        networkManager.onServerConnectionState += OnServerConnectionState;

        networkManager.StartHost();
    }

    private void OnServerConnectionState(ConnectionState state)
    {
        // Ignore intermediate states like 'Connecting'
        if (state == ConnectionState.Connecting)
            return;

        // Unsubscribe on ANY final state (Connected or Disconnected) to prevent leaks
        networkManager.onServerConnectionState -= OnServerConnectionState;
        isStartingHost = false;

        if (state == ConnectionState.Connected)
        {
            networkManager.sceneModule.LoadSceneAsync(gameSceneName);
        }
        else
        {
            Debug.LogError($"[EOS] Failed to start host. Connection state: {state}");
        }
    }


    private static void AddAttribute(LobbyModification mod, string key, string value)
    {
        LobbyModificationAddAttributeOptions options = new()
        {
            Attribute = new AttributeData
            {
                Key = key,
                Value = new AttributeDataValue { AsUtf8 = value }
            },
            Visibility = LobbyAttributeVisibility.Public
        };
        mod.AddAttribute(ref options);
    }

    #endregion

    #region Search & Join

    public void RefreshLobbies()
    {
        if (lobbyBrowser == null || !IsLoggedIn)
            return;

        LobbyInterface lobby = EOSManager.Instance.GetEOSLobbyInterface();
        CreateLobbySearchOptions searchOptions = new() { MaxResults = 25 };

        if (lobby.CreateLobbySearch(ref searchOptions, out LobbySearch search) != Result.Success)
            return;

        LobbySearchSetParameterOptions paramOptions = new()
        {
            ComparisonOp = ComparisonOp.Equal,
            Parameter = new AttributeData
            {
                Key = "GAME_TAG",
                Value = new AttributeDataValue { AsUtf8 = GameTag }
            }
        };
        search.SetParameter(ref paramOptions);

        LobbySearchFindOptions findOptions = new() { LocalUserId = localUserId };

        search.Find(ref findOptions, null, (ref LobbySearchFindCallbackInfo info) =>
        {
            if (info.ResultCode == Result.Success && lobbyBrowser != null)
                PopulateBrowser(search);

            search.Release();
        });
    }

    private void PopulateBrowser(LobbySearch search)
    {
        lobbyBrowser.ClearList();

        LobbySearchGetSearchResultCountOptions countOptions = new();
        uint count = search.GetSearchResultCount(ref countOptions);

        for (uint i = 0; i < count; i++)
        {
            LobbySearchCopySearchResultByIndexOptions copyOptions = new() { LobbyIndex = i };
            if (search.CopySearchResultByIndex(ref copyOptions, out LobbyDetails details) != Result.Success)
                continue;

            AddToBrowser(details);
            details.Release();
        }
    }

    private void AddToBrowser(LobbyDetails details)
    {
        LobbyDetailsCopyInfoOptions infoOptions = new();
        if (details.CopyInfo(ref infoOptions, out LobbyDetailsInfo? info) != Result.Success || !info.HasValue)
            return;

        int maxPlayers = (int)info.Value.MaxMembers;
        int currentPlayers = maxPlayers - (int)info.Value.AvailableSlots;

        lobbyBrowser.AddLobby(
            info.Value.LobbyId,
            GetAttribute(details, "LOBBY_NAME", "Unnamed Room"),
            GetAttribute(details, "ROOM_CODE", "------"),
            currentPlayers,
            maxPlayers);
    }

    public void JoinLobby(string lobbyId)
    {
        if (!IsLoggedIn)
            return;

        ResolveReferences();

        if (networkManager == null || eosTransport == null)
        {
            Debug.LogError("[EOS] NetworkManager or EOSTransport reference is missing.");
            return;
        }

        LobbyInterface lobby = EOSManager.Instance.GetEOSLobbyInterface();
        CreateLobbySearchOptions searchOptions = new() { MaxResults = 1 };

        if (lobby.CreateLobbySearch(ref searchOptions, out LobbySearch search) != Result.Success)
            return;

        LobbySearchSetLobbyIdOptions idOptions = new() { LobbyId = lobbyId };
        search.SetLobbyId(ref idOptions);

        LobbySearchFindOptions findOptions = new() { LocalUserId = localUserId };

        search.Find(ref findOptions, null, (ref LobbySearchFindCallbackInfo findInfo) =>
        {
            LobbySearchCopySearchResultByIndexOptions copyOptions = new() { LobbyIndex = 0 };

            if (findInfo.ResultCode != Result.Success ||
                search.CopySearchResultByIndex(ref copyOptions, out LobbyDetails details) != Result.Success)
            {
                search.Release();
                return;
            }

            string hostPuid = GetAttribute(details, "HOST_PUID", string.Empty);

            JoinLobbyOptions joinOptions = new()
            {
                LobbyDetailsHandle = details,
                LocalUserId = localUserId,
                PresenceEnabled = false
            };

            lobby.JoinLobby(ref joinOptions, null, (ref JoinLobbyCallbackInfo joinInfo) =>
            {
                details.Release();
                search.Release();

                if (joinInfo.ResultCode != Result.Success || string.IsNullOrEmpty(hostPuid))
                {
                    Debug.LogError($"[EOS] JoinLobby failed: {joinInfo.ResultCode}");
                    return;
                }

                currentLobbyId = lobbyId;
                isHost = false;
                eosTransport.remoteProductUserId = hostPuid;
                networkManager.StartClient();
            });
        });
    }

    #endregion

    #region Cleanup

    private void LeaveCurrentLobby()
    {
        if (string.IsNullOrEmpty(currentLobbyId) || !IsLoggedIn || EOSManager.Instance == null)
            return;

        LobbyInterface lobby = EOSManager.Instance.GetEOSLobbyInterface();
        if (lobby == null)
            return;

        if (isHost)
        {
            DestroyLobbyOptions options = new() { LobbyId = currentLobbyId, LocalUserId = localUserId };
            lobby.DestroyLobby(ref options, null, (ref DestroyLobbyCallbackInfo _) => { });
        }
        else
        {
            LeaveLobbyOptions options = new() { LobbyId = currentLobbyId, LocalUserId = localUserId };
            lobby.LeaveLobby(ref options, null, (ref LeaveLobbyCallbackInfo _) => { });
        }

        currentLobbyId = null;
    }

    #endregion

    #region Helpers

    private static string GetAttribute(LobbyDetails details, string key, string fallback)
    {
        LobbyDetailsCopyAttributeByKeyOptions options = new() { AttrKey = key };

        if (details.CopyAttributeByKey(ref options, out Attribute? attr) == Result.Success
            && attr.HasValue
            && attr.Value.Data.HasValue)
        {
            return attr.Value.Data.Value.Value.AsUtf8 ?? fallback;
        }

        return fallback;
    }

    private static string GenerateRoomCode()
    {
        char[] code = new char[RoomCodeLength];
        for (int i = 0; i < code.Length; i++)
            code[i] = RoomCodeChars[Random.Range(0, RoomCodeChars.Length)];

        return new string(code);
    }

    #endregion
}