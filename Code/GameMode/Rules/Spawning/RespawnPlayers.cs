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
		foreach ( var player in GameUtils.ActivePlayers )
		{
			if ( GameMode.Instance.Get<ISpawnAssigner>() is { } spawnAssigner )
			{
				player.Teleport( spawnAssigner.GetSpawnPoint( player ) );
			}

			player.Respawn();
		}
	}
}
