using UnityEngine;
using System;

public class CampfireRecoveryHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private APRegen apRegen;
    [SerializeField] private HPRegen hpRegen;

    [Header("Debug")]
    [SerializeField] private bool logCampfireRecovery = true;

    public struct CampfireRecoveryResult
    {
        public Vector2Int coord;
        public int restoredHP;
        public int restoredAP;

        public CampfireRecoveryResult(Vector2Int coord, int restoredHP, int restoredAP)
        {
            this.coord = coord;
            this.restoredHP = restoredHP;
            this.restoredAP = restoredAP;
        }

        public bool RestoredAnything => restoredHP > 0 || restoredAP > 0;
    }

    public event Action<CampfireRecoveryResult> OnCampfireRecovered;

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
        int missingAP = playerStats.MaxAP - playerStats.AP;

        if (missingHP > 0)
            playerStats.Heal(missingHP);

        if (missingAP > 0)
            playerStats.GainAP(missingAP);

        if (hpRegen != null)
            hpRegen.ResetRegenState();

        if (apRegen != null)
            apRegen.ResetRegenState();

        if (logCampfireRecovery)
        {
            Debug.Log(
                $"[CampfireRecoveryHandler] Restored HP/AP at campfire {coord}." +
                $" HP is now {playerStats.HP}/{playerStats.MaxHP}, AP is now {playerStats.AP}/{playerStats.MaxAP}.");
        }

        CampfireRecoveryResult result = new CampfireRecoveryResult(coord, missingHP, missingAP);

        bool suppressPopupBecauseOfCheckpointRespawn =
            roomManager != null && roomManager.SuppressNextCampfireRecoveryFeedback;

        if (result.RestoredAnything && !suppressPopupBecauseOfCheckpointRespawn)
        {
            OnCampfireRecovered?.Invoke(result);
        }
        else if (result.RestoredAnything && suppressPopupBecauseOfCheckpointRespawn)
        {
            if (logCampfireRecovery)
            {
                Debug.Log(
                    $"[CampfireRecoveryHandler] Recovery popup suppressed at {coord} " +
                    $"because this campfire entry came from checkpoint respawn.");
            }
        }
        else if (logCampfireRecovery)
        {
            Debug.Log($"[CampfireRecoveryHandler] No recovery popup needed at {coord} because HP/AP were already full.");
        }
    }
}