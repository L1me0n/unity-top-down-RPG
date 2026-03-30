using UnityEngine;

public class HPRegen: MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerStats stats;

    [Header("Tuning")]
    [SerializeField] private bool enabledRegen = true;
    [SerializeField] private float regenPerSecond = 0.6f;     
    [SerializeField] private float delayAfterDamage = 2.0f;
    [SerializeField] private bool stopWhileFull = true;

    private float lastDamageTime;

    private void Awake()
    {
        if (stats == null) stats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (!enabledRegen) return;
        if (stats == null) return;

        if (Time.time - lastDamageTime < delayAfterDamage) return;

        // These properties/methods must exist on PlayerStats:
        // CurrentHP, MaxHP, Heal
        if (stopWhileFull && stats.HP >= stats.MaxHP) return;

        float healFloat = regenPerSecond * Time.deltaTime;
        if (healFloat <= 0f) return;

        ApplyHeal(healFloat);
    }

    // Call this from damage receiver when player gets hit
    public void NotifyDamaged()
    {
        lastDamageTime = Time.time;
    }

    private float healRemainder;

    private void ApplyHeal(float healAmount)
    {
        healRemainder += healAmount;

        int whole = Mathf.FloorToInt(healRemainder);
        if (whole <= 0) return;

        healRemainder -= whole;
        stats.Heal(whole);
    }

    public void ResetRegenState()
    {
        lastDamageTime = 0f;
        healRemainder = 0f;
    }
}