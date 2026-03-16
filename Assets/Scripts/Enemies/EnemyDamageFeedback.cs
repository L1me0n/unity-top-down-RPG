using UnityEngine;

public class EnemyDamageFeedback : MonoBehaviour
{
    [Header("Feedback")]
    [SerializeField] private SpriteRenderer[] renderers;
    [SerializeField] private float hitFlashTime = 0.8f;
    [SerializeField] private float destroyDelay = 0.2f;

    private Health health;
    private Collider2D[] colliders;
    private Color[] originalColors;
    private float flashTimer;
    private bool dying;

    private void Awake()
    {
        health = GetComponent<Health>();
        colliders = GetComponentsInChildren<Collider2D>(true);

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<SpriteRenderer>(true);

        CacheOriginalColors();

        if (health != null)
            health.OnDied += HandleDeath;
            health.OnDamaged += FlashHit;
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnDied -= HandleDeath;
            health.OnDamaged -= FlashHit;
    }

    private void Update()
    {
        if (dying)
            return;

        if (renderers == null || renderers.Length == 0)
            return;

        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;

            if (flashTimer <= 0f)
                RestoreOriginalColors();
        }
    }

    public void FlashHit()
    {
        if (dying)
            return;

        if (renderers == null || renderers.Length == 0)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].color = Color.white;
        }

        flashTimer = hitFlashTime;
    }

    private void HandleDeath()
    {
        if (dying)
            return;

        dying = true;

        DisableAllColliders();
        HideAllRenderers();

        Destroy(gameObject, destroyDelay);
    }

    private void CacheOriginalColors()
    {
        if (renderers == null)
        {
            originalColors = new Color[0];
            return;
        }

        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                originalColors[i] = renderers[i].color;
        }
    }

    private void RestoreOriginalColors()
    {
        if (renderers == null || originalColors == null)
            return;

        int count = Mathf.Min(renderers.Length, originalColors.Length);

        for (int i = 0; i < count; i++)
        {
            if (renderers[i] != null)
                renderers[i].color = originalColors[i];
        }
    }

    private void DisableAllColliders()
    {
        if (colliders == null)
            return;

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
                colliders[i].enabled = false;
        }
    }

    private void HideAllRenderers()
    {
        if (renderers == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].enabled = false;
        }
    }
}