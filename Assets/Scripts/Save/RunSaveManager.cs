using System.IO;
using UnityEngine;

public class RunSaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunCurrency currency;
    [SerializeField] private LevelSystem levelSystem;
    [SerializeField] private BranchProgression branches;
    [SerializeField] private DeathPenaltyTracker penalties;
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private ChallengeEffectManager challengeEffectManager;
    [SerializeField] private TipsSeenTracker tipsSeenTracker;
    [SerializeField] private TradeItemInventory tradeItemInventory;
    [SerializeField] private BossProgressionManager bossProgressionManager;

    [Header("Options")]
    [SerializeField] private bool autoLoadOnStart = true;
    [SerializeField] private bool autoSaveOnChanges = true;

    private bool manualSaveSuppressed;

    private string SavePath => Path.Combine(Application.persistentDataPath, "run_save.json");

    public string SaveFilePath => SavePath;

    private void Awake()
    {
        if (currency == null) currency = FindFirstObjectByType<RunCurrency>();
        if (levelSystem == null) levelSystem = FindFirstObjectByType<LevelSystem>();
        if (branches == null) branches = FindFirstObjectByType<BranchProgression>();
        if (penalties == null) penalties = FindFirstObjectByType<DeathPenaltyTracker>();
        if (roomManager == null) roomManager = FindFirstObjectByType<RoomManager>();
        if (challengeEffectManager == null) challengeEffectManager = FindFirstObjectByType<ChallengeEffectManager>();
        if (tipsSeenTracker == null) tipsSeenTracker = FindFirstObjectByType<TipsSeenTracker>();
        if (tradeItemInventory == null) tradeItemInventory = FindFirstObjectByType<TradeItemInventory>();
        if (bossProgressionManager == null) bossProgressionManager = FindFirstObjectByType<BossProgressionManager>();
    }

    private void Start()
    {
        if (autoLoadOnStart)
            Load();

        if (autoSaveOnChanges)
            Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (currency != null)
        {
            currency.OnChanged += Save;
        }

        if (levelSystem != null)
        {
            levelSystem.OnLevelChanged += OnAnyLevelChanged;
            levelSystem.OnUnspentPointsChanged += OnAnyPointsChanged;
            levelSystem.OnProgressChanged += OnAnyProgressChanged;
        }

        if (branches != null)
            branches.OnBranchChanged += OnAnyBranchChanged;

        if (roomManager != null)
        {
            roomManager.OnRoomEntered += OnRoomEntered;
            roomManager.OnCampfireEntered += OnCampfireEntered;
        }

        if (tradeItemInventory != null)
            tradeItemInventory.OnInventoryChanged += Save;

        if (bossProgressionManager != null)
            bossProgressionManager.OnBossProgressionChanged += Save;
    }

    private void Unsubscribe()
    {
        if (currency != null)
            currency.OnChanged -= Save;

        if (levelSystem != null)
        {
            levelSystem.OnLevelChanged -= OnAnyLevelChanged;
            levelSystem.OnUnspentPointsChanged -= OnAnyPointsChanged;
            levelSystem.OnProgressChanged -= OnAnyProgressChanged;
        }

        if (branches != null)
            branches.OnBranchChanged -= OnAnyBranchChanged;

        if (roomManager != null)
        {
            roomManager.OnRoomEntered -= OnRoomEntered;
            roomManager.OnCampfireEntered -= OnCampfireEntered;
        }

        if (tradeItemInventory != null)
            tradeItemInventory.OnInventoryChanged -= Save;

        if (bossProgressionManager != null)
            bossProgressionManager.OnBossProgressionChanged -= Save;
    }

    public bool HasSaveFile()
    {
        if (File.Exists(SavePath))
            return true;

        string folder = Application.persistentDataPath;

        if (!Directory.Exists(folder))
            return false;

        string[] possibleSaveFiles = Directory.GetFiles(folder, "*run_save*.json");
        return possibleSaveFiles.Length > 0;
    }

    private string GetFirstExistingSavePath()
    {
        if (File.Exists(SavePath))
            return SavePath;

        string folder = Application.persistentDataPath;

        if (!Directory.Exists(folder))
            return "";

        string[] possibleSaveFiles = Directory.GetFiles(folder, "*run_save*.json");

        if (possibleSaveFiles.Length > 0)
            return possibleSaveFiles[0];

        return "";
    }

    public bool TryGetSavePreview(out RunSavePreviewData preview)
    {
        preview = new RunSavePreviewData();

        string savePath = GetFirstExistingSavePath();

        if (string.IsNullOrWhiteSpace(savePath))
        {
            preview.hasSave = false;
            return false;
        }

        string json = File.ReadAllText(savePath);
        RunSaveData data = JsonUtility.FromJson<RunSaveData>(json);

        if (data == null)
        {
            preview.hasSave = false;
            return false;
        }

        preview.hasSave = true;

        preview.roomX = data.playerRoomX;
        preview.roomY = data.playerRoomY;

        preview.level = data.level;
        preview.unspentPoints = data.unspentPoints;

        preview.souls = data.souls;
        preview.xp = data.xp;

        if (data.bossProgression != null)
        {
            data.bossProgression.ClampAndRepair();

            preview.gluttonyClues = data.bossProgression.gluttonyClueCount;
            preview.gluttonyBossUnlocked = data.bossProgression.gluttonyBossUnlocked;
            preview.gluttonyBossDefeated = data.bossProgression.gluttonyBossDefeated;
            preview.hungerClueUnlocked = data.bossProgression.hungerHorsemanClueUnlocked;
            preview.mvpEndingReached = data.bossProgression.mvpEndingReached;
        }

        return true;
    }

    public bool TryLoad()
    {
        if (!File.Exists(SavePath))
            return false;

        Load();
        return true;
    }

    public bool TryDeleteSave()
    {
        manualSaveSuppressed = true;

        bool deletedAny = false;

        // Main current save file.
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            deletedAny = true;
            Debug.Log($"[RunSaveManager] Deleted save -> {SavePath}");
        }

        // Safety cleanup for possible old/duplicate run save files.
        // This only targets files that look like Chief of Sin run-save files.
        string folder = Application.persistentDataPath;

        if (Directory.Exists(folder))
        {
            string[] possibleSaveFiles = Directory.GetFiles(folder, "*run_save*.json");

            for (int i = 0; i < possibleSaveFiles.Length; i++)
            {
                string path = possibleSaveFiles[i];

                if (File.Exists(path))
                {
                    File.Delete(path);
                    deletedAny = true;
                    Debug.Log($"[RunSaveManager] Deleted extra save file -> {path}");
                }
            }
        }

        if (!deletedAny)
            Debug.Log("[RunSaveManager] Delete requested, but no save files were found.");

        return deletedAny;
    }

    public void AllowManualSavingAgain()
    {
        manualSaveSuppressed = false;
    }

    public void Save()
    {
        if (manualSaveSuppressed)
        {
            Debug.Log("[RunSaveManager] Save ignored because saving is suppressed after deleting the save file.");
            return;
        }

        if (currency == null || levelSystem == null || branches == null || roomManager == null) return;

        var data = new RunSaveData
        {
            souls = currency.Souls,
            xp = currency.XP,

            level = levelSystem.Level,
            unspentPoints = levelSystem.UnspentPoints,

            demon = branches.Demon,
            monster = branches.Monster,
            fallenGod = branches.FallenGod,
            hellhound = branches.Hellhound,

            maxHPLoss = penalties != null ? penalties.MaxHPLoss : 0,
            maxAPLoss = penalties != null ? penalties.MaxAPLoss : 0,
            actionRateLoss = penalties != null ? penalties.ActionRateLoss : 0,
            dpLoss = penalties != null ? penalties.DPLoss : 0,

            hasActivatedCampfireCheckpoint = roomManager.HasActivatedCampfireCheckpoint,
            checkpointRoomX = roomManager.LastActivatedCampfireCoord.x,
            checkpointRoomY = roomManager.LastActivatedCampfireCoord.y,

            // 7.5A
            runStepCount = roomManager.CurrentRunStep,

            hasSeenGeneralChallengeTips = tipsSeenTracker != null && tipsSeenTracker.HasSeenGeneralChallengeTips,
            hasSeenBettingTips = tipsSeenTracker != null && tipsSeenTracker.HasSeenChallengeTypeTips(ChallengeType.Betting),
            hasSeenGluttonyTips = tipsSeenTracker != null && tipsSeenTracker.HasSeenChallengeTypeTips(ChallengeType.Gluttony),
            hasSeenSlothTips = tipsSeenTracker != null && tipsSeenTracker.HasSeenChallengeTypeTips(ChallengeType.Sloth),
            hasSeenLieTips = tipsSeenTracker != null && tipsSeenTracker.HasSeenChallengeTypeTips(ChallengeType.Lie),

            // 9.2C: Trade item inventory counts.
            chronosSpellCount = tradeItemInventory != null ? tradeItemInventory.GetCount(TradeItemType.ChronosSpell) : 0,
            bloodlustPotionCount = tradeItemInventory != null ? tradeItemInventory.GetCount(TradeItemType.BloodlustPotion) : 0,
            ectoplasmPotionCount = tradeItemInventory != null ? tradeItemInventory.GetCount(TradeItemType.EctoplasmPotion) : 0,
            horsemenRingCount = tradeItemInventory != null ? tradeItemInventory.GetCount(TradeItemType.HorsemenRing) : 0,

            // 10.0: Boss progression.
            bossProgression = bossProgressionManager != null
                ? bossProgressionManager.ExportState()
                : new BossProgressionState(),
        };

        if (challengeEffectManager != null)
        {
            ChallengeEffectEntry[] exportedEffects = challengeEffectManager.ExportActiveEffects();

            data.activeChallengeEffects = new ChallengeEffectSaveEntry[exportedEffects.Length];

            for (int i = 0; i < exportedEffects.Length; i++)
            {
                ChallengeEffectEntry effect = exportedEffects[i];

                ChallengeEffectSaveEntry saveEntry = new ChallengeEffectSaveEntry();
                saveEntry.sourceChallenge = (int)effect.sourceChallenge;
                saveEntry.effectType = (int)effect.effectType;
                saveEntry.value = effect.value;
                saveEntry.clearsOnNextChallengeEntry = effect.clearsOnNextChallengeEntry;
                saveEntry.debugLabel = effect.debugLabel;

                data.activeChallengeEffects[i] = saveEntry;
            }
        }
        else
        {
            data.activeChallengeEffects = System.Array.Empty<ChallengeEffectSaveEntry>();
        }

        var roomStates = roomManager.ExportRoomStates();
        data.playerRoomX = roomManager.CurrentCoord.x;
        data.playerRoomY = roomManager.CurrentCoord.y;
        data.rooms = roomStates;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[RunSaveManager] Saved -> {SavePath}");
    }

    public void Load()
    {
        if (!File.Exists(SavePath)) return;

        string json = File.ReadAllText(SavePath);
        var data = JsonUtility.FromJson<RunSaveData>(json);
        if (data == null) return;

        // Gate LevelSystem so SetXP doesn't cause "real" levelups
        levelSystem.BeginLoad();

        // Currency
        currency.SetSouls(data.souls);
        currency.SetXP(data.xp);

        // LevelSystem
        levelSystem.LoadState(data.level, data.unspentPoints);

        // Branches
        branches.LoadState(data.demon, data.monster, data.fallenGod, data.hellhound);

        // Penalties
        if (penalties != null)
        {
            penalties.LoadState(data.maxHPLoss, data.maxAPLoss, data.actionRateLoss, data.dpLoss);
        }

        if (tipsSeenTracker != null)
        {
            tipsSeenTracker.LoadChallengeTipsState(
                data.hasSeenGeneralChallengeTips,
                data.hasSeenBettingTips,
                data.hasSeenGluttonyTips,
                data.hasSeenSlothTips,
                data.hasSeenLieTips
            );
        }

        if (tradeItemInventory != null)
        {
            tradeItemInventory.SetCountFromSave(TradeItemType.ChronosSpell, data.chronosSpellCount);
            tradeItemInventory.SetCountFromSave(TradeItemType.BloodlustPotion, data.bloodlustPotionCount);
            tradeItemInventory.SetCountFromSave(TradeItemType.EctoplasmPotion, data.ectoplasmPotionCount);
            tradeItemInventory.SetCountFromSave(TradeItemType.HorsemenRing, data.horsemenRingCount);

            Debug.Log($"[RunSaveManager] Loaded trade inventory: {tradeItemInventory.GetDebugSummary()}");
        }

        if (bossProgressionManager != null)
        {
            bossProgressionManager.ImportState(data.bossProgression);
        }

        if (roomManager != null)
        {
            // 7.5: restore the world progress clock before loading the room.
            roomManager.LoadRunStepState(data.runStepCount);

            // Loading directly into a campfire/checkpoint room should restore silently,
            // without showing the normal recovery popup.
            roomManager.SuppressNextCampfireRecoveryPopup();

            roomManager.ImportRoomStates(data.rooms, new Vector2Int(data.playerRoomX, data.playerRoomY));

            roomManager.LoadCheckpointState(
                data.hasActivatedCampfireCheckpoint,
                new Vector2Int(data.checkpointRoomX, data.checkpointRoomY)
            );
        }

        if (challengeEffectManager != null)
        {
            ChallengeEffectEntry[] restoredEffects;

            if (data.activeChallengeEffects != null && data.activeChallengeEffects.Length > 0)
            {
                restoredEffects = new ChallengeEffectEntry[data.activeChallengeEffects.Length];

                for (int i = 0; i < data.activeChallengeEffects.Length; i++)
                {
                    ChallengeEffectSaveEntry saveEntry = data.activeChallengeEffects[i];

                    ChallengeType sourceChallenge = System.Enum.IsDefined(typeof(ChallengeType), saveEntry.sourceChallenge)
                        ? (ChallengeType)saveEntry.sourceChallenge
                        : ChallengeType.None;

                    ChallengeEffectType effectType = System.Enum.IsDefined(typeof(ChallengeEffectType), saveEntry.effectType)
                        ? (ChallengeEffectType)saveEntry.effectType
                        : ChallengeEffectType.None;

                    restoredEffects[i] = new ChallengeEffectEntry(
                        sourceChallenge,
                        effectType,
                        saveEntry.value,
                        saveEntry.clearsOnNextChallengeEntry,
                        saveEntry.debugLabel
                    );
                }
            }
            else
            {
                restoredEffects = System.Array.Empty<ChallengeEffectEntry>();
            }

            challengeEffectManager.ImportActiveEffects(restoredEffects);
        }

        levelSystem.EndLoad();
    }

    public void DeleteSave()
    {
        TryDeleteSave();
    }

    private void OnRoomEntered(RoomInstance _) => Save();
    private void OnCampfireEntered(Vector2Int _, RoomState __) => Save();
    private void OnAnyLevelChanged(int _) => Save();
    private void OnAnyPointsChanged(int _) => Save();
    private void OnAnyProgressChanged(int _, int __) => Save();
    private void OnAnyBranchChanged(BranchType _, int __) => Save();

    private void OnApplicationQuit()
    {
        Save();
    }

    private void OnDisable()
    {
        Save();
    }
}