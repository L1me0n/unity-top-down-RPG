using System.Collections;
using UnityEngine;
using TMPro;

public class LevelUpPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelSystem levelSystem;

    [Header("UI")]
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private float showSeconds = 1.0f;

    private Coroutine routine;

    private void Awake()
    {
        if (levelSystem == null) levelSystem = FindFirstObjectByType<LevelSystem>();
        if (popupText != null) popupText.gameObject.SetActive(false);
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
        routine = StartCoroutine(ShowRoutine(newLevel));
    }

    private IEnumerator ShowRoutine(int level)
    {
        popupText.text = $"LEVEL UP!  Lvl: {level}  (+1 point)";
        popupText.gameObject.SetActive(true);

        yield return new WaitForSeconds(showSeconds);

        popupText.gameObject.SetActive(false);
        routine = null;
    }
}