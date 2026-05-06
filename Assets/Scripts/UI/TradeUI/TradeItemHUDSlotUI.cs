using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeItemHUDSlotUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Visuals")]
    [SerializeField] private Image iconImage;

    [Header("Texts")]
    [SerializeField] private TMP_Text hotkeyText;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private TMP_Text nameText;

    [Header("Options")]
    [SerializeField] private bool showName;
    [SerializeField] private bool showMaxStack = true;
    [SerializeField] private string countPrefix = "";

    [Header("Fallback Colors")]
    [SerializeField] private Color chronosColor = new Color(0.35f, 0.75f, 1f, 1f);
    [SerializeField] private Color bloodlustColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color ectoplasmColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    [SerializeField] private Color ringColor = new Color(1f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color unknownColor = Color.white;

    public bool IsVisible => root != null && root.activeSelf;
    public TradeItemType CurrentItemType => currentItemType;

    private TradeItemType currentItemType = TradeItemType.None;

    private void Awake()
    {
        if (root == null)
            root = gameObject;
    }

    public void Setup(TradeItemType itemType, int count, bool showWhenEmpty)
    {
        currentItemType = itemType;

        TradeItemDefinition definition = TradeItemCatalog.Get(itemType);

        if (definition == null)
        {
            ApplyUnknownState();
            SetVisible(showWhenEmpty);
            return;
        }

        if (hotkeyText != null)
            hotkeyText.text = definition.hotkeyLabel;

        if (nameText != null)
        {
            nameText.text = definition.displayName;
            nameText.gameObject.SetActive(showName);
        }

        if (iconImage != null)
            iconImage.color = GetFallbackColor(itemType);

        RefreshCount(count, showWhenEmpty);
    }

    public void RefreshCount(int count, bool showWhenEmpty)
    {
        TradeItemDefinition definition = TradeItemCatalog.Get(currentItemType);

        int safeCount = Mathf.Max(0, count);
        int maxStack = definition != null ? definition.maxStack : 0;

        if (countText != null)
        {
            if (showMaxStack)
                countText.text = $"{countPrefix}{safeCount}/{maxStack}";
            else
                countText.text = $"{countPrefix}{safeCount}";
        }

        SetVisible(showWhenEmpty || safeCount > 0);
    }

    public void SetVisible(bool visible)
    {
        if (root != null)
            root.SetActive(visible);
    }

    public void ForceHide()
    {
        SetVisible(false);
    }

    private void ApplyUnknownState()
    {
        if (hotkeyText != null)
            hotkeyText.text = "-";

        if (countText != null)
            countText.text = showMaxStack ? "0/0" : "0";

        if (nameText != null)
        {
            nameText.text = "Unknown";
            nameText.gameObject.SetActive(showName);
        }

        if (iconImage != null)
            iconImage.color = unknownColor;
    }

    private Color GetFallbackColor(TradeItemType itemType)
    {
        switch (itemType)
        {
            case TradeItemType.ChronosSpell:
                return chronosColor;

            case TradeItemType.BloodlustPotion:
                return bloodlustColor;

            case TradeItemType.EctoplasmPotion:
                return ectoplasmColor;

            case TradeItemType.HorsemenRing:
                return ringColor;

            case TradeItemType.None:
            default:
                return unknownColor;
        }
    }

    [ContextMenu("Debug Show Chronos 3/5")]
    private void DebugShowChronos()
    {
        Setup(TradeItemType.ChronosSpell, 3, true);
    }

    [ContextMenu("Debug Show Bloodlust 2/5")]
    private void DebugShowBloodlust()
    {
        Setup(TradeItemType.BloodlustPotion, 2, true);
    }

    [ContextMenu("Debug Show Ectoplasm 4/5")]
    private void DebugShowEctoplasm()
    {
        Setup(TradeItemType.EctoplasmPotion, 4, true);
    }

    [ContextMenu("Debug Show Ring 1/5")]
    private void DebugShowRing()
    {
        Setup(TradeItemType.HorsemenRing, 1, true);
    }

    [ContextMenu("Debug Hide")]
    private void DebugHide()
    {
        ForceHide();
    }
}