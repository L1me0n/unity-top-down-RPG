using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CampfireHostageHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoomManager roomManager;

    [Header("UI")]
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private TMP_Text countText;

    private IEnumerator Start()
    {
        // Wait one frame so RoomManager initial load / save load can finish first.
        yield return null;
        RefreshNow();
    }

    private void Awake()
    {
        if (roomManager == null)
            roomManager = FindFirstObjectByType<RoomManager>();
    }

    private void OnEnable()
    {
        if (roomManager != null)
            roomManager.OnRoomEntered += HandleRoomEntered;

        RefreshNow();
    }

    private void OnDisable()
    {
        if (roomManager != null)
            roomManager.OnRoomEntered -= HandleRoomEntered;
    }

    private void HandleRoomEntered(RoomInstance _)
    {
        RefreshNow();
    }

    private void RefreshNow()
    {
        if (roomManager == null)
        {
            SetVisible(false);
            return;
        }

        Vector2Int coord = roomManager.CurrentCoord;

        if (!roomManager.TryGetRoomState(coord, out RoomState state) || state == null)
        {
            SetVisible(false);
            return;
        }

        bool isCampfire = state.roomType == RoomType.Campfire;

        if (!isCampfire)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);

        int currentStored = Mathf.Max(0, state.storedHostageGhostCount);
        int capacity = roomManager.GetCampfireHostageCapacity();

        if (countText != null)
            countText.text = $"{currentStored}/{capacity}";
    }

    private void SetVisible(bool visible)
    {
        if (hudRoot != null)
            hudRoot.SetActive(visible);
        else
            gameObject.SetActive(visible);
    }
}