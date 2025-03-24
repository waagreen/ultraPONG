using UnityEngine;

public class Goalkeeper : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float boundsDetectionSize = 2f;
    [SerializeField] private LayerMask obstacles;

    private float halfHeight;
    private GameObject ball;
    private Vector3 colisionPoint;

    void Start()
    {
        halfHeight = transform.localScale.y / 2f;
    }
    
    public void InjectTarget(GameObject target)
    {
        ball = target;
    }

    private void FixedUpdate()
    {
        if (ball == null) return;
        if (IsTryingToGoOutBounds()) return;

        Vector3 targetPosition = new(transform.position.x, ball.transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.fixedDeltaTime * moveSpeed);      
    }

    private bool IsTryingToGoOutBounds()
    {
        int isGoingUp = transform.position.y < ball.transform.position.y ? 1 : -1;  
        float verticalOffset = (isGoingUp * halfHeight) + (isGoingUp * boundsDetectionSize);

        colisionPoint = new(transform.position.x, transform.position.y + verticalOffset, transform.position.z);
        bool cantMove = Physics2D.OverlapBox(colisionPoint, Vector2.one * boundsDetectionSize, 0f, obstacles);
        
        
        return cantMove;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(colisionPoint, Vector2.one * boundsDetectionSize);
    }
}
