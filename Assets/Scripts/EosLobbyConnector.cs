using System;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Lobby;
using PlayEveryWare.EpicOnlineServices;
using PurrNet;
using PurrNet.EOSTransport;
using UnityEngine;

public sealed class EosLobbyConnector : MonoBehaviour
{
    public static EosLobbyConnector Instance { get; private set; }

    [Header("References")]
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private EOSTransport eosTransport;

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Lobby Settings")]
    [SerializeField] private string bucketId = "WoolDidIt_v1.0";
    [SerializeField] private uint maxPlayers = 10;

    public ProductUserId LocalUserId { get; private set; }
    public string CurrentLobbyId { get; private set; }

    private bool _isHost;
    private bool _isLoggingIn;

    private void Awake()
    {
        // Keep a single persistent instance across scenes so Start() never wipes your PUID
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureTransportAssigned();
    }

    private void Start()
    {
        // Never re-login if we already have a valid PUID or login is in progress
        if ((LocalUserId != null && LocalUserId.IsValid()) || _isLoggingIn)
            return;

        LoginWithDeviceId();
    }

    private void EnsureTransportAssigned()
    {
        if (networkManager == null)
            networkManager = NetworkManager.main;

        if (eosTransport == null && networkManager != null)
            eosTransport = networkManager.GetComponent<EOSTransport>();

        if (networkManager != null && eosTransport != null && networkManager.transport == null)
            networkManager.transport = eosTransport;
    }

    // 1. Account-free login (fresh PUID once per session in Editor for MPPM; persistent in builds)
    public void LoginWithDeviceId()
    {
        if ((LocalUserId != null && LocalUserId.IsValid()) || _isLoggingIn)
            return;

        _isLoggingIn = true;
        var connect = EOSManager.Instance.GetEOSPlatformInterface().GetConnectInterface();

        var loginOptions = new LoginOptions
        {
            Credentials = new Credentials
            {
                Type = ExternalCredentialType.DeviceidAccessToken,
                Token = null
            },
            UserLoginInfo = new UserLoginInfo
            {
                DisplayName = $"Player_{UnityEngine.Random.Range(100, 999)}"
            }
        };

        #if UNITY_EDITOR
            var deleteOptions = new DeleteDeviceIdOptions();
            connect.DeleteDeviceId(ref deleteOptions, null, (ref DeleteDeviceIdCallbackInfo deleteInfo) =>
            {
                CreateAndLoginDeviceId(connect, loginOptions);
            });
        #else
            connect.Login(ref loginOptions, null, (ref LoginCallbackInfo loginInfo) =>
            {
                if (loginInfo.ResultCode == Result.Success)
                {
                    _isLoggingIn = false;
                    LocalUserId = loginInfo.LocalUserId;
                    Debug.Log($"[EOS] Logged in! PUID: {LocalUserId}");
                }
                else if (loginInfo.ResultCode == Result.NotFound)
                {
                    CreateAndLoginDeviceId(connect, loginOptions);
                }
                else
                {
                    _isLoggingIn = false;
                    Debug.LogError($"[EOS] Login failed: {loginInfo.ResultCode}");
                }
            });
        #endif
    }

    private void CreateAndLoginDeviceId(ConnectInterface connect, LoginOptions loginOptions)
    {
        var createOptions = new CreateDeviceIdOptions
        {
            DeviceModel = SystemInfo.deviceModel
        };

        connect.CreateDeviceId(ref createOptions, null, (ref CreateDeviceIdCallbackInfo createInfo) =>
        {
            if (createInfo.ResultCode == Result.Success)
            {
                connect.Login(ref loginOptions, null, (ref LoginCallbackInfo retryInfo) =>
                {
                    _isLoggingIn = false;
                    if (retryInfo.ResultCode == Result.Success)
                    {
                        LocalUserId = retryInfo.LocalUserId;
                        Debug.Log($"[EOS] Device ID created & logged in! PUID: {LocalUserId}");
                    }
                    else
                    {
                        Debug.LogError($"[EOS] Retry login failed: {retryInfo.ResultCode}");
                    }
                });
            }
            else
            {
                _isLoggingIn = false;
                Debug.LogError($"[EOS] CreateDeviceId failed: {createInfo.ResultCode}");
            }
        });
    }

    // 2. Host creates an EOS Lobby, starts PurrNet Host, and loads the game scene
    public void CreateLobbyAndHost()
    {
        Debug.Log("[UI] Host button clicked!");
        if (LocalUserId == null || !LocalUserId.IsValid()) return;

        EnsureTransportAssigned();

        var lobbyInterface = EOSManager.Instance.GetEOSPlatformInterface().GetLobbyInterface();
        var options = new CreateLobbyOptions
        {
            LocalUserId = LocalUserId,
            MaxLobbyMembers = maxPlayers,
            PermissionLevel = LobbyPermissionLevel.Publicadvertised,
            BucketId = bucketId,
            PresenceEnabled = false
        };

        lobbyInterface.CreateLobby(ref options, null, (ref CreateLobbyCallbackInfo info) =>
        {
            if (info.ResultCode != Result.Success)
            {
                Debug.LogError($"[EOS] CreateLobby failed: {info.ResultCode}");
                return;
            }

            CurrentLobbyId = info.LobbyId;
            _isHost = true;
            Debug.Log($"[EOS] Lobby created: {CurrentLobbyId}");

            EnsureTransportAssigned();
            networkManager.StartHost();
            networkManager.sceneModule.LoadSceneAsync(gameSceneName);
        });
    }

    // 3. Client joins lobby, connects to Host PUID, and automatically syncs to the Host's scene
    public void QuickJoinLobbyAndConnect()
    {
        if (LocalUserId == null || !LocalUserId.IsValid()) return;

        EnsureTransportAssigned();

        var lobbyInterface = EOSManager.Instance.GetEOSPlatformInterface().GetLobbyInterface();
        var searchOptions = new CreateLobbySearchOptions { MaxResults = 10 };

        if (lobbyInterface.CreateLobbySearch(ref searchOptions, out LobbySearch searchHandle) != Result.Success) return;

        var paramOptions = new LobbySearchSetParameterOptions
        {
            ComparisonOp = ComparisonOp.Equal,
            Parameter = new AttributeData
            {
                Key = "bucket",
                Value = new AttributeDataValue { AsUtf8 = bucketId }
            }
        };
        searchHandle.SetParameter(ref paramOptions);

        var findOptions = new LobbySearchFindOptions { LocalUserId = LocalUserId };
        searchHandle.Find(ref findOptions, null, (ref LobbySearchFindCallbackInfo findInfo) =>
        {
            if (findInfo.ResultCode != Result.Success)
            {
                Debug.LogWarning($"[EOS] No lobbies found: {findInfo.ResultCode}");
                searchHandle.Release();
                return;
            }

            var countOptions = new LobbySearchGetSearchResultCountOptions();
            uint count = searchHandle.GetSearchResultCount(ref countOptions);

            if (count == 0)
            {
                Debug.LogWarning("[EOS] Zero lobbies available.");
                searchHandle.Release();
                return;
            }

            var copyOptions = new LobbySearchCopySearchResultByIndexOptions { LobbyIndex = 0 };
            if (searchHandle.CopySearchResultByIndex(ref copyOptions, out LobbyDetails details) == Result.Success)
            {
                var joinOptions = new JoinLobbyOptions
                {
                    LobbyDetailsHandle = details,
                    LocalUserId = LocalUserId,
                    PresenceEnabled = false
                };

                lobbyInterface.JoinLobby(ref joinOptions, null, (ref JoinLobbyCallbackInfo joinInfo) =>
                {
                    if (joinInfo.ResultCode == Result.Success)
                    {
                        CurrentLobbyId = joinInfo.LobbyId;
                        _isHost = false;

                        var ownerOptions = new LobbyDetailsGetLobbyOwnerOptions();
                        ProductUserId hostPuid = details.GetLobbyOwner(ref ownerOptions);

                        EnsureTransportAssigned();
                        eosTransport.remoteProductUserId = hostPuid.ToString();

                        Debug.Log($"[EOS] Joined lobby! Connecting to Host PUID: {hostPuid}");
                        networkManager.StartClient();
                    }
                    details.Release();
                });
            }
            searchHandle.Release();
        });
    }

    // 4. Clean up lobby when closing the game or exiting Play Mode
    private void OnApplicationQuit()
    {
        if (string.IsNullOrEmpty(CurrentLobbyId) || LocalUserId == null || !LocalUserId.IsValid())
            return;

        var lobbyInterface = EOSManager.Instance.GetEOSPlatformInterface()?.GetLobbyInterface();
        if (lobbyInterface == null) return;

        if (_isHost)
        {
            var destroyOptions = new DestroyLobbyOptions
            {
                LocalUserId = LocalUserId,
                LobbyId = CurrentLobbyId
            };
            lobbyInterface.DestroyLobby(ref destroyOptions, null, null);
        }
        else
        {
            var leaveOptions = new LeaveLobbyOptions
            {
                LocalUserId = LocalUserId,
                LobbyId = CurrentLobbyId
            };
            lobbyInterface.LeaveLobby(ref leaveOptions, null, null);
        }
    }
}
