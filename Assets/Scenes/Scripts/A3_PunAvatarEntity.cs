using System;
using System.Collections;
using Oculus.Avatar2;
using Photon.Pun;
using UnityEngine;

public class A3_PunAvatarEntity : OvrAvatarEntity
{
    private PhotonView view;
    private PhotonView parentView;
    private byte[] _avatarBytes;
    private WaitForSeconds _waitTime = new(0.08f);

    [SerializeField] private GameObject mainCamera;
    
    protected override void Awake()
    {
        ConfigureAvatarEntity();
        base.Awake();
    }

    private void Start()
    {
        //_userId = GetUserIdFromPhotonInstantiationData();
        //StartCoroutine(LoadAvatarWIthId());
    }
/*
    public void SetParent(Transform parent)
    {
        if (view.IsMine) transform.SetParent(parent);
    }
*/
    private ulong GetUserIdFromPhotonInstantiationData()
    {
        object[] instantiationData = view.InstantiationData;
        Int64 dataInt = Convert.ToInt64(instantiationData[0]);
        return Convert.ToUInt64(dataInt);
    }

    private void ConfigureAvatarEntity()
    {
        view = GetComponent<PhotonView>();
        parentView = GetComponentInParent<PhotonView>();
        
        
        // get user ID from the rig parent
        
        parentView = GetComponentInParent<PhotonView>();
        object[] args = parentView.InstantiationData;
        Int64 avatarId = (Int64)args[0];
        _userId = Convert.ToUInt64(avatarId);
        
        
        // avatar view for streaming
        view = GetComponent<PhotonView>();
        if (view.IsMine)
        {
            SetIsLocal(true);
            _creationInfo.features = Oculus.Avatar2.CAPI.ovrAvatar2EntityFeatures.Preset_Default;

            //SampleInputManager sampleInputManager =
            //    OvrAvatarManager.Instance.gameObject.GetComponent<SampleInputManager>();
            //SetBodyTracking(sampleInputManager);
            //SetLipSync(FindObjectOfType<OvrAvatarLipSyncContext>());
        }
        else
        {
            SetIsLocal(false);
            _creationInfo.features = Oculus.Avatar2.CAPI.ovrAvatar2EntityFeatures.Preset_Remote;
            mainCamera.SetActive(false);
        }
        StartCoroutine(LoadAvatarWIthId());
    }
    
    private IEnumerator LoadAvatarWIthId()
    {
        var hasAvatarRequest = OvrAvatarManager.Instance.UserHasAvatarAsync(_userId);
        while (!hasAvatarRequest.IsCompleted) { yield return null; }
        LoadUser();
    }
    
    protected override void OnUserAvatarLoaded()
    {
        AvatarCreated();
        base.OnUserAvatarLoaded();
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
    public void RPC_PlayAvatarData(byte[] bytesToSend)
    {
        Debug.Log("CALLING THE RPC FUNCTION");
        if (bytesToSend != null)
        {
            _avatarBytes = bytesToSend;
            ApplyStreamData(_avatarBytes);
            Debug.Log("Applied the Stream Data");
        }
        else
        {
            Debug.LogError("ERROR: DID NOT APPLY STREAM DATA RPC");
        }
    }

    
}