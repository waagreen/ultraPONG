using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float moveTolerance = 1f;
    [SerializeField] private float targetOrthoSize = 8;
    [SerializeField] private Vector3 velocity = Vector3.zero;
    
    private Transform objectToFollow;
    private Vector3 cameraOffset;
    private Sequence zoomInSequence;
    private Camera attachedCamera;

    private void Awake()
    {
        objectToFollow = FindFirstObjectByType<Player>().transform;
        attachedCamera = GetComponent<Camera>();
        cameraOffset = Vector3.forward * transform.position.z;

        zoomInSequence = DOTween.Sequence();
        zoomInSequence.Append(Camera.main.DOOrthoSize(targetOrthoSize, CellsManager.kLevelIntroductionDuration));
        zoomInSequence.Join(transform.DOMove(objectToFollow.position + cameraOffset, CellsManager.kLevelIntroductionDuration));
        zoomInSequence.SetEase(Ease.InOutCubic);
    }

    private void FixedUpdate()
    {
        if (zoomInSequence == null || zoomInSequence.IsPlaying()) return;
        
        Vector3 targetPosition = objectToFollow.position + cameraOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, moveSpeed);
    }
}
