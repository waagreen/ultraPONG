using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    [SerializeField] private LayerMask bounceoffLayer;
    [SerializeField] private SpriteRenderer outlineRenderer;
    [SerializeField] private SpriteRenderer mainRenderer;
    
    [Header("Movement")]
    [SerializeField] private float MovementSpeed;

    private bool canBePickedUp;
    public bool CanBePickedUp
    {
        get => canBePickedUp;
        set
        {
            canBePickedUp = value;
            PickUpFeedback();
        }
    }
    private float acceleration = 0f;

    private CircleCollider2D col;
    private Rigidbody2D rb;
    private Vector2 direction;
    private Sequence canPickFeedback;

    public System.Action OnGoal;

    void Start()
    {
        GetRequiredComponents();
    }

    private void GetRequiredComponents()
    {
        col = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    }
    
    public void ResetMovement()
    {
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        direction = Vector2.zero;
    }

    public void ApplyRandomDirection()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        direction = new(Random.Range(-0.75f, 0.75f), Random.Range(-0.5f, 0.5f));
        if ((direction.x == 0) && (direction.y == 0)) ApplyRandomDirection();
    }

    public void DefineDirection(Vector3 newDirection)
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        direction = newDirection;
    }

    private void ReflectDirection(Collision2D collision)
    {
        ContactPoint2D contact = collision.GetContact(0);
        direction = Vector2.Reflect(direction, contact.normal);
    }

    private void FixedUpdate()
    {
        float currentMoveSpeed = Mathf.Lerp(0, MovementSpeed, acceleration);
        transform.Translate(currentMoveSpeed * Time.fixedDeltaTime * direction.normalized);
        acceleration = Mathf.Min(1f, acceleration + 0.005f);
    }

    private void PickUpFeedback()
    {
        canPickFeedback?.Kill();
        canPickFeedback = DOTween.Sequence();

        canPickFeedback.Append(outlineRenderer.DOFade(canBePickedUp ? 1f : 0f, 0.25f).SetEase(Ease.OutCubic));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & bounceoffLayer) != 0)
        {
            ReflectDirection(collision);
        }
        else OnGoal.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, direction); 
    }
}
