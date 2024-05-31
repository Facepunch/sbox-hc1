using Facepunch;

/// <summary>
/// Split players into two balanced teams.
/// </summary>
public sealed class TeamAssigner : Component, IGameStartListener, IRoundEndListener, IPlayerJoinedListener
{
	[Property]
	public int MaxTeamSize { get; set; } = 5;

	void IPlayerJoinedListener.OnPlayerJoined( PlayerController player )
	{
		Log.Info( nameof( TeamAssigner ) );
		var ts = GameUtils.GetPlayers( Team.Terrorist ).ToList();
		var cts = GameUtils.GetPlayers( Team.CounterTerrorist ).ToList();

		Team assignTeam = Team.Unassigned;

		int compare = ts.Count().CompareTo( cts.Count() );
		if ( compare < 0 )
		{
			assignTeam = Team.Terrorist;
		}
		else if ( compare > 0 )
		{
			assignTeam = Team.CounterTerrorist;
		}
		else if ( cts.Count() < MaxTeamSize )
		{
			bool coinFlip = Random.Shared.Next( 2 ) == 1;
			assignTeam = coinFlip ? Team.Terrorist : Team.CounterTerrorist;
		}

		player.AssignTeam( assignTeam );
	}

	// assigning teams on join now instead, turn this into balance teams?
	/*
	void IGameStartListener.PostGameStart()
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

	void IRoundEndListener.PostRoundEnd()
	{
		// Put spectators on a team at the end of each round

		var unassigned = GameUtils.InactivePlayers.Shuffle();

		var ts = GameUtils.GetPlayers( Team.Terrorist ).ToList();
		var cts = GameUtils.GetPlayers( Team.CounterTerrorist ).ToList();

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
