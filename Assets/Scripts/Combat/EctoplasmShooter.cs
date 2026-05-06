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

    [Header("Debug")]
    [SerializeField] private bool logBloodlustShots = false;

    private PlayerStats stats;
    private PlayerCombatController combat;

    private float nextFireTime;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        combat = GetComponent<PlayerCombatController>();

        if (aimPivot == null)
            Debug.LogError("[EctoplasmShooter] AimPivot not assigned.", this);

        if (projectilePrefab == null)
            Debug.LogError("[EctoplasmShooter] Projectile prefab not assigned.", this);

        if (aimProvider == null)
            Debug.LogError("[EctoplasmShooter] Aim Provider not assigned.", this);

        if (stats == null)
            Debug.LogError("[EctoplasmShooter] PlayerStats missing on player.", this);

        if (combat == null)
            Debug.LogError("[EctoplasmShooter] PlayerCombatController missing on player.", this);
    }

    private void Update()
    {
        if (UIInputBlocker.BlockGameplayInput)
            return;

        if (combat == null || stats == null)
            return;

        if (combat.Mode != CombatMode.Attack)
            return;

        if (!combat.WantsFire)
            return;

        if (Time.time < nextFireTime)
            return;

        Vector2 aimDir = GetAimDirectionSafe();
        if (aimDir.sqrMagnitude < 0.0001f)
            return;

        bool bloodlustActive = IsBloodlustActive();

        if (!bloodlustActive)
        {
            if (!stats.TrySpendAP(apCostPerShot))
                return;
        }
        else if (logBloodlustShots)
        {
            Debug.Log("[EctoplasmShooter] Bloodlust active. Shot fired with no AP cost.", this);
        }

        Fire(aimDir);
        nextFireTime = Time.time + fireCooldown;
    }

    private bool IsBloodlustActive()
    {
        return TradeItemEffectManager.Instance != null &&
               TradeItemEffectManager.Instance.IsBloodlustActive;
    }

    private void Fire(Vector2 aimDir)
    {
        Vector3 spawnPos = aimPivot.position + (Vector3)(aimDir.normalized * muzzleOffset);

        EctoplasmProjectile proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        proj.Init(aimDir, stats.DP);
    }

    private Vector2 GetAimDirectionSafe()
    {
        IAimProvider asInterface = aimProvider as IAimProvider;
        if (asInterface != null)
            return asInterface.AimDirection;

        Debug.LogError("[EctoplasmShooter] AimProvider does not implement IAimProvider. Add IAimProvider to your aim script.", this);
        return Vector2.zero;
    }
}

public interface IAimProvider
{
    Vector2 AimDirection { get; }
}