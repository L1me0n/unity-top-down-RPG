using UnityEngine;
using TMPro;

public class TradeMerchantInteractable : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";

    [Header("Prompt")]
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private string promptMessage = "Press E to trade";

    [Header("Shop UI")]
    [SerializeField] private TradeShopUI shopPanel;

    [Header("Debug")]
    [SerializeField] private bool logInteraction = true;

    private bool playerInside;

    // Prevents this exact bug:
    // E closes shop, then merchant sees the same E press and immediately opens it again.
    private bool waitForInteractKeyRelease;

    private void Awake()
    {
        if (shopPanel == null)
            shopPanel = FindFirstObjectByType<TradeShopUI>(FindObjectsInactive.Include);

        SetPromptVisible(false);

        if (promptText != null)
            promptText.text = promptMessage;
    }

    private void OnEnable()
    {
        if (shopPanel != null)
        {
            shopPanel.OnOpened += HandleShopOpened;
            shopPanel.OnClosed += HandleShopClosed;
        }
    }

    private void OnDisable()
    {
        if (shopPanel != null)
        {
            shopPanel.OnOpened -= HandleShopOpened;
            shopPanel.OnClosed -= HandleShopClosed;
        }

        playerInside = false;
        waitForInteractKeyRelease = false;

        UIInputBlocker.ReleaseOwner(UIInputBlocker.LockTradeMerchant);

        SetPromptVisible(false);
    }

    private void Update()
    {
        if (!playerInside)
            return;

        if (waitForInteractKeyRelease)
        {
            if (!Input.GetKey(interactKey))
                waitForInteractKeyRelease = false;

            return;
        }

        if (shopPanel != null && shopPanel.IsOpen)
            return;

        if (UIInputBlocker.BlockGameplayInput)
            return;

        if (Input.GetKeyDown(interactKey))
            RequestOpenShop();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInside = true;
        UIInputBlocker.SetUpgradeMenuBlocked(UIInputBlocker.LockTradeMerchant, true);

        if (promptText != null)
            promptText.text = promptMessage;

        if (shopPanel == null || !shopPanel.IsOpen)
            SetPromptVisible(true);

        if (logInteraction)
            Debug.Log("[TradeMerchantInteractable] Player entered merchant range.", this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInside = false;
        waitForInteractKeyRelease = false;

        UIInputBlocker.SetUpgradeMenuBlocked(UIInputBlocker.LockTradeMerchant, false);

        SetPromptVisible(false);

        if (logInteraction)
            Debug.Log("[TradeMerchantInteractable] Player left merchant range.", this);
    }

    private void RequestOpenShop()
    {
        if (shopPanel == null)
        {
            Debug.LogWarning("[TradeMerchantInteractable] Cannot open shop because TradeShopUI is missing.", this);
            return;
        }

        if (logInteraction)
            Debug.Log("[TradeMerchantInteractable] Opening shop.", this);

        shopPanel.Open();
    }

    private void HandleShopOpened()
    {
        SetPromptVisible(false);
    }

    private void HandleShopClosed()
    {
        // If the shop was closed with E, we must wait until E is released.
        // Otherwise the merchant can instantly reopen the panel in the same key press.
        waitForInteractKeyRelease = true;

        if (playerInside)
        {
            UIInputBlocker.SetUpgradeMenuBlocked(UIInputBlocker.LockTradeMerchant, true);
            SetPromptVisible(true);
        }
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptRoot != null)
            promptRoot.SetActive(visible);
    }
}