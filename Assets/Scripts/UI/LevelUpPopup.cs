using System.Collections;
using UnityEngine;
using TMPro;

public class LevelUpPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelSystem levelSystem;

    [Header("UI")]
    [SerializeField] private TMP_Text popupText;

    [Header("Timing")]
    [SerializeField] private float fadeInSeconds = 0.08f;
    [SerializeField] private float holdSeconds = 0.45f;
    [SerializeField] private float fadeOutSeconds = 0.45f;

    private Coroutine routine;

    private void Awake()
    {
        if (levelSystem == null) levelSystem = FindFirstObjectByType<LevelSystem>();
        if (popupText != null) 
        {
            // start hidden
            var c = popupText.color;
            c.a = 0f;
            popupText.color = c;
            popupText.gameObject.SetActive(true);
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

    private void HandleLevelChanged(int newLevel)
    {
        if (popupText == null) return;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Animate(newLevel));
    }

    private IEnumerator Animate(int level)
    {
        popupText.text = $"LEVEL UP!  Lvl: {level}  (+1 point)";

        // Fade in
        yield return FadeAlpha(0f, 1f, fadeInSeconds);

        // Hold
        yield return new WaitForSeconds(holdSeconds);

        // Fade out
        yield return FadeAlpha(1f, 0f, fadeOutSeconds);

        routine = null;
    }

    private IEnumerator FadeAlpha(float from, float to, float seconds)
    {
        if (seconds <= 0f)
        {
            SetAlpha(to);
            yield break;
        }

        float t = 0f;
        SetAlpha(from);

        while (t < seconds)
        {
            t += Time.unscaledDeltaTime; // works even if you pause timeScale elsewhere
            float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / seconds));
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(to);
    }

    private void SetAlpha(float a)
    {
        var c = popupText.color;
        c.a = a;
        popupText.color = c;
    }
}