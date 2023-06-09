using System;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Serialization;

public class A3_NetworkPlayerSpawner : MonoBehaviourPunCallbacks
{
    // Player's owned rig for their scene
    private GameObject oculusRig;
    // Prefab to be instantiated on connection
    [SerializeField] private GameObject rigPrefab;
    
    // Spawn Positions
    [SerializeField] private Transform[] playerSpawnPoints;
    
    // Player's spawned avatar in the scene
    //private GameObject _spawnedPlayerAvatar;

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
        //oculusRig.transform.position = spawnPoint.position;
        //oculusRig.transform.rotation = spawnPoint.rotation;
        
        object[] idObjects = {Convert.ToInt64(A3_OculusManager.Instance.userID)};
        oculusRig = PhotonNetwork.Instantiate(rigPrefab.name,
            spawnPoint.position,
            spawnPoint.rotation,
            0,
            idObjects);
        //_spawnedPlayerAvatar.GetComponent<PunAvatarEntity>().SetParent(oculusRig.transform.GetChild(0));

    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.Destroy(oculusRig);
    }
}
