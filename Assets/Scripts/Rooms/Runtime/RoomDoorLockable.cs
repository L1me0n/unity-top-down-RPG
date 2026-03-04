using UnityEngine;

public class RoomDoorLockable : MonoBehaviour
{
    [SerializeField] private Collider2D doorTrigger;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer[] renderers;
    [SerializeField, Range(0f, 1f)] private float ghostAlpha = 0.35f;

    private bool locked;

    private void Awake()
    {
        if (doorTrigger == null) doorTrigger = GetComponent<Collider2D>();
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    public void SetLocked(bool value)
    {
        locked = value;

        if (doorTrigger != null)
            doorTrigger.enabled = !locked;

        SetAlpha(locked ? ghostAlpha : 1f);
    }

    private void SetAlpha(float a)
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            Color c = renderers[i].color;
            c.a = a;
            renderers[i].color = c;
        }
    }
}