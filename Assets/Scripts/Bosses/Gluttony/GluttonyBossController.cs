using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class GluttonyBossController : MonoBehaviour
{
    [Header("State Durations")]
    [SerializeField] private float introDuration = 1f;
    [SerializeField] private float spawnHellhoundsDuration = 1.25f;
    [SerializeField] private float waveTelegraphDuration = 1.25f;
    [SerializeField] private float waveReleaseDuration = 0.75f;
    [SerializeField] private float sleepMinDuration = 3f;
    [SerializeField] private float sleepMaxDuration = 5f;

    [Header("Hellhound Summons")]
    [SerializeField] private BossSummonedEnemy hellhoundPrefab;
    [SerializeField] private Transform[] hellhoundSpawnPoints;
    [SerializeField] private int hellhoundsPerSummon = 5;
    [SerializeField] private int maxAliveHellhounds = 5;
    [SerializeField] private bool cleanupHellhoundsOnSleep = false;

    [Header("Eating Wave")]
    [SerializeField] private GluttonyEatingWave eatingWavePrefab;
    [SerializeField] private Transform eatingWaveSpawnPoint;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private int eatingWaveMaxHPLoss = 1;

    [Header("Visual Placeholders")]
    [SerializeField] private GameObject idleVisualRoot;
    [SerializeField] private GameObject mouthGlowVisual;
    [SerializeField] private GameObject sleepVisual;
    [SerializeField] private GameObject summonVisual;

    [Header("Health")]
    [SerializeField] private Health health;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private BossRoomController bossRoomController;
    private Coroutine bossLoopRoutine;
    private GluttonyBossState currentState = GluttonyBossState.Inactive;
    private readonly List<BossSummonedEnemy> activeHellhounds = new List<BossSummonedEnemy>();

    private bool fightRunning;
    private bool dead;

    public event Action<GluttonyBossState> OnStateChanged;

    public GluttonyBossState CurrentState => currentState;
    public bool IsSleeping => currentState == GluttonyBossState.Sleeping;
    public bool IsFightRunning => fightRunning;
    public bool IsDead => dead;
    public Health Health => health;

    private void Awake()
    {
        SetAllStateVisualsOff();
        SetIdleVisible(true);

        if (health == null)
            health = GetComponent<Health>();

        if (health == null)
            health = GetComponentInChildren<Health>();
    }

    private void OnDisable()
    {
        StopFight();
    }

    public void StartFight(BossRoomController roomController)
    {
        if (fightRunning)
            return;

        if (dead)
            return;

        bossRoomController = roomController;

        if (playerTarget == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                playerTarget = playerObject.transform;
        }

        fightRunning = true;

        if (bossLoopRoutine != null)
            StopCoroutine(bossLoopRoutine);

        bossLoopRoutine = StartCoroutine(BossLoop());

        Log("Gluttony fight loop started.");
    }

    public void StopFight()
    {
        fightRunning = false;

        if (bossLoopRoutine != null)
        {
            StopCoroutine(bossLoopRoutine);
            bossLoopRoutine = null;
        }

        CleanupAllSummonedHellhounds();

        if (!dead)
            SetState(GluttonyBossState.Inactive);

        SetAllStateVisualsOff();
        SetIdleVisible(!dead);
    }

    public void MarkDead()
    {
        dead = true;
        fightRunning = false;

        if (bossLoopRoutine != null)
        {
            StopCoroutine(bossLoopRoutine);
            bossLoopRoutine = null;
        }

        CleanupAllSummonedHellhounds();

        SetState(GluttonyBossState.Dead);
        SetAllStateVisualsOff();
        SetIdleVisible(false);

        Log("Gluttony marked dead.");
    }

    private IEnumerator BossLoop()
    {
        SetState(GluttonyBossState.Intro);
        yield return WaitForSecondsSafe(introDuration);

        while (fightRunning && !dead)
        {
            SetState(GluttonyBossState.SpawningHellhounds);
            SummonHellhounds();
            yield return WaitForSecondsSafe(spawnHellhoundsDuration);

            SetState(GluttonyBossState.EatingWaveTelegraph);
            yield return WaitForSecondsSafe(waveTelegraphDuration);

            SetState(GluttonyBossState.EatingWaveRelease);
            ReleaseEatingWave();
            yield return WaitForSecondsSafe(waveReleaseDuration);

            float sleepDuration = UnityEngine.Random.Range(sleepMinDuration, sleepMaxDuration);
            SetState(GluttonyBossState.Sleeping);
            yield return WaitForSecondsSafe(sleepDuration);
        }
    }

    private void ReleaseEatingWave()
    {
        if (eatingWavePrefab == null)
        {
            Log("Eating Wave prefab missing.");
            return;
        }

        Transform spawnPoint = eatingWaveSpawnPoint != null ? eatingWaveSpawnPoint : transform;

        Vector2 direction = Vector2.right;

        if (playerTarget != null)
        {
            Vector2 rawDirection = playerTarget.position - spawnPoint.position;

            if (rawDirection.sqrMagnitude > 0.001f)
                direction = rawDirection.normalized;
        }

        GluttonyEatingWave wave = Instantiate(
            eatingWavePrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        wave.Initialize(direction, eatingWaveMaxHPLoss);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        wave.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Log("Released Eating Wave toward " + direction);
    }

    private IEnumerator WaitForSecondsSafe(float seconds)
    {
        float timer = 0f;

        while (timer < seconds)
        {
            if (!fightRunning || dead)
                yield break;

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void SetState(GluttonyBossState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        RefreshVisualsForState();

        Log("State -> " + currentState);

        OnStateChanged?.Invoke(currentState);
    }

    private void RefreshVisualsForState()
    {
        SetAllStateVisualsOff();

        switch (currentState)
        {
            case GluttonyBossState.Inactive:
                SetIdleVisible(true);
                break;

            case GluttonyBossState.Intro:
                SetIdleVisible(true);
                break;

            case GluttonyBossState.SpawningHellhounds:
                SetIdleVisible(true);
                SetSummonVisible(true);
                break;

            case GluttonyBossState.EatingWaveTelegraph:
                SetIdleVisible(true);
                SetMouthGlowVisible(true);
                break;

            case GluttonyBossState.EatingWaveRelease:
                SetIdleVisible(true);
                SetMouthGlowVisible(true);
                break;

            case GluttonyBossState.Sleeping:
                SetIdleVisible(true);
                SetSleepVisible(true);
                break;

            case GluttonyBossState.Dead:
                SetIdleVisible(false);
                break;
        }
    }

    private void SetAllStateVisualsOff()
    {
        SetMouthGlowVisible(false);
        SetSleepVisible(false);
        SetSummonVisible(false);
    }

    private void SetIdleVisible(bool visible)
    {
        if (idleVisualRoot != null)
            idleVisualRoot.SetActive(visible);
    }

    private void SetMouthGlowVisible(bool visible)
    {
        if (mouthGlowVisual != null)
            mouthGlowVisual.SetActive(visible);
    }

    private void SetSleepVisible(bool visible)
    {
        if (sleepVisual != null)
            sleepVisual.SetActive(visible);
    }

    private void SetSummonVisible(bool visible)
    {
        if (summonVisual != null)
            summonVisual.SetActive(visible);
    }

    private void SummonHellhounds()
    {
        CleanupNullHellhoundReferences();

        if (hellhoundPrefab == null)
        {
            Log("Hellhound prefab missing.");
            return;
        }

        if (hellhoundSpawnPoints == null || hellhoundSpawnPoints.Length == 0)
        {
            Log("Hellhound spawn points missing.");
            return;
        }

        int aliveCount = activeHellhounds.Count;
        int remainingCapacity = Mathf.Max(0, maxAliveHellhounds - aliveCount);

        if (remainingCapacity <= 0)
        {
            Log("Skipped Hellhound summon because max alive count is already reached: " + aliveCount);
            return;
        }

        int spawnCount = Mathf.Min(hellhoundsPerSummon, remainingCapacity);

        for (int i = 0; i < spawnCount; i++)
        {
            Transform spawnPoint = hellhoundSpawnPoints[i % hellhoundSpawnPoints.Length];

            if (spawnPoint == null)
                continue;

            BossSummonedEnemy summoned = Instantiate(
                hellhoundPrefab,
                spawnPoint.position,
                Quaternion.identity
            );

            summoned.Initialize(this);
            activeHellhounds.Add(summoned);
        }

        Log("Summoned " + spawnCount + " Gluttony Hellhounds. Alive now: " + activeHellhounds.Count);
    }

    private void CleanupNullHellhoundReferences()
    {
        for (int i = activeHellhounds.Count - 1; i >= 0; i--)
        {
            if (activeHellhounds[i] == null)
                activeHellhounds.RemoveAt(i);
        }
    }

    public void NotifySummonedHellhoundRemoved(BossSummonedEnemy summoned)
    {
        if (summoned == null)
            return;

        activeHellhounds.Remove(summoned);
    }

    private void CleanupAllSummonedHellhounds()
    {
        for (int i = activeHellhounds.Count - 1; i >= 0; i--)
        {
            if (activeHellhounds[i] != null)
                activeHellhounds[i].Cleanup();
        }

        activeHellhounds.Clear();

        Log("Cleaned up all summoned Hellhounds.");
    }

    private void Log(string message)
    {
        if (debugLogs)
            Debug.Log("[GluttonyBossController] " + message, this);
    }

    [ContextMenu("Debug Start Fight")]
    private void DebugStartFight()
    {
        StartFight(null);
    }

    [ContextMenu("Debug Stop Fight")]
    private void DebugStopFight()
    {
        StopFight();
    }

    [ContextMenu("Debug Mark Dead")]
    private void DebugMarkDead()
    {
        MarkDead();
    }
}