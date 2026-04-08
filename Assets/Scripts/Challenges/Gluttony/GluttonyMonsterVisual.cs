using System.Collections;
using UnityEngine;

public class GluttonyMonsterVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform bodyVisual;
    [SerializeField] private Transform headVisual;
    [SerializeField] private Transform eyesVisual;
    [SerializeField] private SpriteRenderer[] bodyRenderers;
    [SerializeField] private SpriteRenderer[] headRenderers;
    [SerializeField] private SpriteRenderer[] eyeRenderers;

    [Header("Scale")]
    [SerializeField] private Vector3 bodyStartScale = new Vector3(1.2f, 1.2f, 1f);
    [SerializeField] private Vector3 bodyMaxScale = new Vector3(2.2f, 2.2f, 1f);

    [SerializeField] private Vector3 headBaseLocalOffset = new Vector3(0f, 0.72f, 0f);
    [SerializeField] private Vector3 headExtraOffsetAtMaxGrowth = new Vector3(0f, 0.28f, 0f);

    [SerializeField] private Vector3 eyesBaseLocalOffset = new Vector3(0f, 0.06f, 0f);
    [SerializeField] private Vector3 eyesExtraOffsetAtMaxGrowth = new Vector3(0f, 0.04f, 0f);

    [Header("Animation")]
    [SerializeField] private float growTweenSeconds = 0.15f;
    [SerializeField] private float successPulseScaleMultiplier = 1.12f;
    [SerializeField] private float successPulseDuration = 0.16f;
    [SerializeField] private float explodeDuration = 0.28f;
    [SerializeField] private float explodeScaleMultiplier = 1.55f;

    [Header("Debug")]
    [SerializeField] private bool log = false;

    private Coroutine activeRoutine;
    private Vector3 cachedBodyScale;
    private Vector3 cachedHeadLocalPos;
    private Vector3 cachedEyesLocalPos;

    private void Awake()
    {
        ResetVisualInstant();
    }

    public void ResetVisualInstant()
    {
        StopActiveRoutine();

        cachedBodyScale = bodyStartScale;
        cachedHeadLocalPos = ComputeHeadLocalOffset(0f);
        cachedEyesLocalPos = ComputeEyesLocalOffset(0f);

        ApplyVisual(cachedBodyScale, cachedHeadLocalPos, cachedEyesLocalPos);

        SetAlpha(bodyRenderers, 1f);
        SetAlpha(headRenderers, 1f);
        SetAlpha(eyeRenderers, 1f);
    }

    public void SetFullnessVisual(int current, int target)
    {
        float t = target <= 0 ? 0f : Mathf.Clamp01((float)current / target);

        Vector3 targetBodyScale = Vector3.Lerp(bodyStartScale, bodyMaxScale, t);
        Vector3 targetHeadOffset = ComputeHeadLocalOffset(t);
        Vector3 targetEyesOffset = ComputeEyesLocalOffset(t);

        StopActiveRoutine();
        activeRoutine = StartCoroutine(CoTweenTo(targetBodyScale, targetHeadOffset, targetEyesOffset, growTweenSeconds));
    }

    public void PlaySuccessPulse()
    {
        StopActiveRoutine();
        activeRoutine = StartCoroutine(CoSuccessPulse());
    }

    public void PlayExplode()
    {
        StopActiveRoutine();
        activeRoutine = StartCoroutine(CoExplode());
    }

    private Vector3 ComputeHeadLocalOffset(float growth01)
    {
        growth01 = Mathf.Clamp01(growth01);

        float bodyHeightScale = Mathf.Lerp(bodyStartScale.y, bodyMaxScale.y, growth01) / Mathf.Max(0.0001f, bodyStartScale.y);
        Vector3 proportionalLift = new Vector3(
            headBaseLocalOffset.x,
            headBaseLocalOffset.y * bodyHeightScale,
            headBaseLocalOffset.z
        );

        return proportionalLift + Vector3.Lerp(Vector3.zero, headExtraOffsetAtMaxGrowth, growth01);
    }

    private Vector3 ComputeEyesLocalOffset(float growth01)
    {
        growth01 = Mathf.Clamp01(growth01);

        float bodyHeightScale = Mathf.Lerp(bodyStartScale.y, bodyMaxScale.y, growth01) / Mathf.Max(0.0001f, bodyStartScale.y);
        Vector3 proportionalLift = new Vector3(
            eyesBaseLocalOffset.x,
            eyesBaseLocalOffset.y * bodyHeightScale,
            eyesBaseLocalOffset.z
        );

        return proportionalLift + Vector3.Lerp(Vector3.zero, eyesExtraOffsetAtMaxGrowth, growth01);
    }

    private IEnumerator CoTweenTo(Vector3 targetScale, Vector3 targetHeadOffset, Vector3 targetEyesOffset, float duration)
    {
        Vector3 startScale = bodyVisual != null ? bodyVisual.localScale : cachedBodyScale;
        Vector3 startHead = headVisual != null ? headVisual.localPosition : cachedHeadLocalPos;
        Vector3 startEyes = eyesVisual != null ? eyesVisual.localPosition : cachedEyesLocalPos;

        if (duration <= 0f)
        {
            ApplyVisual(targetScale, targetHeadOffset, targetEyesOffset);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);

            ApplyVisual(
                Vector3.Lerp(startScale, targetScale, k),
                Vector3.Lerp(startHead, targetHeadOffset, k),
                Vector3.Lerp(startEyes, targetEyesOffset, k)
            );

            yield return null;
        }

        ApplyVisual(targetScale, targetHeadOffset, targetEyesOffset);
        activeRoutine = null;
    }

    private IEnumerator CoSuccessPulse()
    {
        Vector3 startScale = bodyVisual != null ? bodyVisual.localScale : cachedBodyScale;
        Vector3 headPos = headVisual != null ? headVisual.localPosition : cachedHeadLocalPos;
        Vector3 eyesPos = eyesVisual != null ? eyesVisual.localPosition : cachedEyesLocalPos;
        Vector3 pulseScale = startScale * successPulseScaleMultiplier;

        float half = Mathf.Max(0.01f, successPulseDuration * 0.5f);
        yield return CoTweenTo(pulseScale, headPos, eyesPos, half);
        yield return CoTweenTo(startScale, headPos, eyesPos, half);

        activeRoutine = null;
    }

    private IEnumerator CoExplode()
    {
        Vector3 startScale = bodyVisual != null ? bodyVisual.localScale : cachedBodyScale;
        Vector3 headPos = headVisual != null ? headVisual.localPosition : cachedHeadLocalPos;
        Vector3 eyesPos = eyesVisual != null ? eyesVisual.localPosition : cachedEyesLocalPos;
        Vector3 explodeScale = startScale * explodeScaleMultiplier;

        float half = Mathf.Max(0.01f, explodeDuration * 0.5f);

        yield return CoTweenTo(explodeScale, headPos, eyesPos, half);

        float t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / half);
            float a = Mathf.Lerp(1f, 0f, k);

            SetAlpha(bodyRenderers, a);
            SetAlpha(headRenderers, a);
            SetAlpha(eyeRenderers, a);

            yield return null;
        }

        SetAlpha(bodyRenderers, 0f);
        SetAlpha(headRenderers, 0f);
        SetAlpha(eyeRenderers, 0f);

        activeRoutine = null;
    }

    private void ApplyVisual(Vector3 bodyScale, Vector3 headOffset, Vector3 eyesOffset)
    {
        cachedBodyScale = bodyScale;
        cachedHeadLocalPos = headOffset;
        cachedEyesLocalPos = eyesOffset;

        if (bodyVisual != null)
            bodyVisual.localScale = bodyScale;

        if (headVisual != null)
            headVisual.localPosition = headOffset;

        if (eyesVisual != null)
            eyesVisual.localPosition = eyesOffset;
    }

    private void StopActiveRoutine()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
    }

    private void SetAlpha(SpriteRenderer[] renderers, float a)
    {
        if (renderers == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer sr = renderers[i];
            if (sr == null)
                continue;

            Color c = sr.color;
            c.a = a;
            sr.color = c;
        }
    }
}