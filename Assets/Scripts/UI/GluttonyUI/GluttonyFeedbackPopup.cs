using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GluttonyFeedbackPopup : MonoBehaviour
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

    [Header("Font Colors")]
    [SerializeField] private Color neutralTextColor = Color.white;
    [SerializeField] private Color successTextColor = new Color(0.35f, 0.95f, 0.35f, 1f);
    [SerializeField] private Color failTextColor = new Color(1f, 0.35f, 0.35f, 1f);

    [Header("Timing")]
    [SerializeField] private float fadeInSeconds = 0.08f;
    [SerializeField] private float holdSeconds = 1.0f;
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
            Debug.Log($"[GluttonyFeedbackPopup] Queued message: {message} | style={style}", this);
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
        if (popupText == null)
            return;

        Color c = neutralTextColor;

        switch (style)
        {
            case PopupStyle.Success:
                c = successTextColor;
                break;
            case PopupStyle.Fail:
                c = failTextColor;
                break;
            case PopupStyle.Neutral:
            default:
                c = neutralTextColor;
                break;
        }

        c.a = popupText.color.a;
        popupText.color = c;
    }

    private IEnumerator FadeAlpha(float from, float to, float seconds)
    {
        if (popupText == null)
            yield break;

        if (seconds <= 0f)
        {
            SetTextAlpha(to);
            yield break;
        }

        float t = 0f;
        SetTextAlpha(from);

        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / seconds));
            SetTextAlpha(a);
            yield return null;
        }

        SetTextAlpha(to);
    }

    private void SetTextAlpha(float a)
    {
        if (popupText == null)
            return;

        Color c = popupText.color;
        c.a = a;
        popupText.color = c;
    }
}