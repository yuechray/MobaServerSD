using SpacetimeDB;
using System.Collections.Generic;

public static partial class Module
{
    private static void SpawnAllChampions(ReducerContext ctx, List<LobbyPlayer> players, Config cfg)
    {
        foreach (var player in players)
        {
            var spawnPos = player.Team == Team.Radiant ? cfg.RadiantSpawn : cfg.DireSpawn;

            ctx.Db.Champion.Insert(new Champion
            {
                Id              = 0,
                OwnerIdentity   = player.PlayerId,
                Team            = player.Team,
                Name            = player.Name,
                Position        = spawnPos,
                Destination     = spawnPos,
                MoveSpeed       = cfg.MoveSpeed,
                Health          = cfg.ChampMaxHealth,
                MaxHealth       = cfg.ChampMaxHealth,
                Mana            = cfg.ChampMaxMana,
                MaxMana         = cfg.ChampMaxMana,
                AttackRange     = cfg.AttackRange,
                AttackDamage    = cfg.AttackDamage,
                AttackCooldown  = cfg.AttackCooldown,
                RespawnDuration = cfg.RespawnDuration,
                State           = ChampionState.Idle,
                AttackTargetKind = TargetKind.None,
                AttackTargetId  = 0,
                LastAttackTime  = 0,
                DeathTime       = 0,
            });
        }
    }

    private static void SpawnAllStructures(ReducerContext ctx, Config cfg)
    {
        // Thrones
        SpawnStructure(ctx, Team.Radiant, StructureType.Throne, Lane.None, RadiantThronePos, cfg);
        SpawnStructure(ctx, Team.Dire,    StructureType.Throne, Lane.None, DireThronePos,    cfg);

        // Mid towers
        SpawnStructure(ctx, Team.Radiant, StructureType.Tower, Lane.Mid, RadiantMidT1, cfg);
        SpawnStructure(ctx, Team.Radiant, StructureType.Tower, Lane.Mid, RadiantMidT2, cfg);
        SpawnStructure(ctx, Team.Dire,    StructureType.Tower, Lane.Mid, DireMidT1,    cfg);
        SpawnStructure(ctx, Team.Dire,    StructureType.Tower, Lane.Mid, DireMidT2,    cfg);

        // Top towers
        SpawnStructure(ctx, Team.Radiant, StructureType.Tower, Lane.Top, RadiantTopT1, cfg);
        SpawnStructure(ctx, Team.Radiant, StructureType.Tower, Lane.Top, RadiantTopT2, cfg);
        SpawnStructure(ctx, Team.Dire,    StructureType.Tower, Lane.Top, DireTopT1,    cfg);
        SpawnStructure(ctx, Team.Dire,    StructureType.Tower, Lane.Top, DireTopT2,    cfg);

        // Bot towers
        SpawnStructure(ctx, Team.Radiant, StructureType.Tower, Lane.Bottom, RadiantBotT1, cfg);
        SpawnStructure(ctx, Team.Radiant, StructureType.Tower, Lane.Bottom, RadiantBotT2, cfg);
        SpawnStructure(ctx, Team.Dire,    StructureType.Tower, Lane.Bottom, DireBotT1,    cfg);
        SpawnStructure(ctx, Team.Dire,    StructureType.Tower, Lane.Bottom, DireBotT2,    cfg);

        // Forward towers (T0) — pushed toward center
        SpawnStructure(ctx, Team.Radiant, StructureType.Tower, Lane.Mid,    RadiantMidT0, cfg);
        SpawnStructure(ctx, Team.Dire,    StructureType.Tower, Lane.Mid,    DireMidT0,    cfg);
        SpawnStructure(ctx, Team.Radiant, StructureType.Tower, Lane.Top,    RadiantTopT0, cfg);
        SpawnStructure(ctx, Team.Dire,    StructureType.Tower, Lane.Top,    DireTopT0,    cfg);
        SpawnStructure(ctx, Team.Radiant, StructureType.Tower, Lane.Bottom, RadiantBotT0, cfg);
        SpawnStructure(ctx, Team.Dire,    StructureType.Tower, Lane.Bottom, DireBotT0,    cfg);
    }

    private static void SpawnStructure(ReducerContext ctx, Team team, StructureType type, Lane lane, DbVec3 pos, Config cfg)
    {
        float hp  = type == StructureType.Throne ? cfg.ThroneMaxHealth  : cfg.TowerMaxHealth;
        float dmg = type == StructureType.Throne ? 0f                   : cfg.TowerAttackDamage;
        float rng = type == StructureType.Throne ? 0f                   : cfg.TowerAttackRange;
        float cd  = type == StructureType.Throne ? 0f                   : cfg.TowerAttackCooldown;

        ctx.Db.Structure.Insert(new Structure
        {
            Id              = 0,
            Team            = team,
            Type            = type,
            Lane            = lane,
            Position        = pos,
            Health          = hp,
            MaxHealth       = hp,
            AttackDamage    = dmg,
            AttackRange     = rng,
            AttackCooldown  = cd,
            LastAttackTime  = 0,
            AttackTargetId  = 0,
            AttackTargetKind = TargetKind.None,
            IsDestroyed     = false,
        });
    }

    private static void SpawnNeutralCreeps(ReducerContext ctx, Config cfg)
    {
        foreach (var (campId, campPos) in NeutralCamps)
        {
            for (int i = 0; i < 2; i++)
            {
                var offset = V(i * 2.0f, 0f, 0f);
                var pos    = Add(campPos, offset);

                ctx.Db.Creep.Insert(new Creep
                {
                    Id              = 0,
                    Team            = Team.None,
                    Type            = CreepType.Neutral,
                    Lane            = Lane.None,
                    Position        = pos,
                    Destination     = pos,
                    MoveSpeed       = cfg.NeutralMoveSpeed,
                    Health          = cfg.NeutralMaxHealth,
                    MaxHealth       = cfg.NeutralMaxHealth,
                    AttackRange     = cfg.NeutralAttackRange,
                    AttackDamage    = cfg.NeutralAttackDamage,
                    AttackCooldown  = cfg.NeutralAttackCooldown,
                    State           = CreepState.Idle,
                    AttackTargetKind = TargetKind.None,
                    AttackTargetId  = 0,
                    LastAttackTime  = 0,
                    DeathTime       = 0,
                    CampId          = campId,
                    WaypointIndex   = 0,
                });
            }
        }
    }

    private static void ResetAllStructures(ReducerContext ctx)
    {
        var cfg = ctx.Db.Config.Id.Find(0)!.Value;
        foreach (var s in ctx.Db.Structure.Iter().ToList())
        {
            float hp = s.Type == StructureType.Throne ? cfg.ThroneMaxHealth : cfg.TowerMaxHealth;
            ctx.Db.Structure.Id.Update(s with
            {
                Health           = hp,
                IsDestroyed      = false,
                AttackTargetId   = 0,
                AttackTargetKind = TargetKind.None,
                LastAttackTime   = 0,
            });
        }
    }

    private static void ResetAllNeutralCreeps(ReducerContext ctx)
    {
        var cfg = ctx.Db.Config.Id.Find(0)!.Value;
        foreach (var c in ctx.Db.Creep.Iter().ToList())
        {
            if (c.Type != CreepType.Neutral) continue;
            var campPos = NeutralCamps.First(nc => nc.id == c.CampId).pos;
            ctx.Db.Creep.Id.Update(c with
            {
                Position         = campPos,
                Destination      = campPos,
                Health           = cfg.NeutralMaxHealth,
                State            = CreepState.Idle,
                AttackTargetKind = TargetKind.None,
                AttackTargetId   = 0,
                LastAttackTime   = 0,
                DeathTime        = 0,
            });
        }
    }

    [SpacetimeDB.Reducer]
    public static void SpawnCreepWave(ReducerContext ctx, CreepSpawnSchedule _schedule)
    {
        if (ctx.Db.GameState.Id.Find(0) is not { Phase: GamePhase.InGame }) return;

        var cfg = ctx.Db.Config.Id.Find(0)!.Value;

        foreach (var lane in new[] { Lane.Mid, Lane.Top, Lane.Bottom })
        {
            SpawnWaveLane(ctx, cfg, Team.Radiant, lane);
            SpawnWaveLane(ctx, cfg, Team.Dire,    lane);
        }

        Log.Info("[MOBA] Creep wave spawned");
    }

    private static void SpawnWaveLane(ReducerContext ctx, Config cfg, Team team, Lane lane)
    {
        var waypoints = GetLaneWaypoints(team, lane);
        if (waypoints.Length == 0) return;

        DbVec3 startPos = waypoints[0];

        for (int i = 0; i < 3; i++)
        {
            var offset = V(i * 1.5f, 0f, i * 0.5f);
            var pos = Add(startPos, offset);

            ctx.Db.Creep.Insert(new Creep
            {
                Id              = 0,
                Team            = team,
                Type            = CreepType.Wave,
                Lane            = lane,
                Position        = pos,
                Destination     = waypoints.Length > 1 ? waypoints[1] : pos,
                MoveSpeed       = cfg.CreepMoveSpeed,
                Health          = cfg.CreepMaxHealth,
                MaxHealth       = cfg.CreepMaxHealth,
                AttackRange     = cfg.CreepAttackRange,
                AttackDamage    = cfg.CreepAttackDamage,
                AttackCooldown  = cfg.CreepAttackCooldown,
                State           = CreepState.Moving,
                AttackTargetKind = TargetKind.None,
                AttackTargetId  = 0,
                LastAttackTime  = 0,
                DeathTime       = 0,
                CampId          = 0,
                WaypointIndex   = 1,
            });
        }
    }
}
