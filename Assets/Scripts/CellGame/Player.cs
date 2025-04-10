using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
public class Player : MonoBehaviour
{
    [Range(1f, 50f)][SerializeField] private float MovementSpeed;
    [Range(1f, 25f)][SerializeField] private float Acceleration;
    [Range(1f, 25f)][SerializeField] private float Deceleration;
    [Range(1f, 50f)][SerializeField] private float RotationSpeed;
    [Range(0f, 0.5f)][SerializeField] private float IdleThreshold = 0.03f;
    [SerializeField] private Transform holdSlot;
    [SerializeField] private LayerMask pickableLayer;

    private Actions actionMap;
    private Gamepad gamepad;
    private Camera mainCamera;
    private bool isUsingController;

    // Essential components - (do not change)
    private Rigidbody2D rb;
    private CircleCollider2D col;
    private SpriteRenderer sRenderer;
    private CellsManager cellsManager;
    
    // Dynamic variables
    private Vector2 aimInput = Vector2.zero;
    private Vector2 movementInput = Vector2.zero;
    private Vector2 lerpedInput = Vector2.zero;

    private WhiteCell holdingCell;
    private WhiteCell targetPickupCell;

    private float rateOfChange;

    private void Start()
    {
        mainCamera = Camera.main;
        cellsManager = FindFirstObjectByType<CellsManager>();

        CreateActionMap();
        GetRequiredComponents();
    }

    private void CreateActionMap()
    {
        actionMap = new();
        actionMap.Enable();    

        actionMap.Player.Move.performed += UpdateMovementInputVector;
        actionMap.Player.Move.canceled += UpdateMovementInputVector;

        actionMap.Player.Look.performed += UpdateAimInputVector;
        actionMap.Player.Look.canceled += UpdateAimInputVector;

        actionMap.Player.Throw.performed += ThrowBall;
        actionMap.Player.Pickup.performed += AttachBall;

        gamepad = Gamepad.current;
        isUsingController = gamepad != null;

        if(isUsingController)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void GetRequiredComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        sRenderer = GetComponent<SpriteRenderer>();
    }

    private void UpdateMovementInputVector(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>(); 
    }
    
    private void UpdateAimInputVector(InputAction.CallbackContext ctx)
    {
        aimInput = ctx.ReadValue<Vector2>(); 
    }

    private void HandleMovement()
    {
        lerpedInput = Vector2.Lerp(Vector2.zero, movementInput.normalized * MovementSpeed, movementInput.magnitude);
        rateOfChange = Mathf.Lerp(Deceleration, Acceleration, lerpedInput.magnitude);
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, lerpedInput, rateOfChange * Time.deltaTime);
    }
    
    private Vector2 GetRealInputVector()
    {
        if (aimInput.magnitude < 0.1f) return movementInput.normalized;
        if (isUsingController) return aimInput.normalized;

        Vector2 mouseScreenPosition = aimInput;
        Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        return (mouseWorldPosition - (Vector2)transform.position).normalized;
    }

    private void HandleAimRotation()
    {
        Vector2 realInput = GetRealInputVector();
        if (realInput.magnitude < 0.1f) return;

        float targetAngle = Mathf.Atan2(realInput.y, realInput.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        Quaternion interpolatedRotation = Quaternion.Slerp
        (
            transform.rotation,
            targetRotation,
            RotationSpeed * Time.deltaTime
        );

        transform.rotation = interpolatedRotation;
    }

    private Vector2 GetForwardDirection()
    {
        // Get the current rotation angle in degrees
        float angle = transform.eulerAngles.z;
        // Convert angle to radians and get direction vector
        return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
    }

    private void ThrowBall(InputAction.CallbackContext ctx)
    {
        if (holdingCell == null) return;

        holdingCell.AttachedPlayer = null;
        holdingCell.transform.SetParent(null);
        holdingCell.ApplyForce(GetForwardDirection());

        holdingCell = null;
        targetPickupCell = null;
    }

    private void AttachBall(InputAction.CallbackContext ctx)
    {
        if (holdingCell != null || targetPickupCell == null) return;

        holdingCell = targetPickupCell;
        
        holdingCell.transform.DOMove(holdSlot.transform.position, 0.15f).SetEase(Ease.InCubic);
        holdingCell.transform.SetParent(holdSlot);
        holdingCell.ResetVelocity();
        holdingCell.AttachedPlayer = rb;
        holdingCell.CanBePickedUp = false;
        
        targetPickupCell = null;
    }

    private void FixedUpdate()
    {
        if (cellsManager.CurrentGameState != GameState.Running) return;;

        HandleMovement();
        HandleAimRotation();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & pickableLayer) != 0)
        {
            if (targetPickupCell == null)
            {
                targetPickupCell = collision.gameObject.GetComponent<WhiteCell>();
                targetPickupCell.CanBePickedUp = true;
            }
        }
    }    
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & pickableLayer) != 0)
        {
            if (targetPickupCell != null)
            {
                targetPickupCell.CanBePickedUp = false;
                targetPickupCell = null;
            }
        }
    }

    private void OnDestroy()
    {
        actionMap.Player.Move.performed -= UpdateMovementInputVector;
        actionMap.Player.Move.canceled -= UpdateMovementInputVector;

        actionMap.Player.Look.performed -= UpdateAimInputVector;
        actionMap.Player.Look.canceled -= UpdateAimInputVector;

        actionMap.Player.Throw.performed -= ThrowBall;
        actionMap.Player.Pickup.performed -= AttachBall;

        actionMap.Disable();
    }
}
