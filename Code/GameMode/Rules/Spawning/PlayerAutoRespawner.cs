﻿using Sandbox.Events;

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
			if ( player.PlayerPawn.IsValid() && player.PlayerPawn.HealthComponent.State == LifeState.Alive )
				continue;

			if ( !player.IsConnected )
				continue;

			if ( !AllowSpectatorsToSpawn && player.Team == Team.Unassigned )
			{
				// don't spawn these guys right now
				return;
			}

			switch ( player.RespawnState )
			{
				case RespawnState.Requested:
					player.RespawnState = RespawnState.Delayed;

					using ( Rpc.FilterInclude( player.Network.OwnerConnection ) )
					{
						GameMode.Instance.ShowToast( "Respawning...", duration: RespawnDelaySeconds );
					}
					break;

				case RespawnState.Delayed:
					if ( player.TimeSinceRespawnStateChanged > RespawnDelaySeconds )
					{
						player.RespawnState = RespawnState.Immediate;
					}
					break;

				case RespawnState.Immediate:
					player.Respawn( true );
					break;
			}
		}
	}
}
