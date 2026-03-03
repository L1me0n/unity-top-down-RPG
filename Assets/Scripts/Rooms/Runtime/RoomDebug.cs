using TMPro;
using UnityEngine;

public class RoomDebugUI : MonoBehaviour
{
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private TMP_Text label;

    private void Update()
    {
        if (roomManager == null || label == null) return;

        Vector2Int c = roomManager.CurrentCoord;
        label.text = $"Room: ({c.x},{c.y})";
    }
}