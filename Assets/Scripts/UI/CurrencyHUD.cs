using TMPro;
using UnityEngine;

public class CurrencyHUD : MonoBehaviour
{
    [SerializeField] private RunCurrency currency;
    [SerializeField] private LevelSystem levelSystem;
    [SerializeField] private TMP_Text soulsText;
    [SerializeField] private TMP_Text xpText;

    private void Awake()
    {
        if (currency == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) currency = p.GetComponent<RunCurrency>();
        }
    }

    private void OnEnable()
    {
        if (currency != null) currency.OnChanged += HandleSouls;
        if (levelSystem != null) levelSystem.OnProgressChanged += HandleXP;
        HandleXP(levelSystem.ProgressXP, levelSystem.XPToNext);
        HandleSouls();
    }

    private void OnDisable()
    {
        if (currency != null) currency.OnChanged -= HandleSouls;
        if (levelSystem != null) levelSystem.OnProgressChanged -= HandleXP;
    }

    private void HandleSouls()
    {
        if (currency == null) return;

        if (soulsText != null) soulsText.text = $"Souls: {currency.Souls}";
    }

    private void HandleXP(int progressXP, int xpToNext)
    {
        if (levelSystem == null || currency == null) return;
        if (xpText != null) xpText.text = $"XP: {progressXP} / {xpToNext}";
    }
}