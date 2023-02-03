using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is a State Machine

public class SurfingState : IState {

    private PlayerScript playerScript;

    public SurfingState(PlayerScript stateManagerScript) => playerScript = stateManagerScript;

    // This function basicly calculates like circle-circle collision. Only one side of collision handles collision

    // Projects velocity on collision point and uses that variable to addforce to the other rigidbody.
    // I gave 0 to Y variables to be careful.
    // If you hit other player from behind (like target board in sumo.io) you give extra push. It indicate it by angle between collision point and velocities. You have 60 degree chance from behind.

    public void Collide(Collision collision) {
        Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();
        Vector3 otherRbVelocity = otherRb.velocity;
        Vector3 direction = (otherRb.transform.position - playerScript.transform.position).normalized;
        // I gave 0 to Y variables to be careful. They can be small values.
        direction.y = 0f;

        // Projects velocity on collision point and uses that Vector to addforce to the other rigidbody.
        Vector3 collisionAngle;
        Vector3 damage = Vector3.Project(playerScript.rb.velocity, direction);
        damage.y = 0f;
        damage = damage.normalized;


        // If you hit other player from behind (like target board in sumo.io) you give extra push. It indicate it by angle between collision point and velocities. You have 60 degree chance from behind.
        collisionAngle = collision.GetContact(0).point - otherRb.transform.position;
        collisionAngle.y = 0f;
        if (Vector3.Angle(collisionAngle, otherRb.transform.forward) > 150) {
            damage *= 3f;
            // Target board becomes red for a short time
            otherRb.GetComponent<PlayerScript>().TargetBoard();
        }

        // Addforce using coefficients that exposed to GameManager, There is a base force that adds force independent from velocity to add bouncy effect
        otherRb.AddForce((GameManagerScript.instance.scaleCoefficient * playerScript.transform.localScale.x * damage.magnitude + GameManagerScript.instance.collisionBaseForce) * (direction));


        // And same process for himself
        damage = Vector3.Project(otherRbVelocity, -direction);
        damage.y = 0f;
        damage = damage.normalized;

        collisionAngle = collision.GetContact(0).point - playerScript.transform.position;
        collisionAngle.y = 0f;
        if (Vector3.Angle(collisionAngle, playerScript.transform.forward) > 150) {
            damage *= 3f;
            // Target board becomes red for a short time
            playerScript.TargetBoard();
        }

        playerScript.rb.AddForce((GameManagerScript.instance.scaleCoefficient * otherRb.transform.localScale.x * damage.magnitude + GameManagerScript.instance.collisionBaseForce) * (-direction));
    }

    public void Recover() => playerScript.ChangeState(new RunningState(playerScript));
}

public class RunningState : IState {

    private PlayerScript playerScript;

    public RunningState(PlayerScript stateManagerScript) => playerScript = stateManagerScript;


    // Same with SurfingState. I thought somethings may be different, but same function can handle both.
    public void Collide(Collision collision) {
        Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();
        Vector3 otherRbVelocity = otherRb.velocity;
        Vector3 direction = (otherRb.transform.position - playerScript.transform.position).normalized;
        direction.y = 0f;


        Vector3 collisionAngle;
        Vector3 damage = Vector3.Project(playerScript.rb.velocity, direction);
        damage.y = 0f;
        damage = damage.normalized;

        collisionAngle = collision.GetContact(0).point - otherRb.transform.position;
        collisionAngle.y = 0f;
        if (Vector3.Angle(collisionAngle, otherRb.transform.forward) > 150) {
            damage *= 3f;
            otherRb.GetComponent<PlayerScript>().TargetBoard();
        }

        otherRb.AddForce((GameManagerScript.instance.scaleCoefficient * playerScript.transform.localScale.x * damage.magnitude + GameManagerScript.instance.collisionBaseForce) * (direction));


        damage = Vector3.Project(otherRbVelocity, -direction);
        damage.y = 0f;
        damage = damage.normalized;

        collisionAngle = collision.GetContact(0).point - playerScript.transform.position;
        collisionAngle.y = 0f;
        if (Vector3.Angle(collisionAngle, playerScript.transform.forward) > 150) {
            damage *= 3f;
            playerScript.TargetBoard();
        }

        playerScript.rb.AddForce((GameManagerScript.instance.scaleCoefficient * otherRb.transform.localScale.x * damage.magnitude + GameManagerScript.instance.collisionBaseForce) * (-direction));
    }

    // No need recover state from RunningState
    public void Recover() { }
}