using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.12f; // lower = snappier

    [Header("Clamp To Room")]
    [SerializeField] private BoxCollider2D roomBounds;

    private Camera cam;
    private Vector3 velocity; // used by SmoothDamp

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("[CameraFollow] Script must be on a Camera.", this);
        }
        if (!cam.orthographic)
        {
            Debug.LogWarning("[CameraFollow] Camera is not orthographic.", this);
        }
    }

    private void LateUpdate()
    {
        if (target == null || cam == null) return;

        // 1) Desired camera position
        Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);

        // 2) Smooth move toward desired
        Vector3 smoothed = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);

        // 3) Clamp to room bounds
        if (roomBounds != null)
        {
            smoothed = ClampToBounds(smoothed);
        }

        transform.position = smoothed;
    }

    private Vector3 ClampToBounds(Vector3 camPos)
    {
        Bounds b = roomBounds.bounds;

        float camHalfHeight = cam.orthographicSize;
        float camHalfWidth = camHalfHeight * cam.aspect;

        // If the camera view is larger than the room, clamp will invert.
        // In that case, lock camera to the center of the room.
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
}