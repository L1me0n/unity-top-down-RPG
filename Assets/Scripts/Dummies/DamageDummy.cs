using UnityEngine;

public class DamageDummy : MonoBehaviour
{
    [Header("Feedback")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private float hitFlashTime = 0.08f;

    private Health health;
    private float flashTimer;
    private Color originalColor;

    private void Awake()
    {
        health = GetComponent<Health>();

        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();

        if (sr != null)
            originalColor = sr.color;

        health.OnDied += HandleDeath;
    }

    private void Update()
    {
        if (sr == null) return;

        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f)
                sr.color = originalColor;
        }
    }

    // Called by projectile when it hits (through Health.TakeDamage)
    public void FlashHit()
    {
        if (sr == null) return;
        sr.color = Color.white;
        flashTimer = hitFlashTime;
    }

    private void HandleDeath()
    {
        // Disable visuals + collider, destroy after a short delay
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (sr != null) sr.enabled = false;

        Destroy(gameObject, 0.2f);
    }
}