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

    [Header("Buttons")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button closeButton;

    [Header("Behavior")]
    [SerializeField] private bool hideOnAwake = true;
    [SerializeField] private bool log = false;

    private readonly List<TipPageData> pages = new List<TipPageData>();
    private int currentPageIndex = 0;
    private Action onFinished;

    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;
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

        if (panelRoot != null)
            panelRoot.SetActive(true);
        else
            gameObject.SetActive(true);

        UIInputBlocker.BlockGameplayInput = true;

        RefreshView();

        if (log)
            Debug.Log($"[PagedTipsPanelUI] Opened tips panel with {pages.Count} page(s).", this);
    }

    public void HideImmediate()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
        else
            gameObject.SetActive(false);

        UIInputBlocker.BlockGameplayInput = false;
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
}