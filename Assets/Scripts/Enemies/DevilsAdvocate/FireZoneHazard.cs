using UnityEngine;

public class FireZoneHazard : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float lifetime = 4f;

    [Header("Damage")]
    [SerializeField] private int damagePerTick = 1;
    [SerializeField] private float tickSeconds = 0.75f;

    [Header("Debug")]
    [SerializeField] private bool destroyOnOwnerDeath = false;

    private PlayerDamageReceiver inside;
    private float tickTimer;
    private GameObject owner;

    public void Init(
        float zoneLifetime,
        int tickDamage,
        float tickInterval,
        GameObject hazardOwner = null)
    {
        lifetime = Mathf.Max(0.1f, zoneLifetime);
        damagePerTick = Mathf.Max(1, tickDamage);
        tickSeconds = Mathf.Max(0.05f, tickInterval);
        owner = hazardOwner;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (destroyOnOwnerDeath && owner == null)
        {
            Destroy(gameObject);
            return;
        }

        if (inside == null)
            return;

        tickTimer += Time.deltaTime;
        if (tickTimer >= tickSeconds)
        {
            tickTimer -= tickSeconds;
            inside.ApplyDamage(damagePerTick);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerDamageReceiver receiver = other.GetComponent<PlayerDamageReceiver>();
        if (receiver == null)
            return;

        inside = receiver;
        tickTimer = 0f;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerDamageReceiver receiver = other.GetComponent<PlayerDamageReceiver>();
        if (receiver != null && receiver == inside)
            inside = null;
    }
}