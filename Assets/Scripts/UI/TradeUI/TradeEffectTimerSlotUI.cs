using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeEffectTimerSlotUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Fill")]
    [SerializeField] private Image fillImage;

    [Header("Optional Text")]
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private TMP_Text remainingText;

    [Header("Display")]
    [SerializeField] private bool showRemainingSeconds = false;

    public void SetVisible(bool visible)
    {
        if (root != null)
            root.SetActive(visible);
    }

    public void Setup(string label, Color color)
    {
        if (root == null)
            root = gameObject;

        if (labelText != null)
            labelText.text = label;

        if (fillImage != null)
        {
            fillImage.color = color;
            fillImage.fillAmount = 0f;
        }

        if (remainingText != null)
            remainingText.text = "";

        SetVisible(false);
    }

    public void Refresh(bool active, float progress01, float remainingSeconds)
    {
        if (root == null)
            root = gameObject;

        SetVisible(active);

        if (!active)
            return;

        if (fillImage != null)
            fillImage.fillAmount = Mathf.Clamp01(progress01);

        if (remainingText != null)
        {
            if (showRemainingSeconds)
                remainingText.text = $"{Mathf.CeilToInt(remainingSeconds)}";
            else
                remainingText.text = "";
        }
    }
}