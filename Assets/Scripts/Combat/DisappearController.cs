using UnityEngine;

public class DisappearController : MonoBehaviour
{
    [Header("Tuning")]
    [SerializeField] private int apCost = 2;
    [SerializeField] private float rechargeDuration = 1.2f; 

    [Header("Layers")]
    [SerializeField] private string normalLayerName = "Player";
    [SerializeField] private string ghostLayerName = "PlayerGhost";

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer[] renderers;
    [SerializeField, Range(0f, 1f)] private float ghostAlpha = 0.35f;

    public bool IsDisappeared => isDisappeared;

    public float ActiveProgress01
    {
        get
        {
            if (!isDisappeared) return 0f;
            float remaining = endTime - Time.time;
            return activeDuration <= 0f ? 0f : Mathf.Clamp01(remaining / activeDuration);
        }
    }

    public float RechargeDuration => rechargeDuration;
    public float RechargeProgress01
    {
        get
        {
            if (rechargeDuration <= 0f) return 1f;
            return Mathf.Clamp01((Time.time - lastUseTime) / rechargeDuration);
        }
    }
    public bool IsReady => !isDisappeared && RechargeProgress01 >= 1f;

    public bool CanTakeDamage() => !isDisappeared;

    // ---- Internals ----
    private PlayerStats stats;
    private PlayerCombatController combat;

    private bool isDisappeared;
    private float endTime;
    private float lastUseTime = -999f;

    private float activeDuration;

    private int normalLayer;
    private int ghostLayer;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        combat = GetComponent<PlayerCombatController>();

        normalLayer = LayerMask.NameToLayer(normalLayerName);
        ghostLayer = LayerMask.NameToLayer(ghostLayerName);

        if (normalLayer < 0) Debug.LogError($"[Disappear] Layer not found: {normalLayerName}");
        if (ghostLayer < 0) Debug.LogError($"[Disappear] Layer not found: {ghostLayerName}");

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    private void Update()
    {
        // End if time is up
        if (isDisappeared && Time.time >= endTime)
            EndDisappear();

        // Trigger only in Defense mode
        if (combat.Mode != CombatMode.Defense) return;
        if (!combat.WantsDisappear) return;

        if (!IsReady) return;

        // Spend AP
        if (!stats.TrySpendAP(apCost))
            return;

        StartDisappear();
    }

    private void StartDisappear()
    {
        isDisappeared = true;

        activeDuration = GetDisappearDuration();

        endTime = Time.time + activeDuration;
        lastUseTime = Time.time;

        // Collision behavior via layer swap
        gameObject.layer = ghostLayer;

        // Visual feedback
        SetAlpha(ghostAlpha);
    }

    private void EndDisappear()
    {
        isDisappeared = false;

        gameObject.layer = normalLayer;
        SetAlpha(1f);
    }

    private float GetDisappearDuration()
    {
        return Mathf.Max(0.1f, stats.DisappearDuration);
    }

    private void SetAlpha(float a)
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            Color c = renderers[i].color;
            c.a = a;
            renderers[i].color = c;
        }
    }
}