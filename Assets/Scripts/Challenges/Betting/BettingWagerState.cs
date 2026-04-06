using UnityEngine;

[System.Serializable]
public class BettingWagerState
{
    public const int LaneCount = 4;

    [SerializeField] private int snapshotAP;
    [SerializeField] private int minimumTotalWager;
    [SerializeField] private int[] laneWagers = new int[LaneCount];

    public int SnapshotAP => snapshotAP;
    public int MinimumTotalWager => minimumTotalWager;
    public int[] LaneWagers => laneWagers;

    public int TotalWager
    {
        get
        {
            int total = 0;

            if (laneWagers == null)
                return 0;

            for (int i = 0; i < laneWagers.Length; i++)
                total += Mathf.Max(0, laneWagers[i]);

            return total;
        }
    }

    public int RemainingAP => Mathf.Max(0, snapshotAP - TotalWager);

    public bool HasAnyBet
    {
        get
        {
            if (laneWagers == null)
                return false;

            for (int i = 0; i < laneWagers.Length; i++)
            {
                if (laneWagers[i] > 0)
                    return true;
            }

            return false;
        }
    }

    public BettingWagerState()
    {
        snapshotAP = 0;
        minimumTotalWager = 0;
        laneWagers = new int[LaneCount];
    }

    public void InitializeFromSnapshotAP(int startingAP)
    {
        snapshotAP = Mathf.Max(0, startingAP);
        minimumTotalWager = Mathf.CeilToInt(snapshotAP * 0.15f);

        if (snapshotAP > 0)
            minimumTotalWager = Mathf.Max(1, minimumTotalWager);
        else
            minimumTotalWager = 0;

        if (laneWagers == null || laneWagers.Length != LaneCount)
            laneWagers = new int[LaneCount];
        else
            ClearAllBets();
    }

    public int GetLaneWager(int laneIndex)
    {
        if (!IsValidLane(laneIndex))
            return 0;

        return Mathf.Max(0, laneWagers[laneIndex]);
    }

    public bool CanAddToLane(int laneIndex, int amount)
    {
        if (!IsValidLane(laneIndex))
            return false;

        if (amount <= 0)
            return false;

        return RemainingAP >= amount;
    }

    public bool TryAddToLane(int laneIndex, int amount = 1)
    {
        if (!CanAddToLane(laneIndex, amount))
            return false;

        laneWagers[laneIndex] += amount;
        return true;
    }

    public bool CanRemoveFromLane(int laneIndex, int amount)
    {
        if (!IsValidLane(laneIndex))
            return false;

        if (amount <= 0)
            return false;

        return laneWagers[laneIndex] >= amount;
    }

    public bool TryRemoveFromLane(int laneIndex, int amount = 1)
    {
        if (!CanRemoveFromLane(laneIndex, amount))
            return false;

        laneWagers[laneIndex] -= amount;
        laneWagers[laneIndex] = Mathf.Max(0, laneWagers[laneIndex]);
        return true;
    }

    public void ClearLane(int laneIndex)
    {
        if (!IsValidLane(laneIndex))
            return;

        laneWagers[laneIndex] = 0;
    }

    public void ClearAllBets()
    {
        if (laneWagers == null || laneWagers.Length != LaneCount)
            laneWagers = new int[LaneCount];

        for (int i = 0; i < laneWagers.Length; i++)
            laneWagers[i] = 0;
    }

    public bool MeetsMinimumWager()
    {
        return TotalWager >= minimumTotalWager;
    }

    public bool IsValidForRaceStart()
    {
        if (snapshotAP <= 0)
            return false;

        if (!HasAnyBet)
            return false;

        if (!MeetsMinimumWager())
            return false;

        if (TotalWager > snapshotAP)
            return false;

        return true;
    }

    public int GetWinningBetAmount(int winningLaneIndex)
    {
        return GetLaneWager(winningLaneIndex);
    }

    public int GetLosingBetAmount(int winningLaneIndex)
    {
        return Mathf.Max(0, TotalWager - GetWinningBetAmount(winningLaneIndex));
    }

    public string BuildDebugSummary()
    {
        string lane0 = GetLaneWager(0).ToString();
        string lane1 = GetLaneWager(1).ToString();
        string lane2 = GetLaneWager(2).ToString();
        string lane3 = GetLaneWager(3).ToString();

        return
            $"SnapshotAP={snapshotAP} | " +
            $"Minimum={minimumTotalWager} | " +
            $"Total={TotalWager} | " +
            $"Remaining={RemainingAP} | " +
            $"Lanes=[{lane0}, {lane1}, {lane2}, {lane3}] | " +
            $"Valid={IsValidForRaceStart()}";
    }

    private bool IsValidLane(int laneIndex)
    {
        return laneIndex >= 0 && laneIndex < LaneCount;
    }
}