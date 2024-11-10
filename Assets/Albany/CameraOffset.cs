using UnityEngine;

public class CameraOffset : MonoBehaviour
{
    public Camera vrCamera; // Reference to the VR Camera
    public Vector3 positionOffset = new Vector3(0.5f, -0.5f, 2f); // Offset relative to the camera (adjust as needed)
    public float rotationSpeed = 1f; // Optional, for rotating the canvas

    private void Update()
    {
        // Update the position of the canvas based on the camera's position
        FollowPosition();

        // Update the rotation of the canvas to match the camera's yaw (horizontal rotation)
        FollowRotation();
    }

    private void FollowPosition()
    {
        // Set the canvas position relative to the camera's position, applying an offset
        transform.position = vrCamera.transform.position + vrCamera.transform.TransformDirection(positionOffset);
    }

    private void FollowRotation()
    {
        // Get the current rotation of the camera (headset)
        Vector3 cameraRotation = vrCamera.transform.rotation.eulerAngles;

        // Get the yaw (horizontal rotation) and pitch (vertical rotation) components of the camera's rotation
        float h = cameraRotation.y; // yaw (horizontal)
        float v = cameraRotation.x; // pitch (vertical)

        // Lock the vertical rotation (pitch) and allow only the horizontal (yaw) rotation
        Vector3 lockedRotation = new Vector3(0, h, 0); // Lock pitch (x) and roll (z)

        // Apply the locked rotation to the canvas
        transform.rotation = Quaternion.Euler(lockedRotation);
    }
}