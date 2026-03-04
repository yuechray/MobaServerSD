using SpacetimeDB;

[SpacetimeDB.Type]
public partial struct DbVec3
{
    public float X;
    public float Y;
    public float Z;
}

[SpacetimeDB.Type] public enum GamePhase     { Lobby, Starting, InGame, GameOver }
[SpacetimeDB.Type] public enum ChampionState { Idle, Moving, Attacking, Dead }
[SpacetimeDB.Type] public enum Team          { None, Radiant, Dire }
[SpacetimeDB.Type] public enum StructureType { Throne, Tower }
[SpacetimeDB.Type] public enum Lane          { None, Top, Mid, Bottom }
[SpacetimeDB.Type] public enum CreepType     { Wave, Neutral }
[SpacetimeDB.Type] public enum CreepState    { Idle, Moving, Attacking, Dead }
[SpacetimeDB.Type] public enum TargetKind    { None, Champion, Creep, Structure }
