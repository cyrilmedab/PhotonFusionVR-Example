using System;
using System.Collections;
using Oculus.Avatar2;
using UnityEngine;
using Oculus.Platform;
using Photon.Pun;
using Photon.Realtime;


public class OculusInitializer : MonoBehaviourPunCallbacks
{
    public UInt64 userId = 0;
    public bool isServer;
    
    //[SerializeField] private StreamingAvatar _streamingAvatar;
    
    private string _sampleAvatar = "sampleAvatar";
    
    [SerializeField] private int playerNum = 0;
    [SerializeField] private int playersPerRoom = 20;
    // Need to add code to make sure this is always ^^ size, either cutting it short or adding points
    [SerializeField] private Transform[] playerSpawnPoints;
    // won't need this after validating playerSpawnPoints array
    private int _spawnInd = 0;

    public GameObject player, playerPrefab;
    public Transform StartRig;

    private void Awake()
    {
        try
        {
            Core.AsyncInitialize();
            Entitlements.IsUserEntitledToApplication().OnComplete(EntitlementCallback);
        }
        catch (UnityException e)
        {
            Debug.LogError("Platform failed to initialize due to exception.");
            Debug.LogException(e);
            UnityEngine.Application.Quit();
        }
    }
    
    // Called when the Meta Quest Platform completes the async entitlement check request and a result is available
    private void EntitlementCallback(Message msg)
    {
        if (msg.IsError) // User failed entitlement check 
        {
            // Implements a default behavior for an entitlement check failure -- Log the failure and exit the app
            Debug.LogError("You are NOT entitled to use this app.");
            UnityEngine.Application.Quit();
        }
        else // User passed entitlement check
        {
            // Log the succeeded entitlement check for debugging
            Debug.Log("You are entitled to use this app.");
            StartCoroutine(StartOvrPlatform());
        }
    }

    private IEnumerator StartOvrPlatform()
    {
        // Ensure OvrPlatform is Initialized
        if (OvrPlatformInit.status == OvrPlatformInitStatus.NotStarted)
        {
            OvrPlatformInit.InitializeOvrPlatform();
        }

        while (OvrPlatformInit.status != OvrPlatformInitStatus.Succeeded)
        {
            if (OvrPlatformInit.status == OvrPlatformInitStatus.Failed)
            {
                OvrAvatarLog.LogError($"Error initializing OvrPlatform. Falling back to local avatar", _sampleAvatar);
                //LoadLocalAvatar();
                yield break;
            }

            yield return null;
        }

        // user ID == 0 means we want to load logged in user avatar from CDN
        if (userId == 0)
        {
            // Get User ID
            Users.GetLoggedInUser().OnComplete(message =>
            {
                if (message.IsError)
                {
                    var e = message.GetError();
                    OvrAvatarLog.LogError($"Error loading CDN avatar: {e.Message}. " +
                                          "Falling back to local avatar", _sampleAvatar);
                }
                else
                {
                    userId = message.Data.ID;
                    ConnectToServer();
                    // TODO: build multiplayer login room
                    //_streamingAvatar.gameObject.SetActive(true);
                    //_streamingAvatar.StartAvatar(this);
                }
            });
        }
    }

    public void ConnectToServer()
    {
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 20;
        PhotonNetwork.AutomaticallySyncScene = true;
        
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = (byte)playersPerRoom,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.JoinOrCreateRoom("StartRoom", roomOptions, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        isServer = true;
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        // This is where you can do an if-statement to change spawn positions as new people join
        int spawnPosCount = playerSpawnPoints.Length;
        StartRig = playerSpawnPoints[_spawnInd]; //add a .transform here?
        
        playerNum++;
        _spawnInd = (_spawnInd + 1) % spawnPosCount;

        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        object[] userId0 = { Convert.ToInt64(userId) };
        player = PhotonNetwork.Instantiate(playerPrefab.name, StartRig.position, StartRig.rotation, 0, userId0);
        StartRig.gameObject.SetActive(false);
    }
}
