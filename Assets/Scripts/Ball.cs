using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
public class Ball : MonoBehaviour
{
    [SerializeField] private LayerMask bounceoffLayer;
    
    [Header("Movement")]
    [SerializeField] private float MovementSpeed;

    private Rigidbody2D rb;
    private CircleCollider2D col;
    private SpriteRenderer sRenderer;

    private Vector2 direction;
    
    public System.Action OnGoal;

    void Start()
    {
        GetRequiredComponents();
        // ApplyRandomDirection();
    }

    private void GetRequiredComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        sRenderer = GetComponent<SpriteRenderer>();
    }
    
    public void ResetVelocity()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public void ApplyRandomDirection()
    {
        direction = new(Random.Range(-0.75f, 0.75f), Random.Range(-0.5f, 0.5f));
        if ((direction.x == 0) && (direction.y == 0)) ApplyRandomDirection();
    }

    public void ApplyImpulse(Vector3 force)
    {
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    private void ReflectDirection(Collision2D collision)
    {
        ContactPoint2D contact = collision.GetContact(0);
        direction = Vector2.Reflect(direction, contact.normal);
    }

    private void FixedUpdate()
    {
        // transform.Translate(MovementSpeed * Time.fixedDeltaTime * direction.normalized);
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
