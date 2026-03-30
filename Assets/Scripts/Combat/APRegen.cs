using UnityEngine;

public class APRegen : MonoBehaviour
{
    [Header("Regen Rule")]
    [SerializeField] private int apPerTick = 1;
    [SerializeField] private float tickSeconds = 2f;

    [Header("Options")]
    [SerializeField] private bool regenOnlyWhenAlive = true;
    [SerializeField] private PlayerDamageReceiver damageReceiver;

    private int baseAPPerTick = 1;
    private float baseTickSeconds = 2f;

    private PlayerStats stats;
    private float timer;

    public int APPerTick => apPerTick;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();

        if (damageReceiver == null)
            damageReceiver = GetComponent<PlayerDamageReceiver>();

        ResetToBase();
        
    }

    private void ResetToBase()
    {
        apPerTick = baseAPPerTick;
        tickSeconds = baseTickSeconds;
    }

    public void SetActionRate(int newModifier)
    {
        apPerTick = Mathf.Max(1, newModifier);
    }

    private void Update()
    {
        if (regenOnlyWhenAlive && damageReceiver != null && damageReceiver.IsDead)
            return;

        if (stats.AP >= stats.MaxAP)
        {
            timer = 0f; // reset timer when full so it feels consistent
            return;
        }

        timer += Time.deltaTime;
        if (timer >= tickSeconds)
        {
            timer -= tickSeconds;
            stats.GainAP(apPerTick);
        }
    }

    public void ResetRegenState()
    {
        timer = 0f;
    }
}