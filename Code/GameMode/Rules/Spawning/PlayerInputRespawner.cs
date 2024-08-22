using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Respawn players on input, after a delay.
/// </summary>
public sealed class PlayerInputRespawner : Component,
	IGameEventHandler<UpdateStateEvent>
{
	[Property, HostSync] public float RespawnDelaySeconds { get; set; } = 3f;
	[Property] public bool AllowSpectatorsToSpawn { get; set; } = false;
	[Property, InputAction] public string InputAction { get; set; } = "Jump";

	protected override void OnUpdate()
	{
		var player = PlayerState.Local;

		if ( player.PlayerPawn.IsValid() && player.PlayerPawn.HealthComponent.State == LifeState.Alive )
			return;

		if ( Input.Pressed( InputAction ) )
		{
			using ( Rpc.FilterInclude( Connection.Host ) )
			{
				AskToRespawn();
			}
		}
	}

	[Broadcast]
	private void AskToRespawn()
	{
		var rpcCaller = Rpc.Caller;
		var player = GameUtils.AllPlayers.FirstOrDefault( x => x.Network.OwnerConnection == rpcCaller );

		if ( !player.IsValid() )
			return;

		if ( player.PlayerPawn.IsValid() && player.PlayerPawn.HealthComponent.State == LifeState.Alive )
			return;

		player.RespawnState = RespawnState.Immediate;
	}

	void IGameEventHandler<UpdateStateEvent>.OnGameEvent( UpdateStateEvent eventArgs )
	{
		foreach ( var player in GameUtils.AllPlayers )
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

					using ( Rpc.FilterInclude( player.Connection ) )
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
