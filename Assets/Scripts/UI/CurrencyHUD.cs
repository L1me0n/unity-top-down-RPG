using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyHUD : MonoBehaviour
{
    [SerializeField] private RunCurrency currency;
    [SerializeField] private LevelSystem levelSystem;
    [SerializeField] private TMP_Text soulsText;
    [SerializeField] private TMP_Text lvlText;
    [SerializeField] private Image xpFill;

    private void Awake()
    {
        if (currency == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) currency = p.GetComponent<RunCurrency>();
        }
    }

    private void Start()
    {
        HandleLvl(levelSystem.Level);
    }

    private void OnEnable()
    {
        if (currency != null) currency.OnChanged += HandleSouls;
        if (levelSystem != null) {
            levelSystem.OnProgressChanged += HandleXP;
            levelSystem.OnLevelChanged += HandleLvl;
        }
        HandleXP(levelSystem.ProgressXP, levelSystem.XPToNext);
        HandleSouls();
        HandleLvl(levelSystem.Level);
    }

    private void OnDisable()
    {
        if (currency != null) currency.OnChanged -= HandleSouls;
        if (levelSystem != null) 
        {
            levelSystem.OnProgressChanged -= HandleXP;
            levelSystem.OnLevelChanged -= HandleLvl;
        }
    }

    private void HandleSouls()
    {
        if (currency == null) return;

        if (soulsText != null) soulsText.text = $"Souls: {currency.Souls}";
    }

    private void HandleXP(int progressXP, int xpToNext)
    {
        if (levelSystem == null) return;
        if (xpFill != null) xpFill.fillAmount = (float)progressXP / xpToNext;
    }

    private void HandleLvl(int newLevel)
    {
        if (lvlText != null) lvlText.text = $"Lvl {Mathf.Max(newLevel, 1)}";
    }
}