using System;
using UnityEngine;

public class LevelStartCollider : MonoBehaviour
{
    public GameManager gameManager;
    
    private bool levelStarted = false;
    private PlayerLocomotionReference playerLocomotionReference;
    
    private void OnTriggerEnter(Collider other)
    {
        
        
        // if (other.tag == "MainCamera")
        if(other.TryGetComponent(out PlayerLocomotionReference playerReference))
        {
            if (!levelStarted)
            {
                playerReference.ToggleLocomotion(false);
                playerLocomotionReference = playerReference;
                StartExperience();
            }
        }
    }

    public void EnablePlayerLocomotion()
    {
        playerLocomotionReference?.ToggleLocomotion(true);
    }

    private void StartExperience()
    {
        levelStarted = true;
        // gameManager.PlaySpeechSequence();
        gameManager.StartTheExperience();
    }
}
