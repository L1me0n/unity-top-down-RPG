using System.Collections.Generic;
using UnityEngine;

public class GlobalTipRequestOnTriggerEnter2D : MonoBehaviour
{
    [Header("Tips")]
    [SerializeField] private List<string> tipIds = new List<string>();

    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";

    [Header("Rules")]
    [SerializeField] private bool disableAfterSuccessfulRequest = true;

    [Header("Debug")]
    [SerializeField] private bool log = false;

    private bool hasRequested;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasRequested)
            return;

        if (!other.CompareTag(playerTag))
            return;

        if (GlobalTipService.Instance == null)
        {
            if (log)
                Debug.LogWarning("[GlobalTipRequestOnTriggerEnter2D] No GlobalTipService found.", this);

            return;
        }

        if (AllTipsAlreadySeen())
        {
            hasRequested = true;

            if (disableAfterSuccessfulRequest)
                enabled = false;

            return;
        }

        bool shown = GlobalTipService.Instance.TryShowTipSequence(tipIds);

        if (shown)
        {
            hasRequested = true;

            if (log)
                Debug.Log("[GlobalTipRequestOnTriggerEnter2D] Requested tips successfully.", this);

            if (disableAfterSuccessfulRequest)
                enabled = false;
        }
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