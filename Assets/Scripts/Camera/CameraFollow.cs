using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.12f;

    [Header("Clamp To Room")]
    [SerializeField] private BoxCollider2D roomBounds;

    [Header("Zoom")]
    [SerializeField] private float zoomSmoothTime = 0.12f;

    private Camera cam;
    private Vector3 velocity;
    private float zoomVelocity;

    private float defaultOrthoSize;
    private bool hasTemporaryZoom;
    private float temporaryOrthoSize;

    private bool hasTemporaryFocusTarget;
    private Transform temporaryFocusTarget;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("[CameraFollow] Script must be on a Camera.", this);
            return;
        }

        if (!cam.orthographic)
            Debug.LogWarning("[CameraFollow] Camera is not orthographic.", this);

        defaultOrthoSize = cam.orthographicSize;
    }

    private void LateUpdate()
    {
        if (cam == null)
            return;

        Transform activeTarget = hasTemporaryFocusTarget ? temporaryFocusTarget : target;
        if (activeTarget == null)
            return;

        float desiredSize = hasTemporaryZoom ? temporaryOrthoSize : defaultOrthoSize;
        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize,
            desiredSize,
            ref zoomVelocity,
            zoomSmoothTime
        );

        Vector3 desired = new Vector3(activeTarget.position.x, activeTarget.position.y, transform.position.z);
        Vector3 smoothed = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);

        if (roomBounds != null)
            smoothed = ClampToBounds(smoothed);

        transform.position = smoothed;
    }

    private Vector3 ClampToBounds(Vector3 camPos)
    {
        Bounds b = roomBounds.bounds;

        float camHalfHeight = cam.orthographicSize;
        float camHalfWidth = camHalfHeight * cam.aspect;

        float minX = b.min.x + camHalfWidth;
        float maxX = b.max.x - camHalfWidth;
        float minY = b.min.y + camHalfHeight;
        float maxY = b.max.y - camHalfHeight;

        if (minX > maxX) camPos.x = b.center.x;
        else camPos.x = Mathf.Clamp(camPos.x, minX, maxX);

        if (minY > maxY) camPos.y = b.center.y;
        else camPos.y = Mathf.Clamp(camPos.y, minY, maxY);

        return camPos;
    }

    public void SetRoomBounds(BoxCollider2D bounds)
    {
        roomBounds = bounds;
    }

    public void SetTemporaryZoom(float orthoSize)
    {
        temporaryOrthoSize = Mathf.Max(0.1f, orthoSize);
        hasTemporaryZoom = true;
    }

    public void ClearTemporaryZoom()
    {
        hasTemporaryZoom = false;
    }

    public void SetTemporaryFocusTarget(Transform focusTarget)
    {
        temporaryFocusTarget = focusTarget;
        hasTemporaryFocusTarget = temporaryFocusTarget != null;
    }

    public void ClearTemporaryFocusTarget()
    {
        temporaryFocusTarget = null;
        hasTemporaryFocusTarget = false;
    }

    public float GetDefaultZoom()
    {
        return defaultOrthoSize;
    }

    public void SetDefaultZoom(float orthoSize)
    {
        defaultOrthoSize = Mathf.Max(0.1f, orthoSize);

        if (!hasTemporaryZoom && cam != null)
            cam.orthographicSize = defaultOrthoSize;
    }
}