using System;
using System.Collections.Generic;
using UnityEngine;

public class GlobalTipSeenManager : MonoBehaviour
{
    public static GlobalTipSeenManager Instance { get; private set; }

    private const string PlayerPrefsKey = "chief_of_sin_seen_global_tips";

    [Header("Debug")]
    [SerializeField] private bool log = false;

    private readonly HashSet<string> seenTipIds = new HashSet<string>();

    public event Action OnSeenTipsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadFromPlayerPrefs();
    }

    public bool HasSeen(string tipId)
    {
        if (string.IsNullOrWhiteSpace(tipId))
            return false;

        return seenTipIds.Contains(tipId);
    }

    public void MarkSeen(string tipId)
    {
        if (string.IsNullOrWhiteSpace(tipId))
            return;

        if (!seenTipIds.Add(tipId))
            return;

        SaveToPlayerPrefs();
        OnSeenTipsChanged?.Invoke();

        if (log)
            Debug.Log($"[GlobalTipSeenManager] Marked seen: {tipId}", this);
    }

    public void ClearSeenTips()
    {
        seenTipIds.Clear();
        PlayerPrefs.DeleteKey(PlayerPrefsKey);
        PlayerPrefs.Save();

        OnSeenTipsChanged?.Invoke();

        if (log)
            Debug.Log("[GlobalTipSeenManager] Cleared all seen global tips.", this);
    }

    public GlobalTipSeenState ExportState()
    {
        GlobalTipSeenState state = new GlobalTipSeenState();
        state.seenTipIds.AddRange(seenTipIds);
        return state;
    }

    public void ImportState(GlobalTipSeenState state, bool saveAfterImport = true)
    {
        seenTipIds.Clear();

        if (state != null && state.seenTipIds != null)
        {
            for (int i = 0; i < state.seenTipIds.Count; i++)
            {
                string id = state.seenTipIds[i];

                if (!string.IsNullOrWhiteSpace(id))
                    seenTipIds.Add(id);
            }
        }

        if (saveAfterImport)
            SaveToPlayerPrefs();

        OnSeenTipsChanged?.Invoke();
    }

    private void LoadFromPlayerPrefs()
    {
        seenTipIds.Clear();

        string json = PlayerPrefs.GetString(PlayerPrefsKey, "");

        if (string.IsNullOrWhiteSpace(json))
            return;

        try
        {
            GlobalTipSeenState state = JsonUtility.FromJson<GlobalTipSeenState>(json);
            ImportState(state, saveAfterImport: false);

            if (log)
                Debug.Log($"[GlobalTipSeenManager] Loaded {seenTipIds.Count} seen tip id(s).", this);
        }
        catch
        {
            Debug.LogWarning("[GlobalTipSeenManager] Failed to load seen tips. Resetting seen tip data.", this);
            seenTipIds.Clear();
        }
    }

    private void SaveToPlayerPrefs()
    {
        GlobalTipSeenState state = ExportState();
        string json = JsonUtility.ToJson(state);

        PlayerPrefs.SetString(PlayerPrefsKey, json);
        PlayerPrefs.Save();
    }
}