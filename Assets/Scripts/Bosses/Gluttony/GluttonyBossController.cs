using System;
using System.Collections;
using UnityEngine;

public class GluttonyBossController : MonoBehaviour
{
    [Header("State Durations")]
    [SerializeField] private float introDuration = 1f;
    [SerializeField] private float spawnHellhoundsDuration = 1.25f;
    [SerializeField] private float waveTelegraphDuration = 1.25f;
    [SerializeField] private float waveReleaseDuration = 0.75f;
    [SerializeField] private float sleepMinDuration = 3f;
    [SerializeField] private float sleepMaxDuration = 5f;

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
            yield return WaitForSecondsSafe(spawnHellhoundsDuration);

            SetState(GluttonyBossState.EatingWaveTelegraph);
            yield return WaitForSecondsSafe(waveTelegraphDuration);

            SetState(GluttonyBossState.EatingWaveRelease);
            yield return WaitForSecondsSafe(waveReleaseDuration);

            float sleepDuration = UnityEngine.Random.Range(sleepMinDuration, sleepMaxDuration);
            SetState(GluttonyBossState.Sleeping);
            yield return WaitForSecondsSafe(sleepDuration);
        }
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