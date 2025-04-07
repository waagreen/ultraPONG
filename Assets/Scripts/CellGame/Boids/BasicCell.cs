using UnityEngine;

public class BasicCell : MonoBehaviour
{
    private CellSettings settings;

    // State
    [HideInInspector] public Vector3 position;
    [HideInInspector] public Vector3 forward;
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
        forward = cachedTransform.forward;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;
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
        Vector2 acceleration = Vector2.zero;

        if (target != null)
        {
            Vector3 offsetToTarget = target.position - position;
            acceleration = SteerTowards (offsetToTarget) * settings.targetWeight;
        }

        if (numPerceivedFlockmates != 0)
        {
            centreOfFlockmates /= numPerceivedFlockmates;

            Vector2 offsetToFlockmatesCentre = centreOfFlockmates - (Vector2)position;

            Vector2 alignmentForce = SteerTowards (avgFlockHeading) * settings.alignWeight;
            Vector2 cohesionForce = SteerTowards (offsetToFlockmatesCentre) * settings.cohesionWeight;
            Vector2 seperationForce = SteerTowards (avgAvoidanceHeading) * settings.seperateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += seperationForce;
        }

        if (IsHeadingForCollision())
        {
            Vector2 collisionAvoidDir = ObstacleRays();
            Vector2 collisionAvoidForce = SteerTowards (collisionAvoidDir) * settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector2 dir = velocity / speed;
        speed = Mathf.Clamp (speed, settings.minSpeed, settings.maxSpeed);
        velocity = dir * speed;

        cachedTransform.position += (Vector3)(velocity * Time.deltaTime);
        cachedTransform.forward = dir;
        position = cachedTransform.position;
        forward = dir;
    }

    private bool IsHeadingForCollision()
    {
        if (Physics.SphereCast (position, settings.boundsRadius, forward, out RaycastHit hit, settings.collisionAvoidDst, settings.obstacleMask))
        {
            return true;
        } 

        return false;
    }

    private Vector3 ObstacleRays()
    {
        Vector3[] rayDirections = new Vector3[10];

        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 dir = cachedTransform.TransformDirection (rayDirections[i]);
            Ray ray = new(position, dir);
            
            if (!Physics.SphereCast (ray, settings.boundsRadius, settings.collisionAvoidDst, settings.obstacleMask))
            {
                return dir;
            }
        }

        return forward;
    }

    private Vector2 SteerTowards(Vector2 vector)
    {
        Vector2 v = vector.normalized * settings.maxSpeed - velocity;
        return Vector2.ClampMagnitude(v, settings.maxSteerForce);
    }
}
