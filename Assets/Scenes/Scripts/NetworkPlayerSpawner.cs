using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Serialization;

public class NetworkPlayerSpawner : MonoBehaviourPunCallbacks
{
    [Tooltip("Root of the player's Oculus Integration OVR Rig.")] [SerializeField] 
    private GameObject oculusRig;
    
    [Tooltip("Meta Avatar prefab located in the Resources folder.")] [SerializeField] 
    private GameObject avatarPrefab;
    
    [Tooltip("The player's initial \"loading\" location before the spawn point is determined.")] [SerializeField] 
    private Transform startTransform;

    [Tooltip("The rate that the player travels from the start to spawn point.")] [Range(0.001f, 1.0f)] [SerializeField] 
    private float loadingSpeed;
    
    [Tooltip("TESTING")] [SerializeField] 
    private SpawnPointManager spawnPointManager;

    // [Tooltip("Whether or not players should have strictly unique spawn points.")] [SerializeField]
    // private bool reusableSpawns;
    //
    // [Tooltip("Ordered from first to last, the spawn points for the player.")] [SerializeField] 
    // private List<Vector3> playerSpawnPoints;
    //
    // // Reference to a NetworkManager instance that should be attached to the same Game Object
    // private NetworkManager _networkManager;
    
    // Reference to the player's avatar once it's spawned in.
    private GameObject _spawnedPlayerAvatar;
    
    // // Stack used to keep track of used and available player spawn points, derived from playerSpawnPoints
    // public Stack<Vector3> availableSpawnStack;
    //
    // // This player's determined spawn transform
    // private Vector3 _spawnPoint;

    private void Awake()
    {
        // Sets the player rig to the desired starting orientation
        oculusRig.transform.position = startTransform.position;
        oculusRig.transform.rotation = startTransform.rotation;
    }
 /*   
    #region Spawn List Handling
    
    // Sets the spawn list when the player is going to be the first person in the room
    public void PrepareSpawnListForNetwork()
    {
        // If no spawns were added or list is null, creates a new spawn list of one spawn point at origin
        if (!playerSpawnPoints.Any())
        {
            Debug.LogError("No spawn points added. Spawns will default to origin position and rotation.");
            playerSpawnPoints = new List<Vector3> { Vector3.zero };
        }
        
        // Checks if we need to have at least as many spawn points as possible players
        if (!reusableSpawns) PopulateSpawnList();
        
        // Transforms our list into a stack
        GenerateSpawnStack();
    }
    
    // Called if spawn points are not reusable. Adds to the list until the number of spawn points equals max players
    private void PopulateSpawnList()
    {
        // Logs a warning if there are too many spawn points but doesn't reduce the list
        if (playerSpawnPoints.Count > _networkManager.maxPlayersPerRoom)
        {
            Debug.LogWarning("Set spawn points exceeds max player count. Not all spawn points will be used.");
        }
        
        // If the list is underpopulated, adds new spawn points based off an arbitrary offset of the last position.
        // Continues until the list matches the max player count.
        var lastListedSpawnPoint = playerSpawnPoints[^1];
        var offset = new Vector3(0, 0, 1);
        
        while (playerSpawnPoints.Count < _networkManager.maxPlayersPerRoom)
        {
            Debug.LogWarning("Not enough spawn points to account for all players. Generating a new position.");

            var newSpawnPoint = lastListedSpawnPoint + offset;
            
            playerSpawnPoints.Add(newSpawnPoint);
            lastListedSpawnPoint = newSpawnPoint;
        }
    }
    
    // Populates the spawn stack using a reversed copy of the original spawn list, not modifying the list
    private void GenerateSpawnStack()
    {
        availableSpawnStack = new Stack<Vector3>(Enumerable.Reverse(playerSpawnPoints));
    }
    
    #endregion
*/
 
    // Starts the player's spawn. Called by the attached NetworkManager class once the player has joined a room.
    public void BeginSpawn()
    {
        var spawnPoint = spawnPointManager.GetFirstAvailableSpawnPoint();
        StartCoroutine(LerpToSpawn(spawnPoint));
    }
 /*   
    // Determines the target spawn transform for the player.
    private void DecideSpawnPoint()
    {
        // Resets the selection cycle of spawn targets if spawns are reusable and we've already gone through them all
        if (reusableSpawns && availableSpawnStack.Count == 0)
        {
            GenerateSpawnStack();
        }
        
        // Sets the player's spawn target and removes it from the available list
        _spawnPoint = availableSpawnStack.Pop();
    }
*/
    // Moves the player towards the previously determined player spawn transform
    private IEnumerator LerpToSpawn(Vector3 spawnPoint)
    {
        // The player's original position and orientation
        Vector3 startPosition = startTransform.position;
        Quaternion startRotation = startTransform.rotation;

        // The player's targeted position and orientation
        Vector3 endPosition = spawnPoint;
        Quaternion endRotation = Quaternion.identity;
        
        // Lerps the player to spawn based on the serialized loadingSpeed variable. 
        var progress = 0f;
        while (progress < 1f)
        {
            oculusRig.transform.position = Vector3.Lerp(startPosition, endPosition, progress);
            oculusRig.transform.rotation = Quaternion.Lerp(startRotation, endRotation, progress);
            progress += loadingSpeed; 
            yield return null;
        }
        
        // Begins the actual spawning now that the extra effects are complete
        SpawnPlayer(spawnPoint);
    }
    
    // Creates the player's Meta Avatar and integrates it with the OVR Rig
    private void SpawnPlayer(Vector3 spawnPoint)
    {
        // Ensure that the player is oriented at exactly the targeted spawn
        oculusRig.transform.position = spawnPoint;
        oculusRig.transform.rotation = Quaternion.identity;
        
        // Receives the Meta Avatar skin information since this is guaranteed to be the local instance of the avatar
        object[] idObjects = {Convert.ToInt64(OculusManager.Instance.userID)};
        
        // Instantiates the player's Avatar across the network
        _spawnedPlayerAvatar = PhotonNetwork.Instantiate(avatarPrefab.name, 
            spawnPoint,
            Quaternion.identity,
            0,
            idObjects);
        
        // Makes the Meta Avatar move with the OVR Camera Rig of the player
        _spawnedPlayerAvatar.transform.parent = oculusRig.transform.GetChild(0);
    }
    
    // Removes the player avatar across the network
    public void DespawnPlayer()
    {
        spawnPointManager.AddBackSpawnPoint();
        //if (!reusableSpawns) availableSpawnStack.Push(_spawnPoint);
        
        PhotonNetwork.Destroy(_spawnedPlayerAvatar);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        DespawnPlayer();
        PhotonNetwork.Destroy(_spawnedPlayerAvatar);
    }
}
