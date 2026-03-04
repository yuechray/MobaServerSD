using SpacetimeDB;
using System;
using System.Linq;

public static partial class Module
{
    [SpacetimeDB.Reducer]
    public static void MoveToPosition(ReducerContext ctx, float x, float y, float z)
    {
        var champ = RequireChampion(ctx);
        if (champ.State == ChampionState.Dead) return;

        ctx.Db.Champion.Id.Update(champ with
        {
            Destination      = V(x, y, z),
            State            = ChampionState.Moving,
            AttackTargetKind = TargetKind.None,
            AttackTargetId   = 0,
        });
    }

    /// <summary>Order champion to attack any entity type (champion / creep / structure).</summary>
    [SpacetimeDB.Reducer]
    public static void IssueAttack(ReducerContext ctx, TargetKind targetKind, ulong targetId)
    {
        var attacker = RequireChampion(ctx);
        if (attacker.State == ChampionState.Dead) return;

        switch (targetKind)
        {
            case TargetKind.Champion:
                var target = ctx.Db.Champion.Id.Find(targetId) ?? throw new Exception("Champion target not found");
                if (target.Team == attacker.Team) throw new Exception("Cannot attack ally");
                if (target.State == ChampionState.Dead) return;
                break;
            case TargetKind.Creep:
                var creep = ctx.Db.Creep.Id.Find(targetId) ?? throw new Exception("Creep target not found");
                if (creep.Team == attacker.Team) throw new Exception("Cannot attack ally creep");
                if (creep.State == CreepState.Dead) return;
                break;
            case TargetKind.Structure:
                var structure = ctx.Db.Structure.Id.Find(targetId) ?? throw new Exception("Structure target not found");
                if (structure.Team == attacker.Team) throw new Exception("Cannot attack ally structure");
                if (structure.IsDestroyed) return;
                break;
            default:
                throw new Exception("Invalid target kind");
        }

        ctx.Db.Champion.Id.Update(attacker with
        {
            State            = ChampionState.Attacking,
            AttackTargetKind = targetKind,
            AttackTargetId   = targetId,
            Destination      = attacker.Position,
        });
    }

    [SpacetimeDB.Reducer]
    public static void ResetMatch(ReducerContext ctx)
    {
        var state = ctx.Db.GameState.Id.Find(0) ?? throw new Exception("GameState missing");
        ctx.Db.GameState.Id.Update(state with { Phase = GamePhase.Lobby, WinnerTeam = Team.None, Countdown = 0 });

        foreach (var c  in ctx.Db.Champion.Iter().ToList())    ctx.Db.Champion.Id.Delete(c.Id);
        foreach (var s  in ctx.Db.Structure.Iter().ToList())   ctx.Db.Structure.Id.Delete(s.Id);
        foreach (var cr in ctx.Db.Creep.Iter().ToList())       ctx.Db.Creep.Id.Delete(cr.Id);
        foreach (var lp in ctx.Db.LobbyPlayer.Iter().ToList()) ctx.Db.LobbyPlayer.PlayerId.Delete(lp.PlayerId);

        Log.Info("[MOBA] Match reset — back to Lobby");
    }
}
