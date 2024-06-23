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
		var ts = GameUtils.GetPlayers( Team.Terrorist ).ToArray();
		var cts = GameUtils.GetPlayers( Team.CounterTerrorist ).ToArray();

		foreach ( var player in ts )
		{
			player.PlayerState.AssignTeam( Team.CounterTerrorist );
		}

		foreach ( var player in cts )
		{
			player.PlayerState.AssignTeam( Team.Terrorist );
		}

		Scene.Dispatch( new TeamsSwappedEvent() );
	}
}
