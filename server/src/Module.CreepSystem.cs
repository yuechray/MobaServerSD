using SpacetimeDB;
using System.Linq;

public static partial class Module
{
    private static void TickCreep(ReducerContext ctx, Creep c, double now)
    {
        if (c.State == CreepState.Dead)
        {
            HandleDeadCreep(ctx, c, now);
            return;
        }

        // Check existing attack target
        if (c.State == CreepState.Attacking && c.AttackTargetId != 0)
        {
            if (IsTargetDead(ctx, c.AttackTargetKind, c.AttackTargetId))
            {
                ctx.Db.Creep.Id.Update(c with { State = CreepState.Moving, AttackTargetKind = TargetKind.None, AttackTargetId = 0 });
                return;
            }

            var tPos = GetTargetPosition(ctx, c.AttackTargetKind, c.AttackTargetId);
            if (tPos == null)
            {
                ctx.Db.Creep.Id.Update(c with { AttackTargetKind = TargetKind.None, AttackTargetId = 0, State = CreepState.Moving });
                return;
            }

            if (Dist(c.Position, tPos.Value) > c.AttackRange)
            {
                ctx.Db.Creep.Id.Update(MoveCreepStep(c with { Destination = tPos.Value }));
                return;
            }

            if (now - c.LastAttackTime >= c.AttackCooldown)
            {
                ctx.Db.Creep.Id.Update(c with { LastAttackTime = now });
                DealDamageToTarget(ctx, c.Team, "Creep", c.AttackTargetKind, c.AttackTargetId, c.AttackDamage, now);
            }
            return;
        }

        // Check if an enemy is nearby (aggro range = attackRange * 2.5)
        var (aggroKind, aggroId) = FindNearestEnemy(ctx, c.Team, c.Position, c.AttackRange * 2.5f);
        if (aggroId != 0)
        {
            ctx.Db.Creep.Id.Update(c with
            {
                State            = CreepState.Attacking,
                AttackTargetKind = aggroKind,
                AttackTargetId   = aggroId,
                Destination      = c.Position,
            });
            return;
        }

        // Wave creep: advance along lane waypoints
        if (c.Type == CreepType.Wave)
        {
            AdvanceWaveCreep(ctx, c);
            return;
        }

        // Neutral: stay idle at camp
        if (c.State != CreepState.Idle)
            ctx.Db.Creep.Id.Update(c with { State = CreepState.Idle });
    }

    private static void AdvanceWaveCreep(ReducerContext ctx, Creep c)
    {
        var waypoints = GetLaneWaypoints(c.Team, c.Lane);
        if (waypoints.Length == 0) return;

        if (Dist(c.Position, c.Destination) <= ARRIVE_EPS)
        {
            int nextIdx = c.WaypointIndex + 1;
            if (nextIdx >= waypoints.Length)
            {
                ctx.Db.Creep.Id.Update(c with { State = CreepState.Idle });
                return;
            }

            ctx.Db.Creep.Id.Update(c with
            {
                Position      = c.Destination,
                WaypointIndex = nextIdx,
                Destination   = waypoints[nextIdx],
                State         = CreepState.Moving,
            });
        }
        else
        {
            ctx.Db.Creep.Id.Update(MoveCreepStep(c));
        }
    }

    private static void HandleDeadCreep(ReducerContext ctx, Creep c, double now)
    {
        if (c.Type == CreepType.Wave)
        {
            if (now - c.DeathTime >= 5.0)
                ctx.Db.Creep.Id.Delete(c.Id);
            return;
        }

        // Neutral: respawn after delay if no other creep of same camp is alive
        var cfg = ctx.Db.Config.Id.Find(0)!.Value;
        if (now - c.DeathTime < cfg.NeutralRespawnSec) return;

        bool campAlive = ctx.Db.Creep.Iter()
            .Any(cr => cr.CampId == c.CampId && cr.Id != c.Id && cr.State != CreepState.Dead);

        if (!campAlive)
        {
            var campPos = NeutralCamps.First(nc => nc.id == c.CampId).pos;
            ctx.Db.Creep.Id.Update(c with
            {
                Position        = campPos,
                Destination     = campPos,
                Health          = c.MaxHealth,
                State           = CreepState.Idle,
                AttackTargetKind = TargetKind.None,
                AttackTargetId  = 0,
                DeathTime       = 0,
            });
        }
    }

    private static Creep MoveCreepStep(Creep c)
    {
        var (newPos, _) = StepTowards(c.Position, c.Destination, c.MoveSpeed);
        return c with { Position = newPos, State = CreepState.Moving };
    }
}
