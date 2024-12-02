using System.Collections;
using UnityEditor;
using UnityEngine;

//Code written by Miguel Palencia 2024

public class GpsService : MonoBehaviour
{
    
    //[SerializeField] private MapPointer pointer;
    
    /// <summary>
    /// Margin of accuracy. The lower the value the more it will cost in performance.
    /// </summary>
    [SerializeField] private float desiredAccuracyInMeters = 5f;
    
    /// <summary>
    /// How often our location will be updated in meters
    /// </summary>
    [SerializeField] private float updateDistanceInMeters = 0.5f;
    
    /// <summary>
    /// How often our location will be updated in seconds
    /// </summary>
    [SerializeField] private float updateRate = 0.5f;
    
    /// <summary>
    /// Waiting time to reconnect the service
    /// </summary>
    [SerializeField] private float maxStepWaiting = 15f;
    
    private bool _hasUpdated;
    private static GpsService _instance;
    public static GpsService Instance => _instance;
    
    private float _latitude;
    private float _longitude;

    public float Longitude => _longitude;
    public float Latitude => _latitude;

   
    private bool _initializing;
    private float _timer;
    
    private void Awake()
    {
        if (_instance)
            Destroy(this);
        else
            _instance = this;
        
    }

    private void Start()
    {
        _timer = 0;
        _hasUpdated = true;
        StartCoroutine(nameof(InitGpsService));
    }

    private void Update()
    {
        if (_hasUpdated)
        {
            _hasUpdated = false;
            _timer = 0;
            
        }
        else
        {
            //Reset GPS service. So long no update. We control if we are initializing the service to prevent it from being called while it is in process.
            if (_timer > maxStepWaiting && !_initializing)
            {
                Input.location.Stop();                      //Stop the service
                StartCoroutine(nameof(InitGpsService));     //Restart the service
            }
        }

        _timer += Time.deltaTime;
        
    }
    
    private IEnumerator InitGpsService() 
    {
    
        //Stop the UpdatePosition coroutine to avoid Errors
        StopCoroutine(nameof(UpdatePosition));
        _initializing = true;
        
        
        // This region let us test it with Unity Remote while we are developing the app
#if UNITY_EDITOR
        yield return new WaitWhile(() => !EditorApplication.isRemoteConnected);
        yield return new WaitForSecondsRealtime(5f);
        
        
#endif
        
#if UNITY_EDITOR
        // No permission handling needed in Editor
#elif UNITY_ANDROID
        
        RequestPermissionScript.RequestPermission(Permission.CoarseLocation);

        // First, check if user has location service enabled
        if (!UnityEngine.Input.location.isEnabledByUser) {
            // TODO Failure
            Debug.LogFormat("Android and Location not enabled");
            yield break;
        }

#elif UNITY_IOS
        if (!UnityEngine.Input.location.isEnabledByUser) {
            // TODO Failure
            Debug.LogFormat("IOS and Location not enabled");
            yield break;
        }
#endif
        // We start the service to be able to request the location
        Input.location.Start(desiredAccuracyInMeters, updateDistanceInMeters); 
       
        // Wait until service initializes
        var maxWait = maxStepWaiting;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
            yield return new WaitForSecondsRealtime(1);
            maxWait--;
        }

        // Editor has a bug which doesn't set the service status to Initializing. So extra wait in Editor. (https://nosuchstudio.medium.com/how-to-access-gps-location-in-unity-521f1371a7e3)
#if UNITY_EDITOR
        var editorMaxWait = 15;
        while (Input.location.status == LocationServiceStatus.Stopped && editorMaxWait > 0) {
            yield return new WaitForSecondsRealtime(1);
            editorMaxWait--;
        }
#endif
        
        // We cannot initialize
        if (maxWait < 1) {
            // TODO Failure
            Debug.LogFormat("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status != LocationServiceStatus.Running) {
            // TODO Failure
            Debug.LogFormat("Unable to determine device location. Failed with status {0}", Input.location.status);
            yield break;
        }
        
        // Update coordinates
        _latitude = Input.location.lastData.latitude;
        _longitude = Input.location.lastData.longitude;
        
        StartCoroutine(nameof(UpdatePosition));
        
        _initializing = false;
    }

    public IEnumerator UpdatePosition()
    {
        //If the service is running we can access to the location.LastData.
        while (Input.location.status == LocationServiceStatus.Running)
        {
            _latitude = Input.location.lastData.latitude;
            _longitude = Input.location.lastData.longitude;

            _hasUpdated = true;
            yield return new WaitForSeconds(updateRate);
        }

        
        

    }
}
