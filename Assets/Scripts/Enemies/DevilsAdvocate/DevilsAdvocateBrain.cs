using UnityEngine;

public class DevilsAdvocateBrain : EnemyBrainBase
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.3f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float preferredDistance = 8.5f;
    [SerializeField] private float retreatDistance = 5.5f;
    [SerializeField] private float stoppingTolerance = 0.4f;
    [SerializeField] private float stopLerpFactor = 0.2f;

    [Header("Fire Zone")]
    [SerializeField] private FireZoneHazard fireZonePrefab;
    [SerializeField] private float fireZoneLifetime = 4f;
    [SerializeField] private int fireZoneDamagePerTick = 1;
    [SerializeField] private float fireZoneTickSeconds = 0.75f;
    [SerializeField] private Vector3 fireZoneSpawnOffset = Vector3.zero;
    [SerializeField] private bool logFireZoneCast = false;

    [Header("Fire Zone Timing")]
    [SerializeField] private float initialFireDelayMin = 2f;
    [SerializeField] private float initialFireDelayMax = 4f;
    [SerializeField] private float fireCooldownMin = 6f;
    [SerializeField] private float fireCooldownMax = 9f;

    [Header("Summon Timing")]
    [SerializeField] private float initialSummonDelayMin = 6f;
    [SerializeField] private float initialSummonDelayMax = 9f;
    [SerializeField] private float summonCooldownMin = 16f;
    [SerializeField] private float summonCooldownMax = 20f;
    [SerializeField] private int maxActiveSummons = 4;
    [SerializeField] private Vector3 summonOffset = new Vector3(1.2f, 0f, 0f);
    [SerializeField] private float burstSpawnInterval = 0.15f;

    [Header("Debug")]
    [SerializeField] private bool logTimers = false;
    [SerializeField] private bool logRequests = false;

    private float fireTimer;
    private float summonTimer;
    private float burstSpawnTimer;

    private bool fireZoneCastRequested;
    private int pendingBurstSummons;

    private int activeSummons;

    private RoomCombatController roomController;

    public int ActiveSummons => activeSummons;
    public int MaxActiveSummons => maxActiveSummons;

    protected override void Awake()
    {
        base.Awake();

        fireTimer = GetRandomDelay(initialFireDelayMin, initialFireDelayMax);
        summonTimer = GetRandomDelay(initialSummonDelayMin, initialSummonDelayMax);
        burstSpawnTimer = 0f;

        EnemyRoomLink link = GetComponent<EnemyRoomLink>();
        if (link != null)
            roomController = link.GetRoomController();

        if (logTimers)
        {
            Debug.Log($"[DevilsAdvocateBrain] {name} fireTimer={fireTimer:0.00}, summonTimer={summonTimer:0.00}");
        }
    }

    private void Start()
    {
        if (roomController == null)
        {
            EnemyRoomLink link = GetComponent<EnemyRoomLink>();
            if (link != null)
                roomController = link.GetRoomController();
        }
    }

    private void Update()
    {
        if (!HasTarget())
            TryFindPlayerTarget();

        TickFireTimer();
        TickSummonTimer();
        TickBurstSummons();

        HandleFireZoneCast();
    }

    private void FixedUpdate()
    {
        if (!HasTarget())
        {
            StopMovement(0.3f);
            return;
        }

        HandleMovement();
    }

    private void TickFireTimer()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer > 0f)
            return;

        RequestFireZoneCast();
        fireTimer = GetRandomDelay(fireCooldownMin, fireCooldownMax);

        if (logTimers)
            Debug.Log($"[DevilsAdvocateBrain] {name} next fireTimer={fireTimer:0.00}");
    }

    private void TickSummonTimer()
    {
        summonTimer -= Time.deltaTime;
        if (summonTimer > 0f)
            return;

        QueueSummonBurstIfNeeded();
        summonTimer = GetRandomDelay(summonCooldownMin, summonCooldownMax);

        if (logTimers)
            Debug.Log($"[DevilsAdvocateBrain] {name} next summonTimer={summonTimer:0.00}");
    }

    private void TickBurstSummons()
    {
        if (pendingBurstSummons <= 0)
            return;

        burstSpawnTimer -= Time.deltaTime;
        if (burstSpawnTimer > 0f)
            return;

        if (!CanSummonMore())
        {
            pendingBurstSummons = 0;
            return;
        }

        SpawnOneSummonedHellpuppy();

        pendingBurstSummons--;
        burstSpawnTimer = Mathf.Max(0.01f, burstSpawnInterval);
    }

    private void HandleMovement()
    {
        float distance = DistanceToTarget();

        if (distance > preferredDistance + stoppingTolerance)
        {
            MoveTowardsTarget(moveSpeed, acceleration);
            return;
        }

        if (distance < retreatDistance)
        {
            MoveAwayFromTarget(moveSpeed, acceleration);
            return;
        }

        StopMovement(stopLerpFactor);
    }

    private void MoveAwayFromTarget(float speed, float accel)
    {
        if (rb == null || !HasTarget())
            return;

        Vector2 desiredVelocity = -DirectionToTarget() * speed;
        rb.linearVelocity = Vector2.MoveTowards(
            rb.linearVelocity,
            desiredVelocity,
            accel * Time.fixedDeltaTime
        );
    }

    private void HandleFireZoneCast()
    {
        if (!ConsumeFireZoneCastRequest())
            return;

        if (fireZonePrefab == null)
        {
            Debug.LogWarning($"[DevilsAdvocateBrain] {name} has no fireZonePrefab assigned.");
            return;
        }

        Vector3 spawnPos = GetPlayerCastTargetPosition() + fireZoneSpawnOffset;

        FireZoneHazard zone = Instantiate(
            fireZonePrefab,
            spawnPos,
            Quaternion.identity
        );

        zone.Init(
            fireZoneLifetime,
            fireZoneDamagePerTick,
            fireZoneTickSeconds,
            gameObject
        );

        if (logFireZoneCast)
            Debug.Log($"[DevilsAdvocateBrain] {name} cast Fire Zone at {spawnPos}.");
    }

    private void QueueSummonBurstIfNeeded()
    {
        int missing = Mathf.Max(0, maxActiveSummons - activeSummons);
        if (missing <= 0)
            return;

        pendingBurstSummons = missing;
        burstSpawnTimer = 0f;

        if (logRequests)
            Debug.Log($"[DevilsAdvocateBrain] {name} queued SUMMON BURST of {pendingBurstSummons}. Active={activeSummons}/{maxActiveSummons}");
    }

    private void SpawnOneSummonedHellpuppy()
    {
        if (roomController == null)
        {
            EnemyRoomLink link = GetComponent<EnemyRoomLink>();
            if (link != null)
                roomController = link.GetRoomController();
        }

        if (roomController == null)
        {
            Debug.LogWarning($"[DevilsAdvocateBrain] {name} cannot summon because roomController is missing.");
            pendingBurstSummons = 0;
            return;
        }

        Vector2 jitter = Random.insideUnitCircle * 0.65f;
        Vector3 spawnPos = transform.position + summonOffset + new Vector3(jitter.x, jitter.y, 0f);

        GameObject summoned = roomController.SpawnRuntimeSummonedEnemy(EnemyType.Hellpuppy, spawnPos);
        if (summoned == null)
            return;

        SummonedByDevilsAdvocate marker = summoned.GetComponent<SummonedByDevilsAdvocate>();
        if (marker == null)
            marker = summoned.AddComponent<SummonedByDevilsAdvocate>();

        marker.Init(this);
        RegisterSummonedHellpuppy();
    }

    private void RequestFireZoneCast()
    {
        fireZoneCastRequested = true;

        if (logRequests)
            Debug.Log($"[DevilsAdvocateBrain] {name} requested FIRE ZONE.");
    }

    public bool ConsumeFireZoneCastRequest()
    {
        if (!fireZoneCastRequested)
            return false;

        fireZoneCastRequested = false;
        return true;
    }

    public bool CanSummonMore()
    {
        return activeSummons < maxActiveSummons;
    }

    public void RegisterSummonedHellpuppy()
    {
        activeSummons++;
        activeSummons = Mathf.Min(activeSummons, maxActiveSummons);

        if (logRequests)
            Debug.Log($"[DevilsAdvocateBrain] {name} summon registered. Active={activeSummons}/{maxActiveSummons}");
    }

    public void NotifySummonedHellpuppyDied()
    {
        activeSummons--;
        activeSummons = Mathf.Max(0, activeSummons);

        if (logRequests)
            Debug.Log($"[DevilsAdvocateBrain] {name} summon died. Active={activeSummons}/{maxActiveSummons}");
    }

    public Vector3 GetPlayerCastTargetPosition()
    {
        return HasTarget() ? target.position : transform.position;
    }

    private float GetRandomDelay(float min, float max)
    {
        float safeMin = Mathf.Min(min, max);
        float safeMax = Mathf.Max(min, max);

        if (Mathf.Approximately(safeMin, safeMax))
            return safeMin;

        return Random.Range(safeMin, safeMax);
    }
}