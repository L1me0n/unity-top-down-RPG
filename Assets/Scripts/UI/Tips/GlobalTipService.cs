using System.Collections.Generic;
using UnityEngine;

public class GlobalTipService : MonoBehaviour
{
    public static GlobalTipService Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GlobalTipDatabase tipDatabase;
    [SerializeField] private GlobalTipSeenManager seenManager;
    [SerializeField] private PagedTipsPanelUI tipsPanelUI;

    [Header("Behavior")]
    [SerializeField] private bool autoFindReferences = true;
    [SerializeField] private bool doNotOpenOverBlockingUI = true;

    [Header("Debug")]
    [SerializeField] private bool log = false;

    private readonly Queue<List<GlobalTipDefinition>> queuedTipGroups = new Queue<List<GlobalTipDefinition>>();
    private bool isShowing;

    public bool IsShowing => isShowing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        FindReferencesIfNeeded();
    }

    public bool HasSeenTip(string tipId)
    {
        FindReferencesIfNeeded();

        if (seenManager == null)
            return false;

        return seenManager.HasSeen(tipId);
    }

    public bool TryShowTip(string tipId)
    {
        if (string.IsNullOrWhiteSpace(tipId))
            return false;

        return TryShowTipSequence(new List<string> { tipId });
    }

    public bool TryShowTipSequence(List<string> tipIds)
    {
        FindReferencesIfNeeded();

        if (tipDatabase == null)
        {
            LogWarning("No GlobalTipDatabase assigned.");
            return false;
        }

        if (tipsPanelUI == null)
        {
            LogWarning("No PagedTipsPanelUI found.");
            return false;
        }

        if (seenManager == null)
        {
            LogWarning("No GlobalTipSeenManager found.");
            return false;
        }

        if (doNotOpenOverBlockingUI && UIInputBlocker.BlockGameplayInput && !UIInputBlocker.IsGameplayBlockedBy(UIInputBlocker.LockTipsMenu))
        {
            if (log)
                Debug.Log("[GlobalTipService] Refused to open tip because another blocking UI is active.", this);

            return false;
        }

        List<GlobalTipDefinition> tipsToShow = BuildUnseenTipList(tipIds);

        if (tipsToShow.Count == 0)
        {
            if (log)
                Debug.Log("[GlobalTipService] No unseen tips to show.", this);

            return false;
        }

        if (isShowing)
        {
            queuedTipGroups.Enqueue(tipsToShow);
            return true;
        }

        ShowTipGroup(tipsToShow);
        return true;
    }

    public void MarkTipSeen(string tipId)
    {
        FindReferencesIfNeeded();

        if (seenManager != null)
            seenManager.MarkSeen(tipId);
    }

    public void ResetAllSeenTips()
    {
        FindReferencesIfNeeded();

        if (seenManager != null)
            seenManager.ClearSeenTips();
    }

    private List<GlobalTipDefinition> BuildUnseenTipList(List<string> tipIds)
    {
        List<GlobalTipDefinition> result = new List<GlobalTipDefinition>();

        if (tipIds == null)
            return result;

        for (int i = 0; i < tipIds.Count; i++)
        {
            string tipId = tipIds[i];

            if (string.IsNullOrWhiteSpace(tipId))
                continue;

            GlobalTipDefinition tip = tipDatabase.GetTip(tipId);

            if (tip == null)
            {
                if (log)
                    Debug.LogWarning($"[GlobalTipService] No tip found for id: {tipId}", this);

                continue;
            }

            if (tip.showOnlyOnce && seenManager.HasSeen(tip.tipId))
                continue;

            result.Add(tip);
        }

        return result;
    }

    private void ShowTipGroup(List<GlobalTipDefinition> tips)
    {
        if (tips == null || tips.Count == 0)
        {
            TryShowNextQueuedGroup();
            return;
        }

        isShowing = true;

        List<TipPageData> pages = new List<TipPageData>();

        for (int i = 0; i < tips.Count; i++)
        {
            GlobalTipDefinition tip = tips[i];

            if (tip == null)
                continue;

            pages.Add(new TipPageData(tip.title, tip.body));

            // Mark as seen when shown, not when closed.
            // This avoids repeated re-trigger loops if the player closes fast.
            if (tip.showOnlyOnce && seenManager != null)
                seenManager.MarkSeen(tip.tipId);
        }

        tipsPanelUI.ShowPages(pages, HandleTipGroupFinished);

        if (log)
            Debug.Log($"[GlobalTipService] Showing {pages.Count} global tip page(s).", this);
    }

    private void HandleTipGroupFinished()
    {
        isShowing = false;
        TryShowNextQueuedGroup();
    }

    private void TryShowNextQueuedGroup()
    {
        if (queuedTipGroups.Count <= 0)
            return;

        List<GlobalTipDefinition> nextGroup = queuedTipGroups.Dequeue();
        ShowTipGroup(nextGroup);
    }

    private void FindReferencesIfNeeded()
    {
        if (!autoFindReferences)
            return;

        if (seenManager == null)
            seenManager = FindFirstObjectByType<GlobalTipSeenManager>();

        if (tipsPanelUI == null)
            tipsPanelUI = FindFirstObjectByType<PagedTipsPanelUI>();
    }

    private void LogWarning(string message)
    {
        Debug.LogWarning($"[GlobalTipService] {message}", this);
    }
}