using UnityEngine;


public class PlayerAim2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform aimPivot;
    [SerializeField] private Camera aimCamera;

    public Vector2 MouseWorld { get; private set; }
    public Vector2 AimDirection { get; private set; } = Vector2.right; // default

    private PlayerInput input;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();

        if (aimPivot == null)
        {
            Debug.LogError("[PlayerAim] AimPivot is not assigned.", this);
        }

        if (aimCamera == null)
        {
            aimCamera = Camera.main;
        }

        if (aimCamera == null)
        {
            Debug.LogError("[PlayerAim] No camera assigned and Camera.main is null. Tag your camera MainCamera or assign one.", this);
        }
    }

    private void Update()
    {
        if (aimPivot == null || aimCamera == null) return;

        // 1) Screen -> World
        Vector3 mw3 = aimCamera.ScreenToWorldPoint(input.MouseScreen);
        MouseWorld = new Vector2(mw3.x, mw3.y);

        // 2) Direction from pivot to mouse
        Vector2 pivotPos = aimPivot.position;
        Vector2 toMouse = MouseWorld - pivotPos;

        // 3) Deadzone to prevent jitter if mouse sits on pivot
        if (toMouse.sqrMagnitude < GameConfig.AimDeadzone * GameConfig.AimDeadzone)
            return;

        AimDirection = toMouse.normalized;

        // 4) Rotate AimPivot to face mouse
        float angle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
        aimPivot.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
