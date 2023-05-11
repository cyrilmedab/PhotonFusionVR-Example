using System;
using System.Collections;
using Oculus.Avatar2;
using Photon.Pun;
using UnityEngine;

public class PunAvatarEntity : OvrAvatarEntity
{
    private PhotonView view;
    private byte[] _avatarBytes;
    private WaitForSeconds _waitTime = new(0.08f);
    

    protected override void Awake()
    {
        ConfigureAvatarEntity();
        Debug.Log($"USER-ID Before the Rest of Awake {gameObject.name}: {_userId}");
        base.Awake();
        Debug.Log($"USER-ID After the Rest of Awake {gameObject.name}: {_userId}");
    }

    // private void Start()
    // {
    //     _userId = GetUserIdFromParentPhotonInstantiationData();
    //     StartCoroutine(LoadAvatarWIthId());
    // }

    // public void SetParent(Transform parent)
    // {
    //     if (view.IsMine) transform.parent = parent;
    // }
    
    // get user ID from the parent view of the avatar, which for us is the toot of the rig
    private ulong GetUserIdFromParentPhotonInstantiationData()
    {
        PhotonView parentView = GetComponentInParent<PhotonView>();
        object[] instantiationData = parentView.InstantiationData;
        Int64 dataInt = Convert.ToInt64(instantiationData[0]);
        return Convert.ToUInt64(dataInt);
    }
    
    private ulong GetUserIdFromPhotonInstantiationData()
    {
        object[] instantiationData = view.InstantiationData;
        Int64 dataInt = Convert.ToInt64(instantiationData[0]);
        return Convert.ToUInt64(dataInt);
    }

    private void ConfigureAvatarEntity()
    {
        view = GetComponent<PhotonView>();
        _userId = GetUserIdFromPhotonInstantiationData();
        Debug.Log($"USER-ID from PhotonView {gameObject.name}: {_userId}");
        
        //
        // PhotonView parentView = GetComponentInParent<PhotonView>();
        // object[] args = parentView.InstantiationData;
        // Int64 avatarId = (Int64)args[0];
        // _userId = Convert.ToUInt64(avatarId);
        //
        
        // avatar view for streaming
        
        if (view.IsMine)
        {
            SetIsLocal(true);
            _creationInfo.features = Oculus.Avatar2.CAPI.ovrAvatar2EntityFeatures.Preset_Default;

            SampleInputManager sampleInputManager =
                OvrAvatarManager.Instance.gameObject.GetComponent<SampleInputManager>();
            SetBodyTracking(sampleInputManager);
            SetLipSync(FindObjectOfType<OvrAvatarLipSyncContext>());
        }
        else
        {
            SetIsLocal(false);
            _creationInfo.features = Oculus.Avatar2.CAPI.ovrAvatar2EntityFeatures.Preset_Remote;
        }
        
        StartCoroutine(LoadAvatarWIthId());
    }
    
    private IEnumerator LoadAvatarWIthId()
    {
        Debug.Log($"USER-ID Before LoadAvatar {gameObject.name}: {_userId}");
        var hasAvatarRequest = OvrAvatarManager.Instance.UserHasAvatarAsync(_userId);
        Debug.Log($"USER-ID After LoadAvatar {gameObject.name}: {_userId}");
        while (!hasAvatarRequest.IsCompleted) { yield return null; }
        LoadUser();
    }
    
    protected override void OnUserAvatarLoaded()
    {
        AvatarCreated();
    }
    
    public void AvatarCreated()
    {
        if (view.IsMine) StartCoroutine(StreamAvatarData());
    }

    private IEnumerator StreamAvatarData()
    {
        // Once this coroutine is determined to start, we want it to continue sending the data
        // Until the player leaves, for which NetworkPlayerSpawner destroys the object,
        // automatically stopping this coroutine. Therefore, we can just put "while (true)"
        while (true) 
        {
            _avatarBytes = RecordStreamData(activeStreamLod);
            view.RPC(nameof(RPC_PlayAvatarData), RpcTarget.Others, _avatarBytes);
            yield return _waitTime;
        }
    }

    [PunRPC]
    public void RPC_PlayAvatarData(byte[] newMovement)
    {
        Debug.Log($"USER-ID in the RPC (from the remote) {gameObject.name}: {_userId}");
        if (newMovement != null)
        {
            _avatarBytes = newMovement;
            ApplyStreamData(_avatarBytes);
        }
        else Debug.LogError("ERROR: DID NOT APPLY STREAM DATA RPC");
    }
}