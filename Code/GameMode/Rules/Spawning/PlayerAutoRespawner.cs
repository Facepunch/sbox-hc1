using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Respawn players after a delay.
/// </summary>
public sealed class PlayerAutoRespawner : Component,
	IGameEventHandler<UpdateStateEvent>
{
	[Property, HostSync] public float RespawnDelaySeconds { get; set; } = 3f;
	[Property] public bool AllowSpectatorsToSpawn { get; set; } = false;

	void IGameEventHandler<UpdateStateEvent>.OnGameEvent( UpdateStateEvent eventArgs )
	{
		foreach ( var player in GameUtils.AllPlayerStates )
		{
			if ( player.PlayerPawn.IsValid() )
				continue;

			// todo: look into this??
			if ( player.Network.OwnerConnection is null )
				continue;

			if ( !player.Network.OwnerConnection.IsHost && !player.Network.OwnerConnection.IsActive ) // smh
				return;

			if ( !AllowSpectatorsToSpawn && player.Team == Team.Unassigned )
			{
				// don't spawn these guys right now
				return;
			}

			switch ( player.RespawnState )
			{
				case RespawnState.None:
					player.RespawnState = RespawnState.CountingDown;

					using ( Rpc.FilterInclude( player.Network.OwnerConnection ) )
					{
						GameMode.Instance.ShowToast( "Respawning...", duration: RespawnDelaySeconds );
					}

					break;

				case RespawnState.CountingDown:
					if ( player.TimeSinceRespawnStateChanged > RespawnDelaySeconds )
					{
						player.Spawn();
					}

					break;
			}
		}
	}
}
