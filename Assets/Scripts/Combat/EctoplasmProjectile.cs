using UnityEngine;

public class EctoplasmProjectile : MonoBehaviour
{
    [Header("Tuning")]
    [SerializeField] private float speed = 18f;
    [SerializeField] private float lifetime = 2.5f;

    [Header("Runtime")]
    [SerializeField] private int damage = 1; // only used if Init isn't called

    [Header("Warden Reflect")]
    [SerializeField] private EnemyProjectile reflectedProjectilePrefab;
    [SerializeField] private int reflectedDamage = 1;
    [SerializeField] private float reflectedSpeed = 10f;
    [SerializeField] private float reflectedLifetime = 4f;
    [SerializeField] private float reflectSpawnOffset = 0.6f;

    private Vector2 direction;
    private bool consumed;

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
        if (consumed)
            return;

        if (other.isTrigger)
            return;

        WardenBrain warden = other.GetComponent<WardenBrain>();
        if (warden != null && warden.IsInDefenseMode)
        {
            TryReflectFromWarden(warden);
            return;
        }

        var health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        ConsumeAndDestroy();
    }

    private void TryReflectFromWarden(WardenBrain warden)
    {
        consumed = true;

        if (reflectedProjectilePrefab != null)
        {
            Transform target = FindPlayerTarget();
            Vector2 reflectDir = target != null
                ? ((Vector2)(target.position - warden.transform.position)).normalized
                : (-direction).normalized;

            if (reflectDir.sqrMagnitude < 0.0001f)
                reflectDir = (-direction).normalized;

            Vector3 spawnPos = warden.transform.position + (Vector3)(reflectDir * reflectSpawnOffset);

            EnemyProjectile reflected = Instantiate(
                reflectedProjectilePrefab,
                spawnPos,
                Quaternion.identity
            );

            reflected.Init(
                reflectDir,
                Mathf.Max(1, reflectedDamage),
                reflectedSpeed,
                reflectedLifetime,
                warden.gameObject
            );
        }

        Destroy(gameObject);
    }

    private Transform FindPlayerTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform : null;
    }

    private void ConsumeAndDestroy()
    {
        consumed = true;
        Destroy(gameObject);
    }
}