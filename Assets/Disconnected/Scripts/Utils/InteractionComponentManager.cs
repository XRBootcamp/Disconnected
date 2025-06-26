using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Disconnected.Scripts.Utils
{
    public class InteractionComponentManager : MonoBehaviour
    {
        // Add interaction components (XRGrabInteractable, Collider, Rigidbody) to the innermost child
        public void AddInteractionComponents(GameObject model)
        {
            if (model != null)
            {
                // Add Rigidbody to interact with physics (optional for snapping, gravity, etc.)
                // Only add Rigidbody if it doesn't already exist
                Rigidbody rigidbody = model.GetComponent<Rigidbody>();
                if (rigidbody == null)
                {
                    rigidbody = model.AddComponent<Rigidbody>();
                }
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;

                // Recursively find the innermost child object (last child in the deepest path)
                Transform innermostChild = GetInnermostChild(model.transform);

                if (innermostChild != null)
                {
                    // Add Collider to the innermost child (if it doesn't already exist)
                    // NOTE: adding the collider before so it is added to grab interactable automatically
                    if (innermostChild.GetComponent<Collider>() == null)
                    {
                        Collider col = innermostChild.gameObject.AddComponent<BoxCollider>();
                    }

                    // Add XR Grab Interactable component for object interaction
                    // Only add XRGrabInteractable if it doesn't already exist
                    XRGrabInteractable xRGrabInteractable = model.GetComponent<XRGrabInteractable>();
                    if (xRGrabInteractable == null)
                    {
                        xRGrabInteractable = model.AddComponent<XRGrabInteractable>();
                    }

                    var objectInteractable = model.GetComponent<ObjectInteraction>();
                    // Only add ObjectInteraction if it doesn't already exist
                    if (objectInteractable == null)
                    {
                        objectInteractable = model.AddComponent<ObjectInteraction>();
                    }
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

        // Helper method to get the last child in the deepest path from the root
        private Transform GetInnermostChild(Transform parent)
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