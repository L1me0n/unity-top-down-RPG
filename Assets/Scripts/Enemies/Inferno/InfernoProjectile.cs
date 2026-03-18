using UnityEngine;

public class InfernoProjectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float lifetime = 2.5f;

    [Header("Explosion")]
    [SerializeField] private InfernoExplosion explosionPrefab;
    [SerializeField] private int explosionDamage = 2;
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private float explosionDuration = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool logHits = false;

    private Vector2 direction = Vector2.right;
    private float lifeTimer;
    private GameObject owner;
    private bool detonated;

    public void Init(
        Vector2 moveDirection,
        float moveSpeed,
        float lifeSeconds,
        int damageAmount,
        float radius,
        float duration,
        GameObject projectileOwner = null)
    {
        direction = moveDirection.sqrMagnitude > 0.0001f ? moveDirection.normalized : Vector2.right;
        speed = moveSpeed;
        lifetime = lifeSeconds;
        explosionDamage = damageAmount;
        explosionRadius = radius;
        explosionDuration = duration;
        owner = projectileOwner;
        lifeTimer = lifetime;
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
            Detonate();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || detonated)
            return;

        if (owner != null)
        {
            if (other.gameObject == owner || other.transform.IsChildOf(owner.transform))
                return;
        }

        PlayerDamageReceiver receiver = other.GetComponent<PlayerDamageReceiver>();
        if (receiver == null)
            receiver = other.GetComponentInParent<PlayerDamageReceiver>();

        if (receiver != null)
        {
            if (logHits)
                Debug.Log("[InfernoProjectile] Hit player collider, detonating.");
            Detonate();
            return;
        }

        if (!other.isTrigger)
        {
            if (logHits)
                Debug.Log($"[InfernoProjectile] Hit solid object {other.name}, detonating.");
            Detonate();
        }
    }

    private void Detonate()
    {
        if (detonated)
            return;

        detonated = true;

        if (explosionPrefab != null)
        {
            InfernoExplosion explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            explosion.Init(
                explosionDamage,
                explosionRadius,
                explosionDuration,
                owner
            );
        }
        else if (logHits)
        {
            Debug.LogWarning("[InfernoProjectile] No explosion prefab assigned.");
        }

        Destroy(gameObject);
    }
}