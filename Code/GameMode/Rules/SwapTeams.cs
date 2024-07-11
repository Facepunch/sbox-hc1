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
		var ts = GameUtils.GetPlayers( Team.Terrorist ).ToArray();
		var cts = GameUtils.GetPlayers( Team.CounterTerrorist ).ToArray();

		foreach ( var player in ts )
		{
			player.AssignTeam( Team.CounterTerrorist );
		}

		foreach ( var player in cts )
		{
			player.AssignTeam( Team.Terrorist );
		}

		Game.ActiveScene.Dispatch( new TeamsSwappedEvent() );
	}
}
