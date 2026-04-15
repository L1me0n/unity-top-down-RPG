using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    [Header("Mode")]
    [SerializeField] private CombatMode mode = CombatMode.Attack;

    [Header("Input")]
    [SerializeField] private KeyCode toggleModeKey = KeyCode.Q;
    [SerializeField] private KeyCode toggleModeAltKey = KeyCode.Tab;
    [SerializeField] private KeyCode disappearKey = KeyCode.Space;

    public CombatMode Mode => mode;

    // Intent outputs
    public bool WantsFire { get; private set; }
    public bool WantsDisappear { get; private set; }
    public bool ToggledThisFrame { get; private set; }

    public System.Action<CombatMode> OnModeChanged;

    private PlayerStats stats;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        WantsFire = false;
        WantsDisappear = false;
        ToggledThisFrame = false;

        if (UIInputBlocker.BlockGameplayInput)
            return;

        if (Input.GetKeyDown(toggleModeKey) || Input.GetKeyDown(toggleModeAltKey))
        {
            ToggleMode();
        }

        bool firePressed = Input.GetMouseButton(0);
        bool disappearPressed = Input.GetMouseButtonDown(1) || Input.GetKeyDown(disappearKey);

        if (mode == CombatMode.Attack)
            WantsFire = firePressed;
        else
            WantsDisappear = disappearPressed;
    }

    public void SetMode(CombatMode newMode, bool forceEvent = false)
    {
        if (!forceEvent && mode == newMode)
            return;

        mode = newMode;
        OnModeChanged?.Invoke(mode);
    }

    public void ToggleMode()
    {
        mode = (mode == CombatMode.Attack) ? CombatMode.Defense : CombatMode.Attack;
        ToggledThisFrame = true;
        OnModeChanged?.Invoke(mode);
    }

    public void SetAttackMode()
    {
        SetMode(CombatMode.Attack);
    }

    public void SetDefenseMode()
    {
        SetMode(CombatMode.Defense);
    }
}