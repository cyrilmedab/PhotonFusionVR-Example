using System;
using UnityEngine;
using Photon.Pun;

public class NetworkPlayerSpawner : MonoBehaviourPunCallbacks
{
    // Player's owned rig for their scene
    [SerializeField] private GameObject oculusRig;
    // Prefab to be instantiated on connection
    [SerializeField] private GameObject avatarPrefab;
    // Spawn Positions
    [SerializeField] private Transform[] playerSpawnPoints;
    
    // Player's spawned avatar in the scene
    private GameObject _spawnedPlayerAvatar;

    // Spawning a rig object for the player when joining a room
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Transform spawnPoint = DecideSpawnPoint();
        SpawnPlayer(spawnPoint);
    }

    private Transform DecideSpawnPoint()
    {
        int playerCount = PhotonNetwork.PlayerList.Length;
        int spawnInd = (playerCount - 1) % playerSpawnPoints.Length;
        return playerSpawnPoints[spawnInd];
    }

    private void SpawnPlayer(Transform spawnPoint)
    {
        oculusRig.transform.position = spawnPoint.position;
        oculusRig.transform.rotation = spawnPoint.rotation;
        
        object[] idObjects = {Convert.ToInt64(OculusManager.Instance.userID)};
        //Debug.Log($"USER-ID SpawnPlayer for {PhotonNetwork.PlayerList.Length}: {OculusManager.Instance.userID}");
        
        _spawnedPlayerAvatar = PhotonNetwork.Instantiate(avatarPrefab.name, 
            spawnPoint.position,
            spawnPoint.rotation,
            0,
            idObjects);
        
        _spawnedPlayerAvatar.transform.parent = oculusRig.transform.GetChild(0);
        //_spawnedPlayerAvatar.GetComponent<PunAvatarEntity>().SetParent(oculusRig.transform.GetChild(0));

    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.Destroy(_spawnedPlayerAvatar);
    }
}
