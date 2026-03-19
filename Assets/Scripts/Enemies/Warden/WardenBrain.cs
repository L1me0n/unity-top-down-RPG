using UnityEngine;

public class WardenBrain : EnemyBrainBase
{
    public enum WardenMode
    {
        Attack,
        Defense
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float preferredDistance = 5f;
    [SerializeField] private float retreatDistance = 3f;
    [SerializeField] private float stoppingTolerance = 0.35f;

    [Header("Mode Loop")]
    [SerializeField] private float attackModeDurationMin = 1f;
    [SerializeField] private float attackModeDurationMax = 3f;
    [SerializeField] private float defenseModeDurationMin = 1f;
    [SerializeField] private float defenseModeDurationMax = 3f;
    [SerializeField] private bool startInAttackMode = true;

    [Header("Visual")]
    [SerializeField] private GameObject defenseRingVisual;

    [Header("Debug")]
    [SerializeField] private bool logModeChanges = false;

    private WardenMode currentMode;
    private float modeTimer;

    public bool IsInAttackMode => currentMode == WardenMode.Attack;
    public bool IsInDefenseMode => currentMode == WardenMode.Defense;
    public WardenMode CurrentMode => currentMode;

    protected override void Awake()
    {
        base.Awake();
        SetMode(startInAttackMode ? WardenMode.Attack : WardenMode.Defense);
    }

    private void Update()
    {
        if (!HasTarget())
            TryFindPlayerTarget();

        modeTimer -= Time.deltaTime;
        if (modeTimer <= 0f)
        {
            if (currentMode == WardenMode.Attack)
                SetMode(WardenMode.Defense);
            else
                SetMode(WardenMode.Attack);
        }
    }

    private void FixedUpdate()
    {
        if (!HasTarget())
        {
            StopMovement(0.2f);
            return;
        }

        if (IsInDefenseMode)
        {
            StopMovement(0.2f);
            return;
        }

        HandleAttackModeMovement();
    }

    private void HandleAttackModeMovement()
    {
        float distance = DistanceToTarget();

        if (distance > preferredDistance + stoppingTolerance)
        {
            MoveTowardsTarget(moveSpeed, acceleration);
            return;
        }

        if (distance < retreatDistance)
        {
            MoveAwayFromTarget(moveSpeed, acceleration);
            return;
        }

        StopMovement(0.15f);
    }

    private void MoveAwayFromTarget(float speed, float accel)
    {
        if (rb == null || !HasTarget())
            return;

        Vector2 desiredVelocity = -DirectionToTarget() * speed;
        rb.linearVelocity = Vector2.MoveTowards(
            rb.linearVelocity,
            desiredVelocity,
            accel * Time.fixedDeltaTime
        );
    }

    private void SetMode(WardenMode newMode)
    {
        currentMode = newMode;

        if (currentMode == WardenMode.Attack)
            modeTimer = Random.Range(attackModeDurationMin, attackModeDurationMax);
        else
            modeTimer = Random.Range(defenseModeDurationMin, defenseModeDurationMax);
            
        if (defenseRingVisual != null)
            defenseRingVisual.SetActive(currentMode == WardenMode.Defense);

        if (logModeChanges)
            Debug.Log($"[WardenBrain] {name} -> {currentMode} for {modeTimer:0.00}s");
    }
}