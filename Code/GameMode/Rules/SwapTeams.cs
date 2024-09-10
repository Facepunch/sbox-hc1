using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Swap teams when entering this state.
/// </summary>
public sealed class SwapTeams : Component,
	IGameEventHandler<EnterStateEvent>
{
	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		Swap();
	}

	[DeveloperCommand( "Swap Teams", "Game Loop" )]
	public static void Swap()
	{
		var teamSetupTeams = TeamSetup.Instance.Teams;

		if ( teamSetupTeams.Count() > 2 )
		{
			Log.Warning( "swapping teams when over 2 is not supported" );
			return;
		}

		var ts = GameUtils.GetPlayers( teamSetupTeams[0] ).ToArray();
		var cts = GameUtils.GetPlayers( teamSetupTeams[1] ).ToArray();

		foreach ( var player in ts )
		{
			player.AssignTeam( teamSetupTeams[1] );
		}

		foreach ( var player in cts )
		{
			player.AssignTeam( teamSetupTeams[0] );
		}

		Game.ActiveScene.Dispatch( new TeamsSwappedEvent() );
	}
}
