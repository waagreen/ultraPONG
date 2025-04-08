// BasicCell.cs
using UnityEditor;
using UnityEngine;

public class BasicCell : MonoBehaviour
{
    private CellSettings settings;

    // State
    [HideInInspector] public Vector2 position;
    [HideInInspector] public Vector2 up;
    private Vector2 velocity;

    // To Update
    private Vector2 acceleration;
    [HideInInspector] public Vector2 avgFlockHeading;
    [HideInInspector] public Vector2 avgAvoidanceHeading;
    [HideInInspector] public Vector2 centreOfFlockmates;
    [HideInInspector] public int numPerceivedFlockmates;

    // Cached
    private SpriteRenderer sRenderer;
    private Transform target;
    private readonly Vector2[] rayDirections = new Vector2[64];

    private void Awake()
    {
        sRenderer = transform.GetComponentInChildren<SpriteRenderer>();
    }

    public void Initialize(CellSettings settings, Transform target)
    {
        this.target = target;
        this.settings = settings;

        position = transform.position;
        up = transform.up; // Using up for 2D forward direction

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.up * startSpeed;
            
        for (int i = 0; i < rayDirections.Length; i++)
        {
            float angle = i * Mathf.PI * 2 / rayDirections.Length;
            rayDirections[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
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
        Vector2 dir = velocity / speed;
        speed = Mathf.Clamp(speed, settings.minSpeed, settings.maxSpeed);
        velocity = dir * speed;

        transform.position += (Vector3)(velocity * Time.deltaTime);
        transform.up = dir;
        
        position = transform.position;
        up = transform.up;
    }

    private bool IsHeadingForCollision()
    {
        return Physics2D.Raycast(transform.position, transform.up, settings.collisionAvoidDst, settings.obstacleMask);
    }

    private Vector2 ObstacleRays()
    {
        Vector2 bestDirection = up; // Default to current direction
        float bestScore = float.MinValue;

        foreach (Vector2 dir in rayDirections)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, settings.collisionAvoidDst, settings.obstacleMask);
            
            // Calculate score for this direction
            float score = 0;
            
            if (!hit)
            {
                // Prefer directions that are similar to our current movement
                float alignmentWeight = Vector2.Dot(dir, up.normalized);
                // Add small random factor to avoid identical cells choosing same direction
                float randomWeight = Random.Range(0.9f, 1.1f);
                score = alignmentWeight * randomWeight;
            }
            else
            {
                // The closer we are to the obstacle, the more we should avoid it
                float distanceScore = 1 - (hit.distance / settings.collisionAvoidDst);
                // Directions pointing away from obstacles get negative score
                score = -distanceScore;
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = dir;
            }
        }

        // Add some repulsion force from nearby obstacles
        RaycastHit2D forwardHit = Physics2D.Raycast(transform.position, up, settings.collisionAvoidDst, settings.obstacleMask);
        if (forwardHit)
        {
            // Push away from the obstacle
            Vector2 repelDirection = (position - (Vector2)forwardHit.point).normalized;
            bestDirection = (bestDirection + repelDirection * 2f).normalized;
        }

        return bestDirection;
    }

    private Vector2 SteerTowards(Vector2 vector)
    {
        Vector2 v = vector.normalized * settings.maxSpeed - velocity;
        return Vector2.ClampMagnitude(v, settings.maxSteerForce);
    }
}