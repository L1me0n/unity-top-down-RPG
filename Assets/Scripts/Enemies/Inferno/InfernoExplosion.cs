using System.Collections.Generic;
using UnityEngine;

public class InfernoExplosion : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] private int damage = 2;
    [SerializeField] private float radius = 1.5f;
    [SerializeField] private float duration = 0.2f;

    [Header("Filtering")]
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Debug")]
    [SerializeField] private bool logHits = false;
    [SerializeField] private bool drawGizmo = true;

    private readonly HashSet<PlayerDamageReceiver> hitReceivers = new HashSet<PlayerDamageReceiver>();
    private float timeRemaining;
    private GameObject owner;
    private bool initialized;

    public void Init(int damageAmount, float explosionRadius, float lifeSeconds, GameObject explosionOwner = null)
    {
        damage = damageAmount;
        radius = explosionRadius;
        duration = lifeSeconds;
        owner = explosionOwner;
        timeRemaining = duration;
        initialized = true;

        DamageOverlappingTargets();
    }

    private void Awake()
    {
        timeRemaining = duration;
    }

    private void Update()
    {
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamageCollider(other);
    }

    private void DamageOverlappingTargets()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, hitMask);

        for (int i = 0; i < hits.Length; i++)
            TryDamageCollider(hits[i]);
    }

    private void TryDamageCollider(Collider2D other)
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

        if (receiver == null)
            return;

        if (hitReceivers.Contains(receiver))
            return;

        hitReceivers.Add(receiver);
        receiver.ApplyDamage(damage);

        if (logHits)
            Debug.Log($"[InfernoExplosion] Damaged player for {damage}.");
    }

    private void Reset()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null)
            col = gameObject.AddComponent<CircleCollider2D>();

        col.isTrigger = true;
        col.radius = radius;
    }

    private void OnValidate()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
            col.radius = radius;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmo)
            return;

        Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}