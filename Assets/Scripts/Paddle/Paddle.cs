using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
public class Paddle : MonoBehaviour
{
    [Range(1f, 50f)][SerializeField] private float MovementSpeed;
    [Range(1f, 25f)][SerializeField] private float Acceleration;
    [Range(1f, 25f)][SerializeField] private float Deceleration;
    [Range(0f, 0.5f)][SerializeField] private float IdleThreshold = 0.03f;

    private Actions actionMap;
    
    // essential components - (do not change)
    private Rigidbody2D rb;
    private CircleCollider2D col;
    private SpriteRenderer sRenderer;
    
    // dynamic variables
    private Vector2 input = Vector2.zero;
    private Vector2 _lerpedInput = Vector2.zero;
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

        actionMap.Player.Move.performed += UpdateInputVector;
        actionMap.Player.Move.canceled += UpdateInputVector;
    }

    private void GetRequiredComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        sRenderer = GetComponent<SpriteRenderer>();
    }

    private void UpdateInputVector(InputAction.CallbackContext ctx)
    {
        input = ctx.ReadValue<Vector2>(); 
    }

    private void HandleMovement()
    {
        _lerpedInput = Vector2.Lerp(Vector2.zero, input.normalized * MovementSpeed, input.magnitude);
        rateOfChange = Mathf.Lerp(Deceleration, Acceleration, _lerpedInput.magnitude);
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, _lerpedInput, rateOfChange * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void OnDestroy()
    {
        actionMap.Disable();
    }
}
