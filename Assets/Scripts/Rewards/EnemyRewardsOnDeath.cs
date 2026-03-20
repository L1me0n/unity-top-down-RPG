using UnityEngine;

public class EnemyRewardsOnDeath : MonoBehaviour
{
    [Header("Drops")]
    [SerializeField] private Vector2Int soulsRange = new Vector2Int(1, 5);
    [SerializeField] private Vector2Int xpRange = new Vector2Int(1, 2);

    [Header("Refs")]
    [SerializeField] private RunCurrency currency;

    private Health health;
    private bool paid;

    private void Awake()
    {
        health = GetComponent<Health>();

        if (currency == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) currency = p.GetComponent<RunCurrency>();
        }
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnDied += HandleDeath;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDied -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (paid) return;
        paid = true;

        if (currency == null) return;

        SummonedByDevilsAdvocate summonMarker = GetComponent<SummonedByDevilsAdvocate>();

        bool suppressSouls = summonMarker != null && summonMarker.SuppressSoulsReward;
        bool suppressXp = summonMarker != null && summonMarker.SuppressXpReward;

        if (!suppressSouls)
        {
            int souls = Random.Range(soulsRange.x, soulsRange.y + 1);
            currency.AddSouls(souls);
        }

        if (!suppressXp)
        {
            int xp = Random.Range(xpRange.x, xpRange.y + 1);
            currency.AddXP(xp);
        }
    }
}