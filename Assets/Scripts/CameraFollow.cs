using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float moveTolerance = 1f;
    
    private Transform objectToFollow;
    private Vector3 cameraOffset;
    [BlockInInspector][SerializeField] private Vector3 velocity = Vector3.zero;

    private void Awake()
    {
        objectToFollow = FindFirstObjectByType<Player>().transform;
        cameraOffset = Vector3.forward * transform.position.z;
    }

    private void FixedUpdate()
    {
        Vector3 targetPosition = objectToFollow.position + cameraOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, moveSpeed);
    }
}
