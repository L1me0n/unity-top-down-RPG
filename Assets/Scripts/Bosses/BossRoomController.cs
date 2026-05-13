using System;
using UnityEngine;

public class BossRoomController : MonoBehaviour
{
    [Header("Boss Identity")]
    [SerializeField] private BossType bossType = BossType.Gluttony;

    [Header("Boss Content")]
    [SerializeField] private GameObject bossContentRoot;

    [Header("Boss Camera")]
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private float bossRoomOrthoSize = 8.5f;
    [SerializeField] private bool useBossRoomZoom = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private RoomManager roomManager;
    private RoomInstance roomInstance;
    private RoomState roomState;
    private Vector2Int roomCoord;
    private Transform player;

    private bool initialized;
    private bool fightStarted;

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

        if (cameraFollow == null)
            cameraFollow = FindFirstObjectByType<CameraFollow>();

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
        SetDoorLocks(true);
        ApplyBossRoomCamera();

        Log("Boss fight entry started at " + roomCoord + " | bossType=" + bossType);

        OnBossFightStarted?.Invoke(this);
    }

    private void SetupDefeatedBossRoom()
    {
        fightStarted = false;

        SetBossContentVisible(false);
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

        if (bossType == BossType.Gluttony && BossProgressionManager.Instance != null)
            BossProgressionManager.Instance.MarkGluttonyBossDefeated();

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
        ClearBossRoomCamera();
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