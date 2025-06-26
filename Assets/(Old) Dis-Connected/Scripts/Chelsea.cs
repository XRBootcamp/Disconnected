using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Chelsea : MonoBehaviour
{
    public Animator animator;
    public NavMeshAgent agent;
    
    public Transform targetToLookAtGirls;
    public Transform targetToLookAtHologram;
    public Transform targetToLookAtGarbage;
    public float rotationSpeed = 5f;

    private void Update()
    {
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    public void LookAtGirls()
    {
        StartCoroutine(LookAtGirlsEnumerator(targetToLookAtGirls));
    }
    
    public void LookAtHologram()
    {
        StartCoroutine(LookAtGirlsEnumerator(targetToLookAtHologram));
    }
    
    public void LookAtGarbage()
    {
        StartCoroutine(LookAtGirlsEnumerator(targetToLookAtGarbage));
    }

    IEnumerator LookAtGirlsEnumerator(Transform targetToLookAt)
    {
        float elapsedTime = 0;
        float maxTime = 1f;
        
        while (elapsedTime < maxTime)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            
            // Calculate the direction to the target
            Vector3 direction = (targetToLookAt.position - transform.position).normalized;
            // Ignore Y-axis to prevent tilting
            direction.y = 0;
            
            // Only rotate if there's a significant direction to face
            if (direction != Vector3.zero)
            {
                // Calculate the target rotation
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                // Smoothly rotate towards the target
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
