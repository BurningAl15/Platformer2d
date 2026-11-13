using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class LSCameraController : MonoBehaviour
{
    public Vector2 minPos, maxPos;
    public Transform target;
    
    [Header("Auto Find Settings")]
    [SerializeField] private bool autoFindTarget = true;
    [SerializeField] private string targetTag = "Player";
    [Header("Map Navigation")]
    [SerializeField] private float dragThreshold = 12f;

    private bool isDragging;
    private bool followTarget = true;
    private Vector2 previousPointerPosition;
    private float orthographicUnitsPerPixel;

    public float DragThresholdPixels => Mathf.Max(0f, dragThreshold);

    private void Awake()
    {
        CacheWorldScale();
    }

    private void LateUpdate()
    {
        CacheWorldScale();
        HandleMapPan();

        if (target == null && autoFindTarget)
        {
            FindTarget();
        }

        if (target == null || !followTarget || isDragging)
        {
            return;
        }

        SetClampedPosition(target.position);
    }

    public void FollowPlayer()
    {
        followTarget = true;
        isDragging = false;
    }

    private void CacheWorldScale()
    {
        Camera cameraComponent = GetComponent<Camera>();
        if (cameraComponent != null && cameraComponent.orthographic)
            orthographicUnitsPerPixel = cameraComponent.orthographicSize * 2f / Mathf.Max(1, Screen.height);
    }

    private void HandleMapPan()
    {
        Touchscreen touchScreen = Touchscreen.current;
        if (touchScreen != null)
        {
            var touch = touchScreen.primaryTouch;
            if (touch.press.wasPressedThisFrame)
            {
                if (IsPointerOverUI(touch.touchId.ReadValue())) return;
                previousPointerPosition = touch.position.ReadValue();
                isDragging = false;
            }
            else if (touch.press.isPressed)
            {
                Vector2 pointer = touch.position.ReadValue();
                Vector2 delta = pointer - previousPointerPosition;
                if (!isDragging && delta.magnitude >= dragThreshold)
                {
                    isDragging = true;
                    followTarget = false;
                }

                if (isDragging)
                    PanByScreenDelta(delta);
                previousPointerPosition = pointer;
            }
            else if (touch.press.wasReleasedThisFrame)
            {
                isDragging = false;
            }

            if (touch.press.isPressed || touch.press.wasReleasedThisFrame)
                return;
        }

        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (IsPointerOverUI()) return;
            previousPointerPosition = mouse.position.ReadValue();
            isDragging = false;
        }
        else if (mouse.leftButton.isPressed)
        {
            Vector2 pointer = mouse.position.ReadValue();
            Vector2 delta = pointer - previousPointerPosition;
            if (!isDragging && delta.magnitude >= dragThreshold)
            {
                isDragging = true;
                followTarget = false;
            }

            if (isDragging)
                PanByScreenDelta(delta);
            previousPointerPosition = pointer;
        }
        else if (mouse.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }

    private void PanByScreenDelta(Vector2 screenDelta)
    {
        // Move the camera opposite the finger so the map follows the drag.
        Vector3 worldDelta = new Vector3(-screenDelta.x, -screenDelta.y, 0f) * orthographicUnitsPerPixel;
        SetClampedPosition(transform.position + worldDelta);
    }

    private void SetClampedPosition(Vector3 desiredPosition)
    {
        float xPos = Mathf.Clamp(desiredPosition.x, minPos.x, maxPos.x);
        float yPos = Mathf.Clamp(desiredPosition.y, minPos.y, maxPos.y);
        transform.position = new Vector3(xPos, yPos, transform.position.z);
    }

    private static bool IsPointerOverUI(int pointerId = -1)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return false;
        return pointerId >= 0
            ? eventSystem.IsPointerOverGameObject(pointerId)
            : eventSystem.IsPointerOverGameObject();
    }

    private void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag(targetTag);
        if (player != null)
        {
            target = player.transform;
            Debug.Log($"LSCameraController: Found target '{player.name}'");
        }
    }
}
