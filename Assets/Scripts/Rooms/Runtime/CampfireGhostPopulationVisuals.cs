using System.Collections.Generic;
using UnityEngine;

public class CampfireGhostPopulationVisuals : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoomManager roomManager;

    [Header("Prefabs")]
    [SerializeField] private GameObject hostageGhostPrefab;

    [Header("Placement")]
    [SerializeField] private Vector2 campfireGhostOffset = new Vector2(0f, -2f);
    [SerializeField] private float spawnRadius = 5f;

    private readonly List<HostageGhostVisual> activeGhosts = new List<HostageGhostVisual>();

    private Transform visualsRoot;
    private Vector2Int activeRoomCoord;

    private void Awake()
    {
        if (roomManager == null)
            roomManager = FindFirstObjectByType<RoomManager>();
    }

    private void OnEnable()
    {
        if (roomManager == null)
            return;

        roomManager.OnRoomEntered += HandleRoomEntered;
    }

    private void OnDisable()
    {
        if (roomManager == null)
            return;

        roomManager.OnRoomEntered -= HandleRoomEntered;
    }

    private void HandleRoomEntered(RoomInstance room)
    {
        ClearCurrentVisuals();

        if (roomManager == null || room == null)
            return;

        Vector2Int coord = roomManager.CurrentCoord;

        if (!roomManager.TryGetRoomState(coord, out RoomState state))
            return;

        if (state == null)
            return;

        if (state.roomType != RoomType.Campfire)
            return;

        if (state.storedHostageGhostCount <= 0)
            return;

        if (hostageGhostPrefab == null)
            return;

        activeRoomCoord = coord;

        visualsRoot = new GameObject("CampfireGhostVisualsRoot").transform;
        visualsRoot.SetParent(room.transform, false);

        Vector3 basePos = room.GetSpawnPosition(null) + new Vector3(campfireGhostOffset.x, campfireGhostOffset.y, 0f);
        visualsRoot.position = basePos;

        for (int i = 0; i < state.storedHostageGhostCount; i++)
        {
            float baseAngle = (Mathf.PI * 2f * i) / Mathf.Max(1, state.storedHostageGhostCount);
            float jitter = Random.Range(-0.2f, 0.2f);
            float angle = baseAngle + jitter;

            Vector2 offset2D = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnRadius;
            Vector3 spawnPos = basePos + new Vector3(offset2D.x, offset2D.y, 0f);

            GameObject ghostObj = Instantiate(hostageGhostPrefab, spawnPos, Quaternion.identity, visualsRoot);
            HostageGhostVisual ghost = ghostObj.GetComponent<HostageGhostVisual>();

            if (ghost != null)
                activeGhosts.Add(ghost);
        }
    }

    private void ClearCurrentVisuals()
    {
        activeGhosts.Clear();

        if (visualsRoot != null)
        {
            Destroy(visualsRoot.gameObject);
            visualsRoot = null;
        }
    }
}