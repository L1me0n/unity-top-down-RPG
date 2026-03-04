using TMPro;
using UnityEngine;

public class CurrencyHUD : MonoBehaviour
{
    [SerializeField] private RunCurrency currency;
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
        if (currency != null) currency.OnChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (currency != null) currency.OnChanged -= Refresh;
    }

    private void Refresh()
    {
        if (currency == null) return;

        if (soulsText != null) soulsText.text = $"Souls: {currency.Souls}";
        if (xpText != null) xpText.text = $"XP: {currency.XP}";
    }
}