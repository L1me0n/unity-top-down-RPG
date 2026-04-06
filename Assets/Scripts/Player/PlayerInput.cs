using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Vector2 Move { get; private set; }
    public Vector2 MouseScreen { get; private set; }

    private void Update()
    {
        if (UIInputBlocker.BlockGameplayInput)
            return;
        // Movement axes (old Input Manager)
        float x = Input.GetAxisRaw("Horizontal"); // -1, 0, 1
        float y = Input.GetAxisRaw("Vertical");

        // Normalize so diagonal isn't faster
        Vector2 raw = new Vector2(x, y);
        Move = raw.sqrMagnitude > 1f ? raw.normalized : raw;

        // Mouse position in screen coordinates (pixels)
        MouseScreen = Input.mousePosition;
    }
}
