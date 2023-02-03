using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerScript : MonoBehaviour {

    // Easy Access
    [System.NonSerialized] public Rigidbody rb;
    [System.NonSerialized] public Color color;

    // Input Manager
    private TouchManagerScript touchManager;

    // To use certain functions after surfing
    protected IEnumerator collisionCoroutine = null;

    // To keep score
    private GameObject lastTouch;

    // Target Board
    private Material targetBoard;

    private Vector3 movementDirection;


    void Awake() {
        currentState = new RunningState(this);
        rb = GetComponent<Rigidbody>();
        targetBoard = transform.Find("TargetBoard").GetComponent<Renderer>().material;
    }


    private void Update() {
        if (currentState.GetType() == typeof(SurfingState))
            return;

        // Touch manager gives direction via Vector2, so i need to use that Y axis value at Z axis
        movementDirection = touchManager.movementDirection;
        movementDirection.z = movementDirection.y;
        movementDirection.y = 0f;

        // LookRotation gives our desired rotation and we lerp it for smooth rotation
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(movementDirection - Vector3.right, Vector3.up), GameManagerScript.instance.lerpConstant);

        // Constant velocity on forward
        rb.velocity = transform.forward * GameManagerScript.instance.moveForce;
    }


    private void FixedUpdate() {
        // Because of AddForce functions with Scaled objects, angular variables can be affected. I solve it manually because it doesn't have a spesific time
        rb.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
    }



    // It is a little state manager.
    // So I used it in here instead of a new class.
    // Calls functions based on players current State
    #region State Manager

    public IState currentState;

    public void ChangeState(IState newState) => currentState = newState;

    public void Collide(Collision collision) => currentState.Collide(collision);

    public void Recover() => currentState.Recover();

    #endregion

    #region Collision/Trigger

    protected void OnCollisionEnter(Collision collision) {
        // Pandas are all player and AI's
        if (collision.transform.CompareTag("Panda")) {

            // This coroutine indicates that drifting from collision has ended
            if(collisionCoroutine != null)
                StopCoroutine(collisionCoroutine);
            StartCoroutine(collisionCoroutine = CollisionCoroutine());

            // To keep score after dying
            lastTouch = collision.gameObject;

            // Only 1 of the collided objects will calculate collision. So i decide it with a simple variable
            if (transform.rotation.eulerAngles.y < collision.transform.rotation.eulerAngles.y)
                return;
            
            // Calls Collide function according to currentState
            Collide(collision);
        }
       
    }

    private void OnCollisionExit(Collision collision) {

        // This function indicates dying by collision exit. CollisionExit can occur sometimes because of DOScale
        // so I ensure that player is falling by calculating distance from arena position
        if (collision.transform.CompareTag("Arena") && Vector3.Distance(transform.position, collision.transform.position) > 15)
            currentState = new SurfingState(this);
    }

    protected void OnTriggerEnter(Collider other) {
        if (other.transform.CompareTag("Edible")) {

            // Ensure that Edible can't be eaten on animation
            other.GetComponent<Collider>().enabled = false;

            // Ensure that there aren't any tweens
            transform.DOComplete();
            transform.DOScale(new Vector3(transform.localScale.x + .1f, transform.localScale.y + .1f, transform.localScale.z + .1f), .1f);

            // If main player eats edible, camera Raises as effect
            if (this.GetType() == typeof(PlayerScript))
                GameManagerScript.instance.RaiseCamera(.2f);

            // Edible animation that rises a bit, then goes to player and Releases itself to GameManager for later use
            other.transform.DOKill();
            other.transform.DOMoveY(other.transform.position.y + .5f, .2f).OnComplete(() => other.transform.DOMove(transform.position, .1f).OnComplete(() => GameManagerScript.instance.ReleaseEdible(other.gameObject)));

            // Add score to player
            GameManagerScript.instance.AddScore(100, gameObject);
        }
        else if (other.transform.CompareTag("Outer Space")) {
            GameManagerScript.instance.AddScore(Random.Range(10, 16) * 100, lastTouch, true);

            if (color == Color.white)
                GameManagerScript.instance.EndGame();

            gameObject.SetActive(false);
        }
    }

    protected IEnumerator CollisionCoroutine() {
        // Wait until Physics forces occur which we did with Collide function
        yield return new WaitForEndOfFrame();

        // Disable Input from user
        ChangeState(new SurfingState(this));

        // Wait Until drifting ends
        yield return new WaitWhile(() => rb.velocity.magnitude > .1f);

        // Get back to RunningState
        Recover();
        collisionCoroutine = null;
    }

    // If player get hit from target board, it changes color for indication
    public void TargetBoard() {
        targetBoard.DOComplete();
        Tween tw;
        (tw = targetBoard.DOColor(Color.red, .5f)).SetAutoKill(false).OnComplete(() => tw.Rewind());
    }

    #endregion

    #region Setter

    // Set color at the start
    public void SetColor(Color newColor) {
        color = newColor;
        GetComponentInChildren<Renderer>().material.color = color;
    }


    // Dependency Injection
    public void SetTouchManager(TouchManagerScript touchManagerScript) => touchManager = touchManagerScript;

    #endregion

}
