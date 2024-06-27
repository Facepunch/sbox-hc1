using Sandbox.Events;

namespace Facepunch;

public enum RespawnState
{
	Not,
	Requested,
	Delayed,
	Immediate
}

public partial class PlayerState
{
	/// <summary>
	/// The prefab to spawn when we want to make a player pawn for the player.
	/// </summary>
	[Property] public GameObject PlayerPawnPrefab { get; set; }

	[Property] public TimeSince TimeSinceRespawnStateChanged;

	/// <summary>
	/// Are we ready to respawn?
	/// </summary>
	[HostSync, Change( nameof( OnRespawnStateChanged ))] public RespawnState RespawnState { get; set; }

	public bool IsRespawning => RespawnState is RespawnState.Delayed;

	public void Spawn()
	{
		if ( PlayerPawn.IsValid() )
			return;

		Log.Info( $"Spawning player.. ( {GameObject.Name} ({DisplayName}, {Team}) )" );

		// :S
		var spawnPoint = new Transform();
		if ( GameMode.Instance.Get<ISpawnAssigner>() is { } spawnAssigner )
		{
			spawnPoint = spawnAssigner.GetSpawnPoint( this );
		}

		var prefab = PlayerPawnPrefab.Clone( spawnPoint );
		var pawn = prefab.Components.Get<PlayerPawn>();
		pawn.PlayerState = this;
		prefab.NetworkSpawn( Network.OwnerConnection );

		PlayerPawn = pawn;
		if ( IsBot )
			Pawn = pawn;
				
		RespawnState = RespawnState.Not;
		pawn.OnHostRespawn();
	}

	public void Respawn()
	{
		if ( PlayerPawn.IsValid() )
			PlayerPawn.Respawn();
		else
			Spawn();
	}

	protected void OnRespawnStateChanged( LifeState oldValue, LifeState newValue )
	{
		TimeSinceRespawnStateChanged = 0f;
	}
}
