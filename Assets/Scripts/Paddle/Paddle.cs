using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
public class Paddle : MonoBehaviour
{
    [Range(1f, 50f)][SerializeField] private float MovementSpeed;
    [Range(1f, 25f)][SerializeField] private float Acceleration;
    [Range(1f, 25f)][SerializeField] private float Deceleration;
    [Range(0f, 0.5f)][SerializeField] private float IdleThreshold = 0.03f;
    [SerializeField] private Transform holdSlot;
    [SerializeField] private LayerMask pickableLayer;

    private Actions actionMap;
    
    // essential components - (do not change)
    private Rigidbody2D rb;
    private CircleCollider2D col;
    private SpriteRenderer sRenderer;
    
    // dynamic variables
    private Vector2 aimInput = Vector2.zero;
    private Vector2 movementInput = Vector2.zero;
    private Vector2 lerpedInput = Vector2.zero;
    private Ball attachedBall;
    private float rateOfChange;

    private void Start()
    {
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

    private void HandleAim()
    {
        // float TAU = 2 * Mathf.PI;
        // float x = 3f * Mathf.Cos(TAU * aimInput.x);
        // float y = 3f * Mathf.Sin(TAU * aimInput.y);

        Vector3 aimOffset = 10f * (Vector3)aimInput.normalized;
        holdSlot.transform.localPosition = transform.position + aimOffset;
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleAim();
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(holdSlot.transform.position, Vector2.one * 0.25f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & pickableLayer) != 0)
        {
            attachedBall = collision.gameObject.GetComponent<Ball>();
            attachedBall.transform.DOMove(holdSlot.transform.position, 0.2f).SetEase(Ease.InOutCubic);
            attachedBall.transform.SetParent(holdSlot);
        }
    }

    private void OnDestroy()
    {
        actionMap.Disable();
    }
}
