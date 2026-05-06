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

    [Header("Debug")]
    [SerializeField] private bool logEctoplasmBonus = false;

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

        if (normalLayer < 0)
            Debug.LogError($"[Disappear] Layer not found: {normalLayerName}", this);

        if (ghostLayer < 0)
            Debug.LogError($"[Disappear] Layer not found: {ghostLayerName}", this);

        if (stats == null)
            Debug.LogError("[Disappear] PlayerStats missing on player.", this);

        if (combat == null)
            Debug.LogError("[Disappear] PlayerCombatController missing on player.", this);

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    private void Update()
    {
        if (lieSneakOverrideActive)
            return;

        if (isDisappeared && Time.time >= endTime)
            EndDisappear();

        if (UIInputBlocker.BlockGameplayInput)
            return;

        if (combat == null || stats == null)
            return;

        if (combat.Mode != CombatMode.Defense)
            return;

        if (!combat.WantsDisappear)
            return;

        if (!IsReady)
            return;

        if (!stats.TrySpendAP(apCost))
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
        float baseDuration = stats != null ? stats.DisappearDuration : 0.1f;
        float bonusDuration = 0f;

        if (TradeItemEffectManager.Instance != null &&
            TradeItemEffectManager.Instance.IsEctoplasmActive)
        {
            bonusDuration = TradeItemEffectManager.Instance.EctoplasmDisappearBonusSeconds;
        }

        float finalDuration = Mathf.Max(0.1f, baseDuration + bonusDuration);

        if (logEctoplasmBonus && bonusDuration > 0f)
        {
            Debug.Log(
                $"[Disappear] Ectoplasm Potion bonus applied. " +
                $"Base={baseDuration:0.00}s Bonus={bonusDuration:0.00}s Final={finalDuration:0.00}s",
                this
            );
        }

        return finalDuration;
    }

    private void SetAlpha(float a)
    {
        if (renderers == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;

            Color c = renderers[i].color;
            c.a = a;
            renderers[i].color = c;
        }
    }
}