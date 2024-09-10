﻿using Facepunch.UI;
using Sandbox.Events;

namespace Facepunch;

public enum AutoBalanceMode
{
	/// <summary>
	/// Don't auto-balance.
	/// </summary>
	None,

	/// <summary>
	/// Pick random players to move.
	/// </summary>
	Random,

	/// <summary>
	/// Prefer to move players with the lowest scores.
	/// </summary>
	LowestScorers,

	/// <summary>
	/// Prefer to move players with the lowest scores from just the last round.
	/// </summary>
	LowestScorersLastRound
}

/// <summary>
/// Split players into two balanced teams. If <see cref="AllowLateJoiners"/> is true,
/// new players will be assigned as soon as they join. Otherwise, teams will only be
/// assigned when this game state is entered.
/// </summary>
public sealed class TeamAssigner : Component,
	IGameEventHandler<EnterStateEvent>,
	IGameEventHandler<PlayerConnectedEvent>,
	IGameEventHandler<PlayerJoinedEvent>
{
	[Property] public int MaxTeamSize { get; set; } = 5;

	/// <summary>
	/// Target number of Ts per CT.
	/// </summary>
	[Property] public float TargetRatio { get; set; } = 1f;

	/// <summary>
	/// If true, new players will be assigned as soon as they join. Otherwise, teams
	/// will only be assigned when this game state is entered.
	/// </summary>
	[Property] public bool AllowLateJoiners { get; set; } = true;

	/// <summary>
	/// What sort of auto-balancing should we apply when entering this state.
	/// </summary>
	[Property] public AutoBalanceMode AutoBalanceMode { get; set; } = AutoBalanceMode.Random;

	[After<SwapTeams>]
	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		AssignSpectatorsToTeams();
		// AutoBalance();
	}

	private void AssignSpectatorsToTeams()
	{
		foreach ( var player in GameUtils.GetPlayers( null ) )
		{
			AssignTeam( player, true );
		}
	}

	private void AutoBalance()
	{
		if ( AutoBalanceMode == AutoBalanceMode.None )
		{
			return;
		}
		var teams = TeamSetup.Instance.Teams;
		var ts = GameUtils.GetPlayers( teams[0] ).Count();
		var cts = GameUtils.GetPlayers( teams[1] ).Count();

		var curScore = GetTeamCountScore( ts, cts );

		// delta: how many ts to move to ct

		var minDelta = -cts;
		var maxDelta = ts;

		var bestScore = curScore;
		var bestDelta = 0;

		for ( var delta = minDelta; delta <= maxDelta; ++delta )
		{
			var newScore = GetTeamCountScore( ts - delta, cts + delta );

			if ( newScore < bestScore )
			{
				bestScore = newScore;
				bestDelta = delta;
			}
		}

		if ( bestDelta == 0 )
		{
			return;
		}

		var fromTeam = bestDelta < 0 ? teams[1] : teams[0];

		var toSwap = GameUtils.GetPlayers( fromTeam )
			.OrderBy( GetScore )
			.Take( Math.Abs( bestDelta ) )
			.ToArray();

		foreach ( var player in toSwap )
		{
			player.AssignTeam( fromTeam.GetOpponents() );

			// Respawn the player's pawn since we might've changed their spawn
			if ( player.PlayerPawn.IsValid() )
				player.Respawn( true );
		}
	}

	private float GetScore( PlayerState player )
	{
		return AutoBalanceMode switch
		{
			AutoBalanceMode.Random => Random.Shared.NextSingle(),
			AutoBalanceMode.LowestScorers => player.Components.Get<PlayerScore>() is { } playerScore ? playerScore.Score : 0,
			AutoBalanceMode.LowestScorersLastRound => player.Components.Get<PlayerScore>() is { } playerScore ? playerScore.ScoreHistory.LastOrDefault() : 0,
			_ => 0
		};
	}

	private void AssignTeam( PlayerState player, bool dispatch )
	{
		var assignTeam = SelectTeam();

		if ( assignTeam == player.Team )
		{
			return;
		}

		if ( dispatch )
		{
			player.AssignTeam( assignTeam );
			player.Respawn( true );
		}
		else
		{
			player.Team = assignTeam;
		}
	}

	public float GetTeamCountScore( int ts, int cts )
	{
		if ( ts > MaxTeamSize || cts > MaxTeamSize || ts < 0 || cts < 0 ) return float.PositiveInfinity;
		if ( ts == 0 || cts == 0 ) return 1000f;

		var ratio = (float)ts / cts;

		return Math.Abs( ratio - TargetRatio );
	}

	private TeamDefinition SelectTeam()
	{
		var teams = TeamSetup.Instance.Teams;
		var ts = GameUtils.GetPlayers( teams[0] ).Count();
		var cts = GameUtils.GetPlayers( teams[1] ).Count();

		var tScore = GetTeamCountScore( ts + 1, cts );
		var ctScore = GetTeamCountScore( ts, cts + 1 );

		if ( float.IsInfinity( tScore ) && float.IsInfinity( ctScore ) )
		{
			return null;
		}

		return tScore.CompareTo( ctScore ) switch
		{
			> 0 => teams[1],
			< 0 => teams[0],
			_ => RandomTeam()
		};
	}

	private static TeamDefinition RandomTeam()
	{
		return Game.Random.FromList( TeamSetup.Instance.Teams );
	}

	void IGameEventHandler<PlayerConnectedEvent>.OnGameEvent( PlayerConnectedEvent eventArgs )
	{
		if ( AllowLateJoiners )
		{
			AssignTeam( eventArgs.PlayerState, false );
		}
	}

	void IGameEventHandler<PlayerJoinedEvent>.OnGameEvent( PlayerJoinedEvent eventArgs )
	{
		if ( AllowLateJoiners )
		{
			// Calling this will invoke callbacks for any ITeamAssignedListener listeners.
			eventArgs.Player.AssignTeam( eventArgs.Player.Team );
		}
	}
}
