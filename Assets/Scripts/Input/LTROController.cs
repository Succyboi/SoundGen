using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class LTROController : MonoBehaviour
{
    [Header("Bindings")]
    public Key[] WASDKeys = new Key[] { Key.UpArrow, Key.LeftArrow, Key.DownArrow, Key.RightArrow };
    public Key StartKey = Key.Space;
    public Key AKey = Key.Z;
    public Key BKey = Key.X;

    [Header("Settings")]
    public bool normalizeMovement;
    public float movementDamp;

    [Header("Debug")]
    public Vector2 movement;
    public Vector2 movementRaw;
    public Vector2 lastMovementDir;

    public bool startDown;
    public bool startPressed;
    public bool startUp;

    public bool aDown;
    public bool aPressed;
    public bool aUp;

    public bool bDown;
    public bool bPressed;
    public bool bUp;

    private Vector2 movementDampVelocity;

    private void Update()
    {
        Vector2 lastRaw = movementRaw;

        movementRaw =
            //keyboard
            new Vector2(
            (KeyPressed(WASDKeys[1]) ? 0 : 1) + (KeyPressed(WASDKeys[3]) ? 0 : -1),
            (KeyPressed(WASDKeys[0]) ? 0 : -1) + (KeyPressed(WASDKeys[2]) ? 0 : 1)
            )

            //controller
            + (Gamepad.current != null ? Gamepad.current.leftStick.ReadValue() : Vector2.zero);

        movement = Vector2.SmoothDamp(movement, movementRaw, ref movementDampVelocity, movementDamp);
        movement = (normalizeMovement && movement.magnitude > 1) ? movement.normalized : movement;

        if (movementRaw.magnitude != 0)
        {
            lastMovementDir = movement.normalized;
        }

        //buttons
        startDown = Keyboard.current[StartKey].wasPressedThisFrame || (Gamepad.current != null ? Gamepad.current[GamepadButton.Start].wasPressedThisFrame : false);
        startDown = Keyboard.current[StartKey].isPressed || (Gamepad.current != null ? Gamepad.current[GamepadButton.Start].isPressed : false);
        startUp = Keyboard.current[StartKey].wasReleasedThisFrame || (Gamepad.current != null ? Gamepad.current[GamepadButton.Start].wasReleasedThisFrame : false);

        aDown = Keyboard.current[AKey].wasPressedThisFrame || (Gamepad.current != null ? Gamepad.current[GamepadButton.A].wasPressedThisFrame : false);
        aDown = Keyboard.current[AKey].isPressed || (Gamepad.current != null ? Gamepad.current[GamepadButton.A].isPressed : false);
        aUp = Keyboard.current[AKey].wasReleasedThisFrame || (Gamepad.current != null ? Gamepad.current[GamepadButton.A].wasReleasedThisFrame : false);

        bDown = Keyboard.current[BKey].wasPressedThisFrame || (Gamepad.current != null ? Gamepad.current[GamepadButton.B].wasPressedThisFrame : false);
        bDown = Keyboard.current[BKey].isPressed || (Gamepad.current != null ? Gamepad.current[GamepadButton.B].isPressed : false);
        bUp = Keyboard.current[BKey].wasReleasedThisFrame || (Gamepad.current != null ? Gamepad.current[GamepadButton.B].wasReleasedThisFrame : false);
    }

    public bool KeyPressed(Key key)
    {
        return Keyboard.current[key].isPressed;
    }
    public bool KeyDown(Key key)
    {
        return Keyboard.current[key].wasPressedThisFrame;
    }
    public bool KeyUp(Key key)
    {
        return Keyboard.current[key].wasReleasedThisFrame;
    }
}
