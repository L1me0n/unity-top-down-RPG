using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalTipDebugResetButton : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button resetButton;

    [Header("Optional Feedback")]
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private string successMessage = "Seen tips reset.";
    [SerializeField] private float feedbackDuration = 2f;

    [Header("Optional")]
    [SerializeField] private bool log = true;

    private float feedbackTimer;

    private void Awake()
    {
        if (resetButton == null)
            resetButton = GetComponent<Button>();

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetSeenTips);

        if (feedbackText != null)
            feedbackText.text = "";
    }

    private void OnDestroy()
    {
        if (resetButton != null)
            resetButton.onClick.RemoveListener(ResetSeenTips);
    }

    private void Update()
    {
        if (feedbackText == null)
            return;

        if (feedbackTimer <= 0f)
            return;

        feedbackTimer -= Time.unscaledDeltaTime;

        if (feedbackTimer <= 0f)
            feedbackText.text = "";
    }

    public void ResetSeenTips()
    {
        if (GlobalTipService.Instance != null)
        {
            GlobalTipService.Instance.ResetAllSeenTips();
        }
        else if (GlobalTipSeenManager.Instance != null)
        {
            GlobalTipSeenManager.Instance.ClearSeenTips();
        }
        else
        {
            // Emergency fallback if neither runtime object exists.
            PlayerPrefs.DeleteKey("chief_of_sin_seen_global_tips");
            PlayerPrefs.Save();
        }

        if (feedbackText != null)
        {
            feedbackText.text = successMessage;
            feedbackTimer = feedbackDuration;
        }

        if (log)
            Debug.Log("[GlobalTipDebugResetButton] Seen global tips were reset.");
    }
}