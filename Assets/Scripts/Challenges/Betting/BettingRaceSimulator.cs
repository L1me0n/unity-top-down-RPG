using UnityEngine;

[System.Serializable]
public class BettingRaceSimulator
{
    [System.Serializable]
    public class LaneState
    {
        public float progress;
        public float baseSpeed;
        public float burstTimer;
        public float currentBurstBonus;
        public bool finished;
        public float finishTime;
    }

    [SerializeField] private LaneState[] lanes = new LaneState[BettingWagerState.LaneCount];

    [SerializeField] private float finishDistance = 10f;

    [Header("Speed Tuning")]
    [SerializeField] private float minBaseSpeed = 1.7f;
    [SerializeField] private float maxBaseSpeed = 2.1f;

    [SerializeField] private float minBurstInterval = 0.35f;
    [SerializeField] private float maxBurstInterval = 0.8f;

    [SerializeField] private float minBurstBonus = 0.35f;
    [SerializeField] private float maxBurstBonus = 1.2f;

    private int seed;
    private float elapsed;
    private bool initialized;
    private bool finished;
    private int winningLaneIndex = -1;

    public LaneState[] Lanes => lanes;
    public float FinishDistance => finishDistance;
    public bool Initialized => initialized;
    public bool Finished => finished;
    public int WinningLaneIndex => winningLaneIndex;
    public float Elapsed => elapsed;

    public void Initialize(int seed, float finishDistance)
    {
        this.seed = seed;
        this.finishDistance = Mathf.Max(0.5f, finishDistance);

        if (lanes == null || lanes.Length != BettingWagerState.LaneCount)
            lanes = new LaneState[BettingWagerState.LaneCount];

        Random.State previous = Random.state;
        Random.InitState(seed);

        for (int i = 0; i < lanes.Length; i++)
        {
            if (lanes[i] == null)
                lanes[i] = new LaneState();

            lanes[i].progress = 0f;
            lanes[i].baseSpeed = Random.Range(minBaseSpeed, maxBaseSpeed);
            lanes[i].burstTimer = Random.Range(minBurstInterval, maxBurstInterval);
            lanes[i].currentBurstBonus = 0f;
            lanes[i].finished = false;
            lanes[i].finishTime = -1f;
        }

        Random.state = previous;

        elapsed = 0f;
        finished = false;
        winningLaneIndex = -1;
        initialized = true;
    }

    public void Tick(float deltaTime)
    {
        if (!initialized || finished)
            return;

        elapsed += deltaTime;

        for (int i = 0; i < lanes.Length; i++)
        {
            LaneState lane = lanes[i];
            if (lane == null || lane.finished)
                continue;

            lane.burstTimer -= deltaTime;

            if (lane.burstTimer <= 0f)
            {
                float burstBonus = DeterministicBurstBonus(i, elapsed);
                lane.currentBurstBonus = burstBonus;
                lane.burstTimer = DeterministicBurstInterval(i, elapsed);
            }
            else
            {
                lane.currentBurstBonus = Mathf.MoveTowards(lane.currentBurstBonus, 0f, deltaTime * 2.25f);
            }

            float speed = lane.baseSpeed + lane.currentBurstBonus;
            lane.progress += Mathf.Max(0f, speed) * deltaTime;

            if (lane.progress >= finishDistance)
            {
                lane.progress = finishDistance;
                lane.finished = true;
                lane.finishTime = elapsed;
            }
        }

        EvaluateWinnerIfFinished();
    }

    public float GetNormalizedProgress(int laneIndex)
    {
        if (!IsValidLane(laneIndex))
            return 0f;

        LaneState lane = lanes[laneIndex];
        if (lane == null || finishDistance <= 0f)
            return 0f;

        return Mathf.Clamp01(lane.progress / finishDistance);
    }

    private void EvaluateWinnerIfFinished()
    {
        bool anyFinished = false;
        int bestLane = -1;
        float bestTime = float.MaxValue;

        for (int i = 0; i < lanes.Length; i++)
        {
            LaneState lane = lanes[i];
            if (lane == null || !lane.finished)
                continue;

            anyFinished = true;

            if (lane.finishTime < bestTime)
            {
                bestTime = lane.finishTime;
                bestLane = i;
            }
        }

        if (anyFinished)
        {
            finished = true;
            winningLaneIndex = bestLane;
        }
    }

    private float DeterministicBurstBonus(int laneIndex, float timeValue)
    {
        int burstSeed = seed ^ (laneIndex * 92821) ^ Mathf.RoundToInt(timeValue * 1000f);
        Random.State previous = Random.state;
        Random.InitState(burstSeed);

        float value = Random.Range(minBurstBonus, maxBurstBonus);

        Random.state = previous;
        return value;
    }

    private float DeterministicBurstInterval(int laneIndex, float timeValue)
    {
        int burstSeed = seed ^ (laneIndex * 51787) ^ Mathf.RoundToInt(timeValue * 1300f);
        Random.State previous = Random.state;
        Random.InitState(burstSeed);

        float value = Random.Range(minBurstInterval, maxBurstInterval);

        Random.state = previous;
        return value;
    }

    private bool IsValidLane(int laneIndex)
    {
        return laneIndex >= 0 && laneIndex < BettingWagerState.LaneCount;
    }
}