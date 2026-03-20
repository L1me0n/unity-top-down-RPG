using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DevilSpiralBaseLine : MonoBehaviour
{
    [Header("Shape")]
    [SerializeField] private int segments = 80;
    [SerializeField] private int turns = 3;
    [SerializeField] private float maxRadius = 0.85f;
    [SerializeField] private float verticalStretch = 0.45f;

    [Header("Style")]
    [SerializeField] private bool outsideToInside = true;
    [SerializeField] private float wobbleAmplitude = 0.03f;
    [SerializeField] private float wobbleFrequency = 5f;
    [SerializeField] private float seedOffset = 0f;

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
        lr.positionCount = Mathf.Max(4, segments);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth * 0.75f;
    }

    [ContextMenu("Build Spiral")]
    public void Build()
    {
        if (lr == null)
            EnsureLine();

        int count = Mathf.Max(4, segments);
        lr.positionCount = count;

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / (count - 1);
            float spiralT = outsideToInside ? t : 1f - t;

            float angle = spiralT * turns * Mathf.PI * 2f;
            float radius = Mathf.Lerp(maxRadius, 0.08f, spiralT);

            float wobble = Mathf.Sin((spiralT + seedOffset) * Mathf.PI * 2f * wobbleFrequency) * wobbleAmplitude;
            radius += wobble;

            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius * verticalStretch;

            lr.SetPosition(i, new Vector3(x, y, 0f));
        }
    }
}