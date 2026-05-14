using System;
using UnityEngine;

public class BossProgressionManager : MonoBehaviour
{
    public static BossProgressionManager Instance { get; private set; }

    private static readonly Vector2Int GluttonyBossCoord = new Vector2Int(44, 39);

    [SerializeField] private bool debugLogs = true;

    private BossProgressionState state = new BossProgressionState();

    public event Action OnBossProgressionChanged;
    public event Action<int> OnGluttonyClueAwarded;
    public event Action OnGluttonyBossUnlocked;
    public event Action OnGluttonyBossDefeated;
    public event Action OnHungerHorsemanClueUnlocked;
    public event Action OnMvpEndingReached;

    public int GluttonyClueCount => state.gluttonyClueCount;
    public bool GluttonyBossUnlocked => state.gluttonyBossUnlocked;
    public bool GluttonyBossDefeated => state.gluttonyBossDefeated;
    public bool HungerHorsemanClueUnlocked => state.hungerHorsemanClueUnlocked;
    public bool MvpEndingReached => state.mvpEndingReached;

    public Vector2Int GluttonyBossCoordinate => GluttonyBossCoord;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (state == null)
            state = new BossProgressionState();

        state.ClampAndRepair();
    }

    public bool HasRoomAwardedGluttonyClue(Vector2Int roomCoord)
    {
        return state != null && state.HasRoomAwardedGluttonyClue(roomCoord);
    }

    public bool TryAwardGluttonyClue(Vector2Int sourceRoomCoord)
    {
        if (state == null)
            state = new BossProgressionState();

        state.ClampAndRepair();

        if (state.gluttonyBossDefeated)
        {
            Log("Gluttony clue ignored because Gluttony is already defeated.");
            return false;
        }

        if (state.gluttonyBossUnlocked)
        {
            Log("Gluttony clue ignored because Gluttony boss is already unlocked.");
            return false;
        }

        if (state.HasRoomAwardedGluttonyClue(sourceRoomCoord))
        {
            Log("Gluttony clue ignored because room already awarded clue: " + sourceRoomCoord);
            return false;
        }

        state.MarkRoomAwardedGluttonyClue(sourceRoomCoord);
        state.gluttonyClueCount = Mathf.Clamp(state.gluttonyClueCount + 1, 0, 4);

        Log("Gluttony clue awarded from room " + sourceRoomCoord +
            ". Clue count: " + state.gluttonyClueCount + "/4");

        OnGluttonyClueAwarded?.Invoke(state.gluttonyClueCount);

        if (state.gluttonyClueCount >= 4)
        {
            state.gluttonyBossUnlocked = true;
            Log("Gluttony boss unlocked at " + GluttonyBossCoord);
            OnGluttonyBossUnlocked?.Invoke();
        }

        NotifyChanged();
        return true;
    }

    public void MarkGluttonyBossDefeated()
    {
        if (state == null)
            state = new BossProgressionState();

        if (state.gluttonyBossDefeated)
            return;

        state.gluttonyClueCount = 4;
        state.gluttonyBossUnlocked = true;
        state.gluttonyBossDefeated = true;
        state.hungerHorsemanClueUnlocked = true;
        state.mvpEndingReached = true;

        Log("Gluttony boss defeated. Hunger Horseman clue unlocked. MVP ending reached.");

        OnGluttonyBossDefeated?.Invoke();
        OnHungerHorsemanClueUnlocked?.Invoke();
        OnMvpEndingReached?.Invoke();

        NotifyChanged();
    }

    public string GetGluttonyBossClueDisplayText()
    {
        if (state == null)
            return "No clues found.";

        switch (Mathf.Clamp(state.gluttonyClueCount, 0, 4))
        {
            case 0:
                return "No clues found.";

            case 1:
                return "Gluttony Boss: +4_, __";

            case 2:
                return "Gluttony Boss: +44, __";

            case 3:
                return "Gluttony Boss: +44, +3_";

            default:
                return "Gluttony Boss: +44, +39";
        }
    }

    public string GetFullClueDisplayText()
    {
        string text = GetGluttonyBossClueDisplayText();

        if (state != null && state.hungerHorsemanClueUnlocked)
            text += "\nHunger Horseman: +69, __";

        return text;
    }

    public BossProgressionState ExportState()
    {
        if (state == null)
            state = new BossProgressionState();

        state.ClampAndRepair();
        return state.Clone();
    }

    public void ImportState(BossProgressionState savedState)
    {
        if (savedState == null)
        {
            state = new BossProgressionState();
        }
        else
        {
            state = savedState.Clone();
        }

        state.ClampAndRepair();

        Log("Boss progression imported. Gluttony clues: " +
            state.gluttonyClueCount + "/4, unlocked: " +
            state.gluttonyBossUnlocked + ", defeated: " +
            state.gluttonyBossDefeated);

        NotifyChanged();
    }

    [ContextMenu("Debug Award Fake Gluttony Clue")]
    private void DebugAwardFakeGluttonyClue()
    {
        int fakeIndex = state != null ? state.gluttonyClueCount + 1 : 1;
        TryAwardGluttonyClue(new Vector2Int(900 + fakeIndex, 900 + fakeIndex));
    }

    [ContextMenu("Debug Defeat Gluttony Boss")]
    private void DebugDefeatGluttonyBoss()
    {
        MarkGluttonyBossDefeated();
    }

    [ContextMenu("Debug Reset Boss Progression")]
    private void DebugResetBossProgression()
    {
        state = new BossProgressionState();
        NotifyChanged();
        Log("Boss progression reset.");
    }

    private void NotifyChanged()
    {
        state.ClampAndRepair();
        OnBossProgressionChanged?.Invoke();
    }

    private void Log(string message)
    {
        if (debugLogs)
            Debug.Log("[BossProgressionManager] " + message);
    }
}