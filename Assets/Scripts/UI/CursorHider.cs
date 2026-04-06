using UnityEngine;

public class CursorHider : MonoBehaviour
{
    [Header("Gameplay Cursor")]
    [SerializeField] private bool hideCursor = true;
    [SerializeField] private bool confineToWindow = true;

    [Header("UI Cursor")]
    [SerializeField] private bool showCursorWhenGameplayBlocked = true;
    [SerializeField] private bool confineCursorWhenGameplayBlocked = true;

    [Header("Debug")]
    [SerializeField] private bool log = false;

    private KeyCode toggleKey = KeyCode.E;
    private bool lastBlockedState;

    private void OnEnable()
    {
        ApplyForCurrentState(forceLog: false);
    }

    private void OnDisable()
    {
        // restore for editor convenience
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey) && !UIInputBlocker.BlockGameplayInput)
        {
            hideCursor = !hideCursor;
            ApplyGameplayCursorState();

            if (log)
                Debug.Log($"[CursorHider] Toggled gameplay cursor. hideCursor={hideCursor}", this);
        }

        if (lastBlockedState != UIInputBlocker.BlockGameplayInput)
        {
            ApplyForCurrentState(forceLog: true);
        }
    }

    private void ApplyForCurrentState(bool forceLog)
    {
        lastBlockedState = UIInputBlocker.BlockGameplayInput;

        if (lastBlockedState)
            ApplyBlockedUIState();
        else
            ApplyGameplayCursorState();

        if (log || forceLog)
        {
            Debug.Log(
                $"[CursorHider] Applied cursor state | " +
                $"gameplayBlocked={lastBlockedState} | " +
                $"cursorVisible={Cursor.visible} | " +
                $"lockState={Cursor.lockState}",
                this
            );
        }
    }

    private void ApplyGameplayCursorState()
    {
        Cursor.visible = !hideCursor;

        if (hideCursor)
        {
            Cursor.lockState = confineToWindow
                ? CursorLockMode.Confined
                : CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = confineToWindow
                ? CursorLockMode.Confined
                : CursorLockMode.None;
        }
    }

    private void ApplyBlockedUIState()
    {
        Cursor.visible = showCursorWhenGameplayBlocked;

        Cursor.lockState = confineCursorWhenGameplayBlocked
            ? CursorLockMode.Confined
            : CursorLockMode.None;
    }
}