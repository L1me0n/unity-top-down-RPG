using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeShopUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private Button closeButton;

    [Header("Optional Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    [Header("Input")]
    [SerializeField] private KeyCode closeKey = KeyCode.E;

    [Header("Pause")]
    [SerializeField] private bool pauseGameWhileOpen = true;

    [Header("Debug")]
    [SerializeField] private bool log = true;

    public bool IsOpen => rootPanel != null && rootPanel.activeSelf;
    public KeyCode CloseKey => closeKey;

    public event Action OnOpened;
    public event Action OnClosed;

    private void Awake()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        ApplyPlaceholderText();
    }

    private void OnDisable()
    {
        if (IsOpen)
            ForceCloseWithoutEvents();
    }

    private void Update()
    {
        if (!IsOpen)
            return;

        if (Input.GetKeyDown(closeKey))
            Close();
    }

    public void Open()
    {
        if (rootPanel == null)
        {
            Debug.LogWarning("[TradeShopUI] Cannot open shop because rootPanel is missing.", this);
            return;
        }

        if (IsOpen)
            return;

        rootPanel.SetActive(true);

        UIInputBlocker.BlockGameplayInput = true;
        UIInputBlocker.BlockUpgradeMenuToggle = true;

        if (pauseGameWhileOpen)
            Time.timeScale = 0f;

        ApplyPlaceholderText();

        if (log)
            Debug.Log("[TradeShopUI] Shop opened.", this);

        OnOpened?.Invoke();
    }

    public void Close()
    {
        if (!IsOpen)
            return;

        rootPanel.SetActive(false);

        UIInputBlocker.BlockGameplayInput = false;

        // We clear this here, then the merchant will re-claim E if the player
        // is still inside merchant range. This prevents permanent E-locks.
        UIInputBlocker.BlockUpgradeMenuToggle = false;

        if (pauseGameWhileOpen)
            Time.timeScale = 1f;

        if (log)
            Debug.Log("[TradeShopUI] Shop closed.", this);

        OnClosed?.Invoke();
    }

    private void ForceCloseWithoutEvents()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);

        UIInputBlocker.BlockGameplayInput = false;
        UIInputBlocker.BlockUpgradeMenuToggle = false;

        if (pauseGameWhileOpen)
            Time.timeScale = 1f;
    }

    private void ApplyPlaceholderText()
    {
        if (titleText != null)
            titleText.text = "UNDER THE CROSSROADS";

        if (bodyText != null)
        {
            bodyText.text =
                "The merchant smiles.\n\n" +
                "Chronos Spell      Coming soon\n" +
                "Bloodlust Potion   Coming soon\n" +
                "Ectoplasm Potion   Coming soon\n" +
                "Horsemen Ring      Coming soon";
        }
    }
}