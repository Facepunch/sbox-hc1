using Facepunch;

/// <summary>
/// Split players into two balanced teams.
/// </summary>
public sealed class TeamAssigner : Component, IGameStartListener
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
}
