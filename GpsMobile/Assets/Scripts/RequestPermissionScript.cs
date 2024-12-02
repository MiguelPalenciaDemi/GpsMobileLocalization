using UnityEngine;
using UnityEngine.Android;
//Code written by Miguel Palencia 2024
public class RequestPermissionScript : MonoBehaviour
{
    
    /// <summary>
    /// Ask for the permission we need
    /// </summary>
    /// <param name="permissionName"></param>
    public static void RequestPermission(string permissionName)
    {
        if (!Permission.HasUserAuthorizedPermission(permissionName))
            Permission.RequestUserPermission(permissionName);

    }

    
    
}
