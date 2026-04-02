using UnityEngine;

public class HostageGhostVisual : MonoBehaviour
{
    [Header("Idle")]
    [SerializeField] private float idleBobAmplitude = 0.05f;
    [SerializeField] private float idleBobSpeed = 2f;

    [Header("Escape")]
    [SerializeField] private float escapeSpeed = 2.5f;
    [SerializeField] private float reachDistance = 0.08f;
    [SerializeField] private float escapeTimeoutPadding = 1f;

    private Vector3 baseLocalPosition;
    private bool escaping;
    private Vector3 escapeTarget;
    private float escapeTimer;
    private float maxEscapeLifetime;

    private void Awake()
    {
        baseLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (!escaping)
        {
            float bob = Mathf.Sin(Time.time * idleBobSpeed) * idleBobAmplitude;
            transform.localPosition = baseLocalPosition + new Vector3(0f, bob, 0f);
            return;
        }

        escapeTimer += Time.deltaTime;

        Vector3 current = transform.position;
        Vector3 next = Vector3.MoveTowards(current, escapeTarget, escapeSpeed * Time.deltaTime);
        transform.position = next;

        if (Vector3.Distance(next, escapeTarget) <= reachDistance || escapeTimer >= maxEscapeLifetime)
        {
            Destroy(gameObject);
        }
    }

    public void BeginEscape(Vector3 targetWorldPosition)
    {
        escaping = true;
        escapeTarget = targetWorldPosition;
        escapeTimer = 0f;

        float distance = Vector3.Distance(transform.position, escapeTarget);
        maxEscapeLifetime = (distance / Mathf.Max(escapeSpeed, 0.01f)) + escapeTimeoutPadding;
    }
}