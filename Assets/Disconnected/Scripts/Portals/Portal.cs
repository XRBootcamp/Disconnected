using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Tooltip("The name of the scene to load additively.")]
    public string sceneToLoad;

    [HideInInspector]
    public Portal otherPortal;

    private Camera playerCamera;
    private bool isPlayerOverlapping = false;

    void Start()
    {
        playerCamera = Camera.main;
        StartCoroutine(LoadSceneAdditively());
        // Ask the PortalManager to link portals after a short delay to ensure both are present
        Invoke(nameof(RequestLink), 0.5f);
    }

    void RequestLink()
    {
        if (PortalManager.Instance != null)
            PortalManager.Instance.LinkPortals();
    }

    IEnumerator LoadSceneAdditively()
    {
        if (!string.IsNullOrEmpty(sceneToLoad) && !SceneManager.GetSceneByName(sceneToLoad).isLoaded)
        {
            yield return SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOverlapping = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOverlapping = false;
        }
    }

    void Update()
    {
        if (isPlayerOverlapping && otherPortal != null)
        {
            Vector3 portalToPlayer = playerCamera.transform.position - transform.position;
            float dotProduct = Vector3.Dot(transform.forward, portalToPlayer);

            // If the player has crossed the portal plane
            if (dotProduct < 0f)
            {
                // Teleport the player
                float rotationDiff = -Quaternion.Angle(transform.rotation, otherPortal.transform.rotation);
                rotationDiff += 180;
                playerCamera.transform.Rotate(Vector3.up, rotationDiff);

                Vector3 positionOffset = playerCamera.transform.position - transform.position;
                positionOffset = Quaternion.Euler(0, rotationDiff, 0) * positionOffset;
                playerCamera.transform.position = otherPortal.transform.position + positionOffset;

                // Deactivate this portal and activate the other one
                this.gameObject.SetActive(false);
                otherPortal.gameObject.SetActive(true);

                isPlayerOverlapping = false; // Prevent immediate re-teleport
            }
        }
    }
} 