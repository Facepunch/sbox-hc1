using Facepunch;

/// <summary>
/// Split players into two balanced teams.
/// </summary>
public sealed class TeamAssigner : Component, IGameStartListener, IRoundEndListener
{
	[Property]
	public int MaxTeamSize { get; set; } = 5;

	void IGameStartListener.PostGameStart()
	{
		Log.Info( nameof( TeamAssigner ) );

		var players = GameUtils.AllPlayers.Shuffle();

		var ctCount = Math.Min( players.Count / 2, MaxTeamSize );
		var tCount = Math.Min( players.Count - ctCount, MaxTeamSize );

		foreach (var tPlayer in players.Take( tCount ))
		{
			tPlayer.TeamComponent.AssignTeam( Team.Terrorist );
		}

		foreach (var ctPlayer in players.Skip( tCount ).Take( ctCount ))
		{
			ctPlayer.TeamComponent.AssignTeam( Team.CounterTerrorist );
		}
	}

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
				player.TeamComponent.AssignTeam( Team.Terrorist );
			}
			else
			{
				cts.Add( player );
				player.TeamComponent.AssignTeam( Team.CounterTerrorist );
			}
		}
	}
}
