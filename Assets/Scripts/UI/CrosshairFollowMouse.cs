using UnityEngine;

public class CrosshairFollowMouse : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform crosshair; 
    [SerializeField] private Canvas canvas;

    private void Awake()
    {
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (crosshair == null)
            Debug.LogError("[CrosshairFollowMouse] Crosshair is not assigned.", this);
    }

    private void Update()
    {
        if (crosshair == null || canvas == null) return;

        Vector2 mouse = Input.mousePosition;

        crosshair.position = mouse;
    }
}