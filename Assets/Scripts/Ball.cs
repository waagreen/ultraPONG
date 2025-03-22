using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
public class Ball : MonoBehaviour
{
    [SerializeField] private float MovementSpeed;
    [SerializeField] private LayerMask bounceoffLayer;

    private Rigidbody2D rb;
    private CircleCollider2D col;
    private SpriteRenderer sRenderer;
    private Vector2 tiltedNormal = Vector2.zero;
    
    public System.Action OnGoal;

    void Start()
    {
        GetRequiredComponents();
        DoRandomDirectionImpulse();
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

    public void DoRandomDirectionImpulse()
    {
        Vector2 direction = new(Random.Range(-1f, 1f), Random.Range(-0.5f, 0.5f));
        rb.AddForce(direction.normalized * MovementSpeed, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & bounceoffLayer) != 0)
        {
            ContactPoint2D firstContact = collision.GetContact(0);
            int tiltAngle = Random.Range(-45, 45);
            tiltedNormal = Quaternion.AngleAxis(tiltAngle, Vector3.forward) * firstContact.normal.normalized;
            Vector2 oppositeVelocity = tiltedNormal * MovementSpeed;
            rb.AddForce(oppositeVelocity, ForceMode2D.Impulse);
        }
        else OnGoal.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, tiltedNormal); 
    }
}
