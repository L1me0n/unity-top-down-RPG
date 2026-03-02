using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeathHandler : MonoBehaviour
{
    [SerializeField] private KeyCode resetKey = KeyCode.R;

    private PlayerDamageReceiver receiver;
    private bool dead;

    private MonoBehaviour[] disableOnDeath;

    private void Awake()
    {
        receiver = GetComponent<PlayerDamageReceiver>();
        receiver.OnDied += HandleDeath;

        // Disable these on death (movement, combat, aiming, etc.)
        // This grabs all MonoBehaviours on the Player and we filter out ourselves.
        disableOnDeath = GetComponents<MonoBehaviour>();
    }

    private void HandleDeath()
    {
        dead = true;
        Debug.Log("[Death] You Died. Press R to reset.");

        // Disable most player scripts so to stop moving/shooting
        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            var b = disableOnDeath[i];
            if (b == null) continue;
            if (b == this) continue;
            if (b is PlayerDamageReceiver) continue; // keep receiver alive
            b.enabled = false;
        }
    }

    private void Update()
    {
        if (!dead) return;

        if (Input.GetKeyDown(resetKey))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}