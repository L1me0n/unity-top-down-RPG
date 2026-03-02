using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerInput input;

    private Vector2 desiredVelocity; // updated every frame from input

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        // Convert input direction into a target velocity
        desiredVelocity = input.Move * GameConfig.PlayerMoveSpeed;
    }

    private void FixedUpdate()
    {
        // Current velocity from physics
        Vector2 current = rb.linearVelocity;

        // Choose accel or decel depending on whether player is actively pushing input
        float rate = (desiredVelocity.sqrMagnitude > 0.0001f)
            ? GameConfig.PlayerAcceleration
            : GameConfig.PlayerDeceleration;

        // Move current velocity toward target velocity smoothly
        Vector2 next = Vector2.MoveTowards(current, desiredVelocity, rate * Time.fixedDeltaTime);

        rb.linearVelocity = next;
    }
}
