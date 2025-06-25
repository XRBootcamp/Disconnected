using UnityEngine;

public class PortalCameraSync : MonoBehaviour
{
    public Transform playerCamera; // Assign the XR Rig’s Camera
    public Transform portal; // The portal in SceneA
    public Transform destinationPortal; // A reference point in SceneB
    public Camera portalCamera; // The camera in SceneB

    void Update()
    {
        // Calculate the player’s position and rotation relative to the portal
        Vector3 relativePos = portal.InverseTransformPoint(playerCamera.position);
        Quaternion relativeRot = Quaternion.Inverse(portal.rotation) * playerCamera.rotation;

        // Apply the relative position and rotation to the portal camera
        portalCamera.transform.position = destinationPortal.TransformPoint(relativePos);
        portalCamera.transform.rotation = destinationPortal.rotation * relativeRot;
    }
}