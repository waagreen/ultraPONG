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
    [SerializeField] private Transform holdSlot;
    [SerializeField] private WhiteCell cellPrefab;


    // Essential components - (do not change)
    private Rigidbody2D rb;
    private CircleCollider2D col;
    private SpriteRenderer sRenderer;
    private CellsManager cellsManager;
    private Camera mainCamera;
    private InputManager inputManager;
    
    // Dynamic variables
    private Vector2 lerpedInput = Vector2.zero;
    private WhiteCell attachedCell;
    private float rateOfChange;

    private void Start()
    {
        GetRequiredComponents();
        AttachCell();

        inputManager.ThrowAction.performed += ThrowCell;
    }

    private void GetRequiredComponents()
    {
        mainCamera = Camera.main;
        
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        sRenderer = GetComponent<SpriteRenderer>();
        
        cellsManager = FindFirstObjectByType<CellsManager>();   
        inputManager = FindFirstObjectByType<InputManager>();
    }

    private void HandleMovement(Vector2 movementInput)
    {
        lerpedInput = Vector2.Lerp(Vector2.zero, movementInput.normalized * MovementSpeed, movementInput.magnitude);
        rateOfChange = Mathf.Lerp(Deceleration, Acceleration, lerpedInput.magnitude);
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, lerpedInput, rateOfChange * Time.deltaTime);
    }
    
    private Vector2 GetRealInputVector(Vector2 movementInput, Vector2 aimInput)
    {
        if (aimInput.magnitude < 0.1f) return movementInput.normalized;
        if (inputManager.IsUsingController) return aimInput.normalized;

        Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(aimInput);
        return (mouseWorldPosition - (Vector2)transform.position).normalized;
    }

    private void HandleAimRotation(Vector2 movementInput, Vector2 aimInput)
    {
        Vector2 realInput = GetRealInputVector(movementInput, aimInput);
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

    private void ThrowCell(InputAction.CallbackContext ctx)
    {
        if (!attachedCell.CanBeThrowed()) return;
        
        attachedCell.transform.SetParent(null);
        attachedCell.ApplyForce(GetForwardDirection());
        attachedCell.JointConnection = false;
    }

    private void AttachCell()
    {   
        attachedCell = Instantiate(cellPrefab, holdSlot);
        attachedCell.SetHolderRigidBody(rb, holdSlot);
    }

    private void FixedUpdate()
    {
        if (cellsManager.CurrentGameState != GameState.Running) return;;

        Vector2 aimInput = inputManager.AimInput;
        Vector2 movementInput = inputManager.MovementInput;

        HandleMovement(movementInput);
        HandleAimRotation(movementInput, aimInput);
    }

    private void OnDestroy()
    {
        inputManager.ThrowAction.performed -= ThrowCell;
    }
}
