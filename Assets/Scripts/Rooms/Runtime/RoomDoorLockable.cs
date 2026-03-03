using UnityEngine;

public class RoomDoorLockable : MonoBehaviour
{
    [SerializeField] private Collider2D doorTrigger;
    [SerializeField] private GameObject visual;

    private bool locked;

    private void Awake()
    {
        if (doorTrigger == null) doorTrigger = GetComponent<Collider2D>();
    }

    public void SetLocked(bool value)
    {
        locked = value;

        if (doorTrigger != null)
            doorTrigger.enabled = !locked;

        if (visual != null)
            visual.SetActive(!locked);
    }
}