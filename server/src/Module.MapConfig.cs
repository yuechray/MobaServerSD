using SpacetimeDB;

public static partial class Module
{
    private const float TICK_MS    = 50f;
    private const float TICK_DELTA = TICK_MS / 1000f;
    private const float ARRIVE_EPS = 0.5f;

    // Terrain spans 0–128 on X and Z. Radiant = bottom-left, Dire = top-right.
    private static readonly DbVec3 RadiantSpawnPos  = V(8f,   0f, 8f);
    private static readonly DbVec3 DireSpawnPos     = V(120f, 0f, 120f);
    private static readonly DbVec3 RadiantThronePos = V(20f,  0f, 20f);
    private static readonly DbVec3 DireThronePos    = V(108f, 0f, 108f);

    // Tower positions — tier 1 closer to enemy, tier 2 closer to base
    // Mid lane
    private static readonly DbVec3 RadiantMidT1 = V(40f,  0f, 40f);
    private static readonly DbVec3 RadiantMidT2 = V(25f,  0f, 25f);
    private static readonly DbVec3 DireMidT1    = V(87f,  0f, 87f);
    private static readonly DbVec3 DireMidT2    = V(101f, 0f, 101f);
    // Top lane (low X, high Z)
    private static readonly DbVec3 RadiantTopT1 = V(16f, 0f, 67f);
    private static readonly DbVec3 RadiantTopT2 = V(16f, 0f, 40f);
    private static readonly DbVec3 DireTopT1    = V(67f, 0f, 111f);
    private static readonly DbVec3 DireTopT2    = V(90f, 0f, 111f);
    // Bot lane (high X, low Z)
    private static readonly DbVec3 RadiantBotT1 = V(67f,  0f, 16f);
    private static readonly DbVec3 RadiantBotT2 = V(40f,  0f, 16f);
    private static readonly DbVec3 DireBotT1    = V(111f, 0f, 67f);
    private static readonly DbVec3 DireBotT2    = V(111f, 0f, 90f);
    // Forward towers — one per lane per team, pushed toward center
    private static readonly DbVec3 RadiantMidT0 = V(55f,  0f, 55f);
    private static readonly DbVec3 DireMidT0    = V(72f,  0f, 72f);
    private static readonly DbVec3 RadiantTopT0 = V(16f,  0f, 85f);
    private static readonly DbVec3 DireTopT0    = V(45f,  0f, 111f);
    private static readonly DbVec3 RadiantBotT0 = V(85f,  0f, 16f);
    private static readonly DbVec3 DireBotT0    = V(111f, 0f, 45f);

    // 12 neutral camps. River at x+z ≈ 115–132 → camps at x+z < 112 or x+z > 134.
    // T3 exclusion: x+z < 58 (Radiant), x+z > 200 (Dire).
    // Symmetric: camps 4–6 mirror 1–3, camps 10–12 mirror 7–9.
    private static readonly (ulong id, DbVec3 pos)[] NeutralCamps = {
        // Left jungle, Radiant side
        (1,  V(24f,  0f, 50f)),   // x+z=74
        (2,  V(26f,  0f, 74f)),   // x+z=100
        (3,  V(30f,  0f, 78f)),   // x+z=108
        // Right jungle — mirror of 1–3
        (4,  V(50f,  0f, 24f)),
        (5,  V(74f,  0f, 26f)),
        (6,  V(78f,  0f, 30f)),
        // Left jungle, Dire side (past river)
        (7,  V(36f,  0f, 100f)),  // x+z=136
        (8,  V(46f,  0f, 102f)),  // x+z=148
        (9,  V(56f,  0f, 96f)),   // x+z=152
        // Right jungle — mirror of 7–9
        (10, V(100f, 0f, 36f)),
        (11, V(102f, 0f, 46f)),
        (12, V(96f,  0f, 56f)),
    };

    // Mid: Radiant → Dire
    private static readonly DbVec3[] MidRadiantWaypoints =
        { V(20f,0f,20f), V(40f,0f,40f), V(64f,0f,64f), V(87f,0f,87f), V(111f,0f,111f) };
    // Mid: Dire → Radiant
    private static readonly DbVec3[] MidDireWaypoints =
        { V(107f,0f,107f), V(87f,0f,87f), V(64f,0f,64f), V(40f,0f,40f), V(16f,0f,16f) };
    // Top: Radiant → Dire
    private static readonly DbVec3[] TopRadiantWaypoints =
        { V(16f,0f,20f), V(16f,0f,64f), V(16f,0f,107f), V(64f,0f,111f), V(107f,0f,111f) };
    // Top: Dire → Radiant
    private static readonly DbVec3[] TopDireWaypoints =
        { V(107f,0f,111f), V(64f,0f,111f), V(16f,0f,107f), V(16f,0f,64f), V(16f,0f,20f) };
    // Bot: Radiant → Dire
    private static readonly DbVec3[] BotRadiantWaypoints =
        { V(20f,0f,16f), V(64f,0f,16f), V(107f,0f,16f), V(111f,0f,64f), V(111f,0f,107f) };
    // Bot: Dire → Radiant
    private static readonly DbVec3[] BotDireWaypoints =
        { V(111f,0f,107f), V(111f,0f,64f), V(107f,0f,16f), V(64f,0f,16f), V(20f,0f,16f) };

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
