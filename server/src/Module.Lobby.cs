using SpacetimeDB;
using System;
using System.Collections.Generic;
using System.Linq;

public static partial class Module
{
    [SpacetimeDB.Reducer]
    public static void RegisterProfile(ReducerContext ctx, string username)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length > 24)
            throw new Exception("Invalid username (1-24 chars)");

        foreach (var p in ctx.Db.UserProfile.Iter())
        {
            if (p.Username == username && p.PlayerId != ctx.Sender)
                throw new Exception("Username already taken");
        }

        if (ctx.Db.UserProfile.PlayerId.Find(ctx.Sender) is { } existing)
        {
            ctx.Db.UserProfile.PlayerId.Update(existing with { Username = username });
        }
        else
        {
            ctx.Db.UserProfile.Insert(new UserProfile { PlayerId = ctx.Sender, Username = username });
            Log.Info($"[MOBA] Registered new user: {username}");
        }
    }

    [SpacetimeDB.Reducer]
    public static void JoinLobby(ReducerContext ctx, string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 24)
            throw new Exception("Invalid name (1-24 chars)");

        var state = ctx.Db.GameState.Id.Find(0) ?? throw new Exception("GameState missing");
        if (state.Phase == GamePhase.InGame)
            throw new Exception("Match already in progress");

        if (ctx.Db.LobbyPlayer.PlayerId.Find(ctx.Sender) is { } existing)
        {
            ctx.Db.LobbyPlayer.PlayerId.Update(existing with { Name = name, IsReady = false, IsConnected = true });
            return;
        }

        ctx.Db.LobbyPlayer.Insert(new LobbyPlayer
        {
            PlayerId    = ctx.Sender,
            Name        = name,
            Team        = Team.None,
            IsReady     = false,
            IsConnected = true,
        });

        Log.Info($"[MOBA] {name} joined lobby");
    }

    [SpacetimeDB.Reducer]
    public static void SelectTeam(ReducerContext ctx, Team team)
    {
        if (team == Team.None) throw new Exception("Choose Radiant or Dire");

        var lp    = RequireLobbyPlayer(ctx);
        var state = ctx.Db.GameState.Id.Find(0) ?? throw new Exception("GameState missing");
        if (state.Phase != GamePhase.Lobby) throw new Exception("Cannot change team now");

        ctx.Db.LobbyPlayer.PlayerId.Update(lp with { Team = team, IsReady = false });
        Log.Info($"[MOBA] {lp.Name} selected {team}");
    }

    [SpacetimeDB.Reducer]
    public static void SetReady(ReducerContext ctx, bool ready)
    {
        var lp = RequireLobbyPlayer(ctx);
        if (lp.Team == Team.None && ready)
            throw new Exception("Select a team before readying up");

        var state = ctx.Db.GameState.Id.Find(0) ?? throw new Exception("GameState missing");
        if (state.Phase != GamePhase.Lobby) throw new Exception("Cannot change ready state now");

        ctx.Db.LobbyPlayer.PlayerId.Update(lp with { IsReady = ready });
        Log.Info($"[MOBA] {lp.Name} is {(ready ? "ready" : "not ready")}");

        if (!ready) return;

        var connected = ctx.Db.LobbyPlayer.Iter()
            .Where(p => p.IsConnected)
            .ToList();

        if (connected.Count < 1) return;
        if (connected.Any(p => p.Team == Team.None)) return;
        if (connected.Any(p => !p.IsReady)) return;

        StartMatch(ctx, connected);
    }

    private static void StartMatch(ReducerContext ctx, List<LobbyPlayer> players)
    {
        ctx.Db.GameState.Id.Update(
            ctx.Db.GameState.Id.Find(0)!.Value with { Phase = GamePhase.InGame, WinnerTeam = Team.None, Countdown = 0 }
        );

        var cfg = ctx.Db.Config.Id.Find(0)!.Value;

        SpawnAllChampions(ctx, players, cfg);

        Log.Info("[MOBA] Match started!");
    }
}
