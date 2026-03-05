using UnityEngine;

public class DeathPenaltyTracker : MonoBehaviour
{
    [Header("Penalties accumulated this run")]
    [SerializeField] private int maxHPLoss;
    [SerializeField] private int maxAPLoss;
    [SerializeField] private int dpLoss;

    public int MaxHPLoss => maxHPLoss;
    public int MaxAPLoss => maxAPLoss;
    public int DPLoss => dpLoss;

    public void AddDeathPenalty(int hpLoss = 1, int apLoss = 1, int dpLossAmount = 1)
    {
        maxHPLoss += Mathf.Max(0, hpLoss);
        maxAPLoss += Mathf.Max(0, apLoss);
        dpLoss += Mathf.Max(0, dpLossAmount);
    }

    public void ResetPenalties()
    {
        maxHPLoss = 0;
        maxAPLoss = 0;
        dpLoss = 0;
    }

    public void LoadState(int hpLoss, int apLoss, int dpLossAmount)
    {
        maxHPLoss = Mathf.Max(0, hpLoss);
        maxAPLoss = Mathf.Max(0, apLoss);
        dpLoss = Mathf.Max(0, dpLossAmount);
    }
}