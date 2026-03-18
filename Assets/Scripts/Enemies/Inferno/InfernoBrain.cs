using UnityEngine;

public class InfernoBrain : EnemyBrainBase
{
    [Header("Inferno Movement")]
    [SerializeField] private float moveSpeed = 2.8f;
    [SerializeField] private float acceleration = 14f;

    [Header("Distance Control")]
    [SerializeField] private float preferredRange = 7.5f;
    [SerializeField] private float tooCloseRange = 4f;
    [SerializeField] private float stopLerpFactor = 0.22f;

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