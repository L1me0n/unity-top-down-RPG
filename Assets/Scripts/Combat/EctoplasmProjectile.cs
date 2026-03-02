using UnityEngine;

public class EctoplasmProjectile : MonoBehaviour
{
    [Header("Tuning")]
    [SerializeField] private float speed = 18f;
    [SerializeField] private float lifetime = 2.5f;

    [Header("Runtime")]
    [SerializeField] private int damage = 1; // only used if Init isn't called

    private Vector2 direction;

    public void Init(Vector2 dir, int dmg)
    {
        direction = dir.normalized;
        damage = Mathf.Max(1, dmg);
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Wall hit: we just die
        // Later we can add sparks, decals, etc.
        if (other.isTrigger) return;

        // Damage hook
        var health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}