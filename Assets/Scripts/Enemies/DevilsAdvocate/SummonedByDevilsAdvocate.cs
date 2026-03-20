using UnityEngine;

public class SummonedByDevilsAdvocate : MonoBehaviour
{
    private DevilsAdvocateBrain summoner;
    private bool notified;

    public bool SuppressSoulsReward => true;
    public bool SuppressXpReward => true;

    public void Init(DevilsAdvocateBrain owner)
    {
        summoner = owner;
    }

    private void OnDestroy()
    {
        if (notified)
            return;

        notified = true;

        if (summoner != null)
            summoner.NotifySummonedHellpuppyDied();
    }
}