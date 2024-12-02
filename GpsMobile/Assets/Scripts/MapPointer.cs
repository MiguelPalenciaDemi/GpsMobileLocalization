using UnityEngine;

//Code written by Miguel Palencia 2024
public class MapPointer : MonoBehaviour
{
    [SerializeField] private GameObject pointer;

    [Header("COORDS")] 
    
    [Header("Min Coordinate")] 
    [SerializeField] private float minLatitude;
    [SerializeField] private float minLongitude;
    [SerializeField] private Transform minCoordPoint;
    
    [Header("Max Coordinate")]
    [SerializeField] private float maxLatitude;
    [SerializeField] private float maxLongitude;
    [SerializeField] private Transform maxCoordPoint;

    /// <summary>
    ///  Controls the speed at which the pointer moves. Uses interpolation to smooth the movement to the desired position, avoiding abrupt movements or “teleportations”
    /// </summary>
    [Header("Movements Parameters")]
    [SerializeField] private float movementSpeed = 2;
    
    
    private Vector2 _targetMapPosition;
    private static float _minLat;
    private static float _minLon;
    private static float _maxLat;
    private static float _maxLon;
    private static float _distanceX;
    private static float _distanceY;


    private void Start()
    {
        _distanceX = Mathf.Abs(maxCoordPoint.position.x - minCoordPoint.position.x);
        _distanceY = Mathf.Abs(maxCoordPoint.position.y - minCoordPoint.position.y);
        _minLat = minLatitude;
        _minLon = minLongitude;
        _maxLat = maxLatitude;
        _maxLon = maxLongitude;
        _targetMapPosition = new Vector2();
    }
    
    public void UpdateTargetPosition()
    {
        if(Input.location.status == LocationServiceStatus.Failed) return;
        var latitude = GpsService.Instance.Latitude;
        var longitude = GpsService.Instance.Longitude;

        _targetMapPosition = GiveCoords(longitude,latitude);


    }

    public static Vector2 GiveCoords(float longitude, float latitude)
    {
        //We calculate the normalized coordinates within the limit values
        var normalizedX = (longitude - _minLon) / (_maxLon - _minLon);
        var normalizedY = (latitude - _minLat) / (_maxLat - _minLat);
        
        //The value obtained is multiplied by the distance within our game world.
        var posX = normalizedX * _distanceX;
        var posY = normalizedY * _distanceY;
        
        //The result is the representation of our real-life location inside our game.
        return new Vector2(posX, posY);
    }

    //This function update player position on the map. Move the pointer with a lerp to improve the feeling 
    private void UpdatePointer()
    {
        UpdateTargetPosition();
        pointer.transform.localPosition =
            Vector2.Lerp(pointer.transform.localPosition, _targetMapPosition, Time.deltaTime * movementSpeed);
        
        
    }

    private void Update()
    {
        UpdatePointer();
    }
}
