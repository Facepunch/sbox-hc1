using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Respawn all players at the start of this state.
/// </summary>
public sealed class RespawnPlayers : Component,
	IGameEventHandler<EnterStateEvent>
{
	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		GameUtils.LogPlayers();

		foreach ( var player in GameUtils.AllPlayers )
		{
			player.Respawn( false );
		}
	}
}
