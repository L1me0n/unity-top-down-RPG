using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("References (Player)")]
    [SerializeField] private PlayerStats stats;
    [SerializeField] private PlayerCombatController combat;
    [SerializeField] private DisappearController disappear;

    [Header("Attack HUD (Left)")]
    [SerializeField] private GameObject attackHUDRoot;
    [SerializeField] private Image attackHPFill;
    [SerializeField] private Image attackAPFill;
    [SerializeField] private TMP_Text attackDPText;

    [Header("Defense HUD (Right)")]
    [SerializeField] private GameObject defenseHUDRoot;
    [SerializeField] private Image defenseHPFill;
    [SerializeField] private Image defenseAPFill;
    [SerializeField] private Image disappearRingFill; // radial fill

    private void Awake()
    {
        // Auto-find if not assigned
        if (stats == null) stats = FindFirstObjectByType<PlayerStats>();
        if (combat == null) combat = FindFirstObjectByType<PlayerCombatController>();
        if (disappear == null) disappear = FindFirstObjectByType<DisappearController>();
    }

    private void Update()
    {
        if (stats == null || combat == null) return;

        bool isAttack = combat.Mode == CombatMode.Attack;

        if (attackHUDRoot != null) attackHUDRoot.SetActive(isAttack);
        if (defenseHUDRoot != null) defenseHUDRoot.SetActive(!isAttack);

        float hp01 = stats.MaxHP <= 0 ? 0f : (float)stats.HP / stats.MaxHP;
        float ap01 = stats.MaxAP <= 0 ? 0f : (float)stats.AP / stats.MaxAP;

        if (isAttack)
        {
            if (attackHPFill != null) attackHPFill.fillAmount = hp01;
            if (attackAPFill != null) attackAPFill.fillAmount = ap01;
            if (attackDPText != null) attackDPText.text = $"Damage: {stats.DP}";
        }
        else
        {
            if (defenseHPFill != null) defenseHPFill.fillAmount = hp01;
            if (defenseAPFill != null) defenseAPFill.fillAmount = ap01;

            if (disappearRingFill != null && disappear != null)
            {
                // shows recharge progress (0..1)
                disappearRingFill.fillAmount = disappear.ActiveProgress01;
            }
        }
    }
}