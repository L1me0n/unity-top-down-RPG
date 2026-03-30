using UnityEngine;

public class CampfireRecoveryHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private APRegen apRegen;
    [SerializeField] private HPRegen hpRegen;

    [Header("Debug")]
    [SerializeField] private bool logCampfireRecovery = true;

    private void Awake()
    {
        if (roomManager == null)
            roomManager = FindFirstObjectByType<RoomManager>();

        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();
        if (apRegen == null)
            apRegen = FindFirstObjectByType<APRegen>();
        if (hpRegen == null)
            hpRegen = FindFirstObjectByType<HPRegen>();
    }

    private void OnEnable()
    {
        if (roomManager != null)
            roomManager.OnCampfireEntered += HandleCampfireEntered;
    }

    private void OnDisable()
    {
        if (roomManager != null)
            roomManager.OnCampfireEntered -= HandleCampfireEntered;
    }

    private void HandleCampfireEntered(Vector2Int coord, RoomState state)
    {
        if (playerStats == null)
        {
            if (logCampfireRecovery)
                Debug.LogWarning("[CampfireRecoveryHandler] Campfire entered, but PlayerStats was not found.");
            return;
        }

        if (logCampfireRecovery)
            Debug.Log($"[CampfireRecoveryHandler] Campfire recovery hook triggered at {coord}.");

        
        int missingHP = playerStats.MaxHP - playerStats.HP;
        if (missingHP > 0)
        {
            playerStats.Heal(missingHP);
        }

        int missingAP = playerStats.MaxAP - playerStats.AP;
        if (missingAP > 0)
        {
            playerStats.GainAP(missingAP);
        }

        if (hpRegen != null)
            hpRegen.ResetRegenState();

        if (apRegen != null)
            apRegen.ResetRegenState();

        if (logCampfireRecovery)
            Debug.Log(
                
                $"[CampfireRecoveryHandler] Restored HP/AP at campfire {coord}." +
                $" HP is now {playerStats.HP}/{playerStats.MaxHP}, AP is now {playerStats.AP}/{playerStats.MaxAP}.");
    }
}