using Facepunch;
using Sandbox.Events;

/// <summary>
/// Split players into two balanced teams.
/// </summary>
public sealed class TeamAssigner : Component,
	IGameEventHandler<PostRoundEndEvent>,
	IGameEventHandler<PlayerConnectedEvent>,
	IGameEventHandler<PlayerJoinedEvent>
{
	[Property] public int MaxTeamSize { get; set; } = 5;

	void IGameEventHandler<PlayerConnectedEvent>.OnGameEvent( PlayerConnectedEvent eventArgs )
	{
		var ts = GameUtils.GetPlayers( Team.Terrorist ).ToList();
		var cts = GameUtils.GetPlayers( Team.CounterTerrorist ).ToList();
		var assignTeam = Team.Unassigned;

		var compare = ts.Count.CompareTo( cts.Count );
		if ( compare < 0 )
		{
			assignTeam = Team.Terrorist;
		}
		else if ( compare > 0 )
		{
			assignTeam = Team.CounterTerrorist;
		}
		else if ( cts.Count < MaxTeamSize )
		{
			var coinFlip = Random.Shared.Next( 2 ) == 1;
			assignTeam = coinFlip ? Team.Terrorist : Team.CounterTerrorist;
		}

		// Set the team directly to avoid callbacks for ITeamAssignedListener listeners right now.
		eventArgs.Player.TeamComponent.Team = assignTeam;
	}

	void IGameEventHandler<PlayerJoinedEvent>.OnGameEvent( PlayerJoinedEvent eventArgs )
	{
		// Calling this will invoke callbacks for any ITeamAssignedListener listeners.
		eventArgs.Player.AssignTeam( eventArgs.Player.TeamComponent.Team );
	}

	// assigning teams on join now instead, turn this into balance teams?
	/*
	void IGameEventHandler<PostGameStartEvent>.OnGameEvent( PostGameStartEvent eventArgs )
	{
		Log.Info( nameof( TeamAssigner ) );

		var players = GameUtils.AllPlayers.Shuffle();

		var ctCount = Math.Min( players.Count / 2, MaxTeamSize );
		var tCount = Math.Min( players.Count - ctCount, MaxTeamSize );

		foreach (var tPlayer in players.Take( tCount ))
		{
			tPlayer.AssignTeam( Team.Terrorist );
		}

		foreach (var ctPlayer in players.Skip( tCount ).Take( ctCount ))
		{
			ctPlayer.AssignTeam( Team.CounterTerrorist );
		}
	}
	*/

	void IGameEventHandler<PostRoundEndEvent>.OnGameEvent( PostRoundEndEvent eventArgs )
	{
		// Put spectators on a team at the end of each round
		var ts = GameUtils.GetPlayers( Team.Terrorist ).ToList();
		var cts = GameUtils.GetPlayers( Team.CounterTerrorist ).ToList();
		var unassigned = GameUtils.InactivePlayers.Shuffle();

		foreach ( var player in unassigned )
		{
			if ( ts.Count >= MaxTeamSize && cts.Count >= MaxTeamSize )
			{
				// Teams are full
				return;
			}

			if ( ts.Count <= cts.Count )
			{
				ts.Add( player );
				player.AssignTeam( Team.Terrorist );
			}
			else
			{
				cts.Add( player );
				player.AssignTeam( Team.CounterTerrorist );
			}
		}
	}
}
