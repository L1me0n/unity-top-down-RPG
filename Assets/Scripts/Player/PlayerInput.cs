using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Vector2 Move { get; private set; }
    public Vector2 MouseScreen { get; private set; }

    private void Update()
    {
        if (UIInputBlocker.BlockGameplayInput)
        {
            Move = Vector2.zero;
            MouseScreen = Input.mousePosition;
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector2 raw = new Vector2(x, y);
        Move = raw.sqrMagnitude > 1f ? raw.normalized : raw;

        MouseScreen = Input.mousePosition;
    }
}
