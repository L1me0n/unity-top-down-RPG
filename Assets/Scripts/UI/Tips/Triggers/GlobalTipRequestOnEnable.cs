using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalTipRequestOnEnable : MonoBehaviour
{
    [Header("Tips")]
    [SerializeField] private List<string> tipIds = new List<string>();

    [Header("Timing")]
    [SerializeField] private float delaySeconds = 0.25f;
    [SerializeField] private int retryCount = 8;
    [SerializeField] private float retryDelaySeconds = 0.25f;

    [Header("Rules")]
    [SerializeField] private bool requestOnlyOncePerObjectEnable = true;

    [Header("Debug")]
    [SerializeField] private bool log = false;

    private bool requestedThisEnable;

    private void OnEnable()
    {
        if (requestOnlyOncePerObjectEnable && requestedThisEnable)
            return;

        requestedThisEnable = true;
        StartCoroutine(RequestRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        requestedThisEnable = false;
    }

    public void RequestNow()
    {
        StartCoroutine(RequestRoutine());
    }

    private IEnumerator RequestRoutine()
    {
        if (delaySeconds > 0f)
            yield return new WaitForSeconds(delaySeconds);

        int attemptsLeft = Mathf.Max(1, retryCount);

        while (attemptsLeft > 0)
        {
            attemptsLeft--;

            if (GlobalTipService.Instance == null)
            {
                if (log)
                    Debug.LogWarning("[GlobalTipRequestOnEnable] No GlobalTipService found yet.", this);

                yield return new WaitForSeconds(retryDelaySeconds);
                continue;
            }

            if (AllTipsAlreadySeen())
            {
                if (log)
                    Debug.Log("[GlobalTipRequestOnEnable] All tips already seen. Skipping.", this);

                yield break;
            }

            bool shown = GlobalTipService.Instance.TryShowTipSequence(tipIds);

            if (shown)
            {
                if (log)
                    Debug.Log("[GlobalTipRequestOnEnable] Requested tip sequence successfully.", this);

                yield break;
            }

            yield return new WaitForSeconds(retryDelaySeconds);
        }

        if (log)
            Debug.Log("[GlobalTipRequestOnEnable] Tip request failed after retries.", this);
    }

    private bool AllTipsAlreadySeen()
    {
        if (GlobalTipService.Instance == null)
            return false;

        if (tipIds == null || tipIds.Count == 0)
            return true;

        for (int i = 0; i < tipIds.Count; i++)
        {
            string tipId = tipIds[i];

            if (string.IsNullOrWhiteSpace(tipId))
                continue;

            if (!GlobalTipService.Instance.HasSeenTip(tipId))
                return false;
        }

        return true;
    }
}