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
        if (UIInputBlocker.BlockGameplayInput)
            return;
        // Reset per-frame intents
        WantsFire = false;
        WantsDisappear = false;
        ToggledThisFrame = false;

        // Toggle mode
        if (Input.GetKeyDown(toggleModeKey) || Input.GetKeyDown(toggleModeAltKey))
        {
            mode = (mode == CombatMode.Attack) ? CombatMode.Defense : CombatMode.Attack;
            ToggledThisFrame = true;
            OnModeChanged?.Invoke(mode);
        }

        // Read action intent based on current mode.
        // We still read both keys, but only "activate" the one for the mode.
        bool firePressed = Input.GetMouseButton(0);       // hold to fire later
        bool disappearPressed = Input.GetMouseButtonDown(1) || Input.GetKeyDown(disappearKey); // tap to disappear

        if (mode == CombatMode.Attack)
        {
            WantsFire = firePressed;
        }
        else // Defense
        {
            WantsDisappear = disappearPressed;
        }
    }
}
