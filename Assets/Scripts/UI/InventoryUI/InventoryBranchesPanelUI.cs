using System.Text;
using UnityEngine;
using TMPro;

public class InventoryBranchesPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BranchProgression branchProgression;
    [SerializeField] private LevelSystem levelSystem;

    [Header("Text")]
    [SerializeField] private TMP_Text branchesTitleText;
    [SerializeField] private TMP_Text branchesListText;

    [Header("Options")]
    [SerializeField] private bool autoFindReferences = true;
    [SerializeField] private bool showBranchDescriptions = true;

    private void Awake()
    {
        FindReferencesIfNeeded();

        if (branchesTitleText != null)
            branchesTitleText.text = "Branches";
    }

    private void OnEnable()
    {
        if (branchProgression != null)
            branchProgression.OnBranchChanged += HandleBranchChanged;

        if (levelSystem != null)
            levelSystem.OnUnspentPointsChanged += HandleUnspentPointsChanged;
    }

    private void OnDisable()
    {
        if (branchProgression != null)
            branchProgression.OnBranchChanged -= HandleBranchChanged;

        if (levelSystem != null)
            levelSystem.OnUnspentPointsChanged -= HandleUnspentPointsChanged;
    }

    public void Refresh()
    {
        FindReferencesIfNeeded();

        if (branchesListText == null)
            return;

        if (branchProgression == null)
        {
            branchesListText.text = "No branch progression found.";
            return;
        }

        StringBuilder sb = new StringBuilder();

        if (levelSystem != null)
        {
            sb.AppendLine($"Level: {levelSystem.Level}");
            sb.AppendLine($"Unspent Points: {levelSystem.UnspentPoints}");
            sb.AppendLine();
        }

        AppendBranch(sb, BranchType.Demon);
        AppendBranch(sb, BranchType.Monster);
        AppendBranch(sb, BranchType.FallenGod);
        AppendBranch(sb, BranchType.Hellhound);

        branchesListText.text = sb.ToString();
    }

    private void AppendBranch(StringBuilder sb, BranchType branch)
    {
        int points = branchProgression.GetPoints(branch);
        int cap = branchProgression.MaxPointsPerBranch;

        sb.Append("• ");
        sb.Append(GetDisplayName(branch));
        sb.Append(": Lv ");
        sb.Append(points);
        sb.Append("/");
        sb.Append(cap);
        sb.AppendLine();

        if (showBranchDescriptions)
        {
            sb.Append("  ");
            sb.AppendLine();
        }
    }

    private void HandleBranchChanged(BranchType branch, int newValue)
    {
        Refresh();
    }

    private void HandleUnspentPointsChanged(int newValue)
    {
        Refresh();
    }

    private void FindReferencesIfNeeded()
    {
        if (!autoFindReferences)
            return;

        if (branchProgression == null)
            branchProgression = FindFirstObjectByType<BranchProgression>();

        if (levelSystem == null)
            levelSystem = FindFirstObjectByType<LevelSystem>();
    }

    private string GetDisplayName(BranchType branch)
    {
        switch (branch)
        {
            case BranchType.Demon:
                return "Demon";

            case BranchType.Monster:
                return "Monster";

            case BranchType.FallenGod:
                return "Fallen God";

            case BranchType.Hellhound:
                return "Hellhound";

            default:
                return branch.ToString();
        }
    }

    private string GetDescription(BranchType branch)
    {
        switch (branch)
        {
            case BranchType.Demon:
                return "Damage and Disappear growth.";

            case BranchType.Monster:
                return "HP and AP growth.";

            case BranchType.FallenGod:
                return "Balanced stat growth.";

            case BranchType.Hellhound:
                return "Room-entry execution chance.";

            default:
                return "";
        }
    }
}