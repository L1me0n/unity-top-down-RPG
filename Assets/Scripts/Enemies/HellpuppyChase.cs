using UnityEngine;

public class HellpuppyChase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;

    [Header("Move Tuning")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float acceleration = 40f;
    [SerializeField] private float stopDistance = 0.2f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (target == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) target = go.transform;
        }
    }

    private void FixedUpdate()
    {
        if (target == null) return;

        Vector2 toTarget = (Vector2)(target.position - transform.position);
        float dist = toTarget.magnitude;

        if (dist <= stopDistance)
        {
            // soften to stop so it doesn't jitter
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, 0.25f);
            return;
        }

        Vector2 desiredVel = toTarget.normalized * moveSpeed;
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, desiredVel, acceleration * Time.fixedDeltaTime);
    }

    public void SetTarget(Transform t) => target = t;
}