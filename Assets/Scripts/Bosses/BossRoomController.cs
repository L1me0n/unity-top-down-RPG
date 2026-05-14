using System;
using UnityEngine;

public class BossRoomController : MonoBehaviour
{
    [Header("Boss Identity")]
    [SerializeField] private BossType bossType = BossType.Gluttony;

    [Header("Boss Content")]
    [SerializeField] private GameObject bossContentRoot;

    [Header("Gluttony Boss")]
    [SerializeField] private GluttonyBossController gluttonyBossController;

    [Header("Boss Camera")]
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private float bossRoomOrthoSize = 8.5f;
    [SerializeField] private bool useBossRoomZoom = true;

    [Header("Boss UI")]
    [SerializeField] private BossHealthBarUI bossHealthBarUI;

    [Header("Boss Attempt HP Snapshot")]
    [SerializeField] private bool restorePlayerHpSnapshotOnAttemptEnd = true;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerDamageReceiver playerDamageReceiver;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private RoomManager roomManager;
    private RoomInstance roomInstance;
    private RoomState roomState;
    private Vector2Int roomCoord;
    private Transform player;

    private bool initialized;
    private bool fightStarted;

    private bool hpSnapshotCaptured;
    private int hpSnapshotMaxHP;
    private int hpSnapshotCurrentHP;

    public event Action<BossRoomController> OnBossFightStarted;
    public event Action<BossRoomController> OnBossRoomLoadedAsDefeated;

    public BossType BossType => bossType;
    public bool FightStarted => fightStarted;
    public Vector2Int RoomCoord => roomCoord;
    public RoomState RoomState => roomState;

    public void Initialize(
        Vector2Int coord,
        RoomState state,
        RoomManager ownerRoomManager,
        RoomInstance ownerRoomInstance,
        Transform playerTransform)
    {
        roomCoord = coord;
        roomState = state;
        roomManager = ownerRoomManager;
        roomInstance = ownerRoomInstance;
        player = playerTransform;

        if (player != null)
        {
            if (playerStats == null)
                playerStats = player.GetComponent<PlayerStats>();

            if (playerDamageReceiver == null)
                playerDamageReceiver = player.GetComponent<PlayerDamageReceiver>();
        }

        if (cameraFollow == null)
            cameraFollow = FindFirstObjectByType<CameraFollow>();

        if (gluttonyBossController == null)
            gluttonyBossController = GetComponentInChildren<GluttonyBossController>(true);

        if (bossHealthBarUI == null)
            bossHealthBarUI = GetComponentInChildren<BossHealthBarUI>(true);

        initialized = true;
        fightStarted = false;

        if (roomState == null)
        {
            Debug.LogWarning("[BossRoomController] Initialized with null RoomState.", this);
            SetBossContentVisible(false);
            SetDoorLocks(false);
            return;
        }

        if (roomState.roomType != RoomType.Boss)
        {
            Log("Initialized in non-boss room. Disabling boss content.");
            SetBossContentVisible(false);
            SetDoorLocks(false);
            return;
        }

        if (roomState.bossType != bossType)
        {
            Log("Boss type mismatch. Room wants " + roomState.bossType + " but controller is " + bossType + ".");
            SetBossContentVisible(false);
            SetDoorLocks(false);
            return;
        }

        if (roomState.bossDefeated)
        {
            SetupDefeatedBossRoom();
            return;
        }

        SetupActiveBossRoom();
    }

    private void SetupActiveBossRoom()
    {
        fightStarted = true;

        SetBossContentVisible(true);

        if (bossHealthBarUI != null)
            bossHealthBarUI.Show();

        SetDoorLocks(true);
        ApplyBossRoomCamera();

        CapturePlayerHPSnapshot();
        SubscribeToPlayerDeathForSnapshotRestore();

        StartBossLogic();

        Log("Boss fight entry started at " + roomCoord + " | bossType=" + bossType);

        OnBossFightStarted?.Invoke(this);
    }

    private void SetupDefeatedBossRoom()
    {
        fightStarted = false;

        if (gluttonyBossController == null)
            gluttonyBossController = GetComponentInChildren<GluttonyBossController>(true);

        if (gluttonyBossController != null)
            gluttonyBossController.MarkDead();

        SetBossContentVisible(false);

        if (bossHealthBarUI != null)
            bossHealthBarUI.Hide();

        SetDoorLocks(false);
        ApplyBossRoomCamera();

        Log("Boss room loaded as already defeated at " + roomCoord + ".");

        OnBossRoomLoadedAsDefeated?.Invoke(this);
    }

    public void MarkBossDefeated()
    {
        if (!initialized || roomState == null)
            return;

        roomState.bossDefeated = true;
        roomState.cleared = true;

        if (gluttonyBossController != null)
            gluttonyBossController.MarkDead();

        if (bossType == BossType.Gluttony && BossProgressionManager.Instance != null)
            BossProgressionManager.Instance.MarkGluttonyBossDefeated();

        RestorePlayerHPSnapshot(true);
        UnsubscribeFromPlayerDeathForSnapshotRestore();
        hpSnapshotCaptured = false;

        SetupDefeatedBossRoom();
    }

    private void SetBossContentVisible(bool visible)
    {
        if (bossContentRoot != null)
            bossContentRoot.SetActive(visible);
    }

    private void SetDoorLocks(bool locked)
    {
        if (roomInstance == null || roomInstance.DoorLocks == null)
            return;

        for (int i = 0; i < roomInstance.DoorLocks.Length; i++)
        {
            if (roomInstance.DoorLocks[i] != null)
                roomInstance.DoorLocks[i].SetLocked(locked);
        }
    }

    private void ApplyBossRoomCamera()
    {
        if (!useBossRoomZoom)
            return;

        if (cameraFollow == null)
            cameraFollow = FindFirstObjectByType<CameraFollow>();

        if (cameraFollow != null)
            cameraFollow.SetTemporaryZoom(bossRoomOrthoSize);
    }

    private void ClearBossRoomCamera()
    {
        if (cameraFollow != null)
            cameraFollow.ClearTemporaryZoom();
    }

    private void OnDisable()
    {
        RestorePlayerHPSnapshot(true);
        UnsubscribeFromPlayerDeathForSnapshotRestore();
        hpSnapshotCaptured = false;

        ClearBossRoomCamera();
    }

    private void StartBossLogic()
    {
        if (bossType != BossType.Gluttony)
            return;

        if (gluttonyBossController == null)
            gluttonyBossController = GetComponentInChildren<GluttonyBossController>(true);

        if (gluttonyBossController == null)
        {
            Debug.LogWarning("[BossRoomController] GluttonyBossController missing.", this);
            return;
        }

        gluttonyBossController.StartFight(this);
    }

    private void CapturePlayerHPSnapshot()
    {
        if (!restorePlayerHpSnapshotOnAttemptEnd)
            return;

        if (playerStats == null && player != null)
            playerStats = player.GetComponent<PlayerStats>();

        if (playerStats == null)
        {
            Debug.LogWarning("[BossRoomController] Cannot capture HP snapshot because PlayerStats is missing.", this);
            return;
        }

        hpSnapshotMaxHP = playerStats.MaxHP;
        hpSnapshotCurrentHP = playerStats.HP;
        hpSnapshotCaptured = true;

        Log("Captured boss attempt HP snapshot: " + hpSnapshotCurrentHP + "/" + hpSnapshotMaxHP);
    }

    private void RestorePlayerHPSnapshot(bool restoreCurrentHP)
    {
        if (!restorePlayerHpSnapshotOnAttemptEnd)
            return;

        if (!hpSnapshotCaptured)
            return;

        if (playerStats == null && player != null)
            playerStats = player.GetComponent<PlayerStats>();

        if (playerStats == null)
            return;

        playerStats.SetMaxHP(hpSnapshotMaxHP);

        if (restoreCurrentHP)
            playerStats.SetHP(hpSnapshotCurrentHP);

        Log(
            "Restored boss attempt HP snapshot. Current restore=" + restoreCurrentHP +
            " | Snapshot=" + hpSnapshotCurrentHP + "/" + hpSnapshotMaxHP
        );
    }

    private void SubscribeToPlayerDeathForSnapshotRestore()
    {
        if (playerDamageReceiver == null && player != null)
            playerDamageReceiver = player.GetComponent<PlayerDamageReceiver>();

        if (playerDamageReceiver == null)
            return;

        playerDamageReceiver.OnDied -= HandlePlayerDiedDuringBossAttempt;
        playerDamageReceiver.OnDied += HandlePlayerDiedDuringBossAttempt;
    }

    private void UnsubscribeFromPlayerDeathForSnapshotRestore()
    {
        if (playerDamageReceiver == null)
            return;

        playerDamageReceiver.OnDied -= HandlePlayerDiedDuringBossAttempt;
    }

    private void HandlePlayerDiedDuringBossAttempt()
    {
        // Restore MaxHP before checkpoint respawn finishes.
        // Do not restore current HP here, because the death/checkpoint flow owns HP recovery.
        RestorePlayerHPSnapshot(false);

        UnsubscribeFromPlayerDeathForSnapshotRestore();
        hpSnapshotCaptured = false;

        Log("Player died during boss attempt. Restored MaxHP snapshot before checkpoint respawn.");
    }

    private void Log(string message)
    {
        if (debugLogs)
            Debug.Log("[BossRoomController] " + message, this);
    }

    [ContextMenu("Debug Mark Boss Defeated")]
    private void DebugMarkBossDefeated()
    {
        MarkBossDefeated();
    }
}