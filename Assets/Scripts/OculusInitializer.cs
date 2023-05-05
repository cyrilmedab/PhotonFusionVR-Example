using System;
using System.Collections;
using Oculus.Avatar2;
using UnityEngine;
using Oculus.Platform;
using UnityEngine.Serialization;

public class OculusInitializer : MonoBehaviour
{
    public UInt64 userId = 0;
    
    [SerializeField] private StreamingAvatar _streamingAvatar;
    
    private string _sampleAvatar = "sampleAvatar";
    
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
    
    // Called when the Meta Quest Platform completes the async entitlement check request and a result is available
    private void EntitlementCallback(Message msg)
    {
        if (msg.IsError) // User failed entitlement check 
        {
            // Implements a default behavior for an entitlement check failure -- Log the failure and exit the app
            Debug.LogError("You are NOT entitled to use this app.");
            UnityEngine.Application.Quit();
        }
        else // User passed entitlement check
        {
            // Log the succeeded entitlement check for debugging
            Debug.Log("You are entitled to use this app.");
            StartCoroutine(StartOVRPlatform());
        }
    }

    private IEnumerator StartOVRPlatform()
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
                OvrAvatarLog.LogError($"Error initializing OvrPlatform. Falling back to local avatar", _sampleAvatar);
                //LoadLocalAvatar();
                yield break;
            }

            yield return null;
        }

        // user ID == 0 means we want to load logged in user avatar from CDN
        if (userId == 0)
        {
            // Get User ID
            Users.GetLoggedInUser().OnComplete(message =>
            {
                if (!message.IsError)
                {
                    userId = message.Data.ID;
                    // TODO: build multiplayer login room
                    _streamingAvatar.gameObject.SetActive(true);
                    _streamingAvatar.StartAvatar(this);
                }
                else
                {
                    var e = message.GetError();
                    OvrAvatarLog.LogError($"Error loading CDN avatar: {e.Message}. Falling back to local avatar", _sampleAvatar);
                }
            });
        }
    }

    
}
