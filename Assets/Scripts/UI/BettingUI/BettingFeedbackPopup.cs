using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BettingFeedbackPopup : MonoBehaviour
{
    public enum PopupStyle
    {
        Neutral,
        Success,
        Fail
    }

    [Header("Root")]
    [SerializeField] private GameObject rootPanel;

    [Header("UI")]
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private Image backgroundImage;

    [Header("Colors")]
    [SerializeField] private Color neutralBackgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.92f);
    [SerializeField] private Color successBackgroundColor = new Color(0.14f, 0.45f, 0.18f, 0.94f);
    [SerializeField] private Color failBackgroundColor = new Color(0.55f, 0.12f, 0.12f, 0.94f);

    [Header("Timing")]
    [SerializeField] private float fadeInSeconds = 0.08f;
    [SerializeField] private float holdSeconds = 0.9f;
    [SerializeField] private float fadeOutSeconds = 0.35f;
    [SerializeField] private float gapBetweenMessages = 0.06f;

    [Header("Behavior")]
    [SerializeField] private bool hideRootOnAwake = true;

    [Header("Debug")]
    [SerializeField] private bool log = false;

    private struct PendingPopup
    {
        public string message;
        public PopupStyle style;

        public PendingPopup(string message, PopupStyle style)
        {
            this.message = message;
            this.style = style;
        }
    }

    private readonly Queue<PendingPopup> pendingMessages = new Queue<PendingPopup>();
    private Coroutine routine;

    private void Awake()
    {
        if (hideRootOnAwake && rootPanel != null)
            rootPanel.SetActive(false);

        SetTextAlpha(0f);
        SetBackgroundAlpha(0f);
    }

    public void ShowMessage(string message)
    {
        ShowStyledMessage(message, PopupStyle.Neutral);
    }

    public void ShowSuccessMessage(string message)
    {
        ShowStyledMessage(message, PopupStyle.Success);
    }

    public void ShowFailMessage(string message)
    {
        ShowStyledMessage(message, PopupStyle.Fail);
    }

    public void ShowStyledMessage(string message, PopupStyle style)
    {
        if (string.IsNullOrWhiteSpace(message) || popupText == null)
            return;

        pendingMessages.Enqueue(new PendingPopup(message, style));

        if (routine == null)
            routine = StartCoroutine(RunQueue());

        if (log)
            Debug.Log($"[BettingFeedbackPopup] Queued message: {message} | style={style}", this);
    }

    private IEnumerator RunQueue()
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);

        while (pendingMessages.Count > 0)
        {
            PendingPopup popup = pendingMessages.Dequeue();

            popupText.text = popup.message;
            ApplyStyle(popup.style);

            yield return FadeAlpha(0f, 1f, fadeInSeconds);
            yield return new WaitForSecondsRealtime(holdSeconds);
            yield return FadeAlpha(1f, 0f, fadeOutSeconds);

            if (pendingMessages.Count > 0 && gapBetweenMessages > 0f)
                yield return new WaitForSecondsRealtime(gapBetweenMessages);
        }

        if (rootPanel != null)
            rootPanel.SetActive(false);

        routine = null;
    }

    private void ApplyStyle(PopupStyle style)
    {
        if (backgroundImage == null)
            return;

        Color baseColor = neutralBackgroundColor;

        switch (style)
        {
            case PopupStyle.Success:
                baseColor = successBackgroundColor;
                break;
            case PopupStyle.Fail:
                baseColor = failBackgroundColor;
                break;
            case PopupStyle.Neutral:
            default:
                baseColor = neutralBackgroundColor;
                break;
        }

        backgroundImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
    }

    private IEnumerator FadeAlpha(float from, float to, float seconds)
    {
        if (popupText == null)
            yield break;

        if (seconds <= 0f)
        {
            SetTextAlpha(to);
            SetBackgroundAlpha(to);
            yield break;
        }

        float t = 0f;
        SetTextAlpha(from);
        SetBackgroundAlpha(from);

        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / seconds));
            SetTextAlpha(a);
            SetBackgroundAlpha(a);
            yield return null;
        }

        SetTextAlpha(to);
        SetBackgroundAlpha(to);
    }

    private void SetTextAlpha(float a)
    {
        if (popupText == null)
            return;

        Color c = popupText.color;
        c.a = a;
        popupText.color = c;
    }

    private void SetBackgroundAlpha(float a)
    {
        if (backgroundImage == null)
            return;

        Color c = backgroundImage.color;
        float targetMax = GetCurrentBackgroundBaseAlpha();
        c.a = targetMax * Mathf.Clamp01(a);
        backgroundImage.color = c;
    }

    private float GetCurrentBackgroundBaseAlpha()
    {
        if (backgroundImage == null)
            return 0f;

        Color c = backgroundImage.color;
        return Mathf.Max(neutralBackgroundColor.a, Mathf.Max(successBackgroundColor.a, Mathf.Max(failBackgroundColor.a, c.a)));
    }
}