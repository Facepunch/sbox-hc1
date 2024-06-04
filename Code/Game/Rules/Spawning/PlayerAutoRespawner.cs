using Facepunch;

/// <summary>
/// Respawn players after a delay.
/// </summary>
public sealed class PlayerAutoRespawner : Component, IGameStartListener
{
	[Property, HostSync] public float RespawnDelaySeconds { get; set; } = 3f;
	[Property, HostSync] public bool DisableOnGameStart { get; set; }

	void IGameStartListener.PostGameStart()
	{
		if ( DisableOnGameStart )
		{
			Enabled = false;
		}
	}

	protected override void OnFixedUpdate()
	{
		if ( !Networking.IsHost ) return;

		foreach ( var player in GameUtils.AllPlayers )
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

						if ( GameMode.Instance.Components.GetInDescendantsOrSelf<ISpawnAssigner>() is { } spawnAssigner )
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
