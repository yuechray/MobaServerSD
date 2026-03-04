using SpacetimeDB;
using System;

public static partial class Module
{
    [SpacetimeDB.Reducer(ReducerKind.Init)]
    public static void Init(ReducerContext ctx)
    {
        ctx.Db.Config.Insert(new Config
        {
            Id = 0,
            // Champion
            MoveSpeed        = 8.0f,
            AttackRange      = 3.0f,
            AttackDamage     = 80.0f,
            AttackCooldown   = 1.0f,
            RespawnDuration  = 12.0f,
            ChampMaxHealth   = 600.0f,
            ChampMaxMana     = 300.0f,
            RadiantSpawn     = RadiantSpawnPos,
            DireSpawn        = DireSpawnPos,
            // Wave creep
            CreepMoveSpeed       = 4.5f,
            CreepAttackRange     = 2.0f,
            CreepAttackDamage    = 30.0f,
            CreepAttackCooldown  = 1.2f,
            CreepMaxHealth       = 180.0f,
            CreepWaveIntervalSec = 30.0f,
            // Neutral creep
            NeutralMoveSpeed     = 3.5f,
            NeutralAttackRange   = 2.5f,
            NeutralAttackDamage  = 50.0f,
            NeutralAttackCooldown= 1.5f,
            NeutralMaxHealth     = 300.0f,
            NeutralRespawnSec    = 60.0f,
            // Tower
            TowerMaxHealth    = 1200.0f,
            TowerAttackDamage = 120.0f,
            TowerAttackRange  = 12.0f,
            TowerAttackCooldown = 1.0f,
            // Throne
            ThroneMaxHealth = 3000.0f,
        });

        ctx.Db.GameState.Insert(new GameState
        {
            Id = 0, Phase = GamePhase.Lobby, WinnerTeam = Team.None, Countdown = 0
        });

        ctx.Db.GameTickSchedule.Insert(new GameTickSchedule
        {
            ScheduledId = 0,
            ScheduledAt = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(TICK_MS))
        });

        ctx.Db.CreepSpawnSchedule.Insert(new CreepSpawnSchedule
        {
            ScheduledId = 0,
            ScheduledAt = new ScheduleAt.Interval(TimeSpan.FromSeconds(30))
        });

        Log.Info("[MOBA] Server initialised");
    }

    [SpacetimeDB.Reducer(ReducerKind.ClientConnected)]
    public static void ClientConnected(ReducerContext ctx)
    {
        if (ctx.Db.LobbyPlayer.PlayerId.Find(ctx.Sender) is { } lp)
        {
            ctx.Db.LobbyPlayer.PlayerId.Update(lp with { IsConnected = true });
            Log.Info($"[MOBA] Reconnected: {lp.Name}");
        }
        else
        {
            Log.Info($"[MOBA] New connection: {ctx.Sender}");
        }
    }

    [SpacetimeDB.Reducer(ReducerKind.ClientDisconnected)]
    public static void ClientDisconnected(ReducerContext ctx)
    {
        if (ctx.Db.LobbyPlayer.PlayerId.Find(ctx.Sender) is not { } lp) return;

        var state = ctx.Db.GameState.Id.Find(0);
        if (state is { Phase: GamePhase.Lobby } || state is { Phase: GamePhase.Starting })
        {
            ctx.Db.LobbyPlayer.PlayerId.Delete(ctx.Sender);
            Log.Info($"[MOBA] {lp.Name} left lobby");
        }
        else
        {
            ctx.Db.LobbyPlayer.PlayerId.Update(lp with { IsConnected = false, IsReady = false });
            Log.Info($"[MOBA] {lp.Name} disconnected during game");
        }
    }
}
