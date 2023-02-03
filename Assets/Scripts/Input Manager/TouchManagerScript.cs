using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class TouchManagerScript : MonoBehaviour{

    private PlayerInput playerInput;

    private InputAction touchPressedAction;
    [System.NonSerialized] public Vector3 movementDirection;

    void Awake(){
        playerInput = GetComponent<PlayerInput>();
        touchPressedAction = playerInput.actions.FindAction("TouchPress");
    }

    private void OnEnable() {
        EnhancedTouchSupport.Enable();
        TouchSimulation.Enable();
        touchPressedAction.performed += TouchPressed;
    }

    private void OnDisable() {
        EnhancedTouchSupport.Disable();
        TouchSimulation.Disable();
        touchPressedAction.performed -= TouchPressed;
    }

    private void TouchPressed(InputAction.CallbackContext context) => StartCoroutine(TouchCoroutine());

    private IEnumerator TouchCoroutine() {
        yield return null;
        while (Touch.activeTouches.Count != 0) {
            //print(Touch.activeTouches[0].screenPosition +" ve "+ Touch.activeTouches[0].startScreenPosition);
            if(!(Touch.activeTouches[0].screenPosition == Touch.activeTouches[0].startScreenPosition))
                movementDirection = Touch.activeTouches[0].screenPosition - Touch.activeTouches[0].startScreenPosition;
            yield return null;
        }
    }

}
