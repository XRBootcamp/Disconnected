using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

public class PlayerLocomotionReference : MonoBehaviour
{
    public GameObject playerLocomotion;
    public GameObject tunnelingVignette;

    public void ToggleLocomotion(bool _enabled)
    {
        playerLocomotion.SetActive(_enabled);
        tunnelingVignette.SetActive(_enabled);
    }
}
