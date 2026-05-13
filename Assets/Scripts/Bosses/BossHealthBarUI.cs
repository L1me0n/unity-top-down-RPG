using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossRoomController bossRoomController;
    [SerializeField] private GluttonyBossController gluttonyBossController;
    [SerializeField] private Health health;

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text valueText;

    [Header("Text")]
    [SerializeField] private string bossDisplayName = "GLUTTONY";

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        AutoFindReferences();
        BindHealth();
        Refresh();
    }

    private void OnEnable()
    {
        AutoFindReferences();
        BindHealth();
        Refresh();
    }

    private void OnDisable()
    {
        UnbindHealth();
    }

    private void AutoFindReferences()
    {
        if (bossRoomController == null)
            bossRoomController = GetComponentInParent<BossRoomController>();

        if (gluttonyBossController == null)
            gluttonyBossController = GetComponentInParent<GluttonyBossController>();

        if (health == null && gluttonyBossController != null)
            health = gluttonyBossController.Health;

        if (health == null)
            health = GetComponentInParent<Health>();
    }

    private void BindHealth()
    {
        if (health == null)
            return;

        health.OnChanged -= HandleHealthChanged;
        health.OnDied -= HandleBossDied;

        health.OnChanged += HandleHealthChanged;
        health.OnDied += HandleBossDied;
    }

    private void UnbindHealth()
    {
        if (health == null)
            return;

        health.OnChanged -= HandleHealthChanged;
        health.OnDied -= HandleBossDied;
    }

    public void Show()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);

        Refresh();
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void HandleHealthChanged(int current, int max)
    {
        Refresh();
    }

    private void HandleBossDied()
    {
        Refresh();
        Hide();
    }

    private void Refresh()
    {
        if (nameText != null)
            nameText.text = bossDisplayName;

        int current = health != null ? health.CurrentHP : 0;
        int max = health != null ? health.MaxHP : 1;

        float t = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;

        if (fillImage != null)
            fillImage.fillAmount = t;

        if (valueText != null)
            valueText.text = current + " / " + max;

        if (debugLogs)
            Debug.Log("[BossHealthBarUI] Refresh " + current + "/" + max, this);
    }
}