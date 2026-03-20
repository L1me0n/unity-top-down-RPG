using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DevilChaoticLimbLine : MonoBehaviour
{
    [Header("Shape")]
    [SerializeField] private int segments = 18;
    [SerializeField] private float length = 1.8f;
    [SerializeField] private Vector2 overallDirection = new Vector2(1f, -0.15f);

    [Header("Curves")]
    [SerializeField] private float bend1Amount = 0.35f;
    [SerializeField] private float bend2Amount = -0.22f;
    [SerializeField] private float bend1Position = 0.28f;
    [SerializeField] private float bend2Position = 0.72f;

    [Header("Chaos")]
    [SerializeField] private float noiseAmplitude = 0.08f;
    [SerializeField] private float noiseFrequency = 4.0f;
    [SerializeField] private float seedOffset = 0f;

    [Header("Line")]
    [SerializeField] private float lineWidth = 0.09f;
    [SerializeField] private bool taperEnd = true;

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
        lr.positionCount = Mathf.Max(2, segments);
        lr.startWidth = lineWidth;
        lr.endWidth = taperEnd ? lineWidth * 0.55f : lineWidth;
    }

    [ContextMenu("Build Limb")]
    public void Build()
    {
        if (lr == null)
            EnsureLine();

        int count = Mathf.Max(2, segments);
        lr.positionCount = count;

        Vector2 dir = overallDirection.sqrMagnitude > 0.0001f ? overallDirection.normalized : Vector2.right;
        Vector2 normal = new Vector2(-dir.y, dir.x);

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / (count - 1);

            Vector2 basePos = dir * (length * t);

            float bend1 = GaussianLike(t, bend1Position, 0.18f) * bend1Amount;
            float bend2 = GaussianLike(t, bend2Position, 0.15f) * bend2Amount;
            float noise = Mathf.Sin((t + seedOffset) * Mathf.PI * 2f * noiseFrequency) * noiseAmplitude;

            Vector2 offset = normal * (bend1 + bend2 + noise);
            Vector2 final = basePos + offset;

            lr.SetPosition(i, new Vector3(final.x, final.y, 0f));
        }
    }

    private float GaussianLike(float x, float center, float width)
    {
        float d = (x - center) / Mathf.Max(0.0001f, width);
        return Mathf.Exp(-d * d);
    }
}