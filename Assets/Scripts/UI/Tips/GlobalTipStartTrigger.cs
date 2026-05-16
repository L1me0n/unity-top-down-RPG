using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalTipStartTrigger : MonoBehaviour
{
    [Header("Tip Sequence")]
    [SerializeField] private List<string> startTipIds = new List<string>()
    {
        "general_welcome",
        "general_ledger"
    };

    [Header("Timing")]
    [SerializeField] private float delaySeconds = 0.35f;
    [SerializeField] private bool showOnStart = true;

    [Header("Debug")]
    [SerializeField] private bool log = false;

    private IEnumerator Start()
    {
        if (!showOnStart)
            yield break;

        if (delaySeconds > 0f)
            yield return new WaitForSeconds(delaySeconds);

        TryShowStartTips();
    }

    public void TryShowStartTips()
    {
        if (GlobalTipService.Instance == null)
        {
            if (log)
                Debug.LogWarning("[GlobalTipStartTrigger] No GlobalTipService found.", this);

            return;
        }

        bool shown = GlobalTipService.Instance.TryShowTipSequence(startTipIds);

        if (log)
            Debug.Log($"[GlobalTipStartTrigger] Requested start tips. shown={shown}", this);
    }
}