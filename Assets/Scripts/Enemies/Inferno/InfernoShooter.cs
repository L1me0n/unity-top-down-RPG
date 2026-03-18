using UnityEngine;

public class InfernoShooter : EnemyCombatBase
{
    [Header("Projectile")]
    [SerializeField] private InfernoProjectile projectilePrefab;
    [SerializeField] private float projectileSpeed = 6f;
    [SerializeField] private float projectileLifetime = 2.5f;

    [Header("Explosion")]
    [SerializeField] private int explosionDamage = 2;
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private float explosionDuration = 0.2f;

    [Header("Aim")]
    [SerializeField] private bool useFirePointToAim = true;

    private void Update()
    {
        if (!CanAttack())
            return;

        Fire();
    }

    private void Fire()
    {
        Vector2 direction = GetShotDirection();
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        InfernoProjectile projectile = SpawnProjectile(projectilePrefab, direction);
        if (projectile == null)
            return;

        projectile.Init(
            direction,
            projectileSpeed,
            projectileLifetime,
            explosionDamage,
            explosionRadius,
            explosionDuration,
            gameObject
        );

        ConsumeAttackCooldown();
    }

    private Vector2 GetShotDirection()
    {
        Transform target = GetTarget();
        if (target == null)
            return Vector2.zero;

        Vector3 origin = useFirePointToAim && firePoint != null
            ? firePoint.position
            : transform.position;

        Vector2 dir = (Vector2)(target.position - origin);
        if (dir.sqrMagnitude <= 0.0001f)
            return Vector2.zero;

        return dir.normalized;
    }
}