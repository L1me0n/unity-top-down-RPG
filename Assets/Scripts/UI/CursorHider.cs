using UnityEngine;

public class CursorHider : MonoBehaviour
{
    [SerializeField] private bool hideCursor = true;
    [SerializeField] private bool confineToWindow = true;

    private KeyCode toggleKey = KeyCode.E;

    private void OnEnable()
    {
        Apply();
    }

    private void OnDisable()
    {
        // restore for editor convenience
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            hideCursor = !hideCursor;
            Apply();
        }
    }

    private void Apply()
    {
        Cursor.visible = !hideCursor;

        if (confineToWindow)
            Cursor.lockState = CursorLockMode.Confined; // stays inside game window
        else
            Cursor.lockState = CursorLockMode.None;
    }
}