using Facepunch;
using Sandbox.Events;

/// <summary>
/// Respawn all players at the start of each round.
/// </summary>
public sealed class RoundStartPlayerSpawner : Component,
	IGameEventHandler<PreRoundStartEvent>
{
	void IGameEventHandler<PreRoundStartEvent>.OnGameEvent( PreRoundStartEvent eventArgs )
	{
		foreach ( var player in GameUtils.ActivePlayers )
		{
			if ( GameMode.Instance.Components.GetInDescendantsOrSelf<ISpawnAssigner>() is { } spawnAssigner )
			{
				player.Teleport( spawnAssigner.GetSpawnPoint( player ) );
			}

			player.Respawn();
		}
	}
}
