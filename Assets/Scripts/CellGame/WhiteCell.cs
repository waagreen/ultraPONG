using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
public class WhiteCell : MonoBehaviour
{
    [SerializeField] private LayerMask returnLayer;
    [SerializeField] private SpriteRenderer outlineRenderer;
    [SerializeField] private SpriteRenderer mainRenderer;
    
    [Header("Movement")]
    [SerializeField] private float MovementSpeed;

    private CircleCollider2D col;
    private Sequence returnSequence;
    private FixedJoint2D joint;
    
    private Vector2 initalScale;
    private Rigidbody2D rb;
    private Transform connectionPoint;
    private bool isTraveling = false;

    public bool JointConnection
    {
        set => joint.enabled = value;
        get => joint.enabled;
    }

    public bool CanBeThrowed() => JointConnection && !isTraveling;
    
    private void GetRequiredComponents()
    {
        col = GetComponent<CircleCollider2D>();
        joint = GetComponent<FixedJoint2D>();
        rb = GetComponent<Rigidbody2D>();

        JointConnection = false;
        returnSequence = DOTween.Sequence();
    }
    
    private void ReturnToHolder()
    {
        returnSequence?.Kill();
        returnSequence = DOTween.Sequence();
        rb.linearVelocity = Vector3.zero;
        isTraveling = true;

        returnSequence.Append(transform.DOScale(0f, 0.1f).SetEase(Ease.InBack));
        returnSequence.AppendCallback
        (() =>
        {
            JointConnection = true;
            transform.SetParent(connectionPoint);
            transform.position = connectionPoint.position;
        });
        returnSequence.AppendInterval(0.2f);
        returnSequence.Append(transform.DOScale(initalScale, 0.1f).SetEase(Ease.OutBack));
        returnSequence.OnComplete(() => isTraveling = false);
    }

    public void SetHolderRigidBody(Rigidbody2D rb, Transform connectionPoint)
    {
        GetRequiredComponents();
        
        this.connectionPoint = connectionPoint;
        initalScale = transform.localScale;

        JointConnection = true;
        joint.connectedBody = rb;
        rb.linearVelocity = Vector2.zero;
        transform.SetParent(connectionPoint);
    }

    public void ApplyForce(Vector3 force)
    {
        rb.AddForce(force * MovementSpeed, ForceMode2D.Impulse);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // can't return while being held
        if (JointConnection) return;
        
        // check return layer mask
        if (((1 << collision.gameObject.layer) & returnLayer) == 0) return;
        
        ReturnToHolder();
    }
}
