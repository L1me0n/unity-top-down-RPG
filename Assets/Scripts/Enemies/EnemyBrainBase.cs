using UnityEngine;

public abstract class EnemyBrainBase : MonoBehaviour
{
    [Header("Shared References")]
    [SerializeField] protected Transform target;

    protected Rigidbody2D rb;
    protected Health health;
    protected EnemyRoomLink roomLink;

    private bool deathReported;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        roomLink = GetComponent<EnemyRoomLink>();

        if (target == null)
            TryFindPlayerTarget();

        if (health != null)
            health.OnDied += HandleDied;
    }

    protected virtual void OnDestroy()
    {
        if (health != null)
            health.OnDied -= HandleDied;
    }

    protected virtual void HandleDied()
    {
        if (deathReported)
            return;

        deathReported = true;

        if (roomLink != null)
            roomLink.NotifyEnemyDied(gameObject);
    }

    protected bool HasTarget()
    {
        return target != null;
    }

    protected bool TryFindPlayerTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return false;

        target = player.transform;
        return true;
    }

    protected Vector2 DirectionToTarget()
    {
        if (target == null)
            return Vector2.zero;

        Vector2 dir = (Vector2)(target.position - transform.position);
        if (dir.sqrMagnitude <= 0.0001f)
            return Vector2.zero;

        return dir.normalized;
    }

    protected float DistanceToTarget()
    {
        if (target == null)
            return float.MaxValue;

        return Vector2.Distance(transform.position, target.position);
    }

    protected void StopMovement(float lerpFactor = 0.25f)
    {
        if (rb == null)
            return;

        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, lerpFactor);
    }

    protected void MoveTowardsTarget(float moveSpeed, float acceleration)
    {
        if (rb == null || target == null)
            return;

        Vector2 desiredVelocity = DirectionToTarget() * moveSpeed;
        rb.linearVelocity = Vector2.MoveTowards(
            rb.linearVelocity,
            desiredVelocity,
            acceleration * Time.fixedDeltaTime
        );
    }

    public virtual void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public Transform GetTarget()
    {
        return target;
    }
}