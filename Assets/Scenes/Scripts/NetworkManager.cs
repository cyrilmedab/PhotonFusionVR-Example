using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(NetworkPlayerSpawner))]
public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Tooltip("The maximum limit to the number of people per room.")] [Range(1, 20)]
    public byte maxPlayersPerRoom = 20;
    
    [Tooltip("Defines if the room is listed in the lobby.")] [SerializeField]
    private bool isVisible = true;
    
    [Tooltip("Whether the room can currently be joined.")] [SerializeField]
    private bool isOpen = true;

    [SerializeField] private SpawnPointManager _spawnPointManager;
    public readonly string CustomPropertySpawn = "Spawn Points";
    
    private NetworkPlayerSpawner _networkPlayerSpawner;
    
    // Singleton to allow easy access if choosing to place different network components on separate Game Objects.
    private static NetworkManager _networkManager;
    public static NetworkManager Instance => _networkManager;

    private void Awake()
    {
        // Basic singleton check to ensure that only one instance of this class exists in the client scene.
        if (_networkManager != null && _networkManager != this) Destroy(gameObject);
        else _networkManager = this;
        
        _networkPlayerSpawner = GetComponent<NetworkPlayerSpawner>();
        
        //TODO: NEEDS A SpawnPOintManager Ref???
    }
    
    // Connects the client (aka the local player or instance) to the network.
    public void ConnectToServer()
    {
        // list settings to change here.
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = maxPlayersPerRoom,
            IsVisible = isVisible,
            IsOpen = isOpen,
            BroadcastPropsChangeToAll = true
        };

        bool[] placeholder = {};
        var hash = new Hashtable { { CustomPropertySpawn, placeholder} };
        roomOptions.CustomRoomProperties = hash;
        
        PhotonNetwork.JoinOrCreateRoom("StartRoom", roomOptions, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created a new room!");
        
        // Create a new custom room 
        //_networkPlayerSpawner.PrepareSpawnListForNetwork();
        
        base.OnCreatedRoom();
    }

    // Called whenever a client enters the room
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined");
        
        // Tells the linked PlayerSpawner class to start spawning in.
        _networkPlayerSpawner.BeginSpawn();

        base.OnJoinedRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("A new player has joined your room");
        base.OnPlayerEnteredRoom(newPlayer);
    }
    
    #region Extra Functionalities
    
    // Sets a custom property of singular type T for the current room.
    // Allows for values to be shared with all clients in room regardless of when they join or leave.
    //public void SetCustomProperty<T>(string key, T value)
    public void SetCustomProperty(string key, bool[] value)
    {
        //Debug.LogWarning($"Setting Custom Property: {value[0]}");
        //Debug.LogWarning($"Setting Custom Property: {value.Length}");
        // Note that this is ExitGames.Client.Photon.Hashtable, NOT the standard Hashtable.
        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        hash[key] = value;
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        //var hash = new Hashtable { { key, value } };
        //var success = PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        //Debug.LogError($"Setting Custom Property: {value}");
        
    }
    
    /*
    // UNTESTED: Sets multiple custom properties using given arrays of keys and values.
    public void SetCustomProperties(string[] keys, object[] values)
    {
        for (int i = 0; i < keys.Length; i++)
        {
            try
            {
                SetCustomProperty(keys[i], values[i]);
            }
            catch (UnityException e)
            {
                Debug.LogError("Custom properties failed to set. Key and value arrays are differently sized.");
                Debug.LogException(e);
            }
        }
    }
    */
    #endregion
    
}
