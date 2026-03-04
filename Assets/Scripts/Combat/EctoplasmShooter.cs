using UnityEngine;

public class EctoplasmShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform aimPivot;
    [SerializeField] private MonoBehaviour aimProvider;
    [SerializeField] private EctoplasmProjectile projectilePrefab;

    [Header("Fire Settings")]
    [SerializeField] private int apCostPerShot = 1;
    [SerializeField] private float fireCooldown = 0.5f;
    [SerializeField] private float muzzleOffset = 0.7f;

    private PlayerStats stats;
    private PlayerCombatController combat;

    private float nextFireTime;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        combat = GetComponent<PlayerCombatController>();

        if (aimPivot == null)
            Debug.LogError("[EctoplasmShooter] AimPivot not assigned.");

        if (projectilePrefab == null)
            Debug.LogError("[EctoplasmShooter] Projectile prefab not assigned.");

        if (aimProvider == null)
            Debug.LogError("[EctoplasmShooter] Aim Provider not assigned.");
    }

    private void Update()
    {
        if (UIInputBlocker.BlockGameplayInput)
            return;
        if (combat.Mode != CombatMode.Attack) return;
        if (!combat.WantsFire) return;

        if (Time.time < nextFireTime) return;
        if (!stats.TrySpendAP(apCostPerShot)) return;

        Vector2 aimDir = GetAimDirectionSafe();
        if (aimDir.sqrMagnitude < 0.0001f) return;

        Fire(aimDir);
        nextFireTime = Time.time + fireCooldown;
    }

    private void Fire(Vector2 aimDir)
    {
        Vector3 spawnPos = aimPivot.position + (Vector3)(aimDir.normalized * muzzleOffset);

        var proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        proj.Init(aimDir, stats.DP);
    }

    private Vector2 GetAimDirectionSafe()
    {
        var asInterface = aimProvider as IAimProvider;
        if (asInterface != null) return asInterface.AimDirection;

        Debug.LogError("[EctoplasmShooter] AimProvider does not implement IAimProvider. Add IAimProvider to your aim script.");
        return Vector2.zero;
    }
}

public interface IAimProvider
{
    Vector2 AimDirection { get; }
}