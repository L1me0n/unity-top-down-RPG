using UnityEngine;

public class ProgressionApplier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BranchProgression branches;
    [SerializeField] private PlayerStats stats;
    [SerializeField] private APRegen apRegen;
    [SerializeField] private DeathPenaltyTracker penalty;
    [SerializeField] private PlayerDamageReceiver damageReceiver;

    [Header("Tuning")]
    [SerializeField] private int demonDPPerPoint = 2;
    [SerializeField] private float demonDisappearSecondsPerPoint = 0.1f;

    [SerializeField] private int monsterHPPerPoint = 4;
    [SerializeField] private int monsterAPPerPoint = 4;

    [SerializeField] private int fallenGodDPPerPoint = 1;
    [SerializeField] private int fallenGodActionRatePerPoint = 1;

    [Header("Debug")]
    [SerializeField] private bool log = false;

    private void Awake()
    {
        if (branches == null) branches = GetComponent<BranchProgression>();
        if (branches == null) branches = FindFirstObjectByType<BranchProgression>();

        if (stats == null) stats = GetComponent<PlayerStats>();
        if (stats == null) stats = FindFirstObjectByType<PlayerStats>();

        if (apRegen == null) apRegen = GetComponent<APRegen>();

        if (penalty == null) penalty = GetComponent<DeathPenaltyTracker>();

        if (damageReceiver == null) damageReceiver = GetComponent<PlayerDamageReceiver>();
    }

    private void Start()
    {
        // We delay apply until Start to ensure that any Start-based initialization in PlayerStats or APRegen happens first, so we don't accidentally overwrite changes from them.
        ApplyAll();
    }

    private void OnEnable()
    {
        if (branches != null)
            branches.OnBranchChanged += HandleBranchChanged;

        if (damageReceiver != null)
            damageReceiver.OnDied += ApplyAll;

        ApplyAll(); // apply on start/load
    }

    private void OnDisable()
    {
        if (branches != null)
            branches.OnBranchChanged -= HandleBranchChanged;
    }

    private void HandleBranchChanged(BranchType _, int __)
    {
        ApplyAll();
    }

    public void ApplyAll()
    {
        if (branches == null || stats == null) return;

        int demon = branches.Demon;
        int monster = branches.Monster;
        int fallen = branches.FallenGod;

        // Compute totals
        int bonusDP = demon * demonDPPerPoint + fallen * fallenGodDPPerPoint;
        int bonusHP = monster * monsterHPPerPoint;
        int bonusAP = monster * monsterAPPerPoint;

        int bonusActionRate = fallen * fallenGodActionRatePerPoint;

        float bonusDisappear = demon * demonDisappearSecondsPerPoint;

        int hpPenalty = penalty != null ? penalty.MaxHPLoss : 0;
        int apPenalty = penalty != null ? penalty.MaxAPLoss : 0;
        int actionRatePenalty = penalty != null ? penalty.ActionRateLoss : 0;
        int dpPenalty = penalty != null ? penalty.DPLoss : 0;

        int effectiveBonusHP = bonusHP - hpPenalty;
        int effectiveBonusAP = bonusAP - apPenalty;
        int effectiveBonusDP = bonusDP - dpPenalty;

        int effectiveBonusActionRate = bonusActionRate - actionRatePenalty;

        ApplyWithStoredBonuses(effectiveBonusHP, effectiveBonusAP, effectiveBonusDP, effectiveBonusActionRate, bonusDisappear);
    }

    private int appliedBonusHP;
    private int appliedBonusAP;
    private int appliedBonusDP;
    private int appliedBonusActionRate;
    private float appliedBonusDisappear;

    private void ApplyWithStoredBonuses(int newBonusHP, int newBonusAP, int newBonusDP, int newBonusActionRate, float newBonusDisappear)
    {
        // We must avoid stacking upgrades on top of already-upgraded Max values.
        int deltaHP = newBonusHP - appliedBonusHP;
        int deltaAP = newBonusAP - appliedBonusAP;
        int deltaDP = newBonusDP - appliedBonusDP;
        int deltaActionRate = newBonusActionRate - appliedBonusActionRate;
        float deltaDisappear = newBonusDisappear - appliedBonusDisappear;

        if (deltaHP != 0) stats.SetMaxHP(stats.MaxHP + deltaHP);
        if (deltaAP != 0) stats.SetMaxAP(stats.MaxAP + deltaAP);
        if (deltaDP != 0) stats.SetDP(stats.DP + deltaDP);
        if (deltaActionRate != 0 && apRegen != null) apRegen.SetActionRate(apRegen.APPerTick + deltaActionRate);
        if (Mathf.Abs(deltaDisappear) > 0.0001f) stats.SetDisappearDuration(stats.DisappearDuration + deltaDisappear);

        appliedBonusHP = newBonusHP;
        appliedBonusAP = newBonusAP;
        appliedBonusDP = newBonusDP;
        appliedBonusActionRate = newBonusActionRate;
        appliedBonusDisappear = newBonusDisappear;

        if (log)
        {
            Debug.Log($"[ProgressionApplier] Applied bonuses HP:{appliedBonusHP} AP:{appliedBonusAP} DP:{appliedBonusDP} ActionRate:{appliedBonusActionRate} Dis:{appliedBonusDisappear:0.00}s");
        }
    }
}