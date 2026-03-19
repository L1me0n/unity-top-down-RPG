using UnityEngine;

public class SimpleRingVisual : MonoBehaviour
{
    [SerializeField] private int segments = 40;
    [SerializeField] private float radius = 0.9f;

    private LineRenderer lr;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        BuildRing();
    }

    private void OnValidate()
    {
        if (lr == null)
            lr = GetComponent<LineRenderer>();

        if (lr != null)
            BuildRing();
    }

    private void BuildRing()
    {
        if (segments < 3)
            segments = 3;

        lr.loop = true;
        lr.useWorldSpace = false;
        lr.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / segments;
            float angle = t * Mathf.PI * 2f;
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );
            lr.SetPosition(i, pos);
        }
    }
}