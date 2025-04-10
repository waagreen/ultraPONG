using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float moveTolerance = 1f;
    
    private Transform objectToFollow;
    private float initialCameraDepth;

    private void Awake()
    {
        objectToFollow = FindFirstObjectByType<Player>().transform;
        initialCameraDepth = transform.position.z;
    }

    private void Update()
    {
        float distance = Vector2.Distance(transform.position, objectToFollow.position);
        if (distance > moveTolerance)
        {
            Debug.Log("Distance function: " + distance);

            Vector3 targetPosition = new (objectToFollow.position.x, objectToFollow.position.y, initialCameraDepth);
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }
}
