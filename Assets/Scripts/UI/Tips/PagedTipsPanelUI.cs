using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PagedTipsPanelUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject panelRoot;

    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text pageCounterText;
    [SerializeField] private TMP_Text hintText;

    [Header("Buttons")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button closeButton;

    [Header("Input")]
    [SerializeField] private KeyCode nextKey = KeyCode.Space;
    [SerializeField] private KeyCode alternateNextKey = KeyCode.Return;
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;

    [Header("Blocking")]
    [SerializeField] private string ownerKey = UIInputBlocker.LockTipsMenu;
    [SerializeField] private bool blockGameplayWhileOpen = true;
    [SerializeField] private bool pauseGameWhileOpen = true;

    [Header("Behavior")]
    [SerializeField] private bool hideOnAwake = true;
    [SerializeField] private bool log = false;

    private readonly List<TipPageData> pages = new List<TipPageData>();
    private int currentPageIndex = 0;
    private Action onFinished;
    private bool isOpen;
    private float previousTimeScale = 1f;

    public bool IsOpen => isOpen;
    public int CurrentPageIndex => currentPageIndex;
    public int PageCount => pages.Count;

    private void Awake()
    {
        if (hideOnAwake)
            HideImmediate();

        if (previousButton != null)
            previousButton.onClick.AddListener(GoPrevious);

        if (nextButton != null)
            nextButton.onClick.AddListener(GoNext);

        if (continueButton != null)
            continueButton.onClick.AddListener(FinishAndClose);

        if (closeButton != null)
            closeButton.onClick.AddListener(FinishAndClose);

        if (hintText != null)
            hintText.text = "Space / Enter: next   Esc: close";
    }

    private void OnDestroy()
    {
        if (previousButton != null)
            previousButton.onClick.RemoveListener(GoPrevious);

        if (nextButton != null)
            nextButton.onClick.RemoveListener(GoNext);

        if (continueButton != null)
            continueButton.onClick.RemoveListener(FinishAndClose);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(FinishAndClose);

        if (isOpen)
            ReleaseLocksAndTime();
    }

    private void Update()
    {
        if (!isOpen)
            return;

        if (Input.GetKeyDown(closeKey))
        {
            FinishAndClose();
            return;
        }

        if (Input.GetKeyDown(nextKey) || Input.GetKeyDown(alternateNextKey))
        {
            if (currentPageIndex < pages.Count - 1)
                GoNext();
            else
                FinishAndClose();
        }
    }

    public void ShowPages(IEnumerable<TipPageData> newPages, Action finishedCallback = null)
    {
        pages.Clear();

        if (newPages != null)
        {
            foreach (TipPageData page in newPages)
            {
                if (page == null)
                    continue;

                pages.Add(page);
            }
        }

        onFinished = finishedCallback;
        currentPageIndex = 0;

        if (pages.Count == 0)
        {
            if (log)
                Debug.Log("[PagedTipsPanelUI] ShowPages called with no pages.");

            FinishAndClose();
            return;
        }

        isOpen = true;

        if (panelRoot != null)
            panelRoot.SetActive(true);
        else
            gameObject.SetActive(true);

        ApplyLocksAndTime();
        RefreshView();

        if (log)
            Debug.Log($"[PagedTipsPanelUI] Opened tips panel with {pages.Count} page(s).", this);
    }

    public void HideImmediate()
    {
        isOpen = false;

        if (panelRoot != null)
            panelRoot.SetActive(false);
        else
            gameObject.SetActive(false);

        ReleaseLocksAndTime();
    }

    private void GoPrevious()
    {
        if (pages.Count == 0)
            return;

        if (currentPageIndex <= 0)
            return;

        currentPageIndex--;
        RefreshView();
    }

    private void GoNext()
    {
        if (pages.Count == 0)
            return;

        if (currentPageIndex >= pages.Count - 1)
            return;

        currentPageIndex++;
        RefreshView();
    }

    private void FinishAndClose()
    {
        if (log)
            Debug.Log("[PagedTipsPanelUI] Closed tips panel.", this);

        HideImmediate();

        Action callback = onFinished;
        onFinished = null;
        callback?.Invoke();
    }

    private void RefreshView()
    {
        if (pages.Count == 0)
            return;

        currentPageIndex = Mathf.Clamp(currentPageIndex, 0, pages.Count - 1);

        TipPageData page = pages[currentPageIndex];

        if (titleText != null)
            titleText.text = page.title;

        if (bodyText != null)
            bodyText.text = page.body;

        if (pageCounterText != null)
            pageCounterText.text = $"{currentPageIndex + 1}/{pages.Count}";

        bool hasPrevious = currentPageIndex > 0;
        bool hasNext = currentPageIndex < pages.Count - 1;
        bool isLastPage = currentPageIndex == pages.Count - 1;

        if (previousButton != null)
            previousButton.gameObject.SetActive(hasPrevious);

        if (nextButton != null)
            nextButton.gameObject.SetActive(hasNext);

        if (continueButton != null)
            continueButton.gameObject.SetActive(isLastPage);

        if (closeButton != null)
            closeButton.gameObject.SetActive(true);
    }

    private void ApplyLocksAndTime()
    {
        if (blockGameplayWhileOpen)
        {
            UIInputBlocker.SetGameplayBlocked(ownerKey, true);
            UIInputBlocker.SetUpgradeMenuBlocked(ownerKey, true);
            UIInputBlocker.SetPauseToggleBlocked(ownerKey, true);
            UIInputBlocker.SetInventoryToggleBlocked(ownerKey, true);
            UIInputBlocker.SetClueMenuToggleBlocked(ownerKey, true);
            UIInputBlocker.SetTradeItemHotkeysBlocked(ownerKey, true);
        }

        if (pauseGameWhileOpen)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    private void ReleaseLocksAndTime()
    {
        UIInputBlocker.ReleaseOwner(ownerKey);

        if (pauseGameWhileOpen)
            Time.timeScale = previousTimeScale;
    }
}