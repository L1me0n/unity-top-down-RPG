using System.Collections.Generic;
using UnityEngine;

public class HostageGhostRoomVisuals : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoomManager roomManager;

    [Header("Prefabs")]
    [SerializeField] private GameObject hostageGhostPrefab;
    [SerializeField] private GameObject containmentBoxPrefab;

    [Header("Placement")]
    [SerializeField] private float ghostSpawnRadius = 0.35f;
    [SerializeField] private Vector2 containmentOffset = new Vector2(0f, 2.5f);

    private readonly List<HostageGhostVisual> activeGhosts = new List<HostageGhostVisual>();

    private GameObject activeContainmentBox;
    private Transform visualsRoot;
    private Vector2Int activeRoomCoord;
    private bool hasActiveHostageVisuals;

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
        roomManager.OnHostagesRescued += HandleHostagesRescued;
    }

    private void OnDisable()
    {
        if (roomManager == null)
            return;

        roomManager.OnRoomEntered -= HandleRoomEntered;
        roomManager.OnHostagesRescued -= HandleHostagesRescued;
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

        if (state.roomType != RoomType.Combat)
            return;

        if (!state.hasHostageGhosts)
            return;

        if (state.hostageGhostCount <= 0)
            return;

        if (state.hostageGhostsRescued)
            return;

        activeRoomCoord = coord;
        hasActiveHostageVisuals = true;

        visualsRoot = new GameObject("HostageVisualsRoot").transform;
        visualsRoot.SetParent(room.transform, false);
        
        Vector3 basePos = room.GetSpawnPosition(null);
        visualsRoot.position = basePos + new Vector3(containmentOffset.x, containmentOffset.y, 0f);

        if (containmentBoxPrefab != null)
        {
            activeContainmentBox = Instantiate(containmentBoxPrefab, visualsRoot.position, Quaternion.identity, visualsRoot);
        }

        for (int i = 0; i < state.hostageGhostCount; i++)
        {
            if (hostageGhostPrefab == null)
                break;

            Vector2 offset2D = Random.insideUnitCircle * ghostSpawnRadius;
            Vector3 spawnPos = visualsRoot.position + new Vector3(offset2D.x, offset2D.y, 0f);

            GameObject ghostObj = Instantiate(hostageGhostPrefab, spawnPos, Quaternion.identity, visualsRoot);
            HostageGhostVisual ghost = ghostObj.GetComponent<HostageGhostVisual>();

            if (ghost != null)
                activeGhosts.Add(ghost);
        }
    }

    private void HandleHostagesRescued(Vector2Int coord, RoomState state)
    {
        if (!hasActiveHostageVisuals)
            return;

        if (roomManager == null || roomManager.CurrentRoom == null)
            return;

        if (coord != activeRoomCoord)
            return;

        if (activeContainmentBox != null)
        {
            Destroy(activeContainmentBox);
            activeContainmentBox = null;
        }

        Vector3 exitTarget = ChooseEscapeTarget(roomManager.CurrentRoom);

        for (int i = activeGhosts.Count - 1; i >= 0; i--)
        {
            if (activeGhosts[i] == null)
            {
                activeGhosts.RemoveAt(i);
                continue;
            }

            activeGhosts[i].BeginEscape(exitTarget);
        }

        hasActiveHostageVisuals = false;
    }

    private Vector3 ChooseEscapeTarget(RoomInstance room)
    {
        RoomDoor[] doors = room.Doors;
        List<RoomDoor> validDoors = new List<RoomDoor>();

        if (doors != null)
        {
            for (int i = 0; i < doors.Length; i++)
            {
                if (doors[i] == null)
                    continue;

                if (!doors[i].gameObject.activeInHierarchy)
                    continue;

                validDoors.Add(doors[i]);
            }
        }

        if (validDoors.Count > 0)
        {
            int pick = Random.Range(0, validDoors.Count);
            return validDoors[pick].transform.position;
        }

        return room.GetSpawnPosition(null);
    }

    private void ClearCurrentVisuals()
    {
        activeGhosts.Clear();
        hasActiveHostageVisuals = false;

        if (visualsRoot != null)
        {
            Destroy(visualsRoot.gameObject);
            visualsRoot = null;
        }

        activeContainmentBox = null;
    }
}