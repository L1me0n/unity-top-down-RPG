using UnityEngine;

public class DeathPenaltyTracker : MonoBehaviour
{
    [Header("Penalties accumulated this run")]
    [SerializeField] private int maxHPLoss;
    [SerializeField] private int maxAPLoss;
    [SerializeField] private int actionRateLoss;
    [SerializeField] private int dpLoss;

    public int MaxHPLoss => maxHPLoss;
    public int MaxAPLoss => maxAPLoss;
    public int ActionRateLoss => actionRateLoss;
    public int DPLoss => dpLoss;

    public void AddDeathPenalty(int hpLoss = 1, int apLoss = 1, int actionRateLossAmount = 1, int dpLossAmount = 1)
    {
        maxHPLoss += Mathf.Max(0, hpLoss);
        maxAPLoss += Mathf.Max(0, apLoss);
        actionRateLoss += Mathf.Max(0, actionRateLossAmount);
        dpLoss += Mathf.Max(0, dpLossAmount);
    }

    public void ResetPenalties()
    {
        maxHPLoss = 0;
        maxAPLoss = 0;
        actionRateLoss = 0;
        dpLoss = 0;
    }

    public void LoadState(int hpLoss, int apLoss, int actionRateLossAmount, int dpLossAmount)
    {
        maxHPLoss = Mathf.Max(0, hpLoss);
        maxAPLoss = Mathf.Max(0, apLoss);
        actionRateLoss = Mathf.Max(0, actionRateLoss);
        dpLoss = Mathf.Max(0, dpLossAmount);
    }
}