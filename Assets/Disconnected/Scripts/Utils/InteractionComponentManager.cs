using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Disconnected.Scripts.Utils
{
    public class InteractionComponentManager : MonoBehaviour
    {
        // Add interaction components (XRGrabInteractable, Collider, Rigidbody) to the innermost child
        public static void AddInteractionComponents(GameObject model)
        {
            if (model != null)
            {
                // Recursively find the innermost child object (where the mesh is)
                Transform innermostChild = GetInnermostChild(model.transform);

                if (innermostChild != null)
                {
                    // Add XR Grab Interactable component for object interaction to the innermost child
                    XRGrabInteractable grabInteractable = innermostChild.gameObject.AddComponent<XRGrabInteractable>();

                    // Add Collider to the innermost child (if it doesn't already exist)
                    if (innermostChild.GetComponent<Collider>() == null)
                    {
                        innermostChild.gameObject
                            .AddComponent<
                                BoxCollider>(); // You can change this to any collider type (BoxCollider, MeshCollider, etc.)
                    }

                    // Add Rigidbody to interact with physics (optional for snapping, gravity, etc.)
                    Rigidbody rigidbody = innermostChild.gameObject.AddComponent<Rigidbody>();
                    rigidbody.isKinematic = true; // Set to true to prevent physics calculations (e.g., falling)
                    rigidbody.useGravity = false; // Disable gravity so it doesn't fall

                    // Optionally, add custom interaction script (e.g., for scaling, rotating, etc.)
                    ObjectInteraction interactionScript = innermostChild.gameObject.AddComponent<ObjectInteraction>();

                    Debug.Log("Interaction components added to the innermost child of the model.");
                }
                else
                {
                    Debug.LogError("No innermost child found to add components to.");
                }
            }
            else
            {
                Debug.LogError("Model is null, cannot add interaction components.");
            }
        }

        // Helper method to recursively find the innermost child object
        private static Transform GetInnermostChild(Transform parent)
        {
            // If there are no children, return the parent itself (innermost child)
            if (parent.childCount == 0)
            {
                return parent;
            }

            Transform deepestChild = null;

            foreach (Transform child in parent)
            {
                // Recursively find the innermost child
                Transform innermost = GetInnermostChild(child);
                if (innermost != null)
                {
                    deepestChild = innermost;
                }
            }

            return deepestChild; // Return the innermost child
        }
    }
}