using UnityEngine;

public abstract class EnemyCombatBase : MonoBehaviour
{
    [Header("Shared Combat")]
    [SerializeField] protected float attackCooldown = 1f;
    [SerializeField] protected float attackRange = 5f;
    [SerializeField] protected Transform firePoint;

    protected EnemyBrainBase brain;
    protected Health health;

    private float nextAttackTime;

    protected virtual void Awake()
    {
        brain = GetComponent<EnemyBrainBase>();
        health = GetComponent<Health>();

        if (firePoint == null)
            firePoint = transform;
    }

    protected bool HasTarget()
    {
        return brain != null && brain.GetTarget() != null;
    }

    protected Transform GetTarget()
    {
        if (brain == null)
            return null;

        return brain.GetTarget();
    }

    protected Vector2 DirectionToTarget()
    {
        Transform target = GetTarget();
        if (target == null)
            return Vector2.zero;

        Vector2 dir = (Vector2)(target.position - transform.position);
        if (dir.sqrMagnitude <= 0.0001f)
            return Vector2.zero;

        return dir.normalized;
    }

    protected float DistanceToTarget()
    {
        Transform target = GetTarget();
        if (target == null)
            return float.MaxValue;

        return Vector2.Distance(transform.position, target.position);
    }

    protected bool IsTargetInRange()
    {
        return DistanceToTarget() <= attackRange;
    }

    protected bool IsAttackOffCooldown()
    {
        return Time.time >= nextAttackTime;
    }

    protected bool CanAttack()
    {
        if (health == null)
            return false;

        if (!HasTarget())
            return false;

        if (!IsTargetInRange())
            return false;

        if (!IsAttackOffCooldown())
            return false;

        return true;
    }

    protected void ConsumeAttackCooldown()
    {
        nextAttackTime = Time.time + attackCooldown;
    }

    protected T SpawnProjectile<T>(T projectilePrefab, Vector2 direction) where T : MonoBehaviour
    {
        if (projectilePrefab == null)
            return null;

        Vector2 dir = direction.normalized;
        if (dir.sqrMagnitude <= 0.0001f)
            return null;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        return Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
    }
}