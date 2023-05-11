using System;
using System.Collections;
using UnityEngine;
using Photon.Pun;

public class NetworkPlayerSpawner : MonoBehaviourPunCallbacks
{
    // Player's owned rig for their scene
    [SerializeField] private GameObject oculusRig;
    // Reference to the overhead camera object that shows the scene before the player spawns in
    [SerializeField] private GameObject startCamera;
    // Prefab to be instantiated on connection
    [SerializeField] private GameObject avatarPrefab;
    // Spawn Positions
    [SerializeField] private Transform[] playerSpawnPoints;
    
    // Player's spawned avatar in the scene
    private GameObject _spawnedPlayerAvatar;
    
    // Singleton implementation to allow for easy reference to Camera and spawn point manipulation (for the future)
    private static NetworkPlayerSpawner _networkPlayerSpawner;
    public NetworkPlayerSpawner Instance => _networkPlayerSpawner;
    
    [Tooltip("The speed at which the start camera moves to the player's spawn position")]
    [Range(0.001f, 1.0f)]
    [SerializeField] private float startCameraSpeed;
    [Tooltip("Smooth the camera transition by estimating the player height at spawn")]
    [SerializeField] private float playerHeightOffset = 1.6f;

    private void Awake()
    {
        // Basic singleton check to ensure that multiple instances of the class never occur
        if (_networkPlayerSpawner == null) _networkPlayerSpawner = this;
        else Destroy(gameObject);
    }

    // Spawning a rig object for the player when joining a room
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Transform spawnPoint = DecideSpawnPoint();
        StartCoroutine(LerpStartCamera(spawnPoint));
        //SpawnPlayer(spawnPoint);
    }

    private Transform DecideSpawnPoint()
    {
        int playerCount = PhotonNetwork.PlayerList.Length;
        int spawnInd = (playerCount - 1) % playerSpawnPoints.Length;
        return playerSpawnPoints[spawnInd];
    }

    private IEnumerator LerpStartCamera(Transform spawnPoint)
    {
        Vector3 startPosition = startCamera.transform.position;
        Quaternion startRotation = startCamera.transform.rotation;

        Vector3 endPosition = spawnPoint.position + new Vector3(0, playerHeightOffset, 0);
        Quaternion endRotation = spawnPoint.rotation;

        float progress = 0f;
        
        while (progress < 1f)
        {
            startCamera.transform.position = Vector3.Lerp(startPosition, endPosition, progress);
            startCamera.transform.rotation = Quaternion.Lerp(startRotation, endRotation, progress);
            progress += startCameraSpeed;
            yield return null;
        }

        SpawnPlayer(spawnPoint);
    }

    private void SpawnPlayer(Transform spawnPoint)
    {
        oculusRig.transform.position = spawnPoint.position;
        oculusRig.transform.rotation = spawnPoint.rotation;
        
        object[] idObjects = {Convert.ToInt64(OculusManager.Instance.userID)};
        //Debug.Log($"USER-ID SpawnPlayer for {PhotonNetwork.PlayerList.Length}: {OculusManager.Instance.userID}");
        
        oculusRig.SetActive(true);
        _spawnedPlayerAvatar = PhotonNetwork.Instantiate(avatarPrefab.name, 
            spawnPoint.position,
            spawnPoint.rotation,
            0,
            idObjects);
        
        _spawnedPlayerAvatar.transform.parent = oculusRig.transform.GetChild(0);
        //oculusRig.SetActive(true);
        //mainCamera.enabled = true;
        //_spawnedPlayerAvatar.GetComponent<PunAvatarEntity>().SetParent(oculusRig.transform.GetChild(0));

    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.Destroy(_spawnedPlayerAvatar);
    }
}
