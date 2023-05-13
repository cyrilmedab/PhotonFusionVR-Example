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
        // Can choose to automate the ownership option here
        //_view.OwnershipTransfer = OwnershipOption.Takeover; 
        
        base.Awake();
    }
    
    // Called by an attached or child Pointable Unity Event Wrapper component to get server permission on grab
    public void RequestOwnership()
    {
        _view.RequestOwnership();
    }
}
