using TMPro;
using UnityEngine;

public class GhostDialogueBubble : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Follow")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.9f, 0f);

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 2.5f;
    [SerializeField] private float fadeInDuration = 0.12f;
    [SerializeField] private float fadeOutDuration = 0.18f;

    private Camera mainCam;
    private float timer;
    private bool initialized;
    private bool destroyWhenFinished = true;

    public void Initialize(
        Transform target,
        string text,
        float customLifetime = -1f,
        bool autoDestroy = true)
    {
        followTarget = target;
        destroyWhenFinished = autoDestroy;

        if (dialogueText != null)
            dialogueText.text = text ?? string.Empty;

        if (customLifetime > 0f)
            lifetime = customLifetime;

        timer = 0f;
        initialized = true;

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        SnapToTarget();
    }

    private void Awake()
    {
        if (mainCam == null)
            mainCam = Camera.main;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    private void LateUpdate()
    {
        if (!initialized)
            return;

        if (followTarget == null)
        {
            FinishNow();
            return;
        }

        if (mainCam == null)
            mainCam = Camera.main;

        timer += Time.deltaTime;

        UpdatePosition();
        UpdateFade();

        if (timer >= lifetime)
            FinishNow();
    }

    private void UpdatePosition()
    {
        SnapToTarget();
    }

    private void SnapToTarget()
    {
        if (followTarget == null)
            return;

        transform.position = followTarget.position + worldOffset;
    }

    private void UpdateFade()
    {
        if (canvasGroup == null)
            return;

        float alpha = 1f;

        if (fadeInDuration > 0f && timer < fadeInDuration)
        {
            alpha = timer / fadeInDuration;
        }
        else if (fadeOutDuration > 0f && timer > lifetime - fadeOutDuration)
        {
            float fadeTimer = lifetime - timer;
            alpha = Mathf.Clamp01(fadeTimer / fadeOutDuration);
        }

        canvasGroup.alpha = alpha;
    }

    private void FinishNow()
    {
        initialized = false;

        if (destroyWhenFinished)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}