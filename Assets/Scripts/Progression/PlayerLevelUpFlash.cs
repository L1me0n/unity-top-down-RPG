using System.Collections;
using UnityEngine;

public class PlayerLevelUpFlash : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelSystem levelSystem;

    [Header("Renderers to Flash")]
    [SerializeField] private SpriteRenderer[] renderers;

    [Header("Flash Tuning")]
    [SerializeField] private Color flashColor = new Color(1f, 0.2f, 0.2f, 1f); // red tint
    [SerializeField] private float flashInSeconds = 0.06f;
    [SerializeField] private float flashOutSeconds = 0.16f;

    private Color[] originalColors;
    private Coroutine routine;

    private void Awake()
    {
        if (levelSystem == null) levelSystem = FindFirstObjectByType<LevelSystem>();

        if (renderers == null || renderers.Length == 0)
        {
            // try to grab sprite renderers on this object + children
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i] != null ? renderers[i].color : Color.white;
        }
    }

    private void OnEnable()
    {
        if (levelSystem != null)
            levelSystem.OnLevelChanged += HandleLevelChanged;
    }

    private void OnDisable()
    {
        if (levelSystem != null)
            levelSystem.OnLevelChanged -= HandleLevelChanged;
    }

    private void HandleLevelChanged(int _)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // In case disappear changes colors dynamically, refresh original colors at flash time
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                originalColors[i] = renderers[i].color;
        }

        // Flash in (lerp to red)
        yield return LerpColors(originalColors, flashColor, flashInSeconds);

        // Flash out (lerp back to original per-renderer)
        yield return LerpBack(originalColors, flashOutSeconds);

        routine = null;
    }

    private IEnumerator LerpColors(Color[] fromColors, Color toColor, float seconds)
    {
        if (seconds <= 0f)
        {
            SetAll(toColor);
            yield break;
        }

        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / seconds);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;
                // preserve alpha of each renderer
                Color from = fromColors[i];
                Color target = toColor;
                target.a = from.a;

                renderers[i].color = Color.Lerp(from, target, k);
            }

            yield return null;
        }
    }

    private IEnumerator LerpBack(Color[] toColors, float seconds)
    {
        if (seconds <= 0f)
        {
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null) renderers[i].color = toColors[i];
            yield break;
        }

        // capture current
        Color[] from = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            from[i] = renderers[i] != null ? renderers[i].color : Color.white;

        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / seconds);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;
                renderers[i].color = Color.Lerp(from[i], toColors[i], k);
            }

            yield return null;
        }

        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].color = toColors[i];
    }

    private void SetAll(Color c)
    {
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].color = c;
    }
}