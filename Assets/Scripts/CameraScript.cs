using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{

    // Smooth camera transition and raise camera when needed

    // Player
    [System.NonSerialized] public Transform target;

    // Increments when player eats edible
    private Vector3 offset;

    private void Awake() => offset = new Vector3(0f, 13f, -13f);

    void FixedUpdate() => transform.position = Vector3.Lerp(transform.position, target.position + offset, .15f);

    public void AddOffset(Vector3 addOff) => offset += addOff;
}
