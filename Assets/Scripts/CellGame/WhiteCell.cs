using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
public class WhiteCell : MonoBehaviour
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
    
    public Rigidbody2D AttachedPlayer
    {
        get
        {
            if (joint == null) return null;
            return joint.connectedBody;
        }
        set
        {
            joint.enabled = value != null;
            joint.connectedBody = value;
        }
    }

    private CircleCollider2D col;
    private Rigidbody2D rb;
    private Sequence canPickFeedback;
    private RelativeJoint2D joint;

    public System.Action OnGoal;

    void Start()
    {
        GetRequiredComponents();
    }

    private void GetRequiredComponents()
    {
        col = GetComponent<CircleCollider2D>();
        joint = GetComponent<RelativeJoint2D>();
        rb = GetComponent<Rigidbody2D>();

        joint.enabled = false;
    }
    
    public void ResetVelocity()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public void ApplyForce(Vector3 force)
    {
        rb.AddForce(force * MovementSpeed, ForceMode2D.Impulse);
    }

    private void PickUpFeedback()
    {
        canPickFeedback?.Kill();
        canPickFeedback = DOTween.Sequence();

        canPickFeedback.Append(outlineRenderer.DOFade(canBePickedUp ? 1f : 0f, 0.25f).SetEase(Ease.OutCubic));
    }
}
