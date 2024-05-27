using System.Threading.Tasks;
using Facepunch;

public sealed class TeamAssigner : Component, IGameStartListener
{
	[Property]
	public int MaxTeamSize { get; set; } = 5;

	public Task OnGameStart()
	{
		var players = GameUtils.AllPlayers.Shuffle();

		var tCount = Math.Min( players.Count / 2, MaxTeamSize );
		var ctCount = Math.Min( players.Count - tCount, MaxTeamSize );

		foreach ( var tPlayer in players.Take( tCount ) )
		{
			tPlayer.TeamComponent.Team = Team.Terrorist;
		}

		foreach ( var ctPlayer in players.Skip( tCount ).Take( ctCount ) )
		{
			ctPlayer.TeamComponent.Team = Team.CounterTerrorist;
		}

		return Task.CompletedTask;
	}
}
