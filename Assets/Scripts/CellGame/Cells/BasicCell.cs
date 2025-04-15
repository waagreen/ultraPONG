using UnityEngine;

public class BasicCell : MonoBehaviour
{
    [SerializeField] private LayerMask harmLayer;
    
    private SpriteRenderer sRenderer;
    private CellSettings settings;

    // To be updated by the cells manager with updated data from compute shader
    [HideInInspector] public Vector2 avgFlockHeading;
    [HideInInspector] public Vector2 avgAvoidanceHeading;
    [HideInInspector] public Vector2 centreOfFlockmates;
    [HideInInspector] public int numPerceivedFlockmates;
    private Vector2 acceleration;
    private Vector2 velocity;

    private Vector2 spawnPosition;
    private bool isContaminated = false;
    public bool IsContaminated => isContaminated;

    public System.Action<BasicCell> OnDeath;

    private void Awake()
    {
        sRenderer = transform.GetComponentInChildren<SpriteRenderer>();
    }

    public void Initialize(CellSettings settings, bool isContaminated)
    {
        this.settings = settings;
        this.isContaminated = isContaminated;

        spawnPosition = transform.position;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.up * startSpeed;
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

        if (numPerceivedFlockmates != 0)
        {
            centreOfFlockmates /= numPerceivedFlockmates;

            acceleration += SteerTowards(avgFlockHeading) * settings.alignFactor; // Alignment force
            acceleration += SteerTowards(centreOfFlockmates - (Vector2)transform.position) * settings.cohesionFactor; // Cohesion Force
            acceleration += SteerTowards(avgAvoidanceHeading) * settings.avoidFactor; //Separation Force
        }

        if (IsHeadingForCollision())
        {
            Vector2 collisionAvoidForce = SteerTowards(GetUnobstructedDirection()) * settings.avoidCollisionFactor;
            acceleration += collisionAvoidForce;
        }

        acceleration += SteerTowards(spawnPosition - (Vector2)transform.position) * settings.spawnAttractionBias;

        // Update velocity based on all external influences
        velocity += acceleration * Time.deltaTime;
        
        float speed = Mathf.Clamp(velocity.magnitude, settings.minSpeed, settings.maxSpeed);
        transform.up = velocity / speed;
        velocity = transform.up * speed;

        // Update actual transform position
        transform.position += (Vector3)(velocity * Time.deltaTime);
    }

    private bool IsHeadingForCollision()
    {
        return Physics2D.Raycast(transform.position, transform.up, settings.collisionAvoidDst, settings.obstacleMask);
    }

    private Vector2 GetUnobstructedDirection()
    {
        for (int i = 0; i < CellsManager.rayDirections.Length; i++)
        {
            Vector2 dir = transform.TransformDirection(CellsManager.rayDirections[i]);
            Ray ray = new(transform.position, dir);
            if (!Physics.SphereCast (ray, 0.3f, settings.collisionAvoidDst, settings.obstacleMask))
            {
                return dir;
            }
        }

        return transform.up;
    }

    private Vector2 SteerTowards(Vector2 vector)
    {
        Vector2 v = vector.normalized * settings.maxSpeed - velocity;
        return Vector2.ClampMagnitude(v, settings.maxSteerForce);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & harmLayer) != 0)
        {
            OnDeath.Invoke(this);
        }
    }
}