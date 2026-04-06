using UnityEngine;

public class ChallengeRoomContext
{
    public RoomManager RoomManager { get; private set; }
    public ChallengeEffectManager ChallengeEffectManager { get; private set; }
    public ChallengeRoomController RoomController { get; private set; }
    public RoomState RoomState { get; private set; }
    public Vector2Int Coord { get; private set; }

    public ChallengeType ChallengeType
    {
        get
        {
            if (RoomState == null)
                return ChallengeType.None;

            return RoomState.challengeType;
        }
    }

    public ChallengeRoomContext(
        RoomManager roomManager,
        ChallengeEffectManager challengeEffectManager,
        ChallengeRoomController roomController,
        RoomState roomState,
        Vector2Int coord)
    {
        RoomManager = roomManager;
        ChallengeEffectManager = challengeEffectManager;
        RoomController = roomController;
        RoomState = roomState;
        Coord = coord;
    }
}