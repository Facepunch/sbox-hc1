using System.Threading.Tasks;
using Facepunch;

public sealed class TeamAssigner : Component, IGameStartListener, IRoundStartListener
{
	[Property]
	public int MaxTeamSize { get; set; } = 5;

	void IGameStartListener.PostGameStart()
	{
		var players = GameUtils.AllPlayers.Shuffle();

		var tCount = Math.Min( players.Count / 2, MaxTeamSize );
		var ctCount = Math.Min( players.Count - tCount, MaxTeamSize );

		foreach (var tPlayer in players.Take( tCount ))
		{
			Log.Info( $"{tPlayer.Network.OwnerConnection.DisplayName} is a T" );
			tPlayer.TeamComponent.Team = Team.Terrorist;
		}

		foreach (var ctPlayer in players.Skip( tCount ).Take( ctCount ))
		{
			Log.Info( $"{ctPlayer.Network.OwnerConnection.DisplayName} is a CT" );
			ctPlayer.TeamComponent.Team = Team.CounterTerrorist;
		}
	}
}
