using UnityEngine;

public class SimpleHazardZone : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damagePerTick = 1;
    [SerializeField] private float tickSeconds = 0.5f;

    private PlayerDamageReceiver inside;
    private float timer;

    private void Update()
    {
        if (inside == null) return;

        timer += Time.deltaTime;
        if (timer >= tickSeconds)
        {
            timer -= tickSeconds;
            inside.ApplyDamage(damagePerTick);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var r = other.GetComponent<PlayerDamageReceiver>();
        if (r != null)
        {
            inside = r;
            timer = 0f;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var r = other.GetComponent<PlayerDamageReceiver>();
        if (r != null && r == inside)
            inside = null;
    }
}