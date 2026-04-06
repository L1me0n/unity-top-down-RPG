using System.Collections.Generic;
using UnityEngine;

public class ChallengeTipsPresenter : MonoBehaviour
{
    [System.Serializable]
    private class ChallengeSpecificTipSet
    {
        public ChallengeType challengeType;
        [TextArea(3, 8)] public string title;
        [TextArea(6, 16)] public string body;
    }

    [Header("References")]
    [SerializeField] private ChallengeRoomController challengeRoomController;
    [SerializeField] private PagedTipsPanelUI tipsPanelUI;
    [SerializeField] private TipsSeenTracker tipsSeenTracker;

    [Header("Behavior")]
    [SerializeField] private bool autoShowOnInitialize = true;
    [SerializeField] private bool log = false;

    [Header("General Challenge Pages")]
    [SerializeField] private List<TipPageData> generalChallengePages = new List<TipPageData>()
    {
        new TipPageData(
            "CHALLENGE ROOMS",
            "Challenge rooms are risky bargains.\n\n" +
            "Instead of normal combat, they offer a special trial. " +
            "If you succeed, you gain a temporary effect. If you fail, you may suffer a temporary penalty."
        ),
        new TipPageData(
            "TEMPORARY EFFECTS",
            "Challenge effects stay with you until you enter another Challenge room.\n\n" +
            "That means each new challenge can replace your old bargain with a new one."
        )
    };

    [Header("Specific Challenge Pages")]
    [SerializeField] private List<ChallengeSpecificTipSet> specificTipSets = new List<ChallengeSpecificTipSet>()
    {
        new ChallengeSpecificTipSet()
        {
            challengeType = ChallengeType.Betting,
            title = "BETTING",
            body =
                "In Betting, you wager AP on hellhound racers.\n\n" +
                "Choose carefully. A good bet can strengthen your run. " +
                "A bad bet can cripple your action economy."
        },
        new ChallengeSpecificTipSet()
        {
            challengeType = ChallengeType.Gluttony,
            title = "GLUTTONY",
            body =
                "In Gluttony, greed is the trap.\n\n" +
                "You will be tempted to push for more than is safe. " +
                "Too much hunger can turn reward into punishment."
        },
        new ChallengeSpecificTipSet()
        {
            challengeType = ChallengeType.Sloth,
            title = "SLOTH",
            body =
                "In Sloth, timing matters more than aggression.\n\n" +
                "Do not rush. Wait, read, and act at the right moment."
        },
        new ChallengeSpecificTipSet()
        {
            challengeType = ChallengeType.Lie,
            title = "LIE",
            body =
                "Lie is the cruelest challenge.\n\n" +
                "Its rules may shift, deceive, or chain multiple punishments and rewards together. " +
                "Trust nothing blindly."
        }
    };

    private void Awake()
    {
        if (challengeRoomController == null)
            challengeRoomController = GetComponent<ChallengeRoomController>();

        if (tipsPanelUI == null)
            tipsPanelUI = FindFirstObjectByType<PagedTipsPanelUI>();

        if (tipsSeenTracker == null)
            tipsSeenTracker = FindFirstObjectByType<TipsSeenTracker>();
    }

    private void OnEnable()
    {
        if (challengeRoomController != null)
            challengeRoomController.OnChallengeRoomInitialized += HandleChallengeRoomInitialized;
    }

    private void OnDisable()
    {
        if (challengeRoomController != null)
            challengeRoomController.OnChallengeRoomInitialized -= HandleChallengeRoomInitialized;
    }

    private void HandleChallengeRoomInitialized(ChallengeRoomController controller)
    {
        if (!autoShowOnInitialize)
            return;

        if (controller == null)
            return;

        if (!controller.IsChallengeRoom)
            return;

        if (controller.IsChallengeCompleted)
            return;

        ShowTipsForChallenge(controller.ChallengeType);
    }

    public void ShowTipsForChallenge(ChallengeType challengeType)
    {
        if (tipsPanelUI == null)
        {
            if (log)
                Debug.LogWarning("[ChallengeTipsPresenter] No PagedTipsPanelUI found.", this);

            MoveRoomToAwaitingStart();
            return;
        }

        List<TipPageData> pages = BuildPages(challengeType);
        if (pages.Count == 0)
        {
            if (log)
            {
                Debug.Log(
                    $"[ChallengeTipsPresenter] No unseen challenge tips remain for challengeType={challengeType}. " +
                    $"Skipping panel.",
                    this
                );
            }

            MoveRoomToAwaitingStart();
            return;
        }

        MarkShownTipsAsSeen(challengeType);

        if (challengeRoomController != null)
            challengeRoomController.SetChallengeUIOpen(true, "Challenge tips opened");

        if (log)
        {
            Debug.Log(
                $"[ChallengeTipsPresenter] Showing {pages.Count} unseen tip page(s) " +
                $"for challengeType={challengeType}.",
                this
            );
        }

        tipsPanelUI.ShowPages(pages, HandleTipsFinished);
    }

    private List<TipPageData> BuildPages(ChallengeType challengeType)
    {
        List<TipPageData> result = new List<TipPageData>();

        bool showGeneral = ShouldShowGeneralChallengeTips();
        bool showSpecific = ShouldShowSpecificChallengeTips(challengeType);

        if (showGeneral)
        {
            for (int i = 0; i < generalChallengePages.Count; i++)
            {
                TipPageData page = generalChallengePages[i];
                if (page == null)
                    continue;

                result.Add(new TipPageData(page.title, page.body));
            }
        }

        if (showSpecific)
        {
            ChallengeSpecificTipSet matchingSet = FindSpecificSet(challengeType);
            if (matchingSet != null)
            {
                result.Add(new TipPageData(matchingSet.title, matchingSet.body));
            }
            else
            {
                result.Add(new TipPageData(
                    challengeType.ToString().ToUpper(),
                    "A special challenge waits in this room.\n\n" +
                    "Study its rules before you commit."
                ));
            }
        }

        return result;
    }

    private bool ShouldShowGeneralChallengeTips()
    {
        if (tipsSeenTracker == null)
            return true;

        return !tipsSeenTracker.HasSeenGeneralChallengeTips;
    }

    private bool ShouldShowSpecificChallengeTips(ChallengeType challengeType)
    {
        if (tipsSeenTracker == null)
            return true;

        return !tipsSeenTracker.HasSeenChallengeTypeTips(challengeType);
    }

    private void MarkShownTipsAsSeen(ChallengeType challengeType)
    {
        if (tipsSeenTracker == null)
            return;

        if (!tipsSeenTracker.HasSeenGeneralChallengeTips)
            tipsSeenTracker.MarkGeneralChallengeTipsSeen();

        if (!tipsSeenTracker.HasSeenChallengeTypeTips(challengeType))
            tipsSeenTracker.MarkChallengeTypeTipsSeen(challengeType);
    }

    private ChallengeSpecificTipSet FindSpecificSet(ChallengeType challengeType)
    {
        for (int i = 0; i < specificTipSets.Count; i++)
        {
            if (specificTipSets[i] == null)
                continue;

            if (specificTipSets[i].challengeType == challengeType)
                return specificTipSets[i];
        }

        return null;
    }

    private void HandleTipsFinished()
    {
        if (challengeRoomController != null)
            challengeRoomController.SetChallengeUIOpen(false, "Challenge tips finished");

        MoveRoomToAwaitingStart();

        if (log)
            Debug.Log("[ChallengeTipsPresenter] Tips finished -> Challenge room moved to AwaitingStart.", this);
    }

    private void MoveRoomToAwaitingStart()
    {
        if (challengeRoomController == null)
            return;

        if (!challengeRoomController.IsChallengeRoom)
            return;

        if (challengeRoomController.IsChallengeCompleted)
            return;

        challengeRoomController.EnterAwaitingStart();
    }
}