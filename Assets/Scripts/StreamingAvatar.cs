using System.Collections;
using System.Collections.Generic;
using Oculus.Avatar2;
using UnityEngine;
using UnityEngine.Serialization;

public class StreamingAvatar : OvrAvatarEntity
{
    public OculusInitializer oculusInitializer;
    
    public void StartAvatar(OculusInitializer player)
    {
        oculusInitializer = player;
        _userId = oculusInitializer.userId;
        StartCoroutine(LoadAvatarWIthId());
    }

    private IEnumerator LoadAvatarWIthId()
    {
        var hasAvatarRequest = OvrAvatarManager.Instance.UserHasAvatarAsync(_userId);
        while (!hasAvatarRequest.IsCompleted) { yield return null; }
        LoadUser();
    }
}
