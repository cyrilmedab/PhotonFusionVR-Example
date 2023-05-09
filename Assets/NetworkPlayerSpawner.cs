using UnityEngine;
using Photon.Pun;

public class NetworkPlayerSpawner : MonoBehaviourPunCallbacks
{
    // Player Game Object - Assigned on join
    private GameObject _spawnedPlayerRig;
    
    // Spawn Positions
    [SerializeField] private Transform[] playerSpawnPoints;
    private int _spawnInd;
    
    // Spawning a rig object for the player when joining a room
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Transform spawnPoint = DecideSpawnPoint();
        SpawnPlayer(spawnPoint);
    }

    private Transform DecideSpawnPoint()
    {
        _spawnInd = 3;
        Transform spawnPoint = playerSpawnPoints[_spawnInd];
        _spawnInd = (_spawnInd + 1) % playerSpawnPoints.Length;
        Debug.Log(_spawnInd);
        Debug.Log($"{spawnPoint.position.x} {spawnPoint.position.y} {spawnPoint.position.z}");
        return spawnPoint;
    }

    private void SpawnPlayer(Transform spawnPoint)
    {
        _spawnedPlayerRig = PhotonNetwork.Instantiate("OculusRig_Pun2",
            spawnPoint.position,
            spawnPoint.rotation);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.Destroy(_spawnedPlayerRig);
    }
}
