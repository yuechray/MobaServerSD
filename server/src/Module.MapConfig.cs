using SpacetimeDB;

public static partial class Module
{
    private const float TICK_MS    = 50f;
    private const float TICK_DELTA = TICK_MS / 1000f;
    private const float ARRIVE_EPS = 0.5f;

    // Terrain spans 0–512 on X and Z. Radiant = bottom-left, Dire = top-right.
    private static readonly DbVec3 RadiantSpawnPos  = V(65f,  0f, 65f);
    private static readonly DbVec3 DireSpawnPos     = V(445f, 0f, 445f);
    private static readonly DbVec3 RadiantThronePos = V(40f,  0f, 40f);
    private static readonly DbVec3 DireThronePos    = V(460f, 0f, 460f);

    // Tower positions — tier 1 closer to enemy, tier 2 closer to base
    // Mid lane
    private static readonly DbVec3 RadiantMidT1 = V(160f, 0f, 160f);
    private static readonly DbVec3 RadiantMidT2 = V(100f, 0f, 100f);
    private static readonly DbVec3 DireMidT1    = V(350f, 0f, 350f);
    private static readonly DbVec3 DireMidT2    = V(405f, 0f, 405f);
    // Top lane (low X, high Z)
    private static readonly DbVec3 RadiantTopT1 = V(65f,  0f, 270f);
    private static readonly DbVec3 RadiantTopT2 = V(65f,  0f, 160f);
    private static readonly DbVec3 DireTopT1    = V(270f, 0f, 445f);
    private static readonly DbVec3 DireTopT2    = V(160f, 0f, 445f);
    // Bot lane (high X, low Z)
    private static readonly DbVec3 RadiantBotT1 = V(270f, 0f, 65f);
    private static readonly DbVec3 RadiantBotT2 = V(160f, 0f, 65f);
    private static readonly DbVec3 DireBotT1    = V(445f, 0f, 270f);
    private static readonly DbVec3 DireBotT2    = V(445f, 0f, 160f);

    private static readonly (ulong id, DbVec3 pos)[] NeutralCamps = {
        (1, V(145f, 0f, 340f)),  // top jungle
        (2, V(340f, 0f, 145f)),  // bot jungle
        (3, V(110f, 0f, 420f)),  // top jungle deep
        (4, V(420f, 0f, 110f)),  // bot jungle deep
    };

    // Mid: Radiant → Dire
    private static readonly DbVec3[] MidRadiantWaypoints =
        { V(80f,0f,80f), V(160f,0f,160f), V(256f,0f,256f), V(350f,0f,350f), V(445f,0f,445f) };
    // Mid: Dire → Radiant
    private static readonly DbVec3[] MidDireWaypoints =
        { V(430f,0f,430f), V(350f,0f,350f), V(256f,0f,256f), V(160f,0f,160f), V(65f,0f,65f) };
    // Top: Radiant → Dire
    private static readonly DbVec3[] TopRadiantWaypoints =
        { V(65f,0f,80f), V(65f,0f,256f), V(65f,0f,430f), V(256f,0f,445f), V(430f,0f,445f) };
    // Top: Dire → Radiant
    private static readonly DbVec3[] TopDireWaypoints =
        { V(445f,0f,430f), V(256f,0f,445f), V(65f,0f,430f), V(65f,0f,256f), V(65f,0f,80f) };
    // Bot: Radiant → Dire
    private static readonly DbVec3[] BotRadiantWaypoints =
        { V(80f,0f,65f), V(256f,0f,65f), V(430f,0f,65f), V(445f,0f,256f), V(445f,0f,430f) };
    // Bot: Dire → Radiant
    private static readonly DbVec3[] BotDireWaypoints =
        { V(430f,0f,80f), V(445f,0f,256f), V(430f,0f,65f), V(256f,0f,65f), V(80f,0f,65f) };

    private static DbVec3[] GetLaneWaypoints(Team team, Lane lane)
    {
        return (team, lane) switch
        {
            (Team.Radiant, Lane.Mid)    => MidRadiantWaypoints,
            (Team.Dire,    Lane.Mid)    => MidDireWaypoints,
            (Team.Radiant, Lane.Top)    => TopRadiantWaypoints,
            (Team.Dire,    Lane.Top)    => TopDireWaypoints,
            (Team.Radiant, Lane.Bottom) => BotRadiantWaypoints,
            (Team.Dire,    Lane.Bottom) => BotDireWaypoints,
            _                           => Array.Empty<DbVec3>()
        };
    }
}
