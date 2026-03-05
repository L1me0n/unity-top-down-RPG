using UnityEngine;

public class ProgressionApplier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BranchProgression branches;
    [SerializeField] private PlayerStats stats;
    [SerializeField] private APRegen apRegen;

    [Header("Tuning")]
    [SerializeField] private int demonDPPerPoint = 2;
    [SerializeField] private float demonDisappearSecondsPerPoint = 0.1f;

    [SerializeField] private int monsterHPPerPoint = 4;
    [SerializeField] private int monsterAPPerPoint = 4;
    [SerializeField] private int monsterActionRatePerPoint = 1;

    [SerializeField] private int fallenGodDPPerPoint = 1;
    [SerializeField] private int fallenGodHPPerPoint = 1;
    [SerializeField] private int fallenGodAPPerPoint = 1;

    [Header("Debug")]
    [SerializeField] private bool log = false;

    private void Awake()
    {
        if (branches == null) branches = GetComponent<BranchProgression>();
        if (branches == null) branches = FindFirstObjectByType<BranchProgression>();

        if (stats == null) stats = GetComponent<PlayerStats>();
        if (stats == null) stats = FindFirstObjectByType<PlayerStats>();
    }

    private void OnEnable()
    {
        if (branches != null)
            branches.OnBranchChanged += HandleBranchChanged;

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
        int bonusHP = monster * monsterHPPerPoint + fallen * fallenGodHPPerPoint;
        int bonusAP = monster * monsterAPPerPoint + fallen * fallenGodAPPerPoint;

        int bonusActionRate = monster * monsterActionRatePerPoint;

        float bonusDisappear = demon * demonDisappearSecondsPerPoint;

        ApplyWithStoredBonuses(bonusHP, bonusAP, bonusDP, bonusActionRate, bonusDisappear);
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