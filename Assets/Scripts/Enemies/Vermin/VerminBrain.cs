using UnityEngine;

public class VerminBrain : EnemyBrainBase
{
    [Header("Vermin Movement")]
    [SerializeField] private float moveSpeed = 3.2f;
    [SerializeField] private float acceleration = 16f;

    [Header("Distance Control")]
    [SerializeField] private float preferredRange = 6f;
    [SerializeField] private float tooCloseRange = 3f;
    [SerializeField] private float stopLerpFactor = 0.25f;

    private void FixedUpdate()
    {

        if (!HasTarget())
        {
            if (!TryFindPlayerTarget())
            {
                StopMovement(0.35f);
                return;
            }
        }

        float distance = DistanceToTarget();

        if (distance > preferredRange)
        {
            MoveTowardsTarget(moveSpeed, acceleration);
        }
        else if (distance < tooCloseRange)
        {
            MoveAwayFromTarget(moveSpeed, acceleration);
        }
        else
        {
            StopMovement(stopLerpFactor);
        }
    }

    private void MoveAwayFromTarget(float speed, float accel)
    {
        if (rb == null || target == null)
            return;

        Vector2 desiredVelocity = -DirectionToTarget() * speed;
        rb.linearVelocity = Vector2.MoveTowards(
            rb.linearVelocity,
            desiredVelocity,
            accel * Time.fixedDeltaTime
        );
    }
}