using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Oculus.Avatar2;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

public class StreamingAvatar : OvrAvatarEntity
{
    public OculusInitializer networkCon;
    // TODO: Remove this and make it unnecessary
    //public GameObject mainCam;
    
    private PhotonView view;
    private byte[] _avatarBytes;
    private WaitForSeconds waitTime = new(0.08f);
    

    protected override void Awake()
    {
        StartLoadingAvatar();
        base.Awake();
    }

    public void StartLoadingAvatar()
    {
        // get user ID from the rig parent
        PhotonView parentView = GetComponentInParent<PhotonView>();
        object[] args = parentView.InstantiationData;
        Int64 avatarId = (Int64)args[0];
        _userId = Convert.ToUInt64(avatarId);
        
        // avatar view for streaming
        view = GetComponent<PhotonView>();
        if (view.IsMine)
        {
            SetIsLocal(true);
            _creationInfo.features = Oculus.Avatar2.CAPI.ovrAvatar2EntityFeatures.Preset_Default;
        }
        else
        {
            SetIsLocal(false);
            _creationInfo.features = Oculus.Avatar2.CAPI.ovrAvatar2EntityFeatures.Preset_Remote;
            //mainCam.SetActive(false);
        }

        SetBodyTracking(FindObjectOfType<SampleInputManager>());
        SetLipSync(FindObjectOfType<OvrAvatarLipSyncContext>());
        StartCoroutine(LoadAvatarWIthId());
    }
    
    private IEnumerator LoadAvatarWIthId()
    {
        var hasAvatarRequest = OvrAvatarManager.Instance.UserHasAvatarAsync(_userId);
        while (!hasAvatarRequest.IsCompleted) { yield return null; }
        LoadUser();
    }

    public void AvatarCreated()
    {
        if (view.IsMine) StartCoroutine(StreamAvatarData());
    }

    private IEnumerator StreamAvatarData()
    {
        _avatarBytes = RecordStreamData(activeStreamLod);
        view.RPC(nameof(RPC_PlayAvatarData), RpcTarget.Others, _avatarBytes);
        yield return waitTime;
        StartCoroutine(StreamAvatarData());
    }

    [PunRPC]
    public void RPC_PlayAvatarData(byte[] posBytes)
    {
        _avatarBytes = posBytes;
        ApplyStreamData(_avatarBytes);
    }
}
