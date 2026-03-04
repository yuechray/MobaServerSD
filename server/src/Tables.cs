using SpacetimeDB;

/// <summary>Game constants — singleton, Id=0</summary>
[SpacetimeDB.Table(Name = "Config", Public = true)]
public partial struct Config
{
    [SpacetimeDB.PrimaryKey]
    public uint Id;

    public float MoveSpeed;
    public float AttackRange;
    public float AttackDamage;
    public float AttackCooldown;
    public float RespawnDuration;
    public float ChampMaxHealth;
    public float ChampMaxMana;

    public DbVec3 RadiantSpawn;
    public DbVec3 DireSpawn;

    public float CreepMoveSpeed;
    public float CreepAttackRange;
    public float CreepAttackDamage;
    public float CreepAttackCooldown;
    public float CreepMaxHealth;
    public float CreepWaveIntervalSec;

    public float NeutralMoveSpeed;
    public float NeutralAttackRange;
    public float NeutralAttackDamage;
    public float NeutralAttackCooldown;
    public float NeutralMaxHealth;
    public float NeutralRespawnSec;

    public float TowerMaxHealth;
    public float TowerAttackDamage;
    public float TowerAttackRange;
    public float TowerAttackCooldown;

    public float ThroneMaxHealth;
}

/// <summary>Overall match state — singleton, Id=0</summary>
[SpacetimeDB.Table(Name = "GameState", Public = true)]
public partial struct GameState
{
    [SpacetimeDB.PrimaryKey]
    public uint Id;

    public GamePhase Phase;
    public Team      WinnerTeam;
    public int       Countdown;
}

/// <summary>Persistent user profile — one row per registered player.</summary>
[SpacetimeDB.Table(Name = "UserProfile", Public = true)]
public partial struct UserProfile
{
    [SpacetimeDB.PrimaryKey]
    public Identity PlayerId;

    [SpacetimeDB.Unique]
    public string Username;
}

/// <summary>One row per player currently in the lobby or game.</summary>
[SpacetimeDB.Table(Name = "LobbyPlayer", Public = true)]
public partial struct LobbyPlayer
{
    [SpacetimeDB.PrimaryKey]
    public Identity PlayerId;

    public string Name;
    public Team   Team;
    public bool   IsReady;
    public bool   IsConnected;
}

/// <summary>Champion entity — spawned when game starts.</summary>
[SpacetimeDB.Table(Name = "Champion", Public = true)]
public partial struct Champion
{
    [SpacetimeDB.PrimaryKey]
    [SpacetimeDB.AutoInc]
    public ulong Id;

    public Identity OwnerIdentity;
    public Team     Team;
    public string   Name;

    public DbVec3        Position;
    public DbVec3        Destination;
    public float         MoveSpeed;
    public ChampionState State;

    public float Health;
    public float MaxHealth;
    public float Mana;
    public float MaxMana;

    public TargetKind AttackTargetKind;
    public ulong      AttackTargetId;
    public double     LastAttackTime;
    public float      AttackRange;
    public float      AttackDamage;
    public float      AttackCooldown;

    public double DeathTime;
    public float  RespawnDuration;
}

/// <summary>Static structure — tower or throne.</summary>
[SpacetimeDB.Table(Name = "Structure", Public = true)]
public partial struct Structure
{
    [SpacetimeDB.PrimaryKey]
    [SpacetimeDB.AutoInc]
    public ulong Id;

    public Team          Team;
    public StructureType Type;
    public Lane          Lane;

    public DbVec3 Position;

    public float  Health;
    public float  MaxHealth;
    public float  AttackDamage;
    public float  AttackRange;
    public float  AttackCooldown;
    public double LastAttackTime;
    public ulong  AttackTargetId;
    public TargetKind AttackTargetKind;
    public bool   IsDestroyed;
}

/// <summary>Creep entity — wave minions and jungle neutrals.</summary>
[SpacetimeDB.Table(Name = "Creep", Public = true)]
public partial struct Creep
{
    [SpacetimeDB.PrimaryKey]
    [SpacetimeDB.AutoInc]
    public ulong Id;

    public Team      Team;         // None for neutrals
    public CreepType Type;
    public Lane      Lane;

    public DbVec3    Position;
    public DbVec3    Destination;
    public float     MoveSpeed;

    public float     Health;
    public float     MaxHealth;
    public float     AttackRange;
    public float     AttackDamage;
    public float     AttackCooldown;

    public CreepState State;

    public TargetKind AttackTargetKind;
    public ulong      AttackTargetId;
    public double     LastAttackTime;

    public double DeathTime;
    public ulong  CampId;          // Neutrals: identifies respawn camp
    public int    WaypointIndex;   // Wave creeps: current path index
}

/// <summary>Drives the 20 Hz server-side game loop.</summary>
[SpacetimeDB.Table(Name = "GameTickSchedule", Scheduled = nameof(Module.GameTick))]
public partial struct GameTickSchedule
{
    [SpacetimeDB.PrimaryKey]
    [SpacetimeDB.AutoInc]
    public ulong ScheduledId;
    public ScheduleAt ScheduledAt;
}

/// <summary>Drives periodic creep wave spawning.</summary>
[SpacetimeDB.Table(Name = "CreepSpawnSchedule", Scheduled = nameof(Module.SpawnCreepWave))]
public partial struct CreepSpawnSchedule
{
    [SpacetimeDB.PrimaryKey]
    [SpacetimeDB.AutoInc]
    public ulong ScheduledId;
    public ScheduleAt ScheduledAt;
}
