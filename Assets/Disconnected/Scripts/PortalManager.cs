using UnityEngine;

public class PortalManager : MonoBehaviour
{
    private static PortalManager _instance;
    public static PortalManager Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LinkPortals();
    }

    public void LinkPortals()
    {
        Portal[] portals = FindObjectsOfType<Portal>();
        if (portals.Length == 2)
        {
            portals[0].otherPortal = portals[1];
            portals[1].otherPortal = portals[0];
        }
        else
        {
            Debug.LogWarning($"PortalManager: Expected 2 portals, found {portals.Length}.");
        }
    }
} 