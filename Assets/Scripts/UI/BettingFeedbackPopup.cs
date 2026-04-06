using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BettingFeedbackPopup : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject rootPanel;

    [Header("UI")]
    [SerializeField] private TMP_Text popupText;

    [Header("Timing")]
    [SerializeField] private float fadeInSeconds = 0.08f;
    [SerializeField] private float holdSeconds = 0.9f;
    [SerializeField] private float fadeOutSeconds = 0.35f;
    [SerializeField] private float gapBetweenMessages = 0.06f;

    [Header("Behavior")]
    [SerializeField] private bool hideRootOnAwake = true;

    [Header("Debug")]
    [SerializeField] private bool log = false;

    private readonly Queue<string> pendingMessages = new Queue<string>();
    private Coroutine routine;

    private void Awake()
    {
        if (hideRootOnAwake && rootPanel != null)
            rootPanel.SetActive(false);

        if (popupText != null)
        {
            var c = popupText.color;
            c.a = 0f;
            popupText.color = c;
        }
    }

    public void ShowMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message) || popupText == null)
            return;

        pendingMessages.Enqueue(message);

        if (routine == null)
            routine = StartCoroutine(RunQueue());

        if (log)
            Debug.Log($"[BettingFeedbackPopup] Queued message: {message}", this);
    }

    private IEnumerator RunQueue()
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);

        while (pendingMessages.Count > 0)
        {
            string message = pendingMessages.Dequeue();
            popupText.text = message;

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

    private IEnumerator FadeAlpha(float from, float to, float seconds)
    {
        if (popupText == null)
            yield break;

        if (seconds <= 0f)
        {
            SetAlpha(to);
            yield break;
        }

        float t = 0f;
        SetAlpha(from);

        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / seconds));
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(to);
    }

    private void SetAlpha(float a)
    {
        if (popupText == null)
            return;

        var c = popupText.color;
        c.a = a;
        popupText.color = c;
    }
}