using System.Collections.Generic;
using UnityEngine;

public class GlobalTipManualRequester : MonoBehaviour
{
    [Header("Tips")]
    [SerializeField] private List<string> tipIds = new List<string>();

    [Header("Debug")]
    [SerializeField] private bool log = false;

    public void RequestTips()
    {
        if (GlobalTipService.Instance == null)
        {
            if (log)
                Debug.LogWarning("[GlobalTipManualRequester] No GlobalTipService found.", this);

            return;
        }

        bool shown = GlobalTipService.Instance.TryShowTipSequence(tipIds);

        if (log)
            Debug.Log($"[GlobalTipManualRequester] Requested tips. shown={shown}", this);
    }

    public void RequestSingleTip(string tipId)
    {
        if (GlobalTipService.Instance == null)
        {
            if (log)
                Debug.LogWarning("[GlobalTipManualRequester] No GlobalTipService found.", this);

            return;
        }

        bool shown = GlobalTipService.Instance.TryShowTip(tipId);

        if (log)
            Debug.Log($"[GlobalTipManualRequester] Requested tip '{tipId}'. shown={shown}", this);
    }
}