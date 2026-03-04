using SpacetimeDB;

public static partial class Module
{
    private static void TickStructure(ReducerContext ctx, Structure s, double now)
    {
        if (s.IsDestroyed || s.Type != StructureType.Tower) return;

        // Check existing target is still valid
        if (s.AttackTargetId != 0)
        {
            if (IsTargetDead(ctx, s.AttackTargetKind, s.AttackTargetId))
            {
                ctx.Db.Structure.Id.Update(s with { AttackTargetId = 0, AttackTargetKind = TargetKind.None });
                s = ctx.Db.Structure.Id.Find(s.Id)!.Value;
            }
            else
            {
                var tPos = GetTargetPosition(ctx, s.AttackTargetKind, s.AttackTargetId);
                if (tPos == null || Dist(s.Position, tPos.Value) > s.AttackRange + 2f)
                {
                    ctx.Db.Structure.Id.Update(s with { AttackTargetId = 0, AttackTargetKind = TargetKind.None });
                    s = ctx.Db.Structure.Id.Find(s.Id)!.Value;
                }
            }
        }

        // Acquire new target: prioritize enemy champions, then wave creeps
        if (s.AttackTargetId == 0)
        {
            var (kind, id) = FindNearestEnemy(ctx, s.Team, s.Position, s.AttackRange);
            if (id != 0)
                ctx.Db.Structure.Id.Update(s with { AttackTargetKind = kind, AttackTargetId = id });
            s = ctx.Db.Structure.Id.Find(s.Id)!.Value;
        }

        // Attack
        if (s.AttackTargetId != 0 && now - s.LastAttackTime >= s.AttackCooldown)
        {
            ctx.Db.Structure.Id.Update(s with { LastAttackTime = now });
            DealDamageToTarget(ctx, s.Team, "Tower", s.AttackTargetKind, s.AttackTargetId, s.AttackDamage, now);
        }
    }

    private static (TargetKind, ulong) FindNearestEnemy(ReducerContext ctx, Team myTeam, DbVec3 pos, float range)
    {
        ulong  bestId   = 0;
        float  bestDist = float.MaxValue;
        TargetKind bestKind = TargetKind.None;

        // Check enemy champions first (higher priority)
        foreach (var c in ctx.Db.Champion.Iter())
        {
            if (c.Team == myTeam || c.State == ChampionState.Dead) continue;
            float d = Dist(pos, c.Position);
            if (d <= range && d < bestDist)
            {
                bestDist = d; bestId = c.Id; bestKind = TargetKind.Champion;
            }
        }
        if (bestId != 0) return (bestKind, bestId);

        // Check enemy wave creeps
        foreach (var cr in ctx.Db.Creep.Iter())
        {
            if (cr.Team == myTeam || cr.Team == Team.None || cr.State == CreepState.Dead) continue;
            float d = Dist(pos, cr.Position);
            if (d <= range && d < bestDist)
            {
                bestDist = d; bestId = cr.Id; bestKind = TargetKind.Creep;
            }
        }

        return (bestKind, bestId);
    }
}
