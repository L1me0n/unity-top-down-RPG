using UnityEngine;

public class BossSummonedEnemy : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField] private Health health;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private GluttonyBossController gluttonyOwner;
    private bool reportedRemoved;

    public void Initialize(GluttonyBossController owner)
    {
        gluttonyOwner = owner;

        if (health == null)
            health = GetComponent<Health>();

        if (health == null)
            health = GetComponentInChildren<Health>();

        if (health != null)
        {
            health.OnDied -= HandleDied;
            health.OnDied += HandleDied;
        }

        reportedRemoved = false;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDied -= HandleDied;
    }

    private void OnDestroy()
    {
        ReportRemoved();
    }

    private void HandleDied()
    {
        ReportRemoved();
    }

    public void Cleanup()
    {
        ReportRemoved();

        if (gameObject != null)
            Destroy(gameObject);
    }

    private void ReportRemoved()
    {
        if (reportedRemoved)
            return;

        reportedRemoved = true;

        if (gluttonyOwner != null)
            gluttonyOwner.NotifySummonedHellhoundRemoved(this);

        if (debugLogs)
            Debug.Log("[BossSummonedEnemy] Removed from boss summon tracking.", this);
    }
}