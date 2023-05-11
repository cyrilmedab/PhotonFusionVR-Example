using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(PhotonView), typeof(PhotonRigidbodyView))]
public class PunGrabbable : Grabbable
{
    private PhotonView _view;
    
    protected override void Awake()
    {
        _view = GetComponent<PhotonView>();
        _view.OwnershipTransfer = OwnershipOption.Takeover; //could do this in the inspector, but safer to automate here
        // remove the above line if we want to have request instead, or anything like that. For us, this is fine
        
        base.Awake();
    }

    public void RequestOwnership()
    {
        _view.RequestOwnership();
    }
}
