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

    [Header("Options")]
    [SerializeField] private bool autoLoadOnStart = true;
    [SerializeField] private bool autoSaveOnChanges = true;

    private string SavePath => Path.Combine(Application.persistentDataPath, "run_save.json");

    private void Awake()
    {
        if (currency == null) currency = FindFirstObjectByType<RunCurrency>();
        if (levelSystem == null) levelSystem = FindFirstObjectByType<LevelSystem>();
        if (branches == null) branches = FindFirstObjectByType<BranchProgression>();
        if (penalties == null) penalties = FindFirstObjectByType<DeathPenaltyTracker>();
        if (roomManager == null) roomManager = FindFirstObjectByType<RoomManager>();
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
        // Currency: your RunCurrency might have OnChanged, or OnSoulsChanged/OnXPChanged depending on your current version.
        // If you have OnChanged, hook it. If not, hook both OnSoulsChanged & OnXPChanged.
        if (currency != null)
        {
            // If your RunCurrency has: public System.Action OnChanged;
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
            roomManager.OnRoomEntered += OnRoomEntered;    
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
            roomManager.OnRoomEntered -= OnRoomEntered;
    }

    public void Save()
    {
        if (currency == null || levelSystem == null || branches == null) return;

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
        };

        var roomStates = roomManager.ExportRoomStates();
        data.playerRoomX = roomManager.CurrentCoord.x;
        data.playerRoomY = roomManager.CurrentCoord.y;
        data.rooms = roomStates;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        // Debug.Log($"[RunSaveManager] Saved -> {SavePath}");
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

        roomManager.ImportRoomStates(data.rooms, new Vector2Int(data.playerRoomX, data.playerRoomY));

        levelSystem.EndLoad();

         Debug.Log("[RunSaveManager] Loaded run save.");
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    private void OnRoomEntered(RoomInstance _) => Save();
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