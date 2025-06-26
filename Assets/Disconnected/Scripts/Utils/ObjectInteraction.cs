using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Disconnected.Scripts.Utils
{
    public class ObjectInteraction : MonoBehaviour
    {
        [Header("Object Interaction Settings")]
        public float moveSpeed = 1.0f; // Speed of object movement

        public float rotationSpeed = 90.0f; // Degrees per second for rotation
        public float scaleSpeed = 0.1f; // Speed of scaling
        public float snapDistance = 0.2f; // Distance for snapping to a surface
        public float snapSpeed = 0.15f; // Speed of snapping effect
        public Transform objectPreview; // Object preview for showing placement position
        public LayerMask validSurfaceLayer; // Layer for valid surfaces to snap onto

        private XRGrabInteractable _grabInteractable;
        private Transform _controllerTransform;
        private Vector3 _targetSnapPosition;
        private bool _isBeingHeld = false;
        private bool _isSnapping = false;

        private XRController _leftController;
        private XRController _rightController;

        void Start()
        {
            _grabInteractable = GetComponent<XRGrabInteractable>();
            if (_grabInteractable == null)
            {
                Debug.LogWarning("ObjectInteraction: XRGrabInteractable component not found on this GameObject.");
            }
            else
            {
                _grabInteractable.selectEntered.AddListener(OnGrab);
                _grabInteractable.selectExited.AddListener(OnRelease);
            }

            // Get the XR controllers from the scene
            GameObject leftControllerObj = GameObject.Find("LeftHand Controller");
            GameObject rightControllerObj = GameObject.Find("RightHand Controller");
            if (leftControllerObj != null)
            {
                _leftController = leftControllerObj.GetComponent<XRController>();
                if (_leftController == null)
                {
                    Debug.LogWarning("ObjectInteraction: XRController component not found on LeftHand Controller GameObject.");
                }
            }
            else
            {
                Debug.LogWarning("ObjectInteraction: LeftHand Controller GameObject not found in the scene.");
            }
            if (rightControllerObj != null)
            {
                _rightController = rightControllerObj.GetComponent<XRController>();
                if (_rightController == null)
                {
                    Debug.LogWarning("ObjectInteraction: XRController component not found on RightHand Controller GameObject.");
                }
            }
            else
            {
                Debug.LogWarning("ObjectInteraction: RightHand Controller GameObject not found in the scene.");
            }

            // TODO: assign object preview
            if (objectPreview != null)
            {
                objectPreview.gameObject.SetActive(false); // Hide the preview initially
            }
            else
            {
                Debug.LogWarning("ObjectInteraction: objectPreview gameObject not found in the scene.");
            }
        }

        void OnDestroy()
        {
            if (_grabInteractable != null)
            {
                _grabInteractable.selectEntered.RemoveListener(OnGrab);
                _grabInteractable.selectExited.RemoveListener(OnRelease);
            }
        }

        void OnGrab(SelectEnterEventArgs arg0)
        {
            _isBeingHeld = true;
            _controllerTransform = arg0.interactorObject.transform;
            objectPreview.gameObject.SetActive(true); // Show the object preview when grabbed
        }

        void OnRelease(SelectExitEventArgs arg0)
        {
            _isBeingHeld = false;
            StartCoroutine(SnapToSurface());
            objectPreview.gameObject.SetActive(false); // Hide preview when released
        }

        void Update()
        {
            if (_isBeingHeld)
            {
                MoveObject();
                RotateObject();
                ScaleObject();
                UpdateObjectPreview();
            }
        }

        // Move the object based on the controller's position
        void MoveObject()
        {
            Vector3 controllerPosition = _controllerTransform.position;
            transform.position =
                Vector3.Lerp(transform.position, controllerPosition, moveSpeed * Time.deltaTime); // Smooth movement
        }

        // Rotate the object with the controller's thumbstick or rotation button
        void RotateObject()
        {
            float rotationInput = 0f;

            // Check if rotation input is active on either controller
            if (_leftController.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis,
                    out Vector2 leftThumbstick))
            {
                rotationInput = leftThumbstick.x; // Use the X-axis of the left thumbstick for rotation
            }
            else if (_rightController.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis,
                         out Vector2 rightThumbstick))
            {
                rotationInput = rightThumbstick.x; // Use the X-axis of the right thumbstick for rotation
            }

            if (Mathf.Abs(rotationInput) > 0.1f) // If the input is substantial
            {
                transform.Rotate(Vector3.up, rotationInput * rotationSpeed * Time.deltaTime);
            }
        }

        // Scale the object with the controller's grip button or trigger
        void ScaleObject()
        {
            float scaleInput = 0f;

            // Check the grip button on both controllers for scaling
            if (_leftController.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out scaleInput) ||
                _rightController.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out scaleInput))
            {
                transform.localScale += Vector3.one * (scaleInput * scaleSpeed);
            }
        }

        // Raycast to detect the surface where the object is placed and preview the position
        void UpdateObjectPreview()
        {
            RaycastHit hit;
            if (Physics.Raycast(_controllerTransform.position, _controllerTransform.forward, out hit, Mathf.Infinity,
                    validSurfaceLayer))
            {
                _targetSnapPosition = hit.point; // Update the position where the object should snap
                objectPreview.position = _targetSnapPosition; // Move the preview
                objectPreview.rotation =
                    Quaternion.FromToRotation(Vector3.up, hit.normal); // Match orientation to surface normal
            }
        }

        // Snap the object to the valid surface once released
        IEnumerator SnapToSurface()
        {
            while (Vector3.Distance(transform.position, _targetSnapPosition) > snapDistance)
            {
                transform.position = Vector3.Lerp(transform.position, _targetSnapPosition, snapSpeed * Time.deltaTime);
                yield return null;
            }

            // Final snap
            transform.position = _targetSnapPosition;
        }
    }
}
