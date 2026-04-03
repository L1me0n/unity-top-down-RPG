using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampfireGhostDialogueDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private CampfireGhostPopulationVisuals campfireGhostVisuals;
    [SerializeField] private GhostDialogueDatabase dialogueDatabase;

    [Header("Timing")]
    [SerializeField] private float minDelayBetweenChecks = 6f;
    [SerializeField] private float maxDelayBetweenChecks = 12f;
    [SerializeField, Range(0f, 1f)] private float idleDialogueChance = 0.5f;
    [SerializeField] private float bubbleLifetime = 2.5f;

    [Header("Conversation")]
    [SerializeField, Range(0f, 1f)] private float questionAnswerChance = 0.35f;
    [SerializeField] private float minAnswerDelay = 1f;
    [SerializeField] private float maxAnswerDelay = 2f;

    private Coroutine loopRoutine;

    private void Awake()
    {
        if (roomManager == null)
            roomManager = FindFirstObjectByType<RoomManager>();

        if (campfireGhostVisuals == null)
            campfireGhostVisuals = FindFirstObjectByType<CampfireGhostPopulationVisuals>();
    }

    private void OnEnable()
    {
        if (roomManager != null)
            roomManager.OnRoomEntered += HandleRoomEntered;

        StartLoopIfNeeded();
    }

    private void OnDisable()
    {
        if (roomManager != null)
            roomManager.OnRoomEntered -= HandleRoomEntered;

        StopLoop();
    }

    private void HandleRoomEntered(RoomInstance _)
    {
        if (IsPlayerInCampfireRoom())
            StartLoopIfNeeded();
        else
            StopLoop();
    }

    private void StartLoopIfNeeded()
    {
        if (!IsPlayerInCampfireRoom())
            return;

        if (loopRoutine != null)
            return;

        loopRoutine = StartCoroutine(DialogueLoop());
    }

    private void StopLoop()
    {
        if (loopRoutine == null)
            return;

        StopCoroutine(loopRoutine);
        loopRoutine = null;
    }

    private IEnumerator DialogueLoop()
    {
        while (true)
        {
            float wait = Random.Range(minDelayBetweenChecks, maxDelayBetweenChecks);
            yield return new WaitForSeconds(wait);

            if (!IsPlayerInCampfireRoom())
            {
                loopRoutine = null;
                yield break;
            }

            TryPlayCampfireDialogue();
        }
    }

    private bool IsPlayerInCampfireRoom()
    {
        if (roomManager == null)
            return false;

        Vector2Int coord = roomManager.CurrentCoord;

        if (!roomManager.TryGetRoomState(coord, out RoomState state))
            return false;

        if (state == null)
            return false;

        return state.roomType == RoomType.Campfire;
    }

    private void TryPlayCampfireDialogue()
    {
        if (dialogueDatabase == null)
            return;

        if (campfireGhostVisuals == null)
            return;

        if (Random.value > idleDialogueChance)
            return;

        IReadOnlyList<HostageGhostVisual> ghosts = campfireGhostVisuals.ActiveGhosts;
        if (ghosts == null || ghosts.Count == 0)
            return;

        List<HostageGhostVisual> validGhosts = GetValidSpeakingGhosts(ghosts);

        if (validGhosts.Count == 0)
            return;

        bool shouldTryExchange =
            validGhosts.Count >= 2 &&
            Random.value <= questionAnswerChance;

        if (shouldTryExchange)
        {
            bool startedExchange = TryStartQuestionAnswerExchange(validGhosts);
            if (startedExchange)
                return;
        }

        TryPlaySingleIdleLine(validGhosts);
    }

    private List<HostageGhostVisual> GetValidSpeakingGhosts(IReadOnlyList<HostageGhostVisual> ghosts)
    {
        List<HostageGhostVisual> validGhosts = new List<HostageGhostVisual>();

        for (int i = 0; i < ghosts.Count; i++)
        {
            HostageGhostVisual ghost = ghosts[i];
            if (ghost == null)
                continue;

            if (ghost.DialogueSpeaker == null)
                continue;

            if (!ghost.DialogueSpeaker.CanSpeak)
                continue;

            validGhosts.Add(ghost);
        }

        return validGhosts;
    }

    private void TryPlaySingleIdleLine(List<HostageGhostVisual> validGhosts)
    {
        if (validGhosts == null || validGhosts.Count == 0)
            return;

        string line = dialogueDatabase.GetRandomCampfireIdleLine();
        if (string.IsNullOrWhiteSpace(line))
            return;

        int pick = Random.Range(0, validGhosts.Count);
        HostageGhostVisual chosenGhost = validGhosts[pick];

        bool spoke = chosenGhost.DialogueSpeaker.Speak(line, bubbleLifetime);

        if (spoke)
        {
            Debug.Log(
                $"[CampfireGhostDialogueDirector] Campfire idle dialogue triggered in room {roomManager.CurrentCoord}: \"{line}\""
            );
        }
    }

    private bool TryStartQuestionAnswerExchange(List<HostageGhostVisual> validGhosts)
    {
        if (validGhosts == null || validGhosts.Count < 2)
            return false;

        string questionLine = dialogueDatabase.GetRandomCampfireQuestionLine();
        if (string.IsNullOrWhiteSpace(questionLine))
            return false;

        string answerLine = dialogueDatabase.GetRandomCampfireAnswerLine();
        if (string.IsNullOrWhiteSpace(answerLine))
            return false;

        int askerIndex = Random.Range(0, validGhosts.Count);
        HostageGhostVisual asker = validGhosts[askerIndex];

        List<HostageGhostVisual> possibleResponders = new List<HostageGhostVisual>();
        for (int i = 0; i < validGhosts.Count; i++)
        {
            if (i == askerIndex)
                continue;

            possibleResponders.Add(validGhosts[i]);
        }

        if (possibleResponders.Count == 0)
            return false;

        int responderPick = Random.Range(0, possibleResponders.Count);
        HostageGhostVisual responder = possibleResponders[responderPick];

        bool askerSpoke = asker.DialogueSpeaker.Speak(questionLine, bubbleLifetime);
        if (!askerSpoke)
            return false;

        StartCoroutine(PlayAnswerAfterDelay(responder, answerLine));

        Debug.Log(
            $"[CampfireGhostDialogueDirector] Campfire exchange triggered in room {roomManager.CurrentCoord}: " +
            $"Q=\"{questionLine}\" | A=\"{answerLine}\""
        );

        return true;
    }

    private IEnumerator PlayAnswerAfterDelay(HostageGhostVisual responder, string answerLine)
    {
        if (responder == null)
            yield break;

        float delay = Random.Range(minAnswerDelay, maxAnswerDelay);
        yield return new WaitForSeconds(delay);

        if (!IsPlayerInCampfireRoom())
            yield break;

        if (responder == null)
            yield break;

        if (responder.DialogueSpeaker == null)
            yield break;

        if (!responder.DialogueSpeaker.CanSpeak)
            yield break;

        if (string.IsNullOrWhiteSpace(answerLine))
            yield break;

        responder.DialogueSpeaker.Speak(answerLine, bubbleLifetime);
    }
}