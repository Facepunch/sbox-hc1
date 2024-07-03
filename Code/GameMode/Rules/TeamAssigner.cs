using Sandbox.Events;

namespace Facepunch;

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

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		foreach ( var player in GameUtils.GetPlayers(Team.Unassigned) )
		{
			AssignTeam( player, true );
		}
	}

	private void AssignTeam( PlayerState player, bool dispatch )
	{
		var assignTeam = SelectTeam();

		if ( dispatch )
		{
			player.AssignTeam( assignTeam );

			// Respawn the player's pawn since we might've changed their spawn
			if ( player.PlayerPawn.IsValid() )
				player.PlayerPawn.Respawn();
		}
		else
		{
			player.Team = assignTeam;
		}
	}

	public float GetTeamCountScore( int ts, int cts )
	{
		if ( ts > MaxTeamSize || cts > MaxTeamSize ) return float.PositiveInfinity;
		if ( ts == 0 || cts == 0 ) return 1000f;

		var ratio = (float)ts / cts;

		return Math.Abs( ratio - TargetRatio );
	}

	private Team SelectTeam()
	{
		var ts = GameUtils.GetPlayers( Team.Terrorist ).Count();
		var cts = GameUtils.GetPlayers( Team.CounterTerrorist ).Count();

		var tScore = GetTeamCountScore( ts + 1, cts );
		var ctScore = GetTeamCountScore( ts, cts + 1 );

		if ( float.IsInfinity( tScore ) && float.IsInfinity( ctScore ) )
		{
			return Team.Unassigned;
		}

		return tScore.CompareTo( ctScore ) switch
		{
			> 0 => Team.CounterTerrorist,
			< 0 => Team.Terrorist,
			_ => RandomTeam()
		};
	}

	private static Team RandomTeam()
	{
		return Random.Shared.NextSingle() < 0.5f ? Team.Terrorist : Team.CounterTerrorist;
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
