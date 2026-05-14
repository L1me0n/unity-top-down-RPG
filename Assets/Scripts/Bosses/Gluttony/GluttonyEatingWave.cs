using UnityEngine;

public class GluttonyEatingWave : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 2.5f;

    [Header("Effect")]
    [SerializeField] private int maxHPLoss = 1;
    [SerializeField] private bool destroyOnPlayerHit = false;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private Vector2 direction = Vector2.right;
    private float lifeTimer;
    private bool hasHitPlayer;

    public void Initialize(Vector2 moveDirection, int hpLoss)
    {
        direction = moveDirection.sqrMagnitude > 0.001f
            ? moveDirection.normalized
            : Vector2.right;

        maxHPLoss = Mathf.Max(1, hpLoss);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitPlayer && destroyOnPlayerHit)
            return;

        if (!other.CompareTag("Player"))
            return;

        PlayerDamageReceiver receiver = other.GetComponent<PlayerDamageReceiver>();

        if (receiver == null)
            receiver = other.GetComponentInParent<PlayerDamageReceiver>();

        if (receiver == null)
            return;

        bool applied = receiver.TryApplyMaxHPLoss(maxHPLoss);

        if (applied)
        {
            hasHitPlayer = true;
            Log("Eating Wave hit player. Max HP -" + maxHPLoss);
        }
        else
        {
            Log("Eating Wave touched player but effect was blocked.");
        }

        if (destroyOnPlayerHit)
            Destroy(gameObject);
    }

    private void Log(string message)
    {
        if (debugLogs)
            Debug.Log("[GluttonyEatingWave] " + message, this);
    }
}