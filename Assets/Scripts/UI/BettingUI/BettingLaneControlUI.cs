using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BettingLaneControlUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text laneLabelText;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button increaseButton;

    private BettingChallengePanelUI ownerPanel;
    private int laneIndex;

    public void Initialize(BettingChallengePanelUI owner, int laneIndex)
    {
        ownerPanel = owner;
        this.laneIndex = laneIndex;

        if (laneLabelText != null)
            laneLabelText.text = $"Lane {laneIndex + 1}";

        if (decreaseButton != null)
        {
            decreaseButton.onClick.RemoveAllListeners();
            decreaseButton.onClick.AddListener(HandleDecreaseClicked);
        }

        if (increaseButton != null)
        {
            increaseButton.onClick.RemoveAllListeners();
            increaseButton.onClick.AddListener(HandleIncreaseClicked);
        }

        Refresh(0, false);
    }

    public void Refresh(int amount, bool interactable)
    {
        if (amountText != null)
            amountText.text = $"AP: {Mathf.Max(0, amount)}";

        if (decreaseButton != null)
            decreaseButton.interactable = interactable && amount > 0;

        if (increaseButton != null)
            increaseButton.interactable = interactable;
    }

    private void HandleDecreaseClicked()
    {
        ownerPanel?.RequestDecreaseLane(laneIndex);
    }

    private void HandleIncreaseClicked()
    {
        ownerPanel?.RequestIncreaseLane(laneIndex);
    }
}