using UnityEngine;

public class HellpuppyBiteDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float biteCooldown = 0.65f;

    private float nextBiteTime;

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryBite(collision.collider);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryBite(other);
    }

    private void TryBite(Collider2D other)
    {
        if (TradeItemEffectManager.Instance != null &&
            TradeItemEffectManager.Instance.IsChronosActive)
        {
            return;
        }

        if (Time.time < nextBiteTime)
            return;

        if (!other.CompareTag("Player"))
            return;

        PlayerDamageReceiver receiver = other.GetComponent<PlayerDamageReceiver>();
        if (receiver != null)
        {
            receiver.ApplyDamage(damage);
            nextBiteTime = Time.time + biteCooldown;
        }
    }
}