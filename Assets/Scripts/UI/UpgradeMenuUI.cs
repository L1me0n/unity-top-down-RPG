using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private LevelSystem levelSystem;
    [SerializeField] private BranchProgression progression;

    [Header("Top Readout")]
    [SerializeField] private TMP_Text unspentText;

    [Header("Branch Rows (Text + Button)")]
    [SerializeField] private TMP_Text demonText;
    [SerializeField] private Button demonButton;

    [SerializeField] private TMP_Text monsterText;
    [SerializeField] private Button monsterButton;

    [SerializeField] private TMP_Text fallenGodText;
    [SerializeField] private Button fallenGodButton;

    [SerializeField] private TMP_Text hellhoundText;
    [SerializeField] private Button hellhoundButton;

    private KeyCode toggleKey = KeyCode.E;

    private void Awake()
    {
        if (levelSystem == null) levelSystem = FindFirstObjectByType<LevelSystem>();
        if (progression == null) progression = FindFirstObjectByType<BranchProgression>();

        if (rootPanel != null)
            rootPanel.SetActive(false);

        // Button hooks
        if (demonButton != null) demonButton.onClick.AddListener(() => Spend(BranchType.Demon));
        if (monsterButton != null) monsterButton.onClick.AddListener(() => Spend(BranchType.Monster));
        if (fallenGodButton != null) fallenGodButton.onClick.AddListener(() => Spend(BranchType.FallenGod));
        if (hellhoundButton != null) hellhoundButton.onClick.AddListener(() => Spend(BranchType.Hellhound));
    }

    private void OnEnable()
    {
        if (levelSystem != null)
        {
            levelSystem.OnUnspentPointsChanged += HandleUnspentChanged;
            levelSystem.OnLevelChanged += HandleLevelChanged;
        }

        if (progression != null)
        {
            progression.OnBranchChanged += HandleBranchChanged;
            progression.OnSpendFailed += HandleSpendFailed;
        }

        RefreshAll();
    }

    private void OnDisable()
    {
        if (levelSystem != null)
        {
            levelSystem.OnUnspentPointsChanged -= HandleUnspentChanged;
            levelSystem.OnLevelChanged -= HandleLevelChanged;
        }

        if (progression != null)
        {
            progression.OnBranchChanged -= HandleBranchChanged;
            progression.OnSpendFailed -= HandleSpendFailed;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    private void Toggle()
    {
        if (rootPanel == null) return;

        bool newState = !rootPanel.activeSelf;
        rootPanel.SetActive(newState);

        // Block gameplay input while menu open
        UIInputBlocker.BlockGameplayInput = newState;

        // Pause while upgrading (feels nicer)
        Time.timeScale = newState ? 0f : 1f;

        RefreshAll();
    }

    private void Spend(BranchType branch)
    {
        if (progression == null) return;
        bool ok = progression.TrySpendPoint(branch);

        RefreshAll();
    }

    // Event handlers
    private void HandleUnspentChanged(int _)
    {
        RefreshAll();
    }

    private void HandleLevelChanged(int _)
    {
        RefreshAll();
    }

    private void HandleBranchChanged(BranchType _, int __)
    {
        RefreshAll();
    }

    private void HandleSpendFailed(string reason)
    {
        // Keep it simple for now
        Debug.Log($"[UpgradeMenuUI] Spend failed: {reason}");
    }

    // UI Refresh
    private void RefreshAll()
    {
        if (levelSystem == null || progression == null) return;

        if (unspentText != null)
            unspentText.text = $"Points: {levelSystem.UnspentPoints}";

        SetBranchRow(BranchType.Demon, demonText, demonButton);
        SetBranchRow(BranchType.Monster, monsterText, monsterButton);
        SetBranchRow(BranchType.FallenGod, fallenGodText, fallenGodButton);
        SetBranchRow(BranchType.Hellhound, hellhoundText, hellhoundButton);
    }

    private void SetBranchRow(BranchType branch, TMP_Text text, Button button)
    {
        int pts = progression.GetPoints(branch);
        int cap = progression.MaxPointsPerBranch;

        if (text != null)
            text.text = $"{branch}: {pts}/{cap}";

        bool canSpend = levelSystem.UnspentPoints > 0 && pts < cap;
        if (button != null)
            button.interactable = canSpend;
    }
}