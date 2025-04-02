using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [SerializeField] Vector3 Offset;
    void Update()
    {
        // Get the camera's position
        Quaternion cameraRot = Camera.main.transform.rotation;

        // Make the plane look at the camera
        transform.rotation = cameraRot;
        transform.Rotate(Offset);
    }
}