using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private int damage = 1;

    [Header("Debug")]
    [SerializeField] private bool logHits = false;

    private Vector2 direction = Vector2.right;
    private float lifeTimer;
    private GameObject owner;

    public void Init(Vector2 moveDirection, int damageAmount, float moveSpeed, float lifeSeconds, GameObject projectileOwner = null)
    {
        direction = moveDirection.sqrMagnitude > 0.0001f ? moveDirection.normalized : Vector2.right;
        damage = damageAmount;
        speed = moveSpeed;
        lifetime = lifeSeconds;
        owner = projectileOwner;
        lifeTimer = lifetime;
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null)
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
                Debug.Log("[EnemyProjectile] Hit player.");

            receiver.ApplyDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
        {
            if (logHits)
                Debug.Log($"[EnemyProjectile] Hit solid object: {other.name}");
            Destroy(gameObject);
        }
    }
}