using System.Collections;
using UnityEngine;

public class PlayerLevelUpFlash : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelSystem levelSystem;

    [Header("Renderers to Flash")]
    [SerializeField] private SpriteRenderer[] renderers;

    [Header("Flash Tuning")]
    [SerializeField] private Color flashColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private float flashInSeconds = 0.06f;
    [SerializeField] private float flashOutSeconds = 0.16f;

    private Color[] originalColors;
    private Coroutine routine;

    private void Awake()
    {
        if (levelSystem == null)
            levelSystem = FindFirstObjectByType<LevelSystem>();

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<SpriteRenderer>(true);

        CacheOriginalColors();
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
        PlayFlash();
    }

    public void PlayFlash()
    {
        PlayFlashWithColor(flashColor);
    }

    public void PlayFlashWithColor(Color color)
    {
        if (renderers == null || renderers.Length == 0)
            return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(FlashRoutine(color));
    }

    private IEnumerator FlashRoutine(Color color)
    {
        // In case Disappear or other systems changed colors dynamically,
        // refresh original colors at flash time.
        CacheOriginalColors();

        yield return LerpColors(originalColors, color, flashInSeconds);
        yield return LerpBack(originalColors, flashOutSeconds);

        routine = null;
    }

    private void CacheOriginalColors()
    {
        if (renderers == null)
        {
            originalColors = new Color[0];
            return;
        }

        if (originalColors == null || originalColors.Length != renderers.Length)
            originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i] != null ? renderers[i].color : Color.white;
        }
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
                if (renderers[i] == null)
                    continue;

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
            {
                if (renderers[i] != null)
                    renderers[i].color = toColors[i];
            }

            yield break;
        }

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
                if (renderers[i] == null)
                    continue;

                renderers[i].color = Color.Lerp(from[i], toColors[i], k);
            }

            yield return null;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].color = toColors[i];
        }
    }

    private void SetAll(Color c)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].color = c;
        }
    }
}