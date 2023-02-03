using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class AIScript : PlayerScript
{
    
    // Target Position/Layer
    private LayerMask destinationsLayer;
    private Transform destination;

    private void Start() {
        destinationsLayer = LayerMask.GetMask("Panda", "Edible");
        SetNewDestination();
    }


    // Extra from Player, AI needs to find new Target
    #region Collision/Trigger Overrides

    protected new void OnCollisionEnter(Collision collision) {
        if (collision.transform.CompareTag("Panda")) {
            base.OnCollisionEnter(collision);
            StartCoroutine(NewDestinationCoroutine());
        }
    }

    protected new void OnTriggerEnter(Collider other) {
        base.OnTriggerEnter(other);
        if (other.transform.CompareTag("Edible"))
            SetNewDestination();
    }

    #endregion

    public void SetNewDestination() {
        // Starts from 5 radius, finds closest Edible or Player
        // For no garbage collection, I use NonAlloc
        Collider[] destinations = new Collider[5];
        float overlapRadius = 0f;
        int count;
        do {
            overlapRadius += 5f;
            count = Physics.OverlapSphereNonAlloc(transform.position, overlapRadius, destinations, destinationsLayer);
        } while (count < 2);

        Transform closestDestination = null;
        float closestDistance = float.MaxValue;

        // Rotates all choices
        for (int i = 0;i < destinations.Length;++i) {
            if (destinations[i] == null || destinations[i].gameObject == gameObject)
                continue;
            float newDistance = Vector3.Distance(transform.position, destinations[i].transform.position);
            if (newDistance < closestDistance) {
                closestDistance = newDistance;
                closestDestination = destinations[i].transform;
            }
        }

        // New destination
        destination = closestDestination;
    }


    // Waits until Surfing ends
    private IEnumerator NewDestinationCoroutine() {
        yield return new WaitWhile(() => collisionCoroutine != null);
        SetNewDestination();
    }

    private void Update() {
        if (currentState.GetType() == typeof(SurfingState))
            return;

        // If no destination, deactivated destinations such as edibles and falling players, finds a new destination
        if(destination == null || !destination.gameObject.activeSelf || destination.position.y < 0f) {
            SetNewDestination();
            return;
        }

        // Same with PlayerScript but with different speed
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(destination.position - transform.position, Vector3.up), GameManagerScript.instance.lerpConstant);
        rb.velocity = transform.forward * GameManagerScript.instance.AIMoveForce;
    }

}
