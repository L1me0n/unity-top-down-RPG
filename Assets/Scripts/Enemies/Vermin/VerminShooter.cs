using UnityEngine;

public class VerminShooter : EnemyCombatBase
{
    [Header("Projectile")]
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float projectileLifetime = 4f;
    [SerializeField] private int projectileDamage = 1;

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

        EnemyProjectile projectile = SpawnProjectile(projectilePrefab, direction);
        if (projectile == null)
            return;

        projectile.Init(
            direction,
            projectileDamage,
            projectileSpeed,
            projectileLifetime,
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