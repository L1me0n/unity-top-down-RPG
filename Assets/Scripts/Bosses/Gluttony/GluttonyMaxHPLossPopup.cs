using System.Collections;
using TMPro;
using UnityEngine;

public class GluttonyMaxHPLossPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerDamageReceiver playerDamageReceiver;

    [Header("UI")]
    [SerializeField] private TMP_Text popupText;

    [Header("Text")]
    [SerializeField] private string messageFormat = "-{0} HP";

    [Header("Timing")]
    [SerializeField] private float fadeInSeconds = 0.08f;
    [SerializeField] private float holdSeconds = 0.55f;
    [SerializeField] private float fadeOutSeconds = 0.45f;

    private Coroutine routine;

    private void Awake()
    {
        if (playerDamageReceiver == null)
            playerDamageReceiver = FindFirstObjectByType<PlayerDamageReceiver>();

        if (popupText != null)
        {
            popupText.gameObject.SetActive(true);
            SetAlpha(0f);
        }
    }

    private void OnEnable()
    {
        if (playerDamageReceiver == null)
            playerDamageReceiver = FindFirstObjectByType<PlayerDamageReceiver>();

        if (playerDamageReceiver != null)
            playerDamageReceiver.OnMaxHPLost += HandleMaxHPLost;
    }

    private void OnDisable()
    {
        if (playerDamageReceiver != null)
            playerDamageReceiver.OnMaxHPLost -= HandleMaxHPLost;
    }

    private void HandleMaxHPLost(int amount)
    {
        if (popupText == null)
            return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(Animate(amount));
    }

    private IEnumerator Animate(int amount)
    {
        popupText.text = string.Format(messageFormat, amount);

        yield return FadeAlpha(0f, 1f, fadeInSeconds);
        yield return new WaitForSecondsRealtime(holdSeconds);
        yield return FadeAlpha(1f, 0f, fadeOutSeconds);

        routine = null;
    }

    private IEnumerator FadeAlpha(float from, float to, float seconds)
    {
        if (popupText == null)
            yield break;

        if (seconds <= 0f)
        {
            SetAlpha(to);
            yield break;
        }

        float t = 0f;
        SetAlpha(from);

        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / seconds));
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(to);
    }

    private void SetAlpha(float a)
    {
        if (popupText == null)
            return;

        Color c = popupText.color;
        c.a = a;
        popupText.color = c;
    }
}