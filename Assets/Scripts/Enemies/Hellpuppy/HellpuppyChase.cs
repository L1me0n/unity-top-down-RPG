using UnityEngine;

public class HellpuppyChase : EnemyBrainBase
{
     [Header("Move Tuning")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float acceleration = 40f;
    [SerializeField] private float stopDistance = 0.2f;
    [SerializeField] private float stopLerpFactor = 0.25f;

    protected override void Awake()
    {
        base.Awake();
    }

    private void FixedUpdate()
    {
        if (!HasTarget())
        {
            TryFindPlayerTarget();
            return;
        }

        float dist = DistanceToTarget();

        if (dist <= stopDistance)
        {
            StopMovement(stopLerpFactor);
            return;
        }

        MoveTowardsTarget(moveSpeed, acceleration);
    }
}