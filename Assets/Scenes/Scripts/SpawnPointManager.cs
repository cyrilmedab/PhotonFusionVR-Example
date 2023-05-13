using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

public class SpawnPointManager : MonoBehaviourPunCallbacks
{
    [Tooltip("Whether or not players should have strictly unique spawn points.")] [SerializeField]
    private bool reusableSpawns;
    
    [Tooltip("Ordered from first used to last, the spawn points for the player.")] [SerializeField] 
    private List<Transform> allSpawnTransforms;
    // Stores the spawn positions as Vector3 positions
    private List<Vector3> allSpawnPoints = new();

    // Array used to keep track of used and available player spawn points, derived from allSpawnPoints.
    private bool[] _availableSpawnPoints;
    
    // The index of this client's determined spawn transform.
    private int _spawnIndexUsed;
    
    // The key used to reference the custom spawn property set in our NetworkManager.
    private string _key;
    
    private void Start()
    {
        // Specifically set in Start and not Awake to avoid communication errors.
        _key = NetworkManager.Instance.CustomPropertySpawn;
        
        // Converts list
        ConvertTransformsToVector3();
    }

    // All the methods that are called when the client creates a room.
    #region Local List Setup Methods
    
    // Converts the serialized transform list into Vector3 list
    private void ConvertTransformsToVector3()
    {
        foreach (Transform transform in allSpawnTransforms)
        {
            allSpawnPoints.Add(transform.position);
        }
    }
    
    // Validates and corrects the set spawn point list before the player spawns in.
    private void PrepareSpawnListForNetwork()
    {
        
        // If no spawns were added or list is null, creates a new spawn list of one spawn point at origin
        if (!allSpawnPoints.Any())
        {
            Debug.LogError("No spawn points added. Spawns will default to origin position and rotation.");
            allSpawnPoints = new List<Vector3> { Vector3.zero };
        }
        
        // Checks if we need to have at least as many spawn points as possible players
        if (!reusableSpawns) PopulateSpawnList();
        
        // Creates our Photon-compliant serializable boolean array from our validated list of spawn points
        GenerateAvailabilityArray();
    }
    
    // Called if spawn points are not reusable. Adds to the list until the number of spawn points equals max players.
    private void PopulateSpawnList()
    {
        // Logs a warning if there are too many spawn points but doesn't reduce the list
        if (allSpawnPoints.Count > NetworkManager.Instance.maxPlayersPerRoom)
        {
            Debug.LogWarning("Set spawn points exceeds max player count. Not all spawn points will be used.");
        }
        
        // If the list is underpopulated, adds new spawn points based off an arbitrary offset of the last position.
        // Continues until the list matches the max player count.
        var lastListedSpawnPoint = allSpawnPoints[^1];
        var offset = new Vector3(0, 0, 1);
        
        while (allSpawnPoints.Count < NetworkManager.Instance.maxPlayersPerRoom)
        {
            Debug.LogWarning("Not enough spawn points to account for all players. Generating a new position.");

            var newSpawnPoint = lastListedSpawnPoint + offset;
            
            allSpawnPoints.Add(newSpawnPoint);
            lastListedSpawnPoint = newSpawnPoint;
        }
    }
    
    // Creates a false-filled boolean array of the same size as our total spawn points.
    private void GenerateAvailabilityArray()
    {
        _availableSpawnPoints = Enumerable.Repeat(false, allSpawnPoints.Count).ToArray();
    }
    
    #endregion
    
    // Called when the client is the one who created the room.
    public override void OnCreatedRoom()
    {
        // Starts calling the Local List Setup Methods.
        PrepareSpawnListForNetwork();
        
        //Debug.LogError($"Before First Set: {_availableSpawnPoints.Length}");
        // The very first synchronization of our available spawn points in the room.
        SetNetworkedSpawnPoints();
        //Debug.LogError($"After First Set: {_availableSpawnPoints.Length}");
        
        base.OnCreatedRoom();
    }
    
    // Sets the current room's custom property for spawn points to this client's instance of the available points.
    private void SetNetworkedSpawnPoints()
    {
        // prepares the array for communication over the network and storage in a Photon custom property.
        var serialized = _availableSpawnPoints.Serialize();

        // Stores the available spawn points in the custom property for spawning in this room.
        NetworkManager.Instance.SetCustomProperty(_key, _availableSpawnPoints);
    }
    
    //
    //public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    //{
        
    //}

    // Sets this client's instance of available spawn points to the current room's custom property for spawn points.
    private void RetrieveNetworkedSpawnPoints()
    {
        // Gets the previously serialized data and updates our spawn availability.
        //var received = (byte[])PhotonNetwork.CurrentRoom.CustomProperties[_key];
        Debug.LogError($"In Before First Retrieval: {_availableSpawnPoints.Length}");
        _availableSpawnPoints = (bool[])PhotonNetwork.CurrentRoom.CustomProperties[_key];
        //Debug.LogError($"Before in Retrieval: {_availableSpawnPoints.Length}");
        var received = PhotonNetwork.CurrentRoom.CustomProperties[_key];
        //Debug.LogError($"After in Retrieval: {received}");
        //Debug.LogError(received.GetType());
        _availableSpawnPoints = (bool[]) received;
        //Debug.LogError($"In After First Retrieval: {_availableSpawnPoints.Length}");
            //.}Deserialize<bool[]>();
        
        //_availableSpawnStack = new Stack<Vector3>(PhotonNetwork.CurrentRoom.CustomProperties["Spawn Points"]);
        //
        //
        //
        //_availableSpawnPoints = (bool[])PhotonNetwork.CurrentRoom.CustomProperties[Property];
        /*
        Debug.LogError($"Has Key: {PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(_key)}");
        var test = (byte[])PhotonNetwork.CurrentRoom.CustomProperties[_key];
        Debug.LogError($"After in Retrieval: {test == null}");
        
        Debug.LogError(_availableSpawnPoints.Length);
        Debug.LogError($" After TestLength: {test.Length}");
        _availableSpawnPoints = (bool[])PhotonNetwork.CurrentRoom.CustomProperties[_key];
        Debug.LogError($"After TestLength: {_availableSpawnPoints.Length}");
        _availableSpawnPoints = test.Deserialize<bool[]>();
        */
    }
    
    // Retrieves and removes the earliest available spawn point from our Room Custom Property.
    public Vector3 GetFirstAvailableSpawnPoint()
    {
        // Resets the selection cycle of spawn targets if spawns are reusable and we've already gone through them all.
        //Debug.LogError($"Before First Retrieval: {_availableSpawnPoints.Length}");
        // Makes sure that our current availability list is the most up-to-date.
        RetrieveNetworkedSpawnPoints();
        //Debug.LogError($"After First Retrieval: {_availableSpawnPoints.Length}");
        //Debug.Log(_availableSpawnPoints.Length);
        
        //Debug.LogError("Starting Loop");
        // Iterates through our availability array until we find a 'false' (unused) spawn point.
        var spawnInd = 0;
        while (spawnInd < _availableSpawnPoints.Length && _availableSpawnPoints[spawnInd] )
        {
            spawnInd++;
        }
        
        // Resets the array and iterator if we've gone through the entire length without finding any availability.
        if (spawnInd == _availableSpawnPoints.Length)
        {
            GenerateAvailabilityArray();
            spawnInd = 0;
        }
        
        // Updates the availability array for all players. Buffered so that players that join after will also get it.
        //_view.RPC(nameof(RPC_UpdateSpawnAvailability), RpcTarget.OthersBuffered, _availableSpawnPoints);
        
        // Stores the information about the determined spawn point so that we know it's been taken or used.
        _spawnIndexUsed = spawnInd;
        _availableSpawnPoints[spawnInd] = true;
        
        // Updates the availability array in the room.
        SetNetworkedSpawnPoints();
        
        // Returns the corresponding spawn point from our list of all possible spawns.
        return allSpawnPoints[_spawnIndexUsed];
    }
    
    // Returns this client's spawn point to the availability stack in our Room Custom Property.
    public void AddBackSpawnPoint()
    {
        // Don't re-add this spawn point if spawn points are reusable and are cycling.
        if (reusableSpawns) return;
        
        RetrieveNetworkedSpawnPoints();
        _availableSpawnPoints[_spawnIndexUsed] = false;
        SetNetworkedSpawnPoints();
    }
}
