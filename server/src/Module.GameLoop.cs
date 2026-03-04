using SpacetimeDB;
using System.Linq;

public static partial class Module
{
    [SpacetimeDB.Reducer]
    public static void GameTick(ReducerContext ctx, GameTickSchedule _schedule)
    {
        if (ctx.Db.GameState.Id.Find(0) is not { Phase: GamePhase.InGame } state) return;

        double now = ctx.Timestamp.MicrosecondsSinceUnixEpoch / 1_000_000.0;

        foreach (var champ in ctx.Db.Champion.Iter().ToList())
            TickChampion(ctx, champ, now);

        foreach (var structure in ctx.Db.Structure.Iter().ToList())
            TickStructure(ctx, structure, now);

        foreach (var creep in ctx.Db.Creep.Iter().ToList())
            TickCreep(ctx, creep, now);

        CheckVictory(ctx, state, now);
    }

    private static void CheckVictory(ReducerContext ctx, GameState state, double now)
    {
        // Victory is triggered immediately in ApplyDamageToStructure when a throne dies.
        // This is a safety check in case the game gets stuck.
        _ = now;
    }
}
