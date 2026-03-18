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
        if (Time.time < nextBiteTime) return;

        // only bite the player
        if (!other.CompareTag("Player")) return;

        var receiver = other.GetComponent<PlayerDamageReceiver>();
        if (receiver != null)
        {
            receiver.ApplyDamage(damage);

            nextBiteTime = Time.time + biteCooldown;
            return;
        }

    }
}