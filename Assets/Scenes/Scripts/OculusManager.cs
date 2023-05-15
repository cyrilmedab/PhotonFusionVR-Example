using UnityEngine;
using Oculus.Platform;
using System.Collections;
using System.Threading.Tasks;

public enum AvatarLoadCapability
    {
        None,
        Preset,
        CDN
    }
    
public class OculusManager : MonoBehaviour
{
    public AvatarLoadCapability avatarLoadCapability = AvatarLoadCapability.None;
    
    // User's personal Oculus ID, will be used for their Meta Avatar.
    public ulong userID;

    // Singleton to allow easy access if choosing to place different network components on separate Game Objects.
    private static OculusManager _oculusManager;
    public static OculusManager Instance => _oculusManager;

    private async void Awake()
    {
        // Basic singleton check to ensure that only one instance of this class exists in the client scene.
        if (_oculusManager != null && _oculusManager != this) Destroy(gameObject);
        else _oculusManager = this;
        
        // TODO: Determine if this is the right place to call it
        await PrepareForMetaAvatar();
    }

    //Code is commented out to allow for easy editor testing
    /*
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
*/
    
    // Proactively checks the OVR platform and receives user information necessary for successful Avatar spawning.
    private async Task PrepareForMetaAvatar()
    {
        if (!await CheckSuccessfulOvrInitialization()) { return; }
        
        // Get User ID
        await GetUserId();
        
        // TODO: This should all finish before start starts, but this is definitely a break point, so double check
        NetworkManager.Instance.ConnectToServer();
    }

    // Ensures the OVR Platform is successfully initialized. 
    private async Task<bool> CheckSuccessfulOvrInitialization()
    { 
        // Starts the initialization if necessary.
        if (OvrPlatformInit.status == OvrPlatformInitStatus.NotStarted) { OvrPlatformInit.InitializeOvrPlatform(); }
        
        // Wait until initialization has either succeeded or failed.
        while (OvrPlatformInit.status != OvrPlatformInitStatus.Succeeded)
        {
            // If initialization has failed, log an error message and set the avatarLoadCapability to Preset.
            if (OvrPlatformInit.status == OvrPlatformInitStatus.Failed)
            {
                Debug.LogError("Error loading CDN avatar. Falling back to local avatar");
                avatarLoadCapability = AvatarLoadCapability.Preset;
                return false;
            }
            // Yield to the task scheduler to avoid blocking the main thread.
            await Task.Yield();
        }
        // Return true if initialization was successful
        return true;
    }
    
    // Retrieves and stores the user's id if possible. Returns a task for async usage
    private Task GetUserId()
    {
        // A new task completion source so that we can get the async effect
        var completed = new TaskCompletionSource<object>();
        
        // Wait and try to get the logged in user, so that we can save the user id and finalize our load limit
        Users.GetLoggedInUser().OnComplete(message =>
        {
            // If there's an error, log it and set the avatarLoadCapability to Preset, since we can't load from CDN.
            if (message.IsError)
            {
                var e = message.GetError(); 
                Debug.LogError($"Error loading CDN avatar: {e.Message}. " +
                  "Falling back to local avatar");
                avatarLoadCapability = AvatarLoadCapability.Preset;
            }
            // If there's no error, set the userID to the retrieved user ID and set the avatarLoadCapability to CDN.
            else
            {
                userID = message.Data.ID;
                avatarLoadCapability = AvatarLoadCapability.CDN;
            }
            
            // Signal that the task has completed.
            completed.SetResult(null);
        });
        
        // Return the task.
        return completed.Task;
    }
    
}
/*
public class OculusManager : MonoBehaviour
{
    // User's personal Oculus ID, will be used for their Meta Avatar.
    public ulong userID;
    
    // Singleton to allow easy access if choosing to place different network components on separate Game Objects.
    private static OculusManager _oculusManager;
    public static OculusManager Instance => _oculusManager;
    
    private void Awake()
    {
        // Basic singleton check to ensure that only one instance of this class exists in the client scene.
        if (_oculusManager != null && _oculusManager != this) Destroy(gameObject);
        else _oculusManager = this;
        
        StartCoroutine(StartOvrPlatform());
    }
    
    //Code is commented out to allow for easy editor testing
    /*
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
    */
/*
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
                Debug.LogError("Error initializing OvrPlatform.");
                //OvrAvatarLog.LogError($"Error initializing OvrPlatform. Falling back to local avatar", _sampleAvatar);
                //LoadLocalAvatar();
                yield break;
            }
            yield return null;
        } 
        
        //GetComponent<NetworkManager>().ConnectToServer();

        // user ID == 0 means we want to load logged in user avatar from CDN
        
        // Get User ID
        Users.GetLoggedInUser().OnComplete(message =>
        {
            if (message.IsError)
            {
                var e = message.GetError(); 
                Debug.LogError($"Error loading CDN Avatar: {e.Message}.");
                //OvrAvatarLog.LogError($"Error loading CDN avatar: {e.Message}. " +
                //  "Falling back to local avatar", _sampleAvatar);
            }
            else
            {
                userID = message.Data.ID;
                NetworkManager.Instance.ConnectToServer();
                // TODO: build multiplayer login room
                //_streamingAvatar.gameObject.SetActive(true);
                //_streamingAvatar.StartAvatar(this);
            }
        });
    }
}
*/