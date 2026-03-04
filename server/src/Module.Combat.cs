using SpacetimeDB;

public static partial class Module
{
    private static void DealDamageToTarget(ReducerContext ctx, Team attackerTeam, string attackerName,
        TargetKind kind, ulong targetId, float dmg, double now)
    {
        switch (kind)
        {
            case TargetKind.Champion:
                if (ctx.Db.Champion.Id.Find(targetId) is { } champ)
                    ApplyDamageToChampion(ctx, attackerName, champ, dmg, now);
                break;
            case TargetKind.Creep:
                if (ctx.Db.Creep.Id.Find(targetId) is { } creep)
                    ApplyDamageToCreep(ctx, creep, dmg, now);
                break;
            case TargetKind.Structure:
                if (ctx.Db.Structure.Id.Find(targetId) is { } structure)
                    ApplyDamageToStructure(ctx, attackerTeam, structure, dmg, now);
                break;
        }
    }

    private static void ApplyDamageToChampion(ReducerContext ctx, string attackerName, Champion dst, float dmg, double now)
    {
        float newHp = dst.Health - dmg;
        if (newHp <= 0f)
        {
            ctx.Db.Champion.Id.Update(dst with
            {
                Health = 0f, State = ChampionState.Dead,
                DeathTime = now, AttackTargetKind = TargetKind.None, AttackTargetId = 0,
            });
            Log.Info($"[MOBA] {dst.Name} killed by {attackerName}");
        }
        else
        {
            ctx.Db.Champion.Id.Update(dst with { Health = newHp });
        }
    }

    private static void ApplyDamageToCreep(ReducerContext ctx, Creep dst, float dmg, double now)
    {
        float newHp = dst.Health - dmg;
        if (newHp <= 0f)
        {
            ctx.Db.Creep.Id.Update(dst with
            {
                Health = 0f, State = CreepState.Dead,
                DeathTime = now, AttackTargetKind = TargetKind.None, AttackTargetId = 0,
            });
        }
        else
        {
            ctx.Db.Creep.Id.Update(dst with { Health = newHp });
        }
    }

    private static void ApplyDamageToStructure(ReducerContext ctx, Team attackerTeam, Structure dst, float dmg, double now)
    {
        if (dst.IsDestroyed) return;
        float newHp = dst.Health - dmg;
        if (newHp <= 0f)
        {
            ctx.Db.Structure.Id.Update(dst with { Health = 0f, IsDestroyed = true });
            Log.Info($"[MOBA] {dst.Team} {dst.Type} at lane {dst.Lane} destroyed by {attackerTeam}");

            if (dst.Type == StructureType.Throne)
            {
                var state = ctx.Db.GameState.Id.Find(0)!.Value;
                ctx.Db.GameState.Id.Update(state with { Phase = GamePhase.GameOver, WinnerTeam = attackerTeam });
                Log.Info($"[MOBA] GAME OVER — {attackerTeam} wins!");
            }
        }
        else
        {
            ctx.Db.Structure.Id.Update(dst with { Health = newHp });
        }
    }
}
