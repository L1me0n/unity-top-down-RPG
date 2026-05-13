using UnityEngine;

public class GluttonyBossDamageReceiver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GluttonyBossController bossController;
    [SerializeField] private BossRoomController bossRoomController;
    [SerializeField] private Health health;

    [Header("Feedback")]
    [SerializeField] private GameObject blockedHitVisual;
    [SerializeField] private GameObject vulnerableHitVisual;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    public bool IsVulnerable =>
        bossController != null &&
        bossController.IsSleeping &&
        !bossController.IsDead;

    private void Awake()
    {
        if (bossController == null)
            bossController = GetComponentInParent<GluttonyBossController>();

        if (bossRoomController == null)
            bossRoomController = GetComponentInParent<BossRoomController>();

        if (health == null)
            health = GetComponent<Health>();

        if (health == null)
            health = GetComponentInParent<Health>();
    }

    private void OnEnable()
    {
        if (health == null)
            health = GetComponent<Health>();

        if (health != null)
            health.OnDied += HandleBossDied;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDied -= HandleBossDied;
    }

    public bool TryReceiveDamage(int amount)
    {
        if (amount <= 0)
            return false;

        if (bossController == null)
            bossController = GetComponentInParent<GluttonyBossController>();

        if (health == null)
            health = GetComponent<Health>();

        if (health == null)
        {
            Debug.LogWarning("[GluttonyBossDamageReceiver] Missing Health. Cannot receive boss damage.", this);
            return false;
        }

        if (!IsVulnerable)
        {
            ShowBlockedHit();
            Log("Blocked damage because Gluttony is not sleeping.");
            return false;
        }

        health.TakeDamage(amount);
        ShowVulnerableHit();

        Log("Gluttony took " + amount + " damage. HP: " + health.CurrentHP + "/" + health.MaxHP);

        return true;
    }

    private void HandleBossDied()
    {
        Log("Gluttony health reached 0.");

        if (bossRoomController == null)
            bossRoomController = GetComponentInParent<BossRoomController>();

        if (bossRoomController != null)
        {
            bossRoomController.MarkBossDefeated();
        }
        else if (bossController != null)
        {
            bossController.MarkDead();
        }
    }

    private void ShowBlockedHit()
    {
        if (blockedHitVisual != null)
            blockedHitVisual.SetActive(true);
    }

    private void ShowVulnerableHit()
    {
        if (vulnerableHitVisual != null)
            vulnerableHitVisual.SetActive(true);
    }

    private void Log(string message)
    {
        if (debugLogs)
            Debug.Log("[GluttonyBossDamageReceiver] " + message, this);
    }
}