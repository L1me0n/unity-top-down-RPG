using UnityEngine;
using TMPro;

public class InventoryStatsPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private APRegen apRegen;

    [Header("Text")]
    [SerializeField] private TMP_Text statsTitleText;
    [SerializeField] private TMP_Text dmgText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text apText;
    [SerializeField] private TMP_Text apRateText;

    [Header("Options")]
    [SerializeField] private bool autoFindReferences = true;

    private void Awake()
    {
        FindReferencesIfNeeded();

        if (statsTitleText != null)
            statsTitleText.text = "Stats";
    }

    private void OnEnable()
    {
        if (playerStats != null)
            playerStats.OnChanged += HandleStatsChanged;
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.OnChanged -= HandleStatsChanged;
    }

    public void Refresh()
    {
        FindReferencesIfNeeded();

        if (playerStats == null)
        {
            SetMissingText();
            return;
        }

        if (dmgText != null)
            dmgText.text = $"DMG: {playerStats.DP}";

        if (hpText != null)
            hpText.text = $"HP: {playerStats.HP} / {playerStats.MaxHP}";

        if (apText != null)
            apText.text = $"AP: {playerStats.AP} / {playerStats.MaxAP}";

        if (apRateText != null)
            apRateText.text = GetAPRateText();
    }

    private void HandleStatsChanged()
    {
        Refresh();
    }

    private void FindReferencesIfNeeded()
    {
        if (!autoFindReferences)
            return;

        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();

        if (apRegen == null)
            apRegen = FindFirstObjectByType<APRegen>();
    }

    private string GetAPRateText()
    {
        if (apRegen == null)
            return "AP Rate: ?";

        // Requires the tiny APRegen getters from F1 Step 1.
        float perSecond = apRegen.APPerSecond;

        if (perSecond > 0f)
            return $"AP Rate: {perSecond:0.##} / sec";

        return $"AP Rate: {apRegen.APPerTick} / tick";
    }

    private void SetMissingText()
    {
        if (dmgText != null)
            dmgText.text = "DMG: ?";

        if (hpText != null)
            hpText.text = "HP: ?";

        if (apText != null)
            apText.text = "AP: ?";

        if (apRateText != null)
            apRateText.text = "AP Rate: ?";
    }
}