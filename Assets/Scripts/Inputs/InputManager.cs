using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Actions actionMap;
    private Gamepad gamepad;
    private Mouse mouse;
    private Keyboard keyboard;

    private bool isUsingController;
    private Vector2 aimInput = Vector2.zero;
    private Vector2 movementInput = Vector2.zero;

    public bool IsUsingController => isUsingController;
    public Vector2 AimInput => aimInput;
    public Vector2 MovementInput => movementInput;
    public InputAction ThrowAction => actionMap.Player.Throw;

    public void UpdateGameControlScheme()
    {
        if(Gamepad.current != null)
        {
            gamepad = Gamepad.current;
            isUsingController = true;
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if ((Mouse.current != null) && (Keyboard.current != null))
        {
            mouse = Mouse.current;
            keyboard = Keyboard.current;
            isUsingController = false;
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    public void UpdateGameControlScheme(InputDevice inputDevice, InputDeviceChange change)
    {
        UpdateGameControlScheme();
    }

    private void CreateActionMap()
    {
        actionMap = new();
        actionMap.Enable();    

        actionMap.Player.Move.performed += UpdateMovementInputVector;
        actionMap.Player.AnalogAim.performed += UpdateAnalogAim;
        actionMap.Player.MouseAim.performed += UpdateMouseAim;
        
        actionMap.Player.Move.canceled += UpdateMovementInputVector;
        actionMap.Player.AnalogAim.canceled += UpdateAnalogAim;
        actionMap.Player.MouseAim.canceled += UpdateMouseAim;

        UpdateGameControlScheme();
    }

    private void UpdateMovementInputVector(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>(); 
        Vector2.ClampMagnitude(movementInput, 1f);
    }
    
    private void UpdateAnalogAim(InputAction.CallbackContext ctx)
    {
        if (!isUsingController) return;

        aimInput = ctx.ReadValue<Vector2>(); 
        Vector2.ClampMagnitude(aimInput, 1f);
    }

    private void UpdateMouseAim(InputAction.CallbackContext ctx)
    {
        if (isUsingController) return;

        aimInput = ctx.ReadValue<Vector2>(); 
        Vector2.ClampMagnitude(aimInput, 1f);
    }

    private void Awake()
    {
        InputSystem.onDeviceChange += UpdateGameControlScheme;
        CreateActionMap();   
    }

    private void OnDestroy()
    {
        actionMap.Player.Move.performed -= UpdateMovementInputVector;
        actionMap.Player.AnalogAim.performed -= UpdateAnalogAim;
        actionMap.Player.MouseAim.performed -= UpdateMouseAim;

        actionMap.Player.Move.canceled -= UpdateMovementInputVector;
        actionMap.Player.AnalogAim.canceled -= UpdateAnalogAim;
        actionMap.Player.MouseAim.canceled -= UpdateMouseAim;
        
        InputSystem.onDeviceChange -= UpdateGameControlScheme;

        actionMap.Disable();
    }
}
