using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DevilForkStaffLine : MonoBehaviour
{
    [Header("Main Staff")]
    [SerializeField] private float staffLength = 2.2f;
    [SerializeField] private float bottomY = -1.2f;
    [SerializeField] private float topY = 1.0f;
    [SerializeField] private float xOffset = 0f;

    [Header("Fork Head")]
    [SerializeField] private float headHeight = 0.45f;
    [SerializeField] private float sideProngOut = 0.22f;
    [SerializeField] private float centerProngExtra = 0.12f;
    [SerializeField] private float sideProngTilt = 0.08f;

    [Header("Line")]
    [SerializeField] private float lineWidth = 0.08f;

    private LineRenderer lr;

    private void Awake()
    {
        EnsureLine();
        Build();
    }

    private void OnValidate()
    {
        EnsureLine();
        Build();
    }

    private void EnsureLine()
    {
        if (lr == null)
            lr = GetComponent<LineRenderer>();

        lr.useWorldSpace = false;
        lr.loop = false;
        lr.positionCount = 11;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
    }

    [ContextMenu("Build Fork Staff")]
    public void Build()
    {
        if (lr == null)
            EnsureLine();

        float actualTopY = topY;
        float actualBottomY = bottomY;

        // If someone edits only staffLength mentally, keep top-bottom relation sane
        if (actualTopY <= actualBottomY)
            actualTopY = actualBottomY + Mathf.Max(0.1f, staffLength);

        Vector3 bottom = new Vector3(xOffset, actualBottomY, 0f);
        Vector3 neck = new Vector3(xOffset, actualTopY - headHeight, 0f);
        Vector3 topCenterBase = new Vector3(xOffset, actualTopY - headHeight * 0.15f, 0f);
        Vector3 topCenterTip = new Vector3(xOffset, actualTopY + centerProngExtra, 0f);

        Vector3 leftBase = new Vector3(xOffset - sideProngOut * 0.45f, actualTopY - headHeight * 0.2f, 0f);
        Vector3 leftTip = new Vector3(xOffset - sideProngOut, actualTopY, 0f) + new Vector3(-sideProngTilt, 0f, 0f);

        Vector3 rightBase = new Vector3(xOffset + sideProngOut * 0.45f, actualTopY - headHeight * 0.2f, 0f);
        Vector3 rightTip = new Vector3(xOffset + sideProngOut, actualTopY, 0f) + new Vector3(sideProngTilt, 0f, 0f);

        // Single continuous line with repeated junction points to "branch"
        Vector3[] points = new Vector3[]
        {
            bottom,          // main stick start
            neck,            // up the stick
            leftBase,        // branch to left
            leftTip,         // left prong tip
            leftBase,        // back to junction
            topCenterBase,   // move to center
            topCenterTip,    // center prong tip
            topCenterBase,   // back to junction
            rightBase,       // move to right branch
            rightTip,        // right prong tip
            rightBase        // end back at right junction
        };

        lr.positionCount = points.Length;
        lr.SetPositions(points);
    }
}