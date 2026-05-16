using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Chief Of Sin/Tips/Global Tip Database")]
public class GlobalTipDatabase : ScriptableObject
{
    [SerializeField] private List<GlobalTipDefinition> tips = new List<GlobalTipDefinition>();

    public IReadOnlyList<GlobalTipDefinition> Tips => tips;

    public GlobalTipDefinition GetTip(string tipId)
    {
        if (string.IsNullOrWhiteSpace(tipId))
            return null;

        for (int i = 0; i < tips.Count; i++)
        {
            GlobalTipDefinition tip = tips[i];

            if (tip == null)
                continue;

            if (tip.tipId == tipId)
                return tip;
        }

        return null;
    }

    public bool ContainsTip(string tipId)
    {
        return GetTip(tipId) != null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        HashSet<string> usedIds = new HashSet<string>();

        for (int i = 0; i < tips.Count; i++)
        {
            GlobalTipDefinition tip = tips[i];

            if (tip == null)
                continue;

            if (string.IsNullOrWhiteSpace(tip.tipId))
            {
                Debug.LogWarning($"[GlobalTipDatabase] Tip at index {i} has an empty tipId.", this);
                continue;
            }

            if (!usedIds.Add(tip.tipId))
                Debug.LogWarning($"[GlobalTipDatabase] Duplicate tipId found: {tip.tipId}", this);
        }
    }
#endif
}