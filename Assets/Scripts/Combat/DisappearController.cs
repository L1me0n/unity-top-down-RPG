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
    public bool IsLieSneakOverrideActive => lieSneakOverrideActive;

    public float ActiveProgress01
    {
        get
        {
            if (!isDisappeared) return 0f;
            if (lieSneakOverrideActive) return 1f;

            float remaining = endTime - Time.time;
            return activeDuration <= 0f ? 0f : Mathf.Clamp01(remaining / activeDuration);
        }
    }

    public float RechargeDuration => rechargeDuration;
    public float RechargeProgress01
    {
        get
        {
            if (lieSneakOverrideActive) return 1f;
            if (rechargeDuration <= 0f) return 1f;
            return Mathf.Clamp01((Time.time - lastUseTime) / rechargeDuration);
        }
    }

    public bool IsReady => lieSneakOverrideActive || (!isDisappeared && RechargeProgress01 >= 1f);
    public bool CanTakeDamage() => !isDisappeared;

    private PlayerStats stats;
    private PlayerCombatController combat;

    private bool isDisappeared;
    private float endTime;
    private float lastUseTime = -999f;
    private float activeDuration;

    private int normalLayer;
    private int ghostLayer;

    private bool lieSneakOverrideActive;

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
        if (lieSneakOverrideActive)
            return;

        if (isDisappeared && Time.time >= endTime)
            EndDisappear();

        if (combat.Mode != CombatMode.Defense) return;
        if (!combat.WantsDisappear) return;
        if (!IsReady) return;

        if (!stats.TrySpendAP(apCost))
            return;
        
        if (UIInputBlocker.BlockGameplayInput)
            return;

        StartDisappear();
    }

    public void BeginLieSneakOverride()
    {
        lieSneakOverrideActive = true;

        if (!isDisappeared)
            ForceGhostStateOn();
    }

    public void EndLieSneakOverride()
    {
        lieSneakOverrideActive = false;

        if (isDisappeared)
            EndDisappear();
    }

    public void BreakLieSneakOverride()
    {
        lieSneakOverrideActive = false;

        if (isDisappeared)
            EndDisappear();
    }

    private void StartDisappear()
    {
        isDisappeared = true;

        activeDuration = GetDisappearDuration();

        endTime = Time.time + activeDuration;
        lastUseTime = Time.time;

        gameObject.layer = ghostLayer;
        SetAlpha(ghostAlpha);
    }

    private void ForceGhostStateOn()
    {
        isDisappeared = true;
        activeDuration = 999999f;
        endTime = float.MaxValue;
        lastUseTime = Time.time;

        gameObject.layer = ghostLayer;
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