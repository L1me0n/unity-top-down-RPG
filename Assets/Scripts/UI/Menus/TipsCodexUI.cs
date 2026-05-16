using System.Collections.Generic;
using UnityEngine;

public class TipsCodexUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GlobalTipDatabase tipDatabase;
    [SerializeField] private PagedTipsPanelUI tipsPanelUI;

    [Header("Options")]
    [SerializeField] private bool autoFindReferences = true;
    [SerializeField] private bool includeOnlyNonEmptyTips = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    public void OpenTips()
    {
        FindReferencesIfNeeded();

        if (tipDatabase == null)
        {
            Debug.LogWarning("[TipsCodexUI] Cannot open tips because GlobalTipDatabase is missing.", this);
            return;
        }

        if (tipsPanelUI == null)
        {
            Debug.LogWarning("[TipsCodexUI] Cannot open tips because PagedTipsPanelUI is missing.", this);
            return;
        }

        List<TipPageData> pages = BuildPages();

        if (pages.Count <= 0)
        {
            Debug.LogWarning("[TipsCodexUI] No valid tips found in database.", this);
            return;
        }

        tipsPanelUI.ShowPages(pages);

        if (debugLogs)
            Debug.Log($"[TipsCodexUI] Opened tips codex with {pages.Count} page(s).", this);
    }

    private List<TipPageData> BuildPages()
    {
        List<TipPageData> pages = new List<TipPageData>();

        IReadOnlyList<GlobalTipDefinition> tips = tipDatabase.Tips;

        for (int i = 0; i < tips.Count; i++)
        {
            GlobalTipDefinition tip = tips[i];

            if (tip == null)
                continue;

            if (includeOnlyNonEmptyTips)
            {
                if (string.IsNullOrWhiteSpace(tip.title))
                    continue;

                if (string.IsNullOrWhiteSpace(tip.body))
                    continue;
            }

            string title = tip.title;
            string body = tip.body;

            pages.Add(new TipPageData(title, body));
        }

        return pages;
    }

    private void FindReferencesIfNeeded()
    {
        if (!autoFindReferences)
            return;

        if (tipsPanelUI == null)
            tipsPanelUI = FindFirstObjectByType<PagedTipsPanelUI>();
    }
}