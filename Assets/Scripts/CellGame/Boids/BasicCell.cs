// BasicCell.cs
using UnityEngine;

public class BasicCell : MonoBehaviour
{
    private CellSettings settings;

    // State
    [HideInInspector] public Vector2 position;
    [HideInInspector] public Vector2 forward;
    private Vector2 velocity;

    // To Update
    private Vector2 acceleration;
    [HideInInspector] public Vector2 avgFlockHeading;
    [HideInInspector] public Vector2 avgAvoidanceHeading;
    [HideInInspector] public Vector2 centreOfFlockmates;
    [HideInInspector] public int numPerceivedFlockmates;

    // Cached
    private SpriteRenderer sRenderer;
    private Transform cachedTransform;
    private Transform target;

    private void Awake()
    {
        sRenderer = transform.GetComponentInChildren<SpriteRenderer>();
        cachedTransform = transform;
    }

    public void Initialize(CellSettings settings, Transform target)
    {
        this.target = target;
        this.settings = settings;

        position = cachedTransform.position;
        forward = cachedTransform.up; // Using up for 2D forward direction

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = forward * startSpeed;
    }

    public void SetColour(Color col)
    {
        if (sRenderer != null)
        {
            sRenderer.color = col;
        }
    }

    public void UpdateCell()
    {
        acceleration = Vector2.zero;

        if (target != null)
        {
            Vector2 offsetToTarget = (Vector2)target.position - position;
            acceleration = SteerTowards(offsetToTarget) * settings.targetWeight;
        }

        if (numPerceivedFlockmates != 0)
        {
            centreOfFlockmates /= numPerceivedFlockmates;

            Vector2 offsetToFlockmatesCentre = centreOfFlockmates - position;

            Vector2 alignmentForce = SteerTowards(avgFlockHeading) * settings.alignWeight;
            Vector2 cohesionForce = SteerTowards(offsetToFlockmatesCentre) * settings.cohesionWeight;
            Vector2 seperationForce = SteerTowards(avgAvoidanceHeading) * settings.seperateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += seperationForce;
        }

        if (IsHeadingForCollision())
        {
            Vector2 collisionAvoidDir = ObstacleRays();
            Vector2 collisionAvoidForce = SteerTowards(collisionAvoidDir) * settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector2 dir = velocity.normalized;
        speed = Mathf.Clamp(speed, settings.minSpeed, settings.maxSpeed);
        velocity = dir * speed;

        cachedTransform.position += (Vector3)(velocity * Time.deltaTime);
        cachedTransform.up = dir; // Rotate in 2D using up vector
        position = cachedTransform.position;
        forward = dir;
    }

    private bool IsHeadingForCollision()
    {
        RaycastHit2D hit = Physics2D.CircleCast(position, settings.boundsRadius, forward, settings.collisionAvoidDst, settings.obstacleMask);
        return hit.collider != null;
    }

    private Vector2 ObstacleRays()
    {
        Vector2[] rayDirections = new Vector2[16];
        for (int i = 0; i < rayDirections.Length; i++)
        {
            float angle = i * Mathf.PI * 2 / rayDirections.Length;
            rayDirections[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        foreach (Vector2 dir in rayDirections)
        {
            RaycastHit2D hit = Physics2D.CircleCast(position, settings.boundsRadius, dir, settings.collisionAvoidDst, settings.obstacleMask);
            if (!hit)
            {
                return dir;
            }
        }
        
        // If all rays hit, return reverse direction
        return -forward;
    }

    private Vector2 SteerTowards(Vector2 vector)
    {
        Vector2 v = vector.normalized * settings.maxSpeed - velocity;
        return Vector2.ClampMagnitude(v, settings.maxSteerForce);
    }
}