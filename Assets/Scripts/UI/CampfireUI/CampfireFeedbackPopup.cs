using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CampfireFeedbackPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private CampfireRecoveryHandler recoveryHandler;

    [Header("UI")]
    [SerializeField] private TMP_Text popupText;

    [Header("Timing")]
    [SerializeField] private float fadeInSeconds = 0.08f;
    [SerializeField] private float holdSeconds = 0.55f;
    [SerializeField] private float fadeOutSeconds = 0.45f;
    [SerializeField] private float gapBetweenMessages = 0.08f;

    [Header("Debug")]
    [SerializeField] private bool logFeedback = false;

    private readonly Queue<string> pendingMessages = new Queue<string>();
    private Coroutine routine;

    private void Awake()
    {
        if (roomManager == null)
            roomManager = FindFirstObjectByType<RoomManager>();

        if (recoveryHandler == null)
            recoveryHandler = FindFirstObjectByType<CampfireRecoveryHandler>();

        if (popupText != null)
        {
            var c = popupText.color;
            c.a = 0f;
            popupText.color = c;
            popupText.gameObject.SetActive(true);
        }
    }

    private void OnEnable()
    {
        if (roomManager != null)
            roomManager.OnCheckpointActivated += HandleCheckpointActivated;

        if (recoveryHandler != null)
            recoveryHandler.OnCampfireRecovered += HandleCampfireRecovered;
    }

    private void OnDisable()
    {
        if (roomManager != null)
            roomManager.OnCheckpointActivated -= HandleCheckpointActivated;

        if (recoveryHandler != null)
            recoveryHandler.OnCampfireRecovered -= HandleCampfireRecovered;
    }

    private void HandleCheckpointActivated(Vector2Int coord)
    {
        EnqueueMessage("CHECKPOINT REACHED");

        if (logFeedback)
            Debug.Log($"[CampfireFeedbackPopup] Queued checkpoint popup for {coord}.");
    }

    private void HandleCampfireRecovered(CampfireRecoveryHandler.CampfireRecoveryResult result)
    {
        string message = BuildRestoreMessage(result.restoredHP, result.restoredAP);
        if (string.IsNullOrEmpty(message))
            return;

        EnqueueMessage(message);

        if (logFeedback)
        {
            Debug.Log(
                $"[CampfireFeedbackPopup] Queued recovery popup for {result.coord} | " +
                $"restoredHP={result.restoredHP}, restoredAP={result.restoredAP}");
        }
    }

    private string BuildRestoreMessage(int restoredHP, int restoredAP)
    {
        List<string> parts = new List<string>();

        if (restoredHP > 0)
            parts.Add($"HP +{restoredHP}");

        if (restoredAP > 0)
            parts.Add($"AP +{restoredAP}");

        if (parts.Count == 0)
            return string.Empty;

        return "RESTORED  " + string.Join("  ", parts);
    }

    private void EnqueueMessage(string message)
    {
        if (popupText == null || string.IsNullOrWhiteSpace(message))
            return;

        pendingMessages.Enqueue(message);

        if (routine == null)
            routine = StartCoroutine(RunQueue());
    }

    private IEnumerator RunQueue()
    {
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