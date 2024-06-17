using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Respawn players after a delay.
/// </summary>
public sealed class PlayerAutoRespawner : Component,
	IGameEventHandler<UpdateStateEvent>
{
	[Property, HostSync] public float RespawnDelaySeconds { get; set; } = 3f;

	void IGameEventHandler<UpdateStateEvent>.OnGameEvent( UpdateStateEvent eventArgs )
	{
		foreach ( var player in GameUtils.ActivePlayers )
		{
			if ( player.HealthComponent.State != LifeState.Dead )
			{
				continue;
			}

			switch ( player.HealthComponent.RespawnState )
			{
				case RespawnState.None:
					player.HealthComponent.RespawnState = RespawnState.CountingDown;

					using ( Rpc.FilterInclude( player.Network.OwnerConnection ) )
					{
						GameMode.Instance.ShowToast( "Respawning...", duration: RespawnDelaySeconds );
					}

					break;

				case RespawnState.CountingDown:
					if ( player.HealthComponent.TimeSinceLifeStateChanged > RespawnDelaySeconds )
					{
						player.HealthComponent.RespawnState = RespawnState.Ready;

						// TODO: click to respawn?

						if ( GameMode.Instance.Get<ISpawnAssigner>() is { } spawnAssigner )
						{
							player.Teleport( spawnAssigner.GetSpawnPoint( player ) );
						}

						player.Respawn();
					}

					break;
			}
		}
	}
}
