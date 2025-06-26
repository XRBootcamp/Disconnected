using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class GameManager : MonoBehaviour
{
    public GroqTTS groqTTS;
    
    public LevelStartCollider levelStartCollider;
    public GameObject levelLighting;
    
    public List<string> voiceInputs;
    public List<AudioClip> voiceClips;
    
    public AudioSource audioSource;
    public NavMeshAgent agent;

    public Transform chelseaStart;
    public Transform chelseaMoveTo;
    public Transform chelseaMoveToGarbage;

    public Transform holographicSticker;
    public Transform holographicStickerChelseaPlacement;
    public Transform holographicStickerGarbagePlacement;
    
    public Animator holographicStickerAnimator;
    public Animator pacmanGlyderAnimator;
    
    public Transform pacmanCorner;
    public GameObject ArrowTeleporter1;
    public GameObject ArrowTeleporter2;
    public GameObject ArrowTeleporter3;
    public GameObject ArrowTeleporter4;

    public TeleportationAnchor teleporter1;
    public TeleportationAnchor teleporter2;
    public TeleportationAnchor teleporter3;
    public TeleportationAnchor teleporter4;
    
    public Chelsea chelsea;

    public int currentScriptVal = 0;

    public bool pauseUntilAction = false;
    
    private bool teleporter1Actived = false;
    private bool teleporter2Actived = false;
    private bool teleporter3Actived = false;
    private bool teleporter4Actived = false;

    private void Start()
    {
        //PlaySpeechSequence();

        // Testing
        // StartCoroutine(MoveChelsea());
    }

    public void StartTheExperience()
    {
        levelLighting.SetActive(true);
        PlaySpeechSequence();
    }

    IEnumerator MoveChelsea()
    {
        chelsea.LookAtHologram();
        
        yield return new WaitForSeconds(2);
        
        agent.destination = chelseaMoveTo.position;
        yield return null;
        
        while (agent.remainingDistance > 0.02f)
        {
            yield return null;
        }
        
        chelsea.LookAtHologram();
        
        yield return new WaitForSeconds(1);

        holographicSticker.parent = holographicStickerChelseaPlacement;
        holographicSticker.transform.localPosition = Vector3.zero;
        
        yield return new WaitForSeconds(1.5f);
        
        agent.destination = chelseaMoveToGarbage.position;
        yield return null;
        
        while (agent.remainingDistance > 0.02f)
        {
            yield return null;
        }
        
        chelsea.LookAtGarbage();
        
        yield return new WaitForSeconds(1);
        
        holographicSticker.parent = holographicStickerGarbagePlacement;
        holographicSticker.transform.localPosition = Vector3.zero;
        holographicStickerAnimator.SetTrigger("GarbageAnim");
        
        yield return new WaitForSeconds(2);
        
        chelsea.LookAtGirls();
        
        yield return new WaitForSeconds(1);
        
        pacmanCorner.gameObject.SetActive(false);
        agent.destination = chelseaStart.position;
        yield return null;
        
        while (agent.remainingDistance > 0.02f)
        {
            yield return null;
        }

        chelsea.LookAtGirls();

    }

    public async Task PlaySpeechSequence()
    {
        await Task.Delay(1000);

        while (currentScriptVal < voiceClips.Count)
        {
            // if (!string.IsNullOrEmpty(voiceInputs[currentScriptVal]))
            if (voiceClips[currentScriptVal] != null)
            {
                if (!pauseUntilAction)
                {
                    StartCoroutine(CheckForActionsToExecute());
                }

                if (pauseUntilAction)
                {
                    await Task.Delay(100);
                }
                else
                {
                    // Call the async method and wait for it to complete
                    // await groqTTS.GenerateAndPlaySpeech(voiceInputs[currentScriptVal]);
                    await groqTTS.PlaySpeech(voiceClips[currentScriptVal]);
                    currentScriptVal++;

                    if (currentScriptVal >= voiceClips.Count)
                    {
                        Debug.Log("Experience is Finished");
                        levelStartCollider?.EnablePlayerLocomotion();
                        levelLighting.SetActive(false);
                    }
                }
                
            }
        }
    }

    IEnumerator CheckForActionsToExecute()
    {
        if (currentScriptVal == 2 && !teleporter1Actived)
        {
            teleporter1.enabled = true;
            ArrowTeleporter1.SetActive(true);
            pauseUntilAction = true;
        }
        else if (currentScriptVal == 4)
        {
            while (!audioSource.isPlaying)
            {
                yield return null;
            }
            
            pacmanCorner.gameObject.SetActive(true);
        }
        else if (currentScriptVal == 5 && !teleporter2Actived)
        {
            teleporter2.enabled = true;
            ArrowTeleporter2.SetActive(true);
            pauseUntilAction = true;
        }
        else if (currentScriptVal == 6)
        {
            pacmanGlyderAnimator.SetTrigger("StopGlyderAnim");
        }
        else if (currentScriptVal == 7 && !teleporter3Actived)
        {
            teleporter3.enabled = true;
            ArrowTeleporter3.SetActive(true);
            pauseUntilAction = true;
        }
        else if (currentScriptVal == 8)
        {
            while (!audioSource.isPlaying)
            {
                yield return null;
            }
            
            Debug.Log("Audio Source is Playing");
            yield return new WaitForSeconds(5);
            StartCoroutine(MoveChelsea());
        }
    }

    public void Teleporter1Activated()
    {
        if (!teleporter1Actived && pauseUntilAction)
        {
            teleporter1Actived = true;
            pauseUntilAction = false;
            ArrowTeleporter1.SetActive(false);
        }
    }
    
    public void Teleporter2Activated()
    {
        if (!teleporter2Actived && pauseUntilAction)
        {
            teleporter2Actived = true;
            pauseUntilAction = false;
            ArrowTeleporter2.SetActive(false);

            StartCoroutine(WaitToStartParaglydeAnim());
        }
    }

    IEnumerator WaitToStartParaglydeAnim()
    {
        yield return new WaitForSeconds(2);
        pacmanGlyderAnimator.SetTrigger("GlyderAnim");
    }
    
    public void Teleporter3Activated()
    {
        if (!teleporter3Actived && pauseUntilAction)
        {
            teleporter3Actived = true;
            pauseUntilAction = false;
            ArrowTeleporter3.SetActive(false);
            
            teleporter4.enabled = true;
        }
    }
}
