using SpacetimeDB;
using System;

public static partial class Module
{
    private static DbVec3? GetTargetPosition(ReducerContext ctx, TargetKind kind, ulong id)
    {
        return kind switch
        {
            TargetKind.Champion  => ctx.Db.Champion.Id.Find(id)  is { } c  ? c.Position  : (DbVec3?)null,
            TargetKind.Creep     => ctx.Db.Creep.Id.Find(id)     is { } cr ? cr.Position : (DbVec3?)null,
            TargetKind.Structure => ctx.Db.Structure.Id.Find(id) is { } s  ? s.Position  : (DbVec3?)null,
            _                    => null
        };
    }

    private static bool IsTargetDead(ReducerContext ctx, TargetKind kind, ulong id)
    {
        return kind switch
        {
            TargetKind.Champion  => ctx.Db.Champion.Id.Find(id)  is not { } c  || c.State == ChampionState.Dead,
            TargetKind.Creep     => ctx.Db.Creep.Id.Find(id)     is not { } cr || cr.State == CreepState.Dead,
            TargetKind.Structure => ctx.Db.Structure.Id.Find(id) is not { } s  || s.IsDestroyed,
            _                    => true
        };
    }

    private static LobbyPlayer RequireLobbyPlayer(ReducerContext ctx) =>
        ctx.Db.LobbyPlayer.PlayerId.Find(ctx.Sender) is { } lp
            ? lp
            : throw new Exception("Call JoinLobby first");

    private static Champion RequireChampion(ReducerContext ctx)
    {
        foreach (var c in ctx.Db.Champion.Iter())
            if (c.OwnerIdentity == ctx.Sender) return c;
        throw new Exception("No champion — game not started");
    }

    private static float Dist(DbVec3 a, DbVec3 b)
    {
        float dx = a.X - b.X, dz = a.Z - b.Z;
        return MathF.Sqrt(dx * dx + dz * dz);
    }

    private static DbVec3 V(float x, float y, float z) => new DbVec3 { X = x, Y = y, Z = z };
    private static DbVec3 Add(DbVec3 a, DbVec3 b) => V(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    // Moves pos one tick-step towards dest. Returns new position and whether destination was reached.
    private static (DbVec3 newPos, bool arrived) StepTowards(DbVec3 pos, DbVec3 dest, float speed)
    {
        float dx = dest.X - pos.X;
        float dz = dest.Z - pos.Z;
        float dist = MathF.Sqrt(dx * dx + dz * dz);

        if (dist < 0.001f) return (dest, true);

        float step = speed * TICK_DELTA;
        if (step >= dist) return (dest, true);

        return (V(pos.X + (dx / dist) * step, pos.Y, pos.Z + (dz / dist) * step), false);
    }
}
