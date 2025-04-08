using System;
using UnityEngine;
using static UnityEngine.Mathf;

[RequireComponent(typeof(BoxCollider2D))]
public class ScreenBounds : MonoBehaviour
{
    [SerializeField] float teleportOffset = 0.2f;
    [SerializeField] float cornerOffset = 1f;

    private Camera mainCamera;
    private BoxCollider2D trigger;

    public Action<Collider2D> OnExitBounds;

    private void Awake()
    {
        mainCamera = Camera.main;
        mainCamera.transform.localScale = Vector3.one;

        trigger = GetComponent<BoxCollider2D>();
        trigger.isTrigger = true;
    }   

    private void Start()
    {
        OnExitBounds += RepositionPlayer;
        transform.position = Vector3.zero;
        UpdateBounds();
    }

    private bool CheckHorizontalBound(float xPos, bool offsetCorner = false) => Abs(xPos) > (Abs(trigger.bounds.min.x) - (offsetCorner ? cornerOffset : 0));
    private bool CheckVerticalBound(float yPos, bool offsetCorner = false) => Abs(yPos) > (Abs(trigger.bounds.min.x) - (offsetCorner ? cornerOffset : 0));

    public bool IamOutOfBounds(Vector3 worldPosition)
    {
        return CheckHorizontalBound(worldPosition.x) || CheckVerticalBound(worldPosition.y);
    }

    public void UpdateBounds()
    {
        // Orthographic size is half of screen height
        float verticalSize = mainCamera.orthographicSize * 2;

        Vector2 screenSize = new(verticalSize * mainCamera.aspect, verticalSize);
        trigger.size = screenSize;
    }


    public Vector3 CalculateWrappedPosition(Vector3 worldPosition)
    {
        bool xBounded = CheckHorizontalBound(worldPosition.x, true);
        bool yBounded = CheckVerticalBound(worldPosition.y, true);

        Vector2 signedWorldPosition = new(Sign(worldPosition.x), Sign(worldPosition.y)); 
        Vector2 inverseWorldPosition;
        Vector2 offsetVector;

        if (xBounded && yBounded)
        {
            inverseWorldPosition = Vector2.Scale(worldPosition, Vector2.one * -1f);
            offsetVector = Vector2.Scale(Vector2.one * teleportOffset, signedWorldPosition);
            
            return  inverseWorldPosition + offsetVector;
        }
        else if (xBounded)
        {
            inverseWorldPosition = new(worldPosition.x * -1f, worldPosition.y);
            offsetVector = new(teleportOffset * signedWorldPosition.x, teleportOffset);
            
            return inverseWorldPosition + offsetVector;
        }
        else if (yBounded)
        {
            inverseWorldPosition = new(worldPosition.x, worldPosition.y * -1f);
            offsetVector = new(teleportOffset, teleportOffset * signedWorldPosition.y);

            return inverseWorldPosition + offsetVector;
        }
        else return worldPosition;
    }

    private void RepositionPlayer(Collider2D collision)
    {
        collision.transform.position = CalculateWrappedPosition(collision.transform.position);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        OnExitBounds.Invoke(collision);
    }

}
