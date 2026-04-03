using UnityEngine;

public class GhostDialogueSpeaker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GhostDialogueBubble bubblePrefab;
    [SerializeField] private Transform bubbleAnchor;

    [Header("Timing")]
    [SerializeField] private float defaultBubbleLifetime = 2.5f;
    [SerializeField] private float speakCooldown = 2f;

    private GhostDialogueBubble activeBubble;
    private float cooldownTimer;

    public bool IsSpeaking => activeBubble != null;
    public bool CanSpeak => cooldownTimer <= 0f && activeBubble == null && bubblePrefab != null;

    private void Awake()
    {
        if (bubbleAnchor == null)
            bubbleAnchor = transform;
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (activeBubble == null)
            return;

        // Unity destroyed object safety
        if (activeBubble.Equals(null))
            activeBubble = null;
    }

    public bool Speak(string line, float lifetime = -1f)
    {
        if (!CanSpeak)
            return false;

        if (string.IsNullOrWhiteSpace(line))
            return false;

        GhostDialogueBubble bubble = Instantiate(bubblePrefab);
        bubble.Initialize(
            bubbleAnchor,
            line.Trim(),
            lifetime > 0f ? lifetime : defaultBubbleLifetime,
            autoDestroy: true);

        activeBubble = bubble;
        cooldownTimer = speakCooldown;
        return true;
    }

    public void StopSpeaking()
    {
        if (activeBubble == null)
            return;

        Destroy(activeBubble.gameObject);
        activeBubble = null;
    }
}