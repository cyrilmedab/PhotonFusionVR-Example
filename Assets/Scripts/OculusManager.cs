using UnityEngine;
using Oculus.Platform;
using System.Collections;

public class OculusManager : MonoBehaviour
{
    // Starts the entitlement check procedures on application awake
    private void Awake()
    {
        try
        {
            Core.AsyncInitialize();
            Entitlements.IsUserEntitledToApplication().OnComplete(EntitlementCallback);
        }
        catch (UnityException e)
        {
            Debug.LogError("Platform failed to initialize due to exception.");
            Debug.LogException(e);
            UnityEngine.Application.Quit();
        }
    }
    
    // Called when the Meta Quest Platform completes the async entitlement
    // check request and a result is available
    private void EntitlementCallback(Message msg)
    {
        if (msg.IsError) // User failed entitlement check 
        {
            // Implements a default behavior for an entitlement check failure
            // Log the failure and exit the app
            Debug.LogError("You are NOT entitled to use this app.");
            UnityEngine.Application.Quit();
        }
        else // User passed entitlement check
        {
            // Log the succeeded entitlement check for debugging
            Debug.Log("You are entitled to use this app.");
            //ConnectToServer();
            StartCoroutine(StartOvrPlatform());
        }
    }
    
    private IEnumerator StartOvrPlatform()
    {
        // Ensure OvrPlatform is Initialized
        if (OvrPlatformInit.status == OvrPlatformInitStatus.NotStarted)
        {
            OvrPlatformInit.InitializeOvrPlatform();
        }

        while (OvrPlatformInit.status != OvrPlatformInitStatus.Succeeded)
        {
            if (OvrPlatformInit.status == OvrPlatformInitStatus.Failed)
            {
                Debug.Log("Error initializing OvrPlatform.");
                //OvrAvatarLog.LogError($"Error initializing OvrPlatform. Falling back to local avatar", _sampleAvatar);
                //LoadLocalAvatar();
                yield break;
            }

            yield return null;
        }
        
        GetComponent<NetworkManager>().ConnectToServer();

        // user ID == 0 means we want to load logged in user avatar from CDN
        /*if (userId == 0)
        {
            // Get User ID
            Users.GetLoggedInUser().OnComplete(message =>
            {
                if (message.IsError)
                {
                    var e = message.GetError();
                    //Debug.Log($"Error loading CDN Avatar: {e.Message}.");
                    //OvrAvatarLog.LogError($"Error loading CDN avatar: {e.Message}. " +
                                        //  "Falling back to local avatar", _sampleAvatar);
                }
                else
                {
                    userId = message.Data.ID;
                    ConnectToServer();
                    // TODO: build multiplayer login room
                    //_streamingAvatar.gameObject.SetActive(true);
                    //_streamingAvatar.StartAvatar(this);
                }
            });
        }*/
    }
}