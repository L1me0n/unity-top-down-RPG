using System.Collections;
using UnityEngine;

public class LuciferPalmMagicVisual : MonoBehaviour
{
    [Header("Anchors")]
    [SerializeField] private Transform leftPalmAnchor;
    [SerializeField] private Transform rightPalmAnchor;

    [Header("Prefab")]
    [SerializeField] private GameObject magicPrefab;

    [Header("Animation")]
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private float orbitRadius = 0.12f;
    [SerializeField] private float orbitSpeed = 70f;

    [Header("Optional Orbiters By Name")]
    [SerializeField] private string leftOrbiterRootName = "";
    [SerializeField] private string rightOrbiterRootName = "";

    [Header("Debug")]
    [SerializeField] private bool log = true;

    private bool active;
    private Coroutine animateRoutine;

    private GameObject leftInstance;
    private GameObject rightInstance;

    private Transform[] leftOrbiters;
    private Transform[] rightOrbiters;

    private void Awake()
    {
        EnsureInstances();
        HideImmediate();
    }

    public void Show()
    {
        EnsureInstances();

        if (leftInstance == null || rightInstance == null)
        {
            Log("Show failed because magic instances could not be created.");
            return;
        }

        if (active)
            return;

        active = true;

        leftInstance.SetActive(true);
        rightInstance.SetActive(true);

        SnapToAnchors();

        if (animateRoutine != null)
            StopCoroutine(animateRoutine);

        animateRoutine = StartCoroutine(CoAnimate());
        Log("Palm magic shown.");
    }

    public void Hide()
    {
        active = false;

        if (animateRoutine != null)
        {
            StopCoroutine(animateRoutine);
            animateRoutine = null;
        }

        if (leftInstance != null)
            leftInstance.SetActive(false);

        if (rightInstance != null)
            rightInstance.SetActive(false);

        Log("Palm magic hidden.");
    }

    public void HideImmediate()
    {
        active = false;

        if (animateRoutine != null)
        {
            StopCoroutine(animateRoutine);
            animateRoutine = null;
        }

        if (leftInstance != null)
            leftInstance.SetActive(false);

        if (rightInstance != null)
            rightInstance.SetActive(false);
    }

    private void EnsureInstances()
    {
        if (magicPrefab == null)
        {
            Log("No magicPrefab assigned.");
            return;
        }

        if (leftPalmAnchor == null || rightPalmAnchor == null)
        {
            Log("Palm anchors are missing.");
            return;
        }

        if (leftInstance == null)
        {
            leftInstance = Instantiate(magicPrefab, leftPalmAnchor.position, Quaternion.identity, leftPalmAnchor);
            leftInstance.name = $"{magicPrefab.name}_LeftRuntime";
            leftOrbiters = CollectOrbiters(leftInstance.transform, leftOrbiterRootName);
            Log("Created left palm magic instance.");
        }

        if (rightInstance == null)
        {
            rightInstance = Instantiate(magicPrefab, rightPalmAnchor.position, Quaternion.identity, rightPalmAnchor);
            rightInstance.name = $"{magicPrefab.name}_RightRuntime";
            rightOrbiters = CollectOrbiters(rightInstance.transform, rightOrbiterRootName);
            Log("Created right palm magic instance.");
        }
    }

    private void SnapToAnchors()
    {
        if (leftInstance != null && leftPalmAnchor != null)
        {
            leftInstance.transform.SetParent(leftPalmAnchor, worldPositionStays: false);
            leftInstance.transform.localPosition = Vector3.zero;
        }

        if (rightInstance != null && rightPalmAnchor != null)
        {
            rightInstance.transform.SetParent(rightPalmAnchor, worldPositionStays: false);
            rightInstance.transform.localPosition = Vector3.zero;
        }
    }

    private IEnumerator CoAnimate()
    {
        float t = 0f;

        while (active)
        {
            t += Time.unscaledDeltaTime;

            float pulse01 = 0.5f + 0.5f * Mathf.Sin(t * pulseSpeed * Mathf.PI * 2f);
            float scale = Mathf.Lerp(minScale, maxScale, pulse01);

            if (leftInstance != null)
                leftInstance.transform.localScale = Vector3.one * scale;

            if (rightInstance != null)
                rightInstance.transform.localScale = Vector3.one * scale;

            AnimateOrbiters(leftOrbiters, t, false);
            AnimateOrbiters(rightOrbiters, t, true);

            yield return null;
        }

        animateRoutine = null;
    }

    private void AnimateOrbiters(Transform[] orbiters, float t, bool invert)
    {
        if (orbiters == null || orbiters.Length == 0)
            return;

        for (int i = 0; i < orbiters.Length; i++)
        {
            Transform orbiter = orbiters[i];
            if (orbiter == null)
                continue;

            float angleDeg = t * orbitSpeed * (invert ? -1f : 1f) + (360f / Mathf.Max(1, orbiters.Length)) * i;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Cos(angleRad) * orbitRadius,
                Mathf.Sin(angleRad) * orbitRadius,
                0f
            );

            orbiter.localPosition = offset;
        }
    }

    private Transform[] CollectOrbiters(Transform instanceRoot, string explicitRootName)
    {
        if (instanceRoot == null)
            return System.Array.Empty<Transform>();

        Transform orbitRoot = null;

        if (!string.IsNullOrWhiteSpace(explicitRootName))
            orbitRoot = instanceRoot.Find(explicitRootName);

        if (orbitRoot == null)
            orbitRoot = instanceRoot.Find("Orbiters");

        if (orbitRoot == null)
            return System.Array.Empty<Transform>();

        int childCount = orbitRoot.childCount;
        Transform[] result = new Transform[childCount];

        for (int i = 0; i < childCount; i++)
            result[i] = orbitRoot.GetChild(i);

        return result;
    }

    private void Log(string message)
    {
        if (!log)
            return;

        Debug.Log($"[LuciferPalmMagicVisual] {message}", this);
    }
}