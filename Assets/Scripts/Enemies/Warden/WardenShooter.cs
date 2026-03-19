using UnityEngine;

public class WardenShooter : EnemyCombatBase
{
    [Header("Projectile")]
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private int projectileDamage = 1;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float projectileLifetime = 4f;

    [Header("References")]
    [SerializeField] private WardenBrain wardenBrain;

    [Header("Debug")]
    [SerializeField] private bool logShots = false;

    protected override void Awake()
    {
        base.Awake();

        if (wardenBrain == null)
            wardenBrain = GetComponent<WardenBrain>();
    }

    private void Update()
    {
        if (wardenBrain == null)
            return;

        if (!wardenBrain.IsInAttackMode)
            return;

        if (!CanAttack())
            return;

        Vector2 direction = DirectionToTarget();
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

        if (logShots)
            Debug.Log($"[WardenShooter] {name} fired.");
    }
}