using SpacetimeDB;

public static partial class Module
{
    private static void TickChampion(ReducerContext ctx, Champion c, double now)
    {
        if (c.State == ChampionState.Dead)
        {
            if (now - c.DeathTime >= c.RespawnDuration)
                RespawnChampion(ctx, c);
            return;
        }

        if (c.State == ChampionState.Attacking && c.AttackTargetKind != TargetKind.None)
        {
            var targetPos = GetTargetPosition(ctx, c.AttackTargetKind, c.AttackTargetId);
            if (targetPos == null || IsTargetDead(ctx, c.AttackTargetKind, c.AttackTargetId))
            {
                ctx.Db.Champion.Id.Update(c with { State = ChampionState.Idle, AttackTargetKind = TargetKind.None, AttackTargetId = 0 });
                return;
            }

            if (Dist(c.Position, targetPos.Value) > c.AttackRange)
            {
                ctx.Db.Champion.Id.Update(MoveChampStep(c with { Destination = targetPos.Value }));
                return;
            }

            if (now - c.LastAttackTime >= c.AttackCooldown)
            {
                ctx.Db.Champion.Id.Update(c with { LastAttackTime = now });
                DealDamageToTarget(ctx, c.Team, c.Name, c.AttackTargetKind, c.AttackTargetId, c.AttackDamage, now);
            }
            return;
        }

        if (c.State == ChampionState.Moving)
        {
            if (Dist(c.Position, c.Destination) <= ARRIVE_EPS)
                ctx.Db.Champion.Id.Update(c with { Position = c.Destination, State = ChampionState.Idle });
            else
                ctx.Db.Champion.Id.Update(MoveChampStep(c));
        }
    }

    private static void RespawnChampion(ReducerContext ctx, Champion c)
    {
        var cfg      = ctx.Db.Config.Id.Find(0)!.Value;
        var spawnPos = c.Team == Team.Radiant ? cfg.RadiantSpawn : cfg.DireSpawn;

        ctx.Db.Champion.Id.Update(c with
        {
            Position = spawnPos, Destination = spawnPos,
            Health = c.MaxHealth, Mana = c.MaxMana,
            State = ChampionState.Idle, DeathTime = 0,
            AttackTargetKind = TargetKind.None, AttackTargetId = 0,
        });
        Log.Info($"[MOBA] {c.Name} respawned");
    }

    private static Champion MoveChampStep(Champion c)
    {
        var (newPos, arrived) = StepTowards(c.Position, c.Destination, c.MoveSpeed);
        return c with { Position = newPos, State = arrived ? ChampionState.Idle : ChampionState.Moving };
    }
}
